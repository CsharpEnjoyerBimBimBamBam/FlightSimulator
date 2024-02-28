using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class CameraMove : MonoBehaviour
{
    [SerializeField] private float sensitivity = 2;
    [SerializeField] private float limit = 90;
    [SerializeField] private float zoom = 10;
    public static Transform target;
    public Transform Target;
    public static UnityEvent OnCameraTargetChange = new UnityEvent();
    public static UnityEvent OnObjectsMovmentStart = new UnityEvent();
    public static UnityEvent OnObjectsMovmentEnd = new UnityEvent();
    private float X, Y;
    private int CurrentTargetNumber = 0;
    private bool _IsCameraActive = false;
    private Vector3 _Offset;
    private float _CameraAltitude;
    private Vector3 _EarthPositionLocal;
    private Rigidbody _TargetRigidbody;

    void Start()
    {
        Target.position = new Coordinates(56.797351m, 60.468231m).ToWorldPosition(5000);
        target = Target;
        limit = Mathf.Abs(limit);
        if (limit > 90) limit = 90;
        _Offset = new Vector3(_Offset.x, _Offset.y, -10);
        transform.position = target.position + _Offset;
        _EarthPositionLocal = transform.InverseTransformPoint(TerrainGenerator.Earth.transform.position);
        transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.Project(transform.up, transform.TransformDirection(-_EarthPositionLocal)));
        ChangeTargetRigidbody();
        OnCameraTargetChange.AddListener(ChangeTargetRigidbody);
    }

    private void Update()
    {
        _EarthPositionLocal = transform.InverseTransformPoint(TerrainGenerator.Earth.transform.position);
        _CameraAltitude = _EarthPositionLocal.magnitude - Constants.EarthRadius;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IPausable.IsGamePaused)
            {
                IPausable.IsGamePaused = false;
                IPausable.OnGameUnpaused.Invoke();
            }
            else
            {
                IPausable.IsGamePaused = true;
                IPausable.OnGamePaused.Invoke();
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            GameObject[] _TargetableGameObjects = GameObject.FindGameObjectsWithTag("Targetable");
            CurrentTargetNumber++;
            if (CurrentTargetNumber > _TargetableGameObjects.Length - 1)
                CurrentTargetNumber = 0;
            if (_TargetableGameObjects[CurrentTargetNumber].transform == target)
                CurrentTargetNumber++;
            target = _TargetableGameObjects[CurrentTargetNumber].transform;
            OnCameraTargetChange.Invoke();
        }

        if (Input.GetMouseButtonDown(0))
            _IsCameraActive = true;
        if (Input.GetMouseButtonUp(0))
            _IsCameraActive = false;

        if (Input.GetAxis("Mouse ScrollWheel") > 0) _Offset.z += zoom;
        else if (Input.GetAxis("Mouse ScrollWheel") < 0) _Offset.z -= zoom;
        transform.position = transform.localRotation * _Offset + target.position;
        transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.Project(transform.up, transform.TransformDirection(-_EarthPositionLocal)));

        if (_IsCameraActive)
        {
            X = Input.GetAxis("Mouse X") * sensitivity;
            Y = -Input.GetAxis("Mouse Y") * sensitivity;
            float _EarthToCameraAngleTemp = Vector3.Angle(_EarthPositionLocal, Vector3.forward);
            transform.Rotate(Y, X, 0);
            float _EarthToCameraAngle = Vector3.Angle(_EarthPositionLocal, Vector3.forward);
            if ((Y > 0 & _EarthToCameraAngle < _EarthToCameraAngleTemp) || (Y < 0 & _EarthToCameraAngle > _EarthToCameraAngleTemp))
                transform.Rotate(-Y, -X, 0);
            transform.position = transform.localRotation * _Offset + target.position;
        }

        float _DistanceToHorizon = Mathf.Sqrt(Mathf.Pow(Constants.EarthRadius + _CameraAltitude, 2) - Mathf.Pow(Constants.EarthRadius, 2));
        if (float.IsNaN(_DistanceToHorizon))
            _DistanceToHorizon = 50000;
        Camera.main.farClipPlane = _DistanceToHorizon * 1.5f;

        if (Mathf.Abs(target.position.x) > 10000 || Mathf.Abs(target.position.y) > 10000 || Mathf.Abs(target.position.z) > 10000)
        {
            OnObjectsMovmentStart.Invoke();
            TerrainGenerator.Earth.transform.localPosition -= target.position;
            if (_TargetRigidbody != null)
            {
                _TargetRigidbody.transform.position = Vector3.zero;
            }
            else
            {
                Target.position = Vector3.zero;
            }
            OnObjectsMovmentEnd.Invoke();
        }
    }

    private void ChangeTargetRigidbody()
    {
        target.TryGetComponent(out Rigidbody _targetRigidbody);
        _TargetRigidbody = _targetRigidbody;
    }
}
