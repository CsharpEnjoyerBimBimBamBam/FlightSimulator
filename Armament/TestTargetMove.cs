using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTargetMove : MonoBehaviour
{
    private float _Speed = 200;
    private Rigidbody _TargetRigidbody;
    private LineRenderer _LineRenderer;
    private Vector3 _Velocity;

    void Start()
    {
        _TargetRigidbody = transform.GetComponent<Rigidbody>();
        _TargetRigidbody.velocity = new Vector3(_Speed, 0, 0);
        _LineRenderer = GetComponent<LineRenderer>();
        PauseSwithcer.OnGamePaused += Pause;
        PauseSwithcer.OnGameUnpaused += Unpause;
    }

    private void Update()
    {
        if (PauseSwithcer.IsGamePaused)
            return;

        Vector3 _VelocityInWorldSpace = transform.TransformPoint(_TargetRigidbody.velocity);
        _LineRenderer.SetPosition(0, transform.position);
        _LineRenderer.SetPosition(1, _VelocityInWorldSpace);
    }

    public void Pause()
    {
        _Velocity = _TargetRigidbody.velocity;
        _TargetRigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void Unpause()
    {
        _TargetRigidbody.constraints = RigidbodyConstraints.None;
        _TargetRigidbody.velocity = _Velocity;
    }
}
