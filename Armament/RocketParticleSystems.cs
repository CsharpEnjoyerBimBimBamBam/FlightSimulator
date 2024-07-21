using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class RocketParticleSystems : MonoBehaviour
{
    public IHaveEngine Rocket;
    private Stopwatch _EngineRunTimer = new Stopwatch();
    private Dictionary<ParticleSystem, Vector3> _ParticlesPositions = new Dictionary<ParticleSystem, Vector3>();
    private Dictionary<ParticleSystem, Vector3> _TempParticlesPositions = new Dictionary<ParticleSystem, Vector3>();
    private int _ParticleBetweenCount = 5;
    private IEnumerator _SmokeCoroutine;

    private async void Start()
    {
        PauseSwithcer.OnGamePaused += Pause;
        PauseSwithcer.OnGameUnpaused += Unpause;

        if (Rocket.DropTime > 0)
            await Task.Delay(TimeSpan.FromSeconds(Rocket.DropTime));
        //_SmokeCoroutine = SetParticles();
        _EngineRunTimer.Start();
        for (int i = 0; i < Rocket.EngineParticleSystems.Count; i++)
        {
            Rocket.EngineParticleSystems[i].Play();
            if (Rocket.EngineParticleSystems[i].tag == "Smoke")
                _ParticlesPositions[Rocket.EngineParticleSystems[i]] = Rocket.EngineParticleSystems[i].transform.localPosition;
            //StartCoroutine(_SmokeCoroutine);
        }
    }

    private void FixedUpdate()
    {
        if (_EngineRunTimer.Elapsed.TotalSeconds < Rocket.EngineRunTime)
        {
            Rocket.EquipmentRigidbody.AddRelativeForce(0, 0, Rocket.Thrust);
            return;
        }
        else if (_EngineRunTimer.Elapsed.TotalSeconds > Rocket.EngineRunTime)
        {
            for (int i = 0; i < Rocket.EngineParticleSystems.Count; i++)
            {
                Rocket.EngineParticleSystems[i].Stop();
            }
        }
    }

    private IEnumerator SetParticles()
    {
        while (_EngineRunTimer.Elapsed.TotalSeconds < Rocket.EngineRunTime)
        {
            foreach (var _Element in _ParticlesPositions)
            {
                float _DistanceBetweenParticles = Vector3.Distance(_Element.Value, _Element.Key.transform.position);
                Vector3 ParticleSystemPostion = _Element.Key.transform.localPosition;
                for (int j = 1; j <= Mathf.Ceil(_DistanceBetweenParticles) * _ParticleBetweenCount; j++)
                {
                    _Element.Key.transform.localPosition = _Element.Value +
                        ((_Element.Value - _Element.Key.transform.localPosition).normalized * ((float)j / _ParticleBetweenCount));
                    ParticleSystem.MinMaxCurve _MinMaxCurve = _Element.Key.main.startSize;
                    ParticleSystem.MainModule _MainModule = _Element.Key.main;
                    _MinMaxCurve.constantMax = _DistanceBetweenParticles / _ParticleBetweenCount;
                    _MainModule.startSize = _MinMaxCurve;
                    _Element.Key.Emit((int)Mathf.Ceil(2));
                }
                _Element.Key.transform.localPosition = ParticleSystemPostion;
                _TempParticlesPositions[_Element.Key] = _Element.Key.transform.localPosition;
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
        if (_EngineRunTimer.Elapsed.Seconds < Rocket.EngineRunTime)
        {
            for (int i = 0; i < Rocket.EngineParticleSystems.Count; i++)
            {
                Rocket.EngineParticleSystems[i].Pause();
            }
            _EngineRunTimer.Stop();
            StopCoroutine(_SmokeCoroutine);
        }
    }

    public void Unpause()
    {
        if (_EngineRunTimer.Elapsed.Seconds < Rocket.EngineRunTime)
        {
            for (int i = 0; i < Rocket.EngineParticleSystems.Count; i++)
            {
                Rocket.EngineParticleSystems[i].Play();
            }
            _EngineRunTimer.Start();
            StartCoroutine(_SmokeCoroutine);
        }
    }
}
