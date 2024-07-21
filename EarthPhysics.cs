using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthPhysics : MonoBehaviour
{
    private void FixedUpdate()
    {
        foreach (FlyingObject _FlyingObject in FlyingObject.FlyingObjects)
            _FlyingObject.Rigidbody.AddForce(transform.position.normalized * Constants.Gravity * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }
}
