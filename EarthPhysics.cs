using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthPhysics : MonoBehaviour
{
    private List<Rigidbody> _Rigidbodies;

    void Start()
    {
        _Rigidbodies = new List<Rigidbody>(FindObjectsByType<Rigidbody>(FindObjectsSortMode.InstanceID));
    }

    private void FixedUpdate()
    {
        foreach (Rigidbody _Rigidbody in _Rigidbodies)
        {
            _Rigidbody.AddForce(transform.position.normalized * Constants.Gravity * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }
}
