using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Timers;
using System.Diagnostics;

public class OverpassClient : WebServiceClient
{
    public OverpassClient() 
    {
        _BaseUri = "https://overpass-api.de/api/interpreter?";
        _UriBuilder = new UriBuilder(_BaseUri);
        _HttpClient = new HttpClient();
    }

    public void SetQueryToBuildingsGeometry(Coordinates _SouthWestCorner, Coordinates _NorthEastCorner)
    {
        _UriBuilder.Query = $"data=[out:json];way[building]({_SouthWestCorner.Latitude.DecimalDegrees},{_SouthWestCorner.Longitude.DecimalDegrees}," +
            $"{_NorthEastCorner.Latitude.DecimalDegrees},{_NorthEastCorner.Longitude.DecimalDegrees});out geom;";
    }

    public void SetQueryToForestData(Coordinates _SouthWestCorner, Coordinates _NorthEastCorner)
    {
        _UriBuilder.Query = $"data=[out:json];nwr[natural=wood]({_SouthWestCorner.Latitude.DecimalDegrees},{_SouthWestCorner.Longitude.DecimalDegrees}," +
            $"{_NorthEastCorner.Latitude.DecimalDegrees},{_NorthEastCorner.Longitude.DecimalDegrees});out geom;";
    }

    public async Task<ForestData> GetForestData(Coordinates _SouthWestCorner, Coordinates _NorthEastCorner)
    {
        Stopwatch _Timer = new Stopwatch();
        _UriBuilder.Query = $"data=[out:json];nwr[natural=wood]({_SouthWestCorner.Latitude.DecimalDegrees},{_SouthWestCorner.Longitude.DecimalDegrees}," +
            $"{_NorthEastCorner.Latitude.DecimalDegrees},{_NorthEastCorner.Longitude.DecimalDegrees});out geom;";
        string _Response = await GetStringResponse();
        _Timer.Start();
        ForestData _DeserializedResponse = JsonConvert.DeserializeObject<ForestData>(_Response);
        _Timer.Stop();
        UnityEngine.Debug.Log($"ForestData response parse time {_Timer.Elapsed.TotalMilliseconds}");
        return _DeserializedResponse;
    }

    public BuildingsData DeserealizeBuildingResponse(string _BuildingResponse)
    {
        Stopwatch _Timer = new Stopwatch();
        _Timer.Start();
        BuildingsData _DeserializedResponse = JsonConvert.DeserializeObject<BuildingsData>(_BuildingResponse);
        _Timer.Stop();
        return _DeserializedResponse;
    }
}
