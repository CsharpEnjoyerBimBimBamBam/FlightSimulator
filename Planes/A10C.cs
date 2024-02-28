using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A10C : Airplane
{
    public A10C()
    {
        Name = PlaneName.A10C;
        MaxFuelMass = 4853;
        EmptyWeight = 12000;
        MaxWeight = 23134;

        WeaponStations = new List<WeaponStation>
        {
            new WeaponStation
            {
                Number = 1,
                PossibleArmament = new List<ArmamentName> { ArmamentName.None, ArmamentName.Aim9 },
                LocalPosition = new Vector3(-5.702f, -0.332f, -0.926f),
                IsNeedPylon = false,
            },
            new WeaponStation
            {
                Number = 2,
                PossibleArmament = new List<ArmamentName> { ArmamentName.None, ArmamentName.Pylon, ArmamentName.Mk82, ArmamentName.Mk83,
                ArmamentName.AGM65E, ArmamentName.AGM65F, ArmamentName.AGM84E, ArmamentName.AGM84, ArmamentName.Aim120C,ArmamentName.Aim9,
                ArmamentName.Aim7 },
                LocalPosition = new Vector3(-3.66f, -0.256f, -0.926f),
                IsNeedPylon = true,
            },
            new WeaponStation
            {
                Number = 3,
                PossibleArmament = new List<ArmamentName> { ArmamentName.None, ArmamentName.Pylon, ArmamentName.Mk82, ArmamentName.Mk83,
                ArmamentName.AGM65E, ArmamentName.AGM65F, ArmamentName.AGM84E, ArmamentName.AGM84, ArmamentName.Aim120C, ArmamentName.Aim9,
                ArmamentName.Aim7, ArmamentName.Tank330, ArmamentName.Tank480},
                LocalPosition = new Vector3(-2.572f, -0.217f, -0.214f),
                IsNeedPylon = true,
            },
            new WeaponStation
            {
                Number = 4,
                PossibleArmament = new List<ArmamentName> { ArmamentName.None, ArmamentName.Aim120C, ArmamentName.Aim7, ArmamentName.TGTFLIR},
                LocalPosition = new Vector3(-1.145f, -0.874f, -0.079f),
                IsNeedPylon = false,
            },
            new WeaponStation
            {
                Number = 5,
                PossibleArmament = new List<ArmamentName> { ArmamentName.None, ArmamentName.Mk82, ArmamentName.Mk83, ArmamentName.Tank330},
                LocalPosition = new Vector3(0, -0.995f, -0.079f),
                IsNeedPylon = false,
            },
            new WeaponStation
            {
                Number = 6,
                PossibleArmament = new List<ArmamentName> { ArmamentName.None, ArmamentName.Aim120C, ArmamentName.Aim7, ArmamentName.NAVFLIR},
                LocalPosition = new Vector3(1.145f, -0.874f, -0.079f),
                IsNeedPylon = false,
            },
            new WeaponStation
            {
                Number = 7,
                PossibleArmament = new List<ArmamentName> { ArmamentName.None, ArmamentName.Pylon, ArmamentName.Mk82, ArmamentName.Mk83,
                ArmamentName.AGM65E, ArmamentName.AGM65F, ArmamentName.AGM84E, ArmamentName.AGM84, ArmamentName.Aim120C, ArmamentName.Aim9,
                ArmamentName.Aim7, ArmamentName.Tank330, ArmamentName.Tank480},
                LocalPosition = new Vector3(2.572f, -0.217f, -0.214f),
                IsNeedPylon = true,
            },
            new WeaponStation
            {
                Number = 8,
                PossibleArmament = new List<ArmamentName> { ArmamentName.None, ArmamentName.Pylon, ArmamentName.Mk82, ArmamentName.Mk83,
                ArmamentName.AGM65E, ArmamentName.AGM65F, ArmamentName.AGM84E, ArmamentName.AGM84, ArmamentName.Aim120C, ArmamentName.Aim9,
                ArmamentName.Aim7},
                LocalPosition = new Vector3(3.66f, -0.256f, -0.926f),
                IsNeedPylon = true,
            },
            new WeaponStation
            {
                Number = 9,
                PossibleArmament = new List<ArmamentName> { ArmamentName.None, ArmamentName.Aim9 },
                LocalPosition = new Vector3(5.702f, -0.332f, -0.926f),
                IsNeedPylon = false
            },
            new WeaponStation
            {
                Number = 10,
                PossibleArmament = new List<ArmamentName> { ArmamentName.None, ArmamentName.Aim9 },
                LocalPosition = new Vector3(5.702f, -0.332f, -0.926f),
                IsNeedPylon = false
            },
            new WeaponStation
            {
                Number = 11,
                PossibleArmament = new List<ArmamentName> { ArmamentName.None, ArmamentName.Aim9 },
                LocalPosition = new Vector3(5.702f, -0.332f, -0.926f),
                IsNeedPylon = false
            }
        };
    }
}
