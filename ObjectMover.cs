using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CameraMovement))]
public class ObjectMover : MonoBehaviour
{
    public static event Action<Vector3> OnMovmentStart, OnMovmentEnd;
    [SerializeField] private float CameraTargetMaxPosition = 10000;
    private Transform _CameraTarget;

    private void Start()
    {
        _CameraTarget = CameraMovement.target;
        CameraMovement.OnTargetChanged += () => _CameraTarget = CameraMovement.target;
    }

    private void Update()
    {
        if (_CameraTarget == null)
            return;

        if (Mathf.Abs(_CameraTarget.position.x) < CameraTargetMaxPosition &&
            Mathf.Abs(_CameraTarget.position.y) < CameraTargetMaxPosition &&
            Mathf.Abs(_CameraTarget.position.z) < CameraTargetMaxPosition)
            return;

        Vector3 Shift = -_CameraTarget.position;
        OnMovmentStart.Invoke(Shift);
        foreach (MovableObject CurrentGameObject in MovableObject.MovableObjects)
        {
            if (CurrentGameObject.transform == _CameraTarget)
                continue;
        
            CurrentGameObject.transform.position += Shift;
        }

        _CameraTarget.position = Vector3.zero;
        OnMovmentEnd.Invoke(Shift);
    }
}
