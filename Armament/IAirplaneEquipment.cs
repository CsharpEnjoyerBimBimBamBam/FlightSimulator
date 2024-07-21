using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface IAirplaneEquipment
{
    public Rigidbody EquipmentRigidbody { get; }
    public GameObject EquipmentGameObject { get; }
}
