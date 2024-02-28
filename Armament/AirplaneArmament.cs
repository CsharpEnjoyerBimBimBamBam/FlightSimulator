using System;
using UnityEngine;

public class AirplaneArmament
{
    public float Mass { get; protected set; }
    public float Diameter { get; protected set; }
    public float MaxLaunchRange { get; protected set; }
    public float MinLaunchRange { get; protected set; }
    public ArmamentName Name { get; protected set; }
    public ArmamentType Type { get; protected set; }
    public float TorqueCoefficient { get; protected set; }
    public float DragCoefficient { get; protected set; }
    public int WeaponStationNumber;
    public Airplane ParentAirplane;
    public GameObject gameObject;
    public Rigidbody rigidBody;

    public GameObject LoadGameObject()
    {
        return Resources.Load<GameObject>("ArmSuspensions/" + Name.ToString());
    }

    public static ArmamentType TypeStringToEnum(string _Type)
    {
        return (ArmamentType)Enum.Parse(typeof(ArmamentType), _Type);
    }

    public static ArmamentName NameStringToEnum(string _Name)
    {
        return (ArmamentName)Enum.Parse(typeof(ArmamentName), _Name);
    }

    public static GameObject LoadPylonGameObject(PlaneName _PlaneName)
    {
        return Resources.Load<GameObject>("ArmSuspensions/" + _PlaneName.ToString() + "Pylon");
    }

    public static GameObject LoadGameObject(ArmamentName _ArmamentName)
    {
        return Resources.Load<GameObject>("ArmSuspensions/" + _ArmamentName.ToString());
    }

    public static AirplaneArmament GetArmamentByName(ArmamentName _Name)
    {
        Type _Type = System.Type.GetType(_Name.ToString());
        return (AirplaneArmament)Activator.CreateInstance(_Type);
    }

    public virtual void Launch(Vector3? _TargetVector = null, GameObject _TargetGameObject = null)
    {
        Rigidbody _PlaneRigidbody = ParentAirplane.rigidBody;
        Rigidbody _ArmamentRigidbody = gameObject.AddComponent<Rigidbody>();
        rigidBody = _ArmamentRigidbody;
        _ArmamentRigidbody.mass = Mass;
        if (_PlaneRigidbody != null)
            _ArmamentRigidbody.velocity = _PlaneRigidbody.velocity;

        gameObject.tag = "Targetable";
        ArmamentPhysics ArmamentPhysics = gameObject.AddComponent<ArmamentPhysics>();
        ArmamentPhysics.TorqueCoefficient = TorqueCoefficient;
        ArmamentPhysics.DragCoefficient = DragCoefficient;
        ParentAirplane.Armament.Remove(this);
        ParentAirplane.RecalculateCenterOfMass(true);
        ParentAirplane.RecalculateTotalWeight();
        gameObject.transform.parent = null;
    }
}