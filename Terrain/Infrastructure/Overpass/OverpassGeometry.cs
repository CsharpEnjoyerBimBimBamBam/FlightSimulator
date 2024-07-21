using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OverpassGeometry
{
    public float lat;
    public float lon;
    public GeoPosition Coordinates { get { return new GeoPosition(lat, lon); } }
}
