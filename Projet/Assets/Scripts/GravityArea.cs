using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class GravityArea : MonoBehaviour
{
    [SerializeField] private int priority;
    public int Priority => priority;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    public abstract Vector3 GetGravityDirection(GravityBody gravityBody);

    private void OnTriggerEnter(Collider other)
    {
        GravityBody gravityBody = other.GetComponentInParent<GravityBody>();
        if (gravityBody != null)
        {
            gravityBody.AddGravityArea(this);
            UnityEngine.Debug.Log($"[GravityArea] {name} → Enter : {gravityBody.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GravityBody gravityBody = other.GetComponentInParent<GravityBody>();
        if (gravityBody != null)
        {
            gravityBody.RemoveGravityArea(this);
            UnityEngine.Debug.Log($"[GravityArea] {name} → Exit : {gravityBody.name}");
        }
    }
}