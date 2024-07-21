using System;
using System.Net.Http;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Net;

public class BingMapsClient : WebServiceClient
{
    public BingMapsClient(List<string> _ApiKeys)  : base(_ApiKeys)
    {
        
    }

    public BingMapsClient(List<string> _ApiKeys, List<WebProxy> _Proxys) : base(_ApiKeys, _Proxys)
    {
        
    }

    public int PhotoWidth = 2000;
    public int PhotoHeight = 1500;

    public async Task<Texture2D> GetTerrainTexture(GeoPosition _SouthWestCorner, GeoPosition _NorthEastCorner)
    {
        Stopwatch _ApplyTimer = new Stopwatch();
        Stopwatch _PixelsGenerateTimer = new Stopwatch();
        Stopwatch _PixelsApplyTimer = new Stopwatch();
        Stopwatch _TextureCreateTimer = new Stopwatch();

        float _SouthWestCornerLatNormalized = _SouthWestCorner.Latitude.DecimalDegrees + 90;
        float _NorthEastCornerLatNormalized = _NorthEastCorner.Latitude.DecimalDegrees + 90;
        float _ReferenceTerrainHeight = _NorthEastCornerLatNormalized - _SouthWestCornerLatNormalized;
        GeoPosition _ExtendedSouthWestCorner = _SouthWestCorner - new GeoPosition(_ReferenceTerrainHeight * 0.02f, 0);
        _UriBuilder.Query = $"key={ApiKeys[CurrentApiKeyIndex]}&mapSize={PhotoWidth},{PhotoHeight}&" +
            $"mapArea={_SouthWestCorner.Latitude.DecimalDegrees}," +
            $"{_SouthWestCorner.Longitude.DecimalDegrees}," +
            $"{_NorthEastCorner.Latitude.DecimalDegrees}," +
            $"{_NorthEastCorner.Longitude.DecimalDegrees}";
        Task<HttpResponseMessage> _TerrainPhotoResponseTask = GetResponse();
        _UriBuilder.Query += "&mapMetadata=1";
        Task<HttpResponseMessage> _TerrainMetaDataTask = GetResponse();
        await _TerrainPhotoResponseTask;
        byte[] _TerrainPhotoData = await Task.Run(async() => await _TerrainPhotoResponseTask.Result.Content.ReadAsByteArrayAsync());
        Texture2D _TerrainTexture = new Texture2D(PhotoWidth, PhotoHeight);
        _TerrainTexture.LoadImage(_TerrainPhotoData);
        await _TerrainMetaDataTask;
        BingMapsMetaData _MetaData = JsonUtility.FromJson<BingMapsMetaData>(await _TerrainMetaDataTask.Result.Content.ReadAsStringAsync());
        if (!_TerrainMetaDataTask.Result.IsSuccessStatusCode)
        {
            _MetaData = CreateDefaultMetaData(_ExtendedSouthWestCorner, _NorthEastCorner);
        }
        
        Rectangle _Area = CalculateImageArea(_SouthWestCorner, _NorthEastCorner, _MetaData);
        _PixelsGenerateTimer.Start();
        UnityEngine.Color[] _AreaPixels = _TerrainTexture.GetPixels(_Area.X, _Area.Y, _Area.Width, _Area.Height);
        _PixelsGenerateTimer.Stop();
        _TextureCreateTimer.Start();
        Texture2D _CroppedTerrainTexture = new Texture2D(_Area.Width, _Area.Height);
        _TextureCreateTimer.Stop();
        _PixelsApplyTimer.Start();
        _CroppedTerrainTexture.SetPixels(_AreaPixels);
        _PixelsApplyTimer.Stop();
        _ApplyTimer.Start();
        _CroppedTerrainTexture.Apply();
        _ApplyTimer.Stop();
        UnityEngine.Debug.Log($"Texture apply time: {_ApplyTimer.ElapsedMilliseconds}");
        UnityEngine.Debug.Log($"Pixels generate time: {_PixelsGenerateTimer.ElapsedMilliseconds}");
        UnityEngine.Debug.Log($"Pixels apply time: {_PixelsApplyTimer.ElapsedMilliseconds}");
        UnityEngine.Debug.Log($"Texture create time: {_TextureCreateTimer.ElapsedMilliseconds}");
        return _CroppedTerrainTexture;
    }

