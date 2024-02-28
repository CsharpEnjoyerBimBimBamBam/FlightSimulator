using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionMap
{
    public MapName Name;
    public Vector2 Size;
    public List<Airport> Airports;

    public Sprite LoadSprite()
    {
        return Resources.Load<Sprite>("Sprites/Maps/" + Name.ToString());
    }

    public static Sprite LoadSprite(MapName _MapName)
    {
        return Resources.Load<Sprite>("Sprites/Maps/" + _MapName.ToString());
    }

    public static MapName NameToEnum(string _MapName)
    {
        return (MapName)Enum.Parse(typeof(MapName), _MapName);
    }

    public Vector3 UnityCoordinatesToReal(Vector2 _Coordinates)
    {
        return new Vector3();
    }
}
