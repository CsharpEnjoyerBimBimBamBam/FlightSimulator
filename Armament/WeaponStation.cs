using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponStation
{
    public int Number;
    public List<ArmamentName> PossibleArmament;
    public Vector3 LocalPosition;
    public bool IsNeedPylon;

    public List<string> EnumToString()
    {
        List<string> _ArmamentList = new List<string>();
        foreach(ArmamentName _Armament in PossibleArmament)
        {
            _ArmamentList.Add(_Armament.ToString());
        }
        return _ArmamentList;
    }

    public static ArmamentName StringToEnum(string _WeaponStationName)
    {
        return (ArmamentName)Enum.Parse(typeof(ArmamentName), _WeaponStationName);
    }
}
