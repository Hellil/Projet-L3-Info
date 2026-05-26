using System.Collections;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using DG.Tweening;
using System.Diagnostics;

public class StarLauncher : MonoBehaviour
{
    private PlayerController _playerController;
    private GravityBody _gravityBody;
    private Rigidbody _rigidbody;
    private SplineContainer _currentSpline;

    [Range(0, 50)]
    public float speed = 10f;

    public bool insideLaunchStar;
    private Transform _launchObject;

    void Start()
    {
        _playerController = GetComponent<PlayerController>();
        _gravityBody = GetComponent<GravityBody>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (insideLaunchStar && Input.GetKeyDown(KeyCode.P))
        {
            _playerController.isLaunching = true;
            StartCoroutine(Launch());
        }
    }

    IEnumerator Launch()
    {
        _gravityBody.isActive = false;
        _rigidbody.isKinematic = true;
        DOTween.KillAll();

        _currentSpline = _launchObject.GetComponentInChildren<SplineContainer>();

        if (_currentSpline == null)
        {
            UnityEngine.Debug.LogError("ERREUR : Aucun SplineContainer trouvé !");
            yield break;
        }

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        float pathLength = _currentSpline.CalculateLength();
        float duration = pathLength / speed;

        // Log pour comprendre ce qui se passe
        UnityEngine.Debug.Log($"[StarLaunch] Spline trouvé : {_currentSpline.gameObject.name}");
        UnityEngine.Debug.Log($"[StarLaunch] Position joueur : {transform.position}");
        UnityEngine.Debug.Log($"[StarLaunch] Début spline (t=0) : {GetSplineWorldPosition(0f)}");
        UnityEngine.Debug.Log($"[StarLaunch] Fin spline (t=1) : {GetSplineWorldPosition(1f)}");
        UnityEngine.Debug.Log($"[StarLaunch] Durée du vol : {duration}s");

        // Vol direct le long du spline, sans parentage ni animation
        DOVirtual.Float(0f, 1f, duration, (t) =>
        {
            transform.position = GetSplineWorldPosition(t);
        })
        .SetEase(Ease.InOutSine)
        .OnComplete(Land);
    }

    Vector3 GetSplineWorldPosition(float t)
    {
        _currentSpline.Spline.Evaluate(
            t, out float3 pos, out float3 tangent, out float3 up);
        return _currentSpline.transform.TransformPoint((Vector3)pos);
    }

    void Land()
    {
        UnityEngine.Debug.Log($"[StarLaunch] Atterrissage à : {transform.position}");
        _rigidbody.isKinematic = false;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _gravityBody.isActive = true;
        _playerController.isLaunching = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Launch"))
        {
            insideLaunchStar = true;
            _launchObject = other.transform;
            UnityEngine.Debug.Log($"[StarLaunch] Trigger entré : {other.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Launch"))
            insideLaunchStar = false;
    }
}