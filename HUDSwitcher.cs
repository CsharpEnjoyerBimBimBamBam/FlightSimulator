using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDSwitcher : MonoBehaviour
{
    private bool _IsEnabled = true;
    private Transform _CameraTarget;
    private Rigidbody _CameraTargetRigidbody;
    private FlyingObject _TargetPhysics;
    [SerializeField] private GameObject _HUD;

    void Start()
    {
        UpdateCameraTarget();
        CameraMovement.OnTargetChanged += UpdateCameraTarget;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y) && ValidateCameraTarget())
        {
            _IsEnabled = !_HUD.activeInHierarchy;
            _HUD.SetActive(_IsEnabled);
        }
    }

    private void UpdateCameraTarget()
    {
        _CameraTarget = CameraMovement.target;

        if (_CameraTarget != null)
            _CameraTarget.TryGetComponent(out _TargetPhysics);

        if (_TargetPhysics != null)
            _CameraTargetRigidbody = _TargetPhysics.Rigidbody;

        if (ValidateCameraTarget())
        {
            _HUD.SetActive(_IsEnabled);
            return;
        }
        _HUD.SetActive(false);
    }

    private bool ValidateCameraTarget() => _CameraTarget != null && _TargetPhysics != null && _CameraTargetRigidbody != null;
}
