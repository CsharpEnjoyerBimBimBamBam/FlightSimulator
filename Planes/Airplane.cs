using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;

public class Airplane : MonoBehaviour
{
    public PlaneName Name { get; protected set; }
    public List<WeaponStation> WeaponStations { get; protected set; }
    public float MaxFuelMass { get; protected set; }
    public float EmptyWeight { get; protected set; }
    public float MaxWeight { get; protected set; }
    public Vector3 EmptyCenterOfMass { get; protected set; }
    public Vector3 CenterOfLift { get; protected set; }
    public List<FuelTank> InternalFuelTanks = new List<FuelTank>();
    public List<AirplaneArmament> Armament;
    public PlaneState CurrentState;
    public Rigidbody rigidBody;
    public float FuelMass
    {
        get
        {
            float _FuelMass = 0;
            foreach (FuelTank _Tank in InternalFuelTanks)
            {
                _FuelMass += _Tank.CurrentWeight;
            }
            return _FuelMass;
        }
    }
    public float ArmamentMass 
    { 
        get
        {
            float _ArmamentMass = 0;
            foreach (AirplaneArmament _Armament in Armament)
            {
                _ArmamentMass += _Armament.Mass;
            }
            return _ArmamentMass;
        }
    }
    public float TotalWeight { get { return EmptyWeight + FuelMass + ArmamentMass; } }

    public void RecalculateTotalWeight()
    {
        rigidBody.mass = EmptyWeight + FuelMass + ArmamentMass;
    }

    public void RecalculateCenterOfMass(bool WithArmaments = false)
    {
        Vector3 _CenterOfMass = EmptyCenterOfMass * EmptyWeight;
        float _FuelMass = 0;
        float _ArmamentMass = 0;

        foreach (FuelTank _Tank in InternalFuelTanks)
        {
            _CenterOfMass += _Tank.LocalPosition * _Tank.CurrentWeight;
            _FuelMass += _Tank.CurrentWeight;
        }

        if (WithArmaments)
        {
            foreach (AirplaneArmament _Armament in Armament)
            {
                _CenterOfMass += _Armament.gameObject.transform.localPosition * _Armament.Mass;
                _ArmamentMass += _Armament.Mass;
            }
        }
        rigidBody.centerOfMass = _CenterOfMass / (EmptyWeight + _FuelMass + _ArmamentMass);
    }

    public GameObject LoadGameObject()
    {
        return Resources.Load<GameObject>("Planes/" + Name.ToString());
    }

    public static GameObject LoadGameObject(PlaneName _Name)
    {
        return Resources.Load<GameObject>("Planes/" + _Name.ToString());
    }

    public WeaponStation GetWeaponStationByNumber(int _StationNumber)
    {
        foreach(WeaponStation _WeaponStation in WeaponStations)
        {
            if (_WeaponStation.Number == _StationNumber)
                return _WeaponStation;
        }
        return null;
    }

    public AirplaneArmament GetArmamentByWeaponStationNumber(int _StationNumber)
    {
        foreach (AirplaneArmament _Armament in Armament)
        {
            if (_Armament.WeaponStationNumber == _StationNumber)
                return _Armament;
        }
        return null;
    }

    public static Airplane GetPlaneByName(PlaneName _PlaneName)
    {
        return LoadGameObject(_PlaneName).GetComponent<Airplane>();
    }
}
