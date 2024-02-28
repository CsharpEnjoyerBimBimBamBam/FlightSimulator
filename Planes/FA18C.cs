using System.Collections.Generic;
using UnityEngine;

public class FA18C : Airplane
{
    public FA18C() 
    {
        Name = PlaneName.FA18C;
        MaxFuelMass = 6060;
        EmptyWeight = 10810;
        MaxWeight = 25400;
        EmptyCenterOfMass = new Vector3(0, 0, -0.1f);


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
            }
        };

        InternalFuelTanks = new List<FuelTank>
        {
            new FuelTank
            {
                Name = "Tank1",
                LocalPosition = new Vector3(0, 0.42f, 3.2f),
                MaxWeight = 1288.202f
            },
            new FuelTank
            {
                Name = "Tank2",
                LocalPosition = new Vector3(0, 0.15f, 2.35f),
                MaxWeight = 811.9303f
            },
            new FuelTank
            {
                Name = "Tank3",
                LocalPosition = new Vector3(0, 0.11f, 1.64f),
                MaxWeight = 635.0293f
            },
            new FuelTank
            {
                Name = "Tank4",
                LocalPosition = new Vector3(0, 0.26f, 0.09f),
                MaxWeight = 1642.004f
            },
            new FuelTank
            {
                Name = "LeftWingTank",
                LocalPosition = new Vector3(-2.369f, -0.12f, 0.266f),
                MaxWeight = 263.084f
            },
            new FuelTank
            {
                Name = "RightWingTank",
                LocalPosition = new Vector3(2.369f, -0.12f, 0.266f),
                MaxWeight = 263.084f
            }
        };
    }
}
