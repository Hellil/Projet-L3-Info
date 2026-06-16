using System.Collections.Generic;
using UnityEngine;

public class GravityAreaInversePoint : GravityArea
{
    [SerializeField] private Vector3 center;

    
    public override Vector3 GetGravityDirection(GravityBody gravityBody)
    {
        return (gravityBody.transform.position - center).normalized;
    }
}
