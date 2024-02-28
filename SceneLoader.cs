using UnityEngine;
using System;

public class SceneLoader : MonoBehaviour
{
    public static Airplane Airplane;
    [SerializeField] private GameObject TestTarget;

    private void Awake()
    {
        GameObject _PylonPrefab = AirplaneArmament.LoadPylonGameObject(Airplane.Name);
        GameObject _AirplanePlaneGameObject = Instantiate(Airplane.gameObject);
        Rigidbody _AirplanePlaneRigidbody = _AirplanePlaneGameObject.GetComponent<Rigidbody>();

        Airplane.rigidBody = _AirplanePlaneRigidbody;
        GameObject _Target = Instantiate(TestTarget);
        _Target.transform.position = _AirplanePlaneGameObject.transform.position;
        _AirplanePlaneGameObject.transform.Translate(0, 3000, 0);
        _Target.transform.Translate(0, 5000, -25000);
        MissileLauncher _MissileLauncher = _AirplanePlaneGameObject.GetComponent<MissileLauncher>();
        _MissileLauncher.Target = _Target;
        CameraMove.target = _AirplanePlaneGameObject.transform;
        for (int i = 0; i < Airplane.Armament.Count; i++)
        {
            WeaponStation _WeaponStation = Airplane.GetWeaponStationByNumber(Airplane.Armament[i].WeaponStationNumber);
            GameObject _ArmamentPrefab = Airplane.Armament[i].LoadGameObject();
            GameObject _ArmamentGameObject = Instantiate(_ArmamentPrefab, _AirplanePlaneGameObject.transform);
            Airplane.Armament[i].gameObject = _ArmamentGameObject;
            float _ArmamentDiameter = Airplane.Armament[i].Diameter;
            if (_WeaponStation.IsNeedPylon)
            {
                GameObject _PylonGameObject = Instantiate(_PylonPrefab, _AirplanePlaneGameObject.transform);
                Vector3 _PylonSize = _PylonGameObject.GetComponent<MeshFilter>().sharedMesh.bounds.size;
                _PylonGameObject.transform.localPosition = new Vector3(_WeaponStation.LocalPosition.x,
                    _WeaponStation.LocalPosition.y - _PylonSize.y / 4, _WeaponStation.LocalPosition.z);
                _ArmamentGameObject.transform.localPosition = new Vector3(_PylonGameObject.transform.localPosition.x,
                _PylonGameObject.transform.localPosition.y - (_PylonSize.y / 4) - (_ArmamentDiameter / 2), _PylonGameObject.transform.localPosition.z);
            }
            else
            {
                _ArmamentGameObject.transform.localPosition = new Vector3(_WeaponStation.LocalPosition.x,
                _WeaponStation.LocalPosition.y - (_ArmamentDiameter / 2), _WeaponStation.LocalPosition.z);
            }                     
        }
        Airplane airplane = _AirplanePlaneGameObject.GetComponent<Airplane>();
        airplane.InternalFuelTanks = Airplane.InternalFuelTanks;
        airplane.Armament = Airplane.Armament;       

        Airplane.RecalculateCenterOfMass(true);
        Airplane.RecalculateTotalWeight();
    }
}
