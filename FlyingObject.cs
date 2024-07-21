using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class FlyingObject : MonoBehaviour
{
    public static float SurfaceTemperature = 15;
    public GeoPosition Coordinates => GeoPosition.FromWorldPosition(transform.position);
    public float Altitude => _EarthPositionLocal.magnitude - Constants.EarthRadius;
    public float Heading
    {
        get
        {
            Vector3 EarthUpLocal = _EarthUpLocal;
            Vector3 EarthPositionLocal = _EarthPositionLocal;
            Vector3 EarthUpProject = Vector3.ProjectOnPlane(EarthUpLocal, EarthPositionLocal);
            Vector3 ForwardProject = Vector3.ProjectOnPlane(Vector3.forward, EarthPositionLocal);
            Vector3 RotatedEarthUp = Quaternion.AngleAxis(-90, EarthPositionLocal) * EarthUpLocal;
            float Course = Vector3.Angle(EarthUpProject, ForwardProject);
            if (Vector3.Angle(RotatedEarthUp, ForwardProject) > 90)
                Course = 360 - Course;
            return Course;
        }
    }
    public float AtmosphericPressure => Mathf.Exp(-Altitude / 7450);
    public float VerticalSpeed
    {
        get
        {
            float AbsoluteVerticalSpeed = Vector3.ProjectOnPlane(_VelocityLocalPosition, _VelocityPlaneNormal).magnitude;
            if (90 - Vector3.Angle(-_EarthPositionLocal, _VelocityLocalPosition) < 0)
                AbsoluteVerticalSpeed *= -1;
            return AbsoluteVerticalSpeed;
        }
    }
    public float GroundSpeed => Vector3.ProjectOnPlane(_VelocityLocalPosition, _EarthPositionLocal).magnitude;
    public float Pitch => 90 - Vector3.Angle(-_EarthPositionLocal, Vector3.forward);
    public float Roll
    {
        get
        {
            float AbsoluteRoll = Vector3.Angle(Vector3.ProjectOnPlane(Vector3.up, _UpPlaneNormal), -_EarthPositionLocal);
            if (90 - Vector3.Angle(-_EarthPositionLocal, Vector3.right) < 0)
                AbsoluteRoll *= -1;
            return AbsoluteRoll;
        }
    }
    public float RelativeYawAngleOfAttack => 90 - Vector3.Angle(Rigidbody.transform.right, Rigidbody.velocity);
    public float RelativePitchAngleOfAttack => 90 - Vector3.Angle(Rigidbody.transform.up, Rigidbody.velocity);
    public float PitchAngleOfAttack => Mathf.Abs(RelativePitchAngleOfAttack);
    public float YawAngleOfAttack => Mathf.Abs(RelativeYawAngleOfAttack);
    public float AngleOfAttack => Vector3.Angle(Rigidbody.transform.forward, Rigidbody.velocity);
    public float TrueAirSpeed => Rigidbody.velocity.magnitude;
    public float IndicatedAirSpeed => TrueAirSpeed * ((Temperature + 273f) / (SurfaceTemperature + 273f));
    public float Temperature => SurfaceTemperature - (Altitude / 1000f * 6.5f);
    public float SoundSpeed => Mathf.Sqrt(401.8f * (SurfaceTemperature - Temperature + 273f));
    public float M => TrueAirSpeed / SoundSpeed;
    public float RelativeGForce { get; private set; }
    public float GForce => Mathf.Abs(RelativeGForce);
    public Rigidbody Rigidbody { get; private set; }
    public static IReadOnlyList<FlyingObject> FlyingObjects => _FlyingObjects;
    private static List<FlyingObject> _FlyingObjects = new List<FlyingObject>();
    private Vector3 _TempVelocity = Vector3.zero;
    private Vector3 _EarthPositionLocal => Rigidbody.transform.InverseTransformPoint(_Earth.transform.position);
    private Vector3 _VelocityPlaneNormal => Vector3.RotateTowards(_VelocityLocalPosition, _EarthPositionLocal, 
            (Vector3.Angle(_VelocityLocalPosition, _EarthPositionLocal) - 90) * Mathf.Deg2Rad, 0);
    private Vector3 _UpPlaneNormal => Vector3.RotateTowards(Vector3.forward, _EarthPositionLocal,
            (Vector3.Angle(Vector3.forward, _EarthPositionLocal) - 90) * Mathf.Deg2Rad, 0);
    private Vector3 _VelocityLocalPosition => transform.InverseTransformPoint(Rigidbody.velocity + transform.position);
    private Vector3 _EarthUpLocal => transform.InverseTransformDirection(_Earth.transform.up);
    private GameObject _Earth = TerrainGenerator.Earth;

    private void Awake()
    {
        _FlyingObjects.Add(this);
        Rigidbody = GetComponent<Rigidbody>();
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {     
        
    }

    protected virtual void FixedUpdate()
    {
        Vector3 _CurrentVelocityLocalPosition = _VelocityLocalPosition;
        RelativeGForce = Vector3.Distance(_TempVelocity, _CurrentVelocityLocalPosition) / Time.fixedDeltaTime / Constants.Gravity;
        if (RelativePitchAngleOfAttack < 0)
            RelativeGForce *= -1;
        _TempVelocity = _CurrentVelocityLocalPosition;
    }

    private void OnDestroy() => _FlyingObjects.Remove(this);

    public void SetHeading(float Heading)
    {
        if (Heading < 0 || Heading > 360)
            throw new ArgumentOutOfRangeException(nameof(Heading), "Heading must be greater then 0 and less then 360");

        Vector3 EarthPositionLocal = _EarthPositionLocal;
        Vector3 EarthUpProject = Vector3.ProjectOnPlane(_EarthUpLocal, EarthPositionLocal);
        Vector3 ForwardLocal = Quaternion.AngleAxis(-Heading, EarthPositionLocal) * EarthUpProject;
        Vector3 Up = transform.TransformDirection(-EarthPositionLocal);
        Vector3 Forward = transform.TransformDirection(ForwardLocal);
        transform.rotation = Quaternion.LookRotation(Forward, Up);
    }
}
