using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityBody : MonoBehaviour
{
    private static float GRAVITY_FORCE = 800f;
    [HideInInspector] public bool isActive = true;

    public Vector3 GravityDirection
    {
        get
        {
            if (gravityAreas.Count == 0) return Vector3.zero;
            gravityAreas.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            return gravityAreas.Last().GetGravityDirection(this).normalized;
        }
    }

    private Rigidbody rigidbody;
    private List<GravityArea> gravityAreas;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        gravityAreas = new List<GravityArea>();
    }

    void FixedUpdate()
    {
        if (!isActive) return;

        rigidbody.AddForce(
            GravityDirection * (GRAVITY_FORCE * Time.fixedDeltaTime),
            ForceMode.Acceleration);

        Quaternion upRotation = Quaternion.FromToRotation(transform.up, -GravityDirection);
        Quaternion newRotation = Quaternion.Slerp(
            rigidbody.rotation,
            upRotation * rigidbody.rotation,
            Time.fixedDeltaTime * 3f);
        rigidbody.MoveRotation(newRotation);
    }

    public void SetArea(GravityArea area)
    {
        gravityAreas.Clear();
        gravityAreas.Add(area);
    }

    public void AddGravityArea(GravityArea area)
    {
        if (!gravityAreas.Contains(area))
            gravityAreas.Add(area);
    }

    public void RemoveGravityArea(GravityArea area) => gravityAreas.Remove(area);
}