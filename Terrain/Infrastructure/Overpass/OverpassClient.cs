using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Timers;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using System.Net;

public class OverpassClient : WebServiceClient
{
    public OverpassClient() : base()
    {
        
    }

    public OverpassClient(List<WebProxy> _Proxys) : base(_Proxys)
    {
        
    }

    public async Task<string> GetBuildingsGeometryResponse(GeoPosition _SouthWestCorner, GeoPosition _NorthEastCorner)
    {
        _UriBuilder.Query = $"data=[out:json];way[building]({_SouthWestCorner.Latitude.DecimalDegrees},{_SouthWestCorner.Longitude.DecimalDegrees}," +
            $"{_NorthEastCorner.Latitude.DecimalDegrees},{_NorthEastCorner.Longitude.DecimalDegrees});out geom;";
        return await GetDefaultResponse();
    }

    public async Task<string> GetForestDataResponse(GeoPosition _SouthWestCorner, GeoPosition _NorthEastCorner)
    {
        _UriBuilder.Query = $"data=[out:json];nwr[natural=wood]({_SouthWestCorner.Latitude.DecimalDegrees},{_SouthWestCorner.Longitude.DecimalDegrees}," +
            $"{_NorthEastCorner.Latitude.DecimalDegrees},{_NorthEastCorner.Longitude.DecimalDegrees});out geom;";
        return await GetDefaultResponse();
    }

    protected override void Initialize()
    {
        _BaseUri = "https://overpass-api.de/api/interpreter?";
        _UriBuilder = new UriBuilder(_BaseUri);
    }

    private async Task<string> GetDefaultResponse()
    {
        string Response = "";
        try
        {
            Response = await(await GetResponse()).Content.ReadAsStringAsync();
        }
        catch
        {

        }

        return Response;
    }
}
