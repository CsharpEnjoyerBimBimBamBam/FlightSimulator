using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class PlanePhysics : MonoBehaviour, IPausable
{
    [SerializeField] private float PitchTorqueCoefficient;
    [SerializeField] private float YawTorqueCoefficient;
    [SerializeField] private float LiftForceCoefficient;
    [SerializeField] private float PitchRotationForce;
    [SerializeField] private float YawRotationForce;
    [SerializeField] private float DragCoefficient;
    [SerializeField] private List<ParticleSystem> Wakes = new List<ParticleSystem>();
    [SerializeField] private List<ParticleSystem> WingsCondestaions = new List<ParticleSystem>();
    public float AngleOfAttack; 
    public float TrueAirSpeedInKnots;
    public float Pitch;
    public float GForce;
    private Rigidbody PlaneRigidbody;
    private RigidbodyPhysics _PlanePhysics;
    private Vector3 _PlaneVelocity;
    private Vector3 _PlaneAngularVelocity;
    private Dictionary<ParticleSystem, Vector3> _ParticlesPositions = new Dictionary<ParticleSystem, Vector3>();
    private Dictionary<ParticleSystem, Vector3> _TempParticlesPositions = new Dictionary<ParticleSystem, Vector3>();
    private int _ParticleBetweenCount = 20;
    private IEnumerator _WakesCoroutine;
    private bool _IsWakesCoroutinePlaying = false;
    private bool _IsPhysicsCalculating = true;

    void Start()
    {
        foreach (ParticleSystem _Wake in Wakes)
        {
            _ParticlesPositions[_Wake] = _Wake.transform.position;
        }

        PlaneRigidbody = transform.GetComponent<Rigidbody>();
        _PlanePhysics = GetComponent<RigidbodyPhysics>();
        IPausable.OnGamePaused.AddListener(Pause);
        IPausable.OnGameUnpaused.AddListener(Unpause);
        CameraMove.OnObjectsMovmentStart.AddListener(StopCalculatePhysics);
        CameraMove.OnObjectsMovmentEnd.AddListener(StartCalculatePhysics);
        _WakesCoroutine = SetParticles();
        foreach (ParticleSystem _Wake in Wakes)
        {
            if (!_Wake.isPlaying)
                _Wake.Play();
        }
    }

    private void Update()
    {
        if (!_IsPhysicsCalculating)
            return;

        if (IPausable.IsGamePaused)
            return;

        if (_PlanePhysics.PitchAngleOfAttack > 8 & _PlanePhysics.TrueAirSpeed > 80)
        {
            foreach (ParticleSystem _Wake in Wakes)
            {
                if (!_Wake.isPlaying)
                    _Wake.Play();
            }
            //if (!_IsWakesCoroutinePlaying)
            //{
            //    StartCoroutine(_WakesCoroutine);
            //    _IsWakesCoroutinePlaying = true;
            //}
        }
        else
        {
            foreach (ParticleSystem _Wake in Wakes)
            {
                if (_Wake.isPlaying)
                    _Wake.Stop();
            }
            //if (_IsWakesCoroutinePlaying)
            //{
            //    StopCoroutine(_WakesCoroutine);
            //    _IsWakesCoroutinePlaying = false;
            //}
        }

        if (_PlanePhysics.RelativePitchAngleOfAttack < -5 & _PlanePhysics.TrueAirSpeed > 120)
        {
            foreach (ParticleSystem _WingCondensation in WingsCondestaions)
            {
                if (!_WingCondensation.isPlaying)
                    _WingCondensation.Play();
            }
        }
        else
        {
            foreach (ParticleSystem _WingCondensation in WingsCondestaions)
            {
                if (_WingCondensation.isPlaying)
                    _WingCondensation.Stop();
            }
        }
    }

    private void FixedUpdate()
    {
        if (!_IsPhysicsCalculating)
            return;

        if (IPausable.IsGamePaused)
            return;

        float _TrueAirSpeedSpeedSquared = Mathf.Pow(_PlanePhysics.TrueAirSpeed, 2);
        float _PitchCoefficient = Mathf.Sin(_PlanePhysics.RelativePitchAngleOfAttack * Mathf.Deg2Rad);
        if (_PitchCoefficient > 0)
            _PitchCoefficient += 1;
        else
            _PitchCoefficient -= 1;
        float _YawCoefficient = Mathf.Sin(_PlanePhysics.RelativeYawAngleOfAttack * Mathf.Deg2Rad);
        if (_YawCoefficient > 0)
            _YawCoefficient += 1;
        else
            _YawCoefficient -= 1;

        PlaneRigidbody.AddForce(-PlaneRigidbody.velocity.normalized * _TrueAirSpeedSpeedSquared * 
            Mathf.Abs(_PitchCoefficient) * _PlanePhysics.AtmosphericPressure * DragCoefficient);

        PlaneRigidbody.AddForceAtPosition(PlaneRigidbody.gameObject.transform.up * _TrueAirSpeedSpeedSquared * LiftForceCoefficient *
            _PlanePhysics.AtmosphericPressure, PlaneRigidbody.gameObject.transform.position);

        PlaneRigidbody.AddRelativeForce(-_YawCoefficient * _TrueAirSpeedSpeedSquared * YawRotationForce, 
            -_PitchCoefficient * _TrueAirSpeedSpeedSquared * PitchRotationForce, 0);

        PlaneRigidbody.AddRelativeTorque(-_PitchCoefficient * _TrueAirSpeedSpeedSquared * 
            PitchTorqueCoefficient * _PlanePhysics.AtmosphericPressure, _YawCoefficient *
            _TrueAirSpeedSpeedSquared * YawTorqueCoefficient * _PlanePhysics.AtmosphericPressure, 0);
    }

    public void Pause()
    {
        _PlaneVelocity = PlaneRigidbody.velocity;
        _PlaneAngularVelocity = PlaneRigidbody.angularVelocity;
        PlaneRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        //StopCoroutine(_WakesCoroutine);
        foreach (ParticleSystem _Wake in Wakes)
        {
            _Wake.Pause();
        }

        foreach (ParticleSystem _WingCondensation in WingsCondestaions)
        {
            _WingCondensation.Pause();
        }
    }

    public void Unpause()
    {
        PlaneRigidbody.constraints = RigidbodyConstraints.None;
        PlaneRigidbody.velocity = _PlaneVelocity;  
        PlaneRigidbody.angularVelocity = _PlaneAngularVelocity;
        //if (_IsWakesCoroutinePlaying)
        //    StartCoroutine(_WakesCoroutine);
        foreach (ParticleSystem _Wake in Wakes)
        {
            _Wake.Play();
        }
    }

    private IEnumerator SetParticles()
    {
        while (true)
        {
            foreach (var _Element in _ParticlesPositions)
            {
                float _DistanceBetweenParticles = Vector3.Distance(_Element.Value, _Element.Key.transform.position);
                Vector3 ParticleSystemPostion = _Element.Key.transform.localPosition;
                for (int j = 1; j <= Mathf.Ceil(_DistanceBetweenParticles) * _ParticleBetweenCount; j++)
                {
                    _Element.Key.transform.position = _Element.Value +
                        ((_Element.Value - _Element.Key.transform.position).normalized * ((float)j / _ParticleBetweenCount));
                    ParticleSystem.MinMaxCurve _MinMaxCurve = _Element.Key.main.startSize;
                    ParticleSystem.MainModule _MainModule = _Element.Key.main;
                    _MinMaxCurve.constantMax = 2.5f;
                    _MainModule.startSize = _MinMaxCurve;
                    _Element.Key.Emit(10);
                }
                _Element.Key.transform.localPosition = ParticleSystemPostion;
                _TempParticlesPositions[_Element.Key] = _Element.Key.transform.position;
            }

            foreach (var _Element in _TempParticlesPositions)
            {
                _ParticlesPositions[_Element.Key] = _Element.Value;
            }
            yield return null;
        }
    }

    private void StopCalculatePhysics()
    {
        _IsPhysicsCalculating = false;
    }

    private async void StartCalculatePhysics()
    {
        Stopwatch _Timer = Stopwatch.StartNew();
        while (_Timer.Elapsed.TotalSeconds < Time.fixedDeltaTime * 1.2f)
        {
            await Task.Delay(1);
        }
        _IsPhysicsCalculating = true;
    }
}
