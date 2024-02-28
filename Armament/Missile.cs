using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Missile : GuidedArmament
{
    public float EngineRunTime { get; protected set; }
    public float Thrust { get; protected set; }
    public SeekerType SeekerType { get; protected set; }
    public bool IsSeekerActive { get; protected set; }
    public float DropTime;
    public List<ParticleSystem> EngineParticleSystems = new List<ParticleSystem>();

    public override void Launch(Vector3? _TargetVector = null, GameObject _TargetGameObject = null)
    {
        if (_TargetGameObject == null)
            throw new Exception("The missile has no target");
        base.Launch();
        ArmamentControlSurfaces _MissileControlSurfaces = gameObject.GetComponent<ArmamentControlSurfaces>();
        EngineParticleSystems = _MissileControlSurfaces.EngineParticleSystems;
        Rudders = _MissileControlSurfaces.Rudders;
        MissileGuidanceSystem _MissileGuidanceSystem = gameObject.AddComponent<MissileGuidanceSystem>();
        _MissileGuidanceSystem.Missile = this;
        _MissileGuidanceSystem.Target = _TargetGameObject;
        _MissileGuidanceSystem.TargetRigidbody = _TargetGameObject.GetComponent<Rigidbody>();
        GuidedArmamentPhysics _MissilePhysics = gameObject.AddComponent<GuidedArmamentPhysics>();
        _MissilePhysics.CurrentArmament = this;
        _MissilePhysics.IsHaveEngine = true;
        _MissilePhysics.Thrust = Thrust;
        _MissilePhysics.EngineParticleSystems = EngineParticleSystems;
        _MissilePhysics.EngineRunTime = EngineRunTime;
    }
}
