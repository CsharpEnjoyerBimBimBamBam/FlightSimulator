using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapsStaticInfo
{
    static MapsStaticInfo()
    {
        Default.Name = MapName.Default;
        Default.Size = new Vector2(500, 400);

        Default.Airports = new List<Airport>
        {
            new Airport(new Vector2(100, 100)),
            new Airport(new Vector2(50, 50)),
            new Airport (new Vector2(200, 50)),
            new Airport(new Vector2(450, 350))
        };
        MissionMaps.Add(Default);
    }

    private static readonly MissionMap Default = new MissionMap();
    private static List<MissionMap> MissionMaps = new List<MissionMap>();

    public static MissionMap GetMapByName(MapName _MapName)
    {
        foreach (var _MissionMap in MissionMaps)
        {
            if (_MissionMap.Name == _MapName)
            {
                MissionMap _missionMap = new MissionMap();
                _missionMap = _MissionMap;
                return _missionMap;
            }
        }
        return null;
    }
}
