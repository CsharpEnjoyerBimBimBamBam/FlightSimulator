using System;

[Serializable]
public enum EquipmentType
{
    Missile,
    CruiseMissile,
    GuidedRocket,
    UnguidedRocket,
    GuidedBomb,
    UnguidedBomb,
    FuelTank,
    TargetingPod,
    Pylon
}

[Serializable]
public enum SeekerType
{
    Infrared,
    Optical,
    Radar
}

[Serializable]
public enum PlaneState
{
    ColdAndDark,
    Started,
    Approach
}

[Serializable]
public enum DistanceUnits
{
    Meter,
    Feet,
    NauticalMile
}