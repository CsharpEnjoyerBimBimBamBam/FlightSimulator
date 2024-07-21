using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class SteeringController
{
    public SteeringController(IEnumerable<GameObject> Rudders, Transform SteeringObject)
    {
        _Rudders = Rudders.ToList();
        _SteeringObject = SteeringObject;
        MaxDeflectionAngle = 25;
        foreach (GameObject _Rudder in _Rudders)
        {
            _RuddersAngles[_Rudder] = 0;
        }
    }

    public float SteerPower = 1;
    public float MaxDeflectionAngle = 25;
    public float RuddersRotationSpeed = 300;
    public float DeadZone = 1f;
    public float RuddersDeadAngle;
    public float DampingCoefficient = 0.001f;
    public float MinRuddersRotationSpeed = 300;
    private Stopwatch _Timer = new Stopwatch();
    private Transform _SteeringObject;
    private List<GameObject> _Rudders;
    private Dictionary<GameObject, float> _RuddersAngles = new Dictionary<GameObject, float>();

    public void SetSteering(Vector3 _TargetPosition)
    {
        Vector3 _TargetLocalPosition = _SteeringObject.transform.InverseTransformPoint(_TargetPosition);
        float _RotationAngle = Vector3.Angle(Vector3.forward, _TargetLocalPosition);
        if (_RotationAngle < DeadZone)
            return;
        Vector3 _TargetVectorProjection = Vector3.ProjectOnPlane(_TargetLocalPosition, Vector3.forward);
        foreach (GameObject _Rudder in _Rudders)
        {
            float _CurrentRudderAngle = _RuddersAngles[_Rudder];
            Vector3 _RudderUpLocal = _SteeringObject.InverseTransformDirection(_Rudder.transform.up);
            Vector3 _RudderRightLocal = _SteeringObject.InverseTransformDirection(_Rudder.transform.right);
            Vector3 _RudderUpProjection = Vector3.ProjectOnPlane(_RudderUpLocal, Vector3.forward);
            Vector3 _RudderRightProjection = Vector3.ProjectOnPlane(_RudderRightLocal, Vector3.forward);
            float _RudderUpToTargetAngle = Vector3.Angle(_RudderUpProjection, _TargetVectorProjection);
            float _RudderRightToTargetAngle = Vector3.Angle(_RudderRightProjection, _TargetVectorProjection);
            if (_RudderUpToTargetAngle > 90)
                _RudderUpToTargetAngle = Vector3.Angle(-_RudderUpProjection, _TargetVectorProjection);
            float _AngleCoefficient = _RudderUpToTargetAngle / 90;
            if (_RudderRightToTargetAngle < 90)
                _AngleCoefficient *= -1;
            float _TargetAngle = Mathf.Clamp(_RotationAngle * SteerPower, -MaxDeflectionAngle, MaxDeflectionAngle) * _AngleCoefficient;
            if (Mathf.Abs(_TargetAngle) < RuddersDeadAngle)
                _TargetAngle = 0;
            if (_CurrentRudderAngle > _TargetAngle)
                _CurrentRudderAngle -= RuddersRotationSpeed * ((float)_Timer.Elapsed.Milliseconds / 1000);
            else if (_CurrentRudderAngle < _TargetAngle)
                _CurrentRudderAngle += RuddersRotationSpeed * ((float)_Timer.Elapsed.Milliseconds / 1000);
            _CurrentRudderAngle = Mathf.Clamp(_CurrentRudderAngle, -Mathf.Abs(_TargetAngle), Mathf.Abs(_TargetAngle));
            _Rudder.transform.Rotate(0, _CurrentRudderAngle - _RuddersAngles[_Rudder], 0);
            _RuddersAngles[_Rudder] = _CurrentRudderAngle;
        }
        _Timer.Restart();
    }
}
