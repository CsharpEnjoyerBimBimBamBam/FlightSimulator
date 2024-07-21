using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AirplaneParameters
{
    public string Name { get; private set; } = "";
    public string FormatedName { get; private set; } = "";
    public IReadOnlyList<WeaponStation> WeaponStations { get; private set; } = new List<WeaponStation>();
    public float MaxFuelMass { get; private set; }
    public float EmptyWeight { get; private set; }
    public float MaxWeight { get; private set; }
    public Vector3 EmptyCenterOfMass { get; private set; }
    public Vector3 CenterOfLift { get; private set; }
    public IReadOnlyList<FuelTank> InternalFuelTanks { get; private set; } = new List<FuelTank>();
    public PlaneState CurrentState { get; private set; }

    public void SetEquipment(string EquipmentName, int WeaponStationNumber)
    {
        if (EquipmentName == AirplaneEquipment.None)
            return;

       WeaponStations[WeaponStationNumber - 1].CurrentEquipment = AirplaneEquipment.Load(EquipmentName);
    }

    public void SetEquipments(List<string> Equipments)
    {
        for (int i = 0; i < Equipments.Count; i++)
        {
            if (Equipments[i] == AirplaneEquipment.None)
                continue;

            WeaponStations[i].CurrentEquipment = AirplaneEquipment.Load(Equipments[i]);
        }
    }
}

