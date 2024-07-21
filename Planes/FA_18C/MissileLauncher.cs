using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileLauncher : MonoBehaviour
{
    [SerializeField] private Airplane CurrentPlane;
    private int _CurrentWeaponStation = 1;
    public GameObject Target;

    private void Start() => CurrentPlane = GetComponent<Airplane>();

    void Update()
    {
        if (PauseSwithcer.IsGamePaused)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            _CurrentWeaponStation = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            _CurrentWeaponStation = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            _CurrentWeaponStation = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            _CurrentWeaponStation = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            _CurrentWeaponStation = 5;
        else if (Input.GetKeyDown(KeyCode.Alpha6))
            _CurrentWeaponStation = 6;
        else if (Input.GetKeyDown(KeyCode.Alpha7))
            _CurrentWeaponStation = 7;
        else if (Input.GetKeyDown(KeyCode.Alpha8))
            _CurrentWeaponStation = 8;
        else if (Input.GetKeyDown(KeyCode.Alpha9))
            _CurrentWeaponStation = 9;

        if (Input.GetKeyDown(KeyCode.Space))
        {          
            AirplaneEquipment _CurrentArmament = CurrentPlane.GetWeaponStationByNumber(_CurrentWeaponStation).CurrentEquipment;
            Pylon _Pylon = _CurrentArmament as Pylon;
            Debug.Log(_CurrentArmament.Name);
            AirplaneEquipment PylonEquipment = _Pylon.WeaponStations[0].CurrentEquipment;
            (PylonEquipment as AirplaneArmament).Launch(_TargetGameObject: Target);
        }
    }
}
