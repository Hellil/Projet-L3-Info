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
            if (_gravityAreas.Count == 0) return Vector3.zero;
            _gravityAreas.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            return _gravityAreas.Last().GetGravityDirection(this).normalized;
        }
    }

    private Rigidbody _rigidbody;
    private List<GravityArea> _gravityAreas;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _gravityAreas = new List<GravityArea>();
    }

    void FixedUpdate()
    {
        if (!isActive) return;

        _rigidbody.AddForce(
            GravityDirection * (GRAVITY_FORCE * Time.fixedDeltaTime),
            ForceMode.Acceleration);

        Quaternion upRotation = Quaternion.FromToRotation(transform.up, -GravityDirection);
        Quaternion newRotation = Quaternion.Slerp(
            _rigidbody.rotation,
            upRotation * _rigidbody.rotation,
            Time.fixedDeltaTime * 3f);
        _rigidbody.MoveRotation(newRotation);
    }

    public void SetArea(GravityArea area)
    {
        _gravityAreas.Clear();
        _gravityAreas.Add(area);
    }

    public void AddGravityArea(GravityArea area)
    {
        if (!_gravityAreas.Contains(area))
            _gravityAreas.Add(area);
    }

    public void RemoveGravityArea(GravityArea area) => _gravityAreas.Remove(area);
}