using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuidedArmament : AirplaneArmament
{
    public float RotationForce { get; protected set; }
    public float LiftForce { get; protected set; }
    public float MaxRuddersDeflectionAngle { get; protected set; }
    public float DeadZone { get; protected set; }
    public float RuddersDeadAngle { get; protected set; }
    public List<GameObject> Rudders = new List<GameObject>();
}
