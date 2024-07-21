using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmamentControlSurfaces : MonoBehaviour
{
    [SerializeField] private List<GameObject> _Rudders = new List<GameObject>();
    [SerializeField] private List<ParticleSystem> _EngineParticleSystems = new List<ParticleSystem>();
    public IReadOnlyList<GameObject> Rudders => _Rudders;
    public IReadOnlyList<ParticleSystem> EngineParticleSystems => _EngineParticleSystems;
}