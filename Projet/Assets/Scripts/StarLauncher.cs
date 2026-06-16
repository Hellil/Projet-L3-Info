using System.Collections;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using DG.Tweening;
using System.Diagnostics;

public class StarLauncher : MonoBehaviour
{
    private PlayerController playerController;
    private GravityBody gravityBody;
    private Rigidbody rigidbody;
    private SplineContainer currentSpline;

    [Range(0, 50)]
    public float speed = 10f;

    public bool insideLaunchStar;
    private Transform launchObject;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        gravityBody = GetComponent<GravityBody>();
        rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (insideLaunchStar && Input.GetKeyDown(KeyCode.Space))
        {
            playerController.isLaunching = true;
            StartCoroutine(Launch());
        }
    }

    IEnumerator Launch()
    {
        gravityBody.isActive = false;
        rigidbody.isKinematic = true;
        DOTween.KillAll();

        currentSpline = launchObject.GetComponentInChildren<SplineContainer>();
        if (currentSpline == null)
        {
            UnityEngine.Debug.LogError("Aucun SplineContainer trouvé !");
            yield break;
        }

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        float duration = currentSpline.CalculateLength() / speed;

        DOVirtual.Float(0f, 1f, duration, (t) =>
        {
            rigidbody.MovePosition(GetSplineWorldPosition(t));
        })
        .SetEase(Ease.InOutSine)
        .OnComplete(Land);
    }

    Vector3 GetSplineWorldPosition(float t)
    {
        currentSpline.Spline.Evaluate(
            t, out float3 pos, out float3 tangent, out float3 up);
        return currentSpline.transform.TransformPoint((Vector3)pos);
    }

    void Land()
    {
        rigidbody.isKinematic = false;
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        playerController.isLaunching = false;

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
            gravityBody.isActive = true;
    }

    IEnumerator ForceGravity(GravityArea target)
    {
        float timer = 2f;
        while (timer > 0f)
        {
            timer -= Time.fixedDeltaTime;

            Vector3 dir = target.GetGravityDirection(gravityBody).normalized;
            rigidbody.AddForce(dir * 800f * Time.fixedDeltaTime, ForceMode.Acceleration);

            Quaternion targetRot = Quaternion.FromToRotation(transform.up, -dir)
                                   * transform.rotation;
            rigidbody.MoveRotation(Quaternion.Slerp(
                rigidbody.rotation, targetRot, Time.fixedDeltaTime * 5f));

            yield return new WaitForFixedUpdate();
        }

        gravityBody.SetArea(target);
        gravityBody.isActive = true;
        UnityEngine.Debug.Log("[Land] GravityBody réactivé");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Launch"))
        {
            insideLaunchStar = true;
            launchObject = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Launch"))
            insideLaunchStar = false;
    }
}