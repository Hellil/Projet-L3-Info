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
        if (insideLaunchStar && Input.GetKeyDown(KeyCode.Space))
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
            UnityEngine.Debug.LogError("Aucun SplineContainer trouvé !");
            yield break;
        }

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        float duration = _currentSpline.CalculateLength() / speed;

        DOVirtual.Float(0f, 1f, duration, (t) =>
        {
            _rigidbody.MovePosition(GetSplineWorldPosition(t));
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
        _rigidbody.isKinematic = false;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _playerController.isLaunching = false;

        GravityArea[] all = FindObjectsByType<GravityArea>(FindObjectsSortMode.None);
        GravityArea nearest = null;
        float best = float.MaxValue;
        foreach (var a in all)
        {
            float d = Vector3.Distance(transform.position, a.transform.position);
            if (d < best) { best = d; nearest = a; }
        }

        if (nearest != null)
            StartCoroutine(ForceGravity(nearest));
        else
            _gravityBody.isActive = true;
    }

    IEnumerator ForceGravity(GravityArea target)
    {
        float timer = 2f;
        while (timer > 0f)
        {
            timer -= Time.fixedDeltaTime;

            Vector3 dir = target.GetGravityDirection(_gravityBody).normalized;
            _rigidbody.AddForce(dir * 800f * Time.fixedDeltaTime, ForceMode.Acceleration);

            Quaternion targetRot = Quaternion.FromToRotation(transform.up, -dir)
                                   * transform.rotation;
            _rigidbody.MoveRotation(Quaternion.Slerp(
                _rigidbody.rotation, targetRot, Time.fixedDeltaTime * 5f));

            yield return new WaitForFixedUpdate();
        }

        _gravityBody.SetArea(target);
        _gravityBody.isActive = true;
        UnityEngine.Debug.Log("[Land] GravityBody réactivé");
    }

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