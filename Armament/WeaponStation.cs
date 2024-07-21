using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeaponStation
{
    public int Number { get; private set; }
    public int Index => Number - 1;
    public IReadOnlyList<string> PossibleEquipment { get; private set; } = new List<string>();
    public Vector3 LocalPosition { get; private set; }
    public bool IsNeedPylon => Pylon != null;
    public bool IsHaveEquipment => _CurrentEquipment != null;
    public bool IsEquipmentInstantiated => _CurrentEquipment?.EquipmentGameObject != null;
    public virtual AirplaneEquipment? CurrentEquipment 
    {
        get => _CurrentEquipment;
        set
        {
            if (value == null)
            {
                _CurrentEquipment = null;
                return;
            }

            value.Station = this;

            if (!IsNeedPylon || value is Pylon)
            {
                _CurrentEquipment = value;
                return;
            }

            Pylon PylonCopy = Pylon.CopyInstance();
            if (PylonCopy.WeaponStations.Count == 0)
                return;
            PylonCopy.Station = this;
            PylonCopy.WeaponStations[0].CurrentEquipment = value;
            _CurrentEquipment = PylonCopy;
        }         
    }
    public Airplane ParentAirplane;
    public Pylon? Pylon { get; private set; }
    private AirplaneEquipment? _CurrentEquipment = null;

    public void RemoveEquipment()
    {
        CurrentEquipment.Station = null;
        CurrentEquipment = null;
    }

    public void InstantiateEquipment()
    {
        if (!IsHaveEquipment)
            return;

        GameObject Equipment = CurrentEquipment.InstantiateGameObject();
        Equipment.transform.parent = ParentAirplane.gameObject.transform;
        Equipment.transform.localPosition = LocalPosition;
        Equipment.transform.localEulerAngles = Vector3.zero;
    }
}
