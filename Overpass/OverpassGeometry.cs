using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OverpassGeometry
{
    public float lat;
    public float lon;
    public Coordinates Coordinates { get { return new Coordinates(lat, lon); } }
}
