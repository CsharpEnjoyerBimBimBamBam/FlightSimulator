using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditorInternal;
using UnityEngine;

public class GuidedArmamentPhysics : MonoBehaviour
{
    public GuidedArmament Armament;
    private FlyingObject _ArmamentPhysics;

    private void Start() => _ArmamentPhysics = GetComponent<FlyingObject>();

    private void FixedUpdate()
    {
        if (PauseSwithcer.IsGamePaused)
            return;

        foreach (GameObject _Rudder in Armament.Rudders)
        {
            Vector3 _RudderForwardLocal = transform.InverseTransformDirection(_Rudder.transform.forward);
            Vector3 _RudderForwardProjection = -Vector3.ProjectOnPlane(_RudderForwardLocal, Vector3.forward);
            Vector3 _TorqueVector = transform.TransformDirection(new Vector3(-_RudderForwardProjection.y, _RudderForwardProjection.x, _RudderForwardProjection.z));
            Armament.EquipmentRigidbody.AddTorque(_TorqueVector * _ArmamentPhysics.TrueAirSpeed * _ArmamentPhysics.AtmosphericPressure * Armament.RotationForce);
        }

        Armament.EquipmentRigidbody.AddRelativeForce(Mathf.Atan(-_ArmamentPhysics.RelativeYawAngleOfAttack / 4) * Mathf.Log10(_ArmamentPhysics.TrueAirSpeed + 1) * 
            _ArmamentPhysics.AtmosphericPressure * Armament.LiftForce,
            Mathf.Atan(-_ArmamentPhysics.RelativePitchAngleOfAttack / 4) * Mathf.Log10(_ArmamentPhysics.TrueAirSpeed + 1) * 
            _ArmamentPhysics.AtmosphericPressure * Armament.LiftForce, 0);
    }
}
