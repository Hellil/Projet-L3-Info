using System.Collections.Generic;
using UnityEngine;

public class GravityAreaPoint : GravityArea
{
    [SerializeField] private Vector3 center;

    
    public override Vector3 GetGravityDirection(GravityBody gravityBody)
    {
        return (center - gravityBody.transform.position).normalized;
    }
}
