using System.Collections;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using Unity.Cinemachine;

public class StarLauncher : MonoBehaviour
{
    private PlayerController _playerController;
    private GravityBody _gravityBody;
    private Rigidbody _rigidbody;
    private TrailRenderer _trail;
    private StarAnimation _starAnimation;

    public AnimationCurve pathCurve;

    [Range(0, 50)]
    public float speed = 10f;
    private float _speedModifier = 1f;

    [Header("État (lecture seule)")]
    public bool insideLaunchStar;
    public bool flying;
    public bool almostFinished;

    private Transform _launchObject;

    [Header("Références")]
    public CinemachineSplineCart dollyCart;
    public Transform playerParent;

    [Header("Séquence de lancement")]
    public float prepMoveDuration = 0.15f;
    public float launchInterval = 0.5f;

    [Header("Particules")]
    public ParticleSystem followParticles;
    public ParticleSystem smokeParticle;

    void Start()
    {
        _playerController = GetComponent<PlayerController>();
        _gravityBody = GetComponent<GravityBody>();
        _rigidbody = GetComponent<Rigidbody>();

        if (dollyCart != null)
            _trail = dollyCart.GetComponentInChildren<TrailRenderer>();
    }

    void Update()
    {
        if (insideLaunchStar && !flying && Input.GetKeyDown(KeyCode.P))
        {
            _playerController.isLaunching = true;
            StartCoroutine(CenterLaunch());
        }

        if (flying && dollyCart != null)
        {
            playerParent.position = dollyCart.transform.position;

            if (!almostFinished)
                playerParent.rotation = dollyCart.transform.rotation;
        }

        if (dollyCart != null && dollyCart.SplinePosition > 0.7f
            && !almostFinished && flying)
        {
            almostFinished = true;
            playerParent
                .DORotate(new Vector3(360 + 180, 0, 0), 0.5f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                    playerParent.DORotate(
                        new Vector3(-90, playerParent.eulerAngles.y,
                                        playerParent.eulerAngles.z), 0.2f));
        }
    }

    IEnumerator CenterLaunch()
    {
        _gravityBody.isActive = false;
        _rigidbody.isKinematic = true;
        transform.parent = null;
        DOTween.KillAll();

        var speedMod = _launchObject.GetComponent<SpeedModifier>();
        _speedModifier = speedMod != null ? speedMod.modifier : 1f;

        _starAnimation = _launchObject.GetComponentInChildren<StarAnimation>();

        SplineContainer splineContainer =
            _launchObject.GetComponentInChildren<SplineContainer>();

        dollyCart.SplinePosition = 0f;
        dollyCart.Spline = splineContainer;
        dollyCart.enabled = true;

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Sequence center = DOTween.Sequence();
        center.Append(transform.DOMove(dollyCart.transform.position, 0.2f));
        center.Join(transform.DORotate(
            dollyCart.transform.eulerAngles + new Vector3(90, 0, 0), 0.2f));
        if (_starAnimation != null)
            center.Join(_starAnimation.Reset(0.2f));
        center.OnComplete(LaunchSequence);
    }

    void LaunchSequence()
    {
        SplineContainer splineContainer =
            _launchObject.GetComponentInChildren<SplineContainer>();

        float pathLength = splineContainer.CalculateLength();
        float finalSpeed = pathLength / (speed * _speedModifier);

        playerParent.position = dollyCart.transform.position;
        playerParent.rotation = transform.rotation;

        Sequence s = DOTween.Sequence();

        s.AppendCallback(() => transform.parent = playerParent);

        s.Append(transform.DOLocalMove(
            transform.localPosition - transform.up, prepMoveDuration));
        s.Join(transform.DOLocalRotate(
            new Vector3(0, 360 * 2, 0), prepMoveDuration,
            RotateMode.LocalAxisAdd).SetEase(Ease.OutQuart));
        if (_starAnimation != null)
            s.Join(_starAnimation.PullStar(prepMoveDuration));

        s.AppendInterval(launchInterval);

        s.AppendCallback(() => flying = true);

        if (_trail != null)
            s.AppendCallback(() => _trail.emitting = true);
        if (followParticles != null)
            s.AppendCallback(() => followParticles.Play());

        s.Append(DOVirtual.Float(0f, 1f, finalSpeed, PathSpeed).SetEase(pathCurve));
        if (_starAnimation != null)
            s.Join(_starAnimation.PunchStar(0.5f));
        s.Join(transform.DOLocalMove(new Vector3(0, 0, -0.5f), 0.5f));
        s.Join(transform.DOLocalRotate(
            new Vector3(0, 360, 0), finalSpeed / 1.3f,
            RotateMode.LocalAxisAdd).SetEase(Ease.InOutSine));

        s.AppendCallback(Land);
    }

    void Land()
    {
        playerParent.DOComplete();
        transform.parent = null;
        dollyCart.enabled = false;
        dollyCart.SplinePosition = 0f;

        if (_trail != null) _trail.emitting = false;
        if (followParticles != null) followParticles.Stop();

        flying = false;
        almostFinished = false;
        _speedModifier = 1f;

        _rigidbody.isKinematic = false;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        _gravityBody.isActive = true;
        _playerController.isLaunching = false;
    }

    public void PathSpeed(float x) => dollyCart.SplinePosition = x;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Launch"))
        {
            insideLaunchStar = true;
            _launchObject = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Launch"))
            insideLaunchStar = false;
    }
}