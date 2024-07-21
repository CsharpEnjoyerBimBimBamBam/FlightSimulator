using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mk82 : AirplaneArmament
{
    public Mk82()
    {
        EquipmentType = EquipmentType.UnguidedBomb;
        Mass = 227;
        Diameter = 0.273f;
        TorqueCoefficient = 0.08f;
        DragCoefficient = 0.7f;
    }
}
