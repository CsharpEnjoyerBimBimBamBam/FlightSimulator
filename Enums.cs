using System;

[Serializable]
public enum ArmamentName
{
    None,
    Pylon,
    AGM65F,
    AGM65E,
    AGM84,
    AGM84E,
    Aim7,
    Aim9,
    Aim120C,
    Mk82,
    Mk83,
    Tank330,
    Tank480,
    TGTFLIR,
    NAVFLIR
}

[Serializable]
public enum ArmamentType
{
    Missile,
    CruiseMissile,
    GuidedRocket,
    UnguidedRocket,
    GuidedBomb,
    UnguidedBomb, 
    FuelTank,
    TargetingPod
}

[Serializable]
public enum SeekerType
{
    Infrared,
    Optical,
    Radar
}

public enum PlaneName
{
    FA18C,
    A10C
}

public enum PlaneState
{
    ColdAndDark,
    Started,
    Approach
}

public enum MapName
{
    Default
}