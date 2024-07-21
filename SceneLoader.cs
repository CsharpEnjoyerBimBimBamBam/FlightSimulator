using UnityEngine;
using System;
using System.Linq;

public class SceneLoader : MonoBehaviour
{
    public static AirplaneParameters Parameters;
    public static IReadOnlyAirport SelectedAirport;
    public static IReadOnlyRunway SelectedRunway;
    [SerializeField] private GameObject TestTarget;

    private void Start()
    {
        GameObject _AirplanePlaneGameObject = Instantiate(Airplane.LoadPrefab(Parameters.Name));
        Airplane _Airplane = _AirplanePlaneGameObject.GetComponent<Airplane>();
        _Airplane.CopyParameters(Parameters);
        Debug.Log(SelectedRunway.Coordinates);
        _Airplane.InstantiateEquipments();
        float Altitude = SelectedAirport.Elevation * Constants.FtToMt;
        _AirplanePlaneGameObject.transform.position = SelectedRunway.Coordinates.ToWorldPosition(5000);
        _Airplane.SetHeading(SelectedRunway.Heading);

        GameObject _Target = Instantiate(TestTarget);
        _Target.transform.position = _AirplanePlaneGameObject.transform.position + new Vector3(20000, 20000, 20000);
        MissileLauncher _MissileLauncher = _AirplanePlaneGameObject.GetComponent<MissileLauncher>();
        _MissileLauncher.Target = _Target;
        CameraMovement.target = _AirplanePlaneGameObject.transform;

        _Airplane.RecalculateCenterOfMass(true);
        _Airplane.RecalculateTotalWeight();
    }
}
