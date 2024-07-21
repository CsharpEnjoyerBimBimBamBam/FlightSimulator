using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Build;

public class Airport : IReadOnlyAirport
{
    public Airport(GeoPosition _Coordinates, List<Runway> _Runways, string _Name, string _ICAO, string _IATA)
    {
        Coordinates = _Coordinates;
        Runways = _Runways;
        Name = _Name;
        ICAO = _ICAO;
        IATA = _IATA;
    }

    public Airport()
    {

    }

    public GeoPosition Coordinates { get; set; }
    public IReadOnlyList<IReadOnlyRunway> Runways { get; set; } = new List<Runway>();
    public string Name { get; set; }
    public string ICAO { get; set; }
    public string IATA { get;set; }
    public float Elevation { get; set; }
    public float TransitionAltitude { get; set; }
}

public interface IReadOnlyAirport
{
    public GeoPosition Coordinates { get; }
    public IReadOnlyList<IReadOnlyRunway> Runways { get; }
    public string Name { get; }
    public string ICAO { get; }
    public string IATA { get; }
    public float Elevation { get; }
    public float TransitionAltitude { get; }
}
