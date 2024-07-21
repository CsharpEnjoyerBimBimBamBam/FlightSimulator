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
    public IReadOnlyList<GameObject> Rudders { get; private set; } = new List<GameObject>();
    protected ArmamentControlSurfaces ControlSurfaces { get; private set; }

    public override GameObject InstantiateGameObject()
    {
        GameObject ArmamentGameObject = base.InstantiateGameObject();
        ControlSurfaces = ArmamentGameObject.GetComponent<ArmamentControlSurfaces>();
        Rudders = ControlSurfaces.Rudders;
        return ArmamentGameObject;
    }
}
