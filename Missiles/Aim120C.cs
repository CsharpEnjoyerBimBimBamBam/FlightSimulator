using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aim120C : Missile
{
    public Aim120C() 
    {
        Mass = 157.7f;
        Diameter = 0.178f;
        MaxLaunchRange = 120000;
        MinLaunchRange = 1000;
        Name = ArmamentName.Aim120C;
        Type = ArmamentType.Missile;
        EngineRunTime = 9;
        Thrust = 20000;
        RotationForce = 0.3f;
        LiftForce = 10000;
        TorqueCoefficient = 0.045f;
        DragCoefficient = 0.0035f;
        DeadZone = 0.1f;
        RuddersDeadAngle = 0.2f;
        MaxRuddersDeflectionAngle = 25;
        SeekerType = SeekerType.Radar;
        IsSeekerActive = true;
    }
}
