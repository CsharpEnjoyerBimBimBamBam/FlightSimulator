using Unity.VisualScripting;
using UnityEngine;

public class ArmamentPhysics : MonoBehaviour
{
    public AirplaneArmament Armament;
    private Rigidbody _ArmamentRigidbody;
    private FlyingObject _Physics;
    private Vector3 _ArmamentVelocity;
    private Vector3 _ArmamentAngularVelocity;

    private void Start()
    {
        _ArmamentRigidbody = Armament.EquipmentRigidbody;
        _Physics = GetComponent<FlyingObject>();
        PauseSwithcer.OnGamePaused += Pause;
        PauseSwithcer.OnGameUnpaused += Unpause;
    }

    private void FixedUpdate()
    {
        if (PauseSwithcer.IsGamePaused)
            return;

        SetDragForce();

        _ArmamentRigidbody.AddRelativeTorque(Mathf.Atan(-_Physics.RelativePitchAngleOfAttack / 4) * 20 * (Mathf.Log10(_Physics.TrueAirSpeed + 1) * 50) *
            Armament.TorqueCoefficient * _Physics.AtmosphericPressure, Mathf.Atan(_Physics.RelativeYawAngleOfAttack / 4) * 20 * 
            (Mathf.Log10(_Physics.TrueAirSpeed + 1) * 50) * Armament.TorqueCoefficient * _Physics.AtmosphericPressure, 0);
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

    private void SetDragForce()
    {
        float DragForce = Mathf.Pow(_Physics.TrueAirSpeed, 2) * Mathf.Exp(_Physics.AngleOfAttack / 100) * _Physics.AtmosphericPressure * Armament.DragCoefficient *
            Time.fixedDeltaTime;
        _ArmamentRigidbody.AddForce(-_ArmamentRigidbody.velocity.normalized * DragForce);
    }
}
