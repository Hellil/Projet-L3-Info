using System.Collections.Generic;
using UnityEngine;

public class GravityAreaCenter : GravityArea
{
    
    public override Vector3 GetGravityDirection(GravityBody gravityBody)
    {
        return (transform.position - gravityBody.transform.position).normalized;
    }
}
