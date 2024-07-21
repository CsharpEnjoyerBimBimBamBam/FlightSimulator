using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface IHaveEngine : IAirplaneEquipment
{
    public IReadOnlyList<ParticleSystem> EngineParticleSystems { get; }
    public float EngineRunTime { get; }
    public float DropTime { get; }
    public float Thrust { get; }
}
