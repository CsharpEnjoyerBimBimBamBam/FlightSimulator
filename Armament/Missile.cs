using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Missile : GuidedArmament, IHaveEngine
{
    public float EngineRunTime { get; protected set; }
    public float Thrust { get; protected set; }
    public SeekerType SeekerType { get; protected set; }
    public bool IsSeekerActive { get; protected set; }
    public float DropTime { get; protected set; }
    public IReadOnlyList<ParticleSystem> EngineParticleSystems { get; protected set; } = new List<ParticleSystem>();

    public override GameObject InstantiateGameObject()
    {
        GameObject ArmamentGameObject = base.InstantiateGameObject();
        EngineParticleSystems = ControlSurfaces.EngineParticleSystems;
        return ArmamentGameObject;
    }

    public override void Launch(Vector3? _TargetVector = null, GameObject _TargetGameObject = null)
    {
        if (_TargetGameObject == null || !_TargetGameObject.TryGetComponent(out Rigidbody TargetRigidbody))
            throw new Exception("The missile has no target or target has no rigidbody");
        base.Launch();
        
        MissileGuidanceSystem _MissileGuidanceSystem = EquipmentGameObject.AddComponent<MissileGuidanceSystem>();
        _MissileGuidanceSystem.Missile = this;
        _MissileGuidanceSystem.Target = TargetRigidbody;
        GuidedArmamentPhysics _MissilePhysics = EquipmentGameObject.AddComponent<GuidedArmamentPhysics>();
        _MissilePhysics.Armament = this;
        RocketParticleSystems _RocketParticleSystems = EquipmentGameObject.AddComponent<RocketParticleSystems>();
        _RocketParticleSystems.Rocket = this;
    }
}
