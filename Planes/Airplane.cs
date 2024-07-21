using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

public class Airplane : FlyingObject
{
    public AirplaneParameters Parameters { get; private set; } = new AirplaneParameters();
    public float FuelMass
    {
        get
        {
            float _FuelMass = 0;
            foreach (FuelTank _Tank in Parameters.InternalFuelTanks)
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
            foreach (WeaponStation Station in Parameters.WeaponStations)
            {
                if (!Station.IsHaveEquipment)
                    continue;

                _ArmamentMass += Station.CurrentEquipment.Mass;
            }
            return _ArmamentMass;
        }
    }
    public float TotalWeight => Parameters.EmptyWeight + FuelMass + ArmamentMass;
    private static Dictionary<string, string> _AirplanesData = new Dictionary<string, string>();

    public void RecalculateTotalWeight()
    {
        if (Rigidbody == null)
            Start();

        Rigidbody.mass = TotalWeight;
    }

    public void RecalculateCenterOfMass(bool WithArmaments = false)
    {
        Vector3 _CenterOfMass = Parameters.EmptyCenterOfMass * Parameters.EmptyWeight;
        float _FuelMass = 0;
        float _ArmamentMass = 0;

        foreach (FuelTank _Tank in Parameters.InternalFuelTanks)
        {
            _CenterOfMass += _Tank.LocalPosition * _Tank.CurrentWeight;
            _FuelMass += _Tank.CurrentWeight;
        }

        if (WithArmaments)
        {
            foreach (WeaponStation Station in Parameters.WeaponStations)
            {
                if (!Station.IsHaveEquipment)
                    continue;

                _CenterOfMass += Station.LocalPosition * Station.CurrentEquipment.Mass;
                _ArmamentMass += Station.CurrentEquipment.Mass;
            }
        }
        if (Rigidbody == null)
            Start();
        Rigidbody.centerOfMass = _CenterOfMass / (Parameters.EmptyWeight + _FuelMass + _ArmamentMass);
    }

    public static GameObject LoadPrefab(string _Name) => Resources.Load<GameObject>(ResourcesPath.Planes + _Name);

    public GameObject LoadPrefab() => LoadPrefab(Parameters.Name);

    public GameObject InstantiateGameObject() => MonoBehaviour.Instantiate(LoadPrefab());

    public static AirplaneParameters LoadParameters(string _PlaneName)
    {
        string Data;
        if (_AirplanesData.ContainsKey(_PlaneName))
            Data = _AirplanesData[_PlaneName];
        else
            Data = File.ReadAllText(AbsolutePath.PlanesSettings + $"{_PlaneName}.json");

        AirplaneParameters Parameters = JsonConvert.DeserializeObject<AirplaneParameters>(Data, new JsonSerializerSettings()
        {
            ContractResolver = new PrivateResolver(),
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        });

        return Parameters;
    }

    public void InstantiateEquipments()
    {
        foreach (WeaponStation Station in Parameters.WeaponStations)
            Station.InstantiateEquipment();
    }

    public WeaponStation GetWeaponStationByNumber(int Number) => Parameters.WeaponStations[Number - 1];

    public void CopyParameters(AirplaneParameters _Parameters)
    {
        foreach (WeaponStation Station in _Parameters.WeaponStations)
            Station.ParentAirplane = this;

        Parameters = _Parameters;
    }

    public class PrivateResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);
            if (!prop.Writable)
            {
                var property = member as PropertyInfo;
                var hasPrivateSetter = property?.GetSetMethod(true) != null;
                prop.Writable = hasPrivateSetter;
            }
            return prop;
        }
    }
}
