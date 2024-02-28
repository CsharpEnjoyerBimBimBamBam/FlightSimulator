using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mk82 : AirplaneArmament
{
    public Mk82()
    {
        Name = ArmamentName.Mk82;
        Type = ArmamentType.UnguidedBomb;
        Mass = 227;
        Diameter = 0.273f;
        TorqueCoefficient = 0.08f;
        DragCoefficient = 0.7f;
    }
}
