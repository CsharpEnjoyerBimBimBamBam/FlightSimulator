using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MissileGuidanceSystem : MonoBehaviour
{
    public bool IsLaunched = false;
    public Rigidbody Target;
    public float EstimatedPreemption;
    public float RealPreemtion;
    public float EstimatedArrivalTime;
    public float DistanceToTarget;
    public float MinDistance;
    public float Count;
    public Missile Missile;
    private Rigidbody MissileRigidbody;
    private SteeringController _SteeringController;
    private float _SpeedTemp = 0;
    private float _Acceleration = 0;

    private void Start()
    {
        MissileRigidbody = Missile.EquipmentRigidbody;
        _SteeringController = new SteeringController(Missile.Rudders, transform);
        _SteeringController.SteerPower = 5;
        _SteeringController.DeadZone = Missile.DeadZone;
        _SteeringController.RuddersDeadAngle = Missile.RuddersDeadAngle;
    }

    private void Update()
    {
        if (PauseSwithcer.IsGamePaused)
            return;

        float _TargetSpeed = Target.velocity.magnitude;
        float _DistanceToTarget = (Target.transform.position - transform.position).magnitude;
        float _MissileArrivalTime = GetMissileArrivalTime(0.1f);
        float _Preemtion = _MissileArrivalTime * _TargetSpeed;
        EstimatedPreemption = _Preemtion;
        EstimatedArrivalTime = _MissileArrivalTime;
        Vector3 _TargetVector = (Target.velocity.normalized * _Preemtion) + Target.transform.position; //Target.transform.TransformPoint(TargetRigidbody.velocity.normalized * _Preemtion);
        float _RealPreemtion = Vector3.Distance(Target.transform.position, _TargetVector);
        //_TargetVector = Target.transform.TransformPoint(TargetRigidbody.velocity.normalized * (_Preemtion * _Preemtion / _RealPreemtion));
        RealPreemtion = Vector3.Distance(Target.transform.position, _TargetVector);
        DistanceToTarget = _DistanceToTarget;
        _SteeringController.SetSteering(_TargetVector);

        if (_DistanceToTarget < MinDistance || MinDistance == 0)
            MinDistance = _DistanceToTarget;
    }

    private void FixedUpdate()
    {
        if (PauseSwithcer.IsGamePaused)
            return;

        _Acceleration = MissileRigidbody.velocity.magnitude - _SpeedTemp / Time.fixedDeltaTime;
        _SpeedTemp = MissileRigidbody.velocity.magnitude;   
    }

    private float GetMissileArrivalTime(float _Accuracy)
    {
        int _Count = 0;
        Vector3 _MissileLocalPosition = Target.transform.InverseTransformPoint(transform.position);
        float _RelativeSpeed = (MissileRigidbody.velocity - Target.velocity).magnitude;
        float _MissileSpeed = MissileRigidbody.velocity.magnitude;
        float _TargetSpeed = Target.velocity.magnitude;
        float _DistanceToTarget = (Target.transform.position - transform.position).magnitude;
        float _MissileToTargetAngle = Vector3.Angle(_MissileLocalPosition, Target.velocity);
        float _MinArrivalTime = _DistanceToTarget / _RelativeSpeed;
        float _MaxArrivalTime = _MinArrivalTime * 10;
        float _MiddleArrivalTime = (_MaxArrivalTime + _MinArrivalTime) / 2;
        while ((_MaxArrivalTime - _MinArrivalTime) / 2 > _Accuracy)
        {
            float _Equation = Mathf.Pow(_MiddleArrivalTime * _TargetSpeed, 2) + Mathf.Pow(_DistanceToTarget, 2) - 2 * _MiddleArrivalTime * _TargetSpeed *
                _DistanceToTarget * Mathf.Cos(_MissileToTargetAngle * Mathf.Deg2Rad) - Mathf.Pow(_MissileSpeed * _MiddleArrivalTime +
                (_Acceleration * Mathf.Pow(_MiddleArrivalTime, 2) / 2), 2);
            if (_Equation < 0)
                _MaxArrivalTime = _MiddleArrivalTime;
            else
                _MinArrivalTime = _MiddleArrivalTime;
            _MiddleArrivalTime = (_MaxArrivalTime + _MinArrivalTime) / 2;
            _Count++;
            if (_Count > 50)
                break;
        }
        Count = _Count;
        return _MiddleArrivalTime;
    }
}
