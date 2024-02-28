using Unity.VisualScripting;
using UnityEngine;

public class ArmamentPhysics : MonoBehaviour, IPausable
{
    public float TorqueCoefficient;
    public float DragCoefficient;
    public float TrueAirSpeed;
    public float AngleOfAttack;
    public float _GForce;
    private Rigidbody _ArmamentRigidbody;
    private RigidbodyPhysics _Physics;
    private Vector3 _ArmamentVelocity;
    private Vector3 _ArmamentAngularVelocity;

    private void Start()
    {
        _ArmamentRigidbody = transform.GetComponent<Rigidbody>();
        _Physics = GetComponent<RigidbodyPhysics>();
        IPausable.OnGamePaused.AddListener(Pause);
        IPausable.OnGameUnpaused.AddListener(Unpause);
    }

    private void FixedUpdate()
    {
        if (IPausable.IsGamePaused)
            return;
        _GForce = _Physics.GForce;
        TrueAirSpeed = _Physics.TrueAirSpeed;
        AngleOfAttack = _Physics.AngleOfAttack;

        _ArmamentRigidbody.AddForce(-_ArmamentRigidbody.velocity.normalized * Mathf.Pow(_Physics.TrueAirSpeed, 2) 
            * Mathf.Exp(_Physics.AngleOfAttack / 100) * _Physics.AtmosphericPressure * DragCoefficient);
        _ArmamentRigidbody.AddRelativeTorque(Mathf.Atan(-_Physics.RelativePitchAngleOfAttack / 4) * 20 * (Mathf.Log10(_Physics.TrueAirSpeed + 1) * 50) * TorqueCoefficient *
            _Physics.AtmosphericPressure, Mathf.Atan(_Physics.RelativeYawAngleOfAttack / 4) * 20 * (Mathf.Log10(_Physics.TrueAirSpeed + 1) * 50) * TorqueCoefficient *
            _Physics.AtmosphericPressure, 0);
    }

    public void Pause()
    {
        _ArmamentVelocity = _ArmamentRigidbody.velocity;
        _ArmamentAngularVelocity = _ArmamentRigidbody.angularVelocity;
        _ArmamentRigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void Unpause()
    {
        _ArmamentRigidbody.constraints = RigidbodyConstraints.None;
        _ArmamentRigidbody.velocity = _ArmamentVelocity;
        _ArmamentRigidbody.angularVelocity = _ArmamentAngularVelocity;
    }
}
