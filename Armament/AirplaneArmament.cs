using System;
using UnityEngine;

public class AirplaneArmament : AirplaneEquipment
{
    public float MaxLaunchRange { get; protected set; }
    public float MinLaunchRange { get; protected set; }
    public float TorqueCoefficient { get; protected set; }
    public float DragCoefficient { get; protected set; }

    public virtual void Launch(Vector3? _TargetVector = null, GameObject _TargetGameObject = null)
    {
        EquipmentRigidbody = EquipmentGameObject.AddComponent<Rigidbody>();
        EquipmentGameObject.AddComponent<FlyingObject>();
        EquipmentRigidbody.mass = Mass;
        if (ParentAirplane.Rigidbody != null)
            EquipmentRigidbody.velocity = ParentAirplane.Rigidbody.velocity;

        EquipmentGameObject.tag = "Targetable";
        ArmamentPhysics ArmamentPhysics = EquipmentGameObject.AddComponent<ArmamentPhysics>();
        ArmamentPhysics.Armament = this;
        Station.RemoveEquipment();
        ParentAirplane.RecalculateCenterOfMass(true);
        ParentAirplane.RecalculateTotalWeight();
        EquipmentGameObject.transform.parent = null;

        EquipmentGameObject.AddComponent<MovableObject>();
    }
}