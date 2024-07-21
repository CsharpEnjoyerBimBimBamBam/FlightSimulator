using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class CameraMovement : MonoBehaviour
{
    public static Transform target;
    public Transform Target;
    public static event Action OnTargetChanged;
    [SerializeField] private float _Sensitivity = 2;
    [SerializeField] private float _Limit = 90;
    [SerializeField] private float _Zoom = 10;
    private float _X, _Y;
    private int _CurrentTargetNumber = 0;
    private bool _IsCameraActive = false;
    private Vector3 _Offset;
    private float _CameraAltitude;
    private Vector3 _EarthPositionLocal;

    void Start()
    {
        //target.position = new GeoPosition(31.248687m, 34.254328m).ToWorldPosition(5000);
        Target = target;
        _Limit = Mathf.Abs(_Limit);
        if (_Limit > 90) _Limit = 90;
        _Offset = new Vector3(_Offset.x, _Offset.y, -10);
        transform.position = target.position + _Offset;
        _EarthPositionLocal = transform.InverseTransformPoint(TerrainGenerator.Earth.transform.position);
        transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.Project(transform.up, transform.TransformDirection(-_EarthPositionLocal)));
    }

    private void LateUpdate()
    {
        _EarthPositionLocal = transform.InverseTransformPoint(TerrainGenerator.Earth.transform.position);
        _CameraAltitude = _EarthPositionLocal.magnitude - Constants.EarthRadius;

        if (Input.GetKeyDown(KeyCode.T))
        {
            GameObject[] _TargetableGameObjects = GameObject.FindGameObjectsWithTag("Targetable");
            _CurrentTargetNumber++;
            if (_CurrentTargetNumber > _TargetableGameObjects.Length - 1)
                _CurrentTargetNumber = 0;
            if (_TargetableGameObjects[_CurrentTargetNumber].transform == target)
                _CurrentTargetNumber++;
            target = _TargetableGameObjects[_CurrentTargetNumber].transform;
            OnTargetChanged.Invoke();
        }

        if (Input.GetMouseButtonDown(0))
            _IsCameraActive = true;
        if (Input.GetMouseButtonUp(0))
            _IsCameraActive = false;

        if (Input.GetAxis("Mouse ScrollWheel") > 0) _Offset.z += _Zoom;
        else if (Input.GetAxis("Mouse ScrollWheel") < 0) _Offset.z -= _Zoom;
        transform.position = transform.localRotation * _Offset + target.position;
        transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.Project(transform.up, transform.TransformDirection(-_EarthPositionLocal)));

        if (_IsCameraActive)
        {
            _X = Input.GetAxis("Mouse X") * _Sensitivity;
            _Y = -Input.GetAxis("Mouse Y") * _Sensitivity;
            float _EarthToCameraAngleTemp = Vector3.Angle(_EarthPositionLocal, Vector3.forward);
            transform.Rotate(_Y, _X, 0);
            float _EarthToCameraAngle = Vector3.Angle(_EarthPositionLocal, Vector3.forward);
            if ((_Y > 0 & _EarthToCameraAngle < _EarthToCameraAngleTemp) || (_Y < 0 & _EarthToCameraAngle > _EarthToCameraAngleTemp))
                transform.Rotate(-_Y, -_X, 0);
            transform.position = transform.localRotation * _Offset + target.position;
        }

        float _DistanceToHorizon = Mathf.Sqrt(Mathf.Pow(Constants.EarthRadius + _CameraAltitude, 2) - Mathf.Pow(Constants.EarthRadius, 2));
        if (float.IsNaN(_DistanceToHorizon))
            _DistanceToHorizon = 50000;
        Camera.main.farClipPlane = _DistanceToHorizon * 1.5f;
    }
}
