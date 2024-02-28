using System.Diagnostics;
using UnityEngine;

public class RigidbodyPhysics : MonoBehaviour
{
    private Rigidbody _Rigidbody;
    private Vector3 _TempVelocity = Vector3.zero;
    private Vector3 _EarthPositionLocal;
    public static float SurfaceTemperature = 15;
    public Coordinates Coordinates { get; private set; }
    public float Altitude { get; private set; }
    public float AtmosphericPressure { get; private set; }
    public float VerticalSpeed { get; private set; }
    public float GroundSpeed { get; private set; }
    public float Pitch { get; private set; }
    public float Roll { get; private set; }
    public float RelativeYawAngleOfAttack { get; private set; }
    public float RelativePitchAngleOfAttack { get; private set; }
    public float PitchAngleOfAttack { get; private set; }
    public float YawAngleOfAttack { get; private set; }
    public float AngleOfAttack { get; private set; }
    public float TrueAirSpeed { get; private set; }
    public float IndicatedAirSpeed { get; private set; }
    public float Temperature { get; private set; }
    public float SoundSpeed { get; private set; }
    public float RelativeGForce { get; private set; }
    public float GForce { get; private set; }

    private void Start()
    {
        _Rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        _EarthPositionLocal = _Rigidbody.transform.InverseTransformPoint(TerrainGenerator.Earth.transform.position);
        Coordinates = Coordinates.FromWorldPosition(_Rigidbody.position);
        Altitude = _EarthPositionLocal.magnitude - Constants.EarthRadius;
        AtmosphericPressure = Mathf.Exp(-Altitude / 7450);
        VerticalSpeed = Vector3.Project(_Rigidbody.velocity, _EarthPositionLocal).magnitude;
        GroundSpeed = Vector3.ProjectOnPlane(_Rigidbody.velocity, _EarthPositionLocal).magnitude;
        Pitch = 90 - Vector3.Angle(-_EarthPositionLocal, Vector3.forward);
        Roll = 90 - Vector3.Angle(-_EarthPositionLocal, Vector3.right);
        RelativeYawAngleOfAttack = 90 - Vector3.Angle(_Rigidbody.transform.right, _Rigidbody.velocity);
        RelativePitchAngleOfAttack = 90 - Vector3.Angle(_Rigidbody.transform.up, _Rigidbody.velocity);
        PitchAngleOfAttack = Mathf.Abs(RelativePitchAngleOfAttack);
        YawAngleOfAttack = Mathf.Abs(RelativeYawAngleOfAttack);
        AngleOfAttack = Vector3.Angle(_Rigidbody.transform.forward, _Rigidbody.velocity);
        TrueAirSpeed = _Rigidbody.velocity.magnitude;
        Temperature = SurfaceTemperature - (Altitude / 1000f * 6.5f);
        IndicatedAirSpeed = TrueAirSpeed * ((Temperature + 273f) / (SurfaceTemperature + 273f));
        SoundSpeed = Mathf.Sqrt(401.8f * (SurfaceTemperature - Temperature + 273f));
    }

    private void FixedUpdate()
    {
        Vector3 _VelocityLocalPosition = _Rigidbody.transform.InverseTransformPoint(_Rigidbody.velocity + _Rigidbody.transform.position);
        RelativeGForce = Vector3.Distance(_TempVelocity, _VelocityLocalPosition) / Time.fixedDeltaTime / Constants.Gravity;
        if (RelativePitchAngleOfAttack < 0)
            RelativeGForce *= -1;
        _TempVelocity = _VelocityLocalPosition;
        GForce = Mathf.Abs(RelativeGForce);
    }
}