    protected override void Initialize()
    {
        _BaseUri = "https://dev.virtualearth.net/REST/v1/Imagery/Map/Aerial/";
        _UriBuilder = new UriBuilder(_BaseUri);
    }

    private Rectangle CalculateImageArea(GeoPosition _ReferenceSouthWestCorner, GeoPosition _ReferenceNorthEastCorner, BingMapsMetaData _ImageMetaData)
    {
        float _SouthWestCornerLatNormalized = _ReferenceSouthWestCorner.Latitude.DecimalDegrees + 90;
        float _SouthWestCornerLngNormalized = _ReferenceSouthWestCorner.Longitude.DecimalDegrees + 180;
        float _NorthEastCornerLatNormalized = _ReferenceNorthEastCorner.Latitude.DecimalDegrees + 90;
        float _NorthEastCornerLngNormalized = _ReferenceNorthEastCorner.Longitude.DecimalDegrees + 180;

        float _ExtendedSouthWestCornerLatNormalized = _ImageMetaData.SouthWestCorner.Latitude.DecimalDegrees + 90;
        float _ExtendedSouthWestCornerLngNormalized = _ImageMetaData.SouthWestCorner.Longitude.DecimalDegrees + 180;
        float _ExtendedNorthEastCornerLatNormalized = _ImageMetaData.NorthEastCorner.Latitude.DecimalDegrees + 90;
        float _ExtendedNorthEastCornerLngNormalized = _ImageMetaData.NorthEastCorner.Longitude.DecimalDegrees + 180;

        float _ExtendedTerrainHeight = _ExtendedNorthEastCornerLatNormalized - _ExtendedSouthWestCornerLatNormalized;
        float _ExtendedTerrainWidth = _ExtendedNorthEastCornerLngNormalized - _ExtendedSouthWestCornerLngNormalized;

        float _ExpansionDownPercent = (_SouthWestCornerLatNormalized - _ExtendedSouthWestCornerLatNormalized) / _ExtendedTerrainHeight;
        float _ExpansionUpPercent = (_ExtendedNorthEastCornerLatNormalized - _NorthEastCornerLatNormalized) / _ExtendedTerrainHeight;
        float _ExpansionLeftPercent = (_SouthWestCornerLngNormalized - _ExtendedSouthWestCornerLngNormalized) / _ExtendedTerrainWidth;
        float _ExpansionRightPercent = (_ExtendedNorthEastCornerLngNormalized - _NorthEastCornerLngNormalized) / _ExtendedTerrainWidth;

        Rectangle _Area = new Rectangle();
        _Area.X = (int)(_ExpansionLeftPercent * PhotoWidth);
        _Area.Y = (int)(_ExpansionUpPercent * PhotoHeight);
        _Area.Width = PhotoWidth - _Area.X - (int)(_ExpansionRightPercent * PhotoWidth);
        _Area.Height = PhotoHeight - _Area.Y - (int)(_ExpansionDownPercent * PhotoHeight);
        return _Area;
    }

    private BingMapsMetaData CreateDefaultMetaData(GeoPosition _SouthWestCorner, GeoPosition _NorthEastCorner)
    {
        BingMapsMetaData _MetaData = new BingMapsMetaData();
        _MetaData.resourceSets = new List<BingMapsMetaData.ResourceSets>
        {
            new BingMapsMetaData.ResourceSets()
        };
        _MetaData.resourceSets[0].resources = new List<BingMapsMetaData.Resources>
        {
            new BingMapsMetaData.Resources()
        };
        _MetaData.resourceSets[0].resources[0].bbox = new List<float>
        {
            _SouthWestCorner.Latitude.DecimalDegrees,
            _SouthWestCorner.Longitude.DecimalDegrees,
            _NorthEastCorner.Latitude.DecimalDegrees,
            _NorthEastCorner.Longitude.DecimalDegrees,
        };
        return _MetaData;
    }
}
