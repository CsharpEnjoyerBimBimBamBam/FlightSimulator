using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditorInternal;
using UnityEngine;

public class GuidedArmamentPhysics : MonoBehaviour, IPausable
{    
    public bool IsHaveEngine;
    public float EngineRunTime;
    public float Thrust;
    public GuidedArmament CurrentArmament;
    public List<ParticleSystem> EngineParticleSystems;
    public Rigidbody ArmamentRigidbody;
    private Stopwatch _EngineRunTimer = new Stopwatch();
    private RigidbodyPhysics _ArmamentPhysics;
    private Dictionary<ParticleSystem, Vector3> _ParticlesPositions = new Dictionary<ParticleSystem, Vector3>();
    private Dictionary<ParticleSystem, Vector3> _TempParticlesPositions = new Dictionary<ParticleSystem, Vector3>();
    private int _ParticleBetweenCount = 5;
    private IEnumerator _SmokeCoroutine;

    void Start()
    {
        ArmamentRigidbody = CurrentArmament.rigidBody;
        _ArmamentPhysics = GetComponent<RigidbodyPhysics>();
        _SmokeCoroutine = SetParticles();
        if (IsHaveEngine)
        {
            _EngineRunTimer.Start();
            for (int i = 0; i < EngineParticleSystems.Count; i++)
            {
                EngineParticleSystems[i].Play();
                if (EngineParticleSystems[i].tag == "Smoke")
                    _ParticlesPositions[EngineParticleSystems[i]] = EngineParticleSystems[i].transform.position;
                StartCoroutine(_SmokeCoroutine);
            }
        }
        IPausable.OnGamePaused.AddListener(Pause);
        IPausable.OnGameUnpaused.AddListener(Unpause);
    }

    private void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (IPausable.IsGamePaused)
            return;

        if (IsHaveEngine & _EngineRunTimer.Elapsed.TotalSeconds < EngineRunTime)
        {
            ArmamentRigidbody.AddRelativeForce(0, 0, Thrust);
        }
        else if (_EngineRunTimer.Elapsed.TotalSeconds > EngineRunTime)
        {
            for (int i = 0; i < EngineParticleSystems.Count; i++)
            {
                EngineParticleSystems[i].Stop();
            }
        }

        foreach (GameObject _Rudder in CurrentArmament.Rudders)
        {
            Vector3 _RudderForwardLocal = transform.InverseTransformDirection(_Rudder.transform.forward);
            Vector3 _RudderForwardProjection = -Vector3.ProjectOnPlane(_RudderForwardLocal, Vector3.forward);
            Vector3 _TorqueVector = transform.TransformDirection(new Vector3(-_RudderForwardProjection.y, _RudderForwardProjection.x, _RudderForwardProjection.z));
            ArmamentRigidbody.AddTorque(_TorqueVector * _ArmamentPhysics.TrueAirSpeed * _ArmamentPhysics.AtmosphericPressure * CurrentArmament.RotationForce);
        }

        ArmamentRigidbody.AddRelativeForce(Mathf.Atan(-_ArmamentPhysics.RelativeYawAngleOfAttack / 4) * Mathf.Log10(_ArmamentPhysics.TrueAirSpeed + 1) * _ArmamentPhysics.AtmosphericPressure * CurrentArmament.LiftForce,
            Mathf.Atan(-_ArmamentPhysics.RelativePitchAngleOfAttack / 4) * Mathf.Log10(_ArmamentPhysics.TrueAirSpeed + 1) * _ArmamentPhysics.AtmosphericPressure * CurrentArmament.LiftForce, 0);
    }

    private IEnumerator SetParticles()
    {
        while(_EngineRunTimer.Elapsed.TotalSeconds < EngineRunTime)
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
                    _MinMaxCurve.constantMax = _DistanceBetweenParticles / _ParticleBetweenCount;
                    _MainModule.startSize = _MinMaxCurve;
                    _Element.Key.Emit((int)Mathf.Ceil(2));
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

    public void Pause()
    {
        if (_EngineRunTimer.Elapsed.Seconds < EngineRunTime)
        {
            for (int i = 0; i < EngineParticleSystems.Count; i++)
            {
                EngineParticleSystems[i].Pause();
            }
            _EngineRunTimer.Stop();
            StopCoroutine(_SmokeCoroutine);
        }
    }

    public void Unpause()
    {
        if (_EngineRunTimer.Elapsed.Seconds < EngineRunTime)
        {
            for (int i = 0; i < EngineParticleSystems.Count; i++)
            {
                EngineParticleSystems[i].Play();
            }
            _EngineRunTimer.Start();
            StartCoroutine(_SmokeCoroutine);
        }
    }
}
