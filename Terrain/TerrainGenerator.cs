using UnityEngine;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using System;

public class TerrainGenerator : MonoBehaviour
{
    public static GameObject Earth { get; private set; }
    private List<WebProxy> _Proxys = new List<WebProxy>
    {
        new WebProxy
        {
            Address = new Uri("http://190.110.226.162:80")
        },
        new WebProxy
        {
            Address = new Uri("http://64.188.4.202:80")
        },
        new WebProxy
        {
            Address = new Uri("http://172.233.255.11:3128")
        },
        new WebProxy
        {
            Address = new Uri("http://179.50.90.166:3128")
        },
        new WebProxy
        {
            Address = new Uri("http://103.115.243.156:83")
        }
    };
    private List<string> _ApiKeys = new List<string>
    {
        "4dfe92b6c614821f061400e362bb220d",
        "65b361b6b32dced795b8c9b91881b538",
        "adff7584c456e30146509371e206dbb5",
        "653d9a681606c1a89ec5278de30f8cb0",
        "bf23283300eb686f81f8a849e00fb9e1",
        "bdde45c42c7b6e5776bdab3e01887bc3",
        "683ba319ff1e500908826c2d37a555ff",
        "16320d09026d0cf2b4a4cfdc0e55ac66",
        "04d0819076ae70ff0c188fbe4ed0843d",
        "a7389d3ec573d0d1c6c8da98f637fad8",
        "756356c0cc89fc734a69bba0c16c952f",
        "22c374e27179a2955891a2f66f59c0aa",
        "d8f18e186e97a305e9a4b89a93baef0b",
        "cee29564d1eb044007beaadd7315ea8a",
    };
    private OpenTopographyClient _OpenTopographyClient;
    private BingMapsClient _BingMapsClient;
    private OverpassClient _OverpassClient;
    [SerializeField] private Sprite _TreeSprite;

    private void Awake()
    {
        Earth = transform.gameObject;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        ServicePointManager.DefaultConnectionLimit = 100000;
    }

    private async void Start()
    {
        _OpenTopographyClient = new OpenTopographyClient(_ApiKeys);
        _BingMapsClient = new BingMapsClient(new List<string> { "AmOi2SRCelXEddtqCBKcSKzN6irlmjH2USCL909rw7ZkPIDOzBChKD1v_pz_gagq" });
        _OverpassClient = new OverpassClient();
        List<Task<EarthTerrain>> Terrains = new List<Task<EarthTerrain>>();
        float _TerrainSize = 0.05f;
        float Count = 3;
        EarthTerrain.SizeInDegrees = _TerrainSize;
        GeoPosition _Coordinates = FindNearestMultipleCoordinates(SceneLoader.SelectedRunway.Coordinates - 
            new GeoPosition(_TerrainSize * (Count / 2), _TerrainSize * (Count / 2)));
        //Terrains.Add(CreateNewTerrain(_Coordinates));
        Terrains.Add(CreateNewTerrain(_Coordinates));
        for (int i = 0; i < Count; i++)
        {
            for (int j = 0; j < Count; j++)
            {
                GeoPosition _CurrentCoordinates = _Coordinates + new GeoPosition(_TerrainSize * i, _TerrainSize * j);
                Terrains.Add(CreateNewTerrain(_CurrentCoordinates));
                await Task.Delay(300);
            }
        }
        await EarthTerrain.AddCollider(await Task.WhenAll(Terrains), 5);
    }

    public async Task Generate(GeoPosition _SouthWestCorner)
    {

    }

    private async Task<EarthTerrain> CreateNewTerrain(GeoPosition _SouthWestCorner)
    {
        EarthTerrain _Terrain = new EarthTerrain(_SouthWestCorner);
        _Terrain.Instantiate();
        bool _IsMeshLoadSuccess = _Terrain.TryLoadMesh();
        bool _IsMaterialLoadSuccess = _Terrain.TryLoadMaterial();
        bool _IsTextureLoadSuccess = _Terrain.TryLoadTexture();
        Task<HeightMapData> _HeightMapTask = null;
        Task<Texture2D> _TerrainTextureTask = null;
        Task<string> _BuildingsResponseTask = null;
        Task<string> _ForestDataResponseTask = null;
        MeshData _MeshData = null;
        if (!_IsMeshLoadSuccess)
        {
            _HeightMapTask = Task.Run(() => _OpenTopographyClient.GetHeightMap(_Terrain.SouthWestCorner, _Terrain.NorthEastCorner));
            _BuildingsResponseTask = _OverpassClient.GetBuildingsGeometryResponse(_Terrain.SouthWestCorner, _Terrain.NorthEastCorner);
            //_ForestDataResponseTask = _OverpassClient.GetForestDataResponse(_Terrain.SouthWestCorner, _Terrain.NorthEastCorner);
        }
        if (!_IsMaterialLoadSuccess || !_IsTextureLoadSuccess)
        {
            _TerrainTextureTask = _BingMapsClient.GetTerrainTexture(_Terrain.SouthWestCorner, _Terrain.NorthEastCorner);
        }
        if (!_IsMaterialLoadSuccess || !_IsTextureLoadSuccess)
        {
            Texture _TerrainTexture = await _TerrainTextureTask;
            _Terrain.ApplyTexture(_TerrainTexture);
            //_Terrain.TrySaveMaterial();
        }
        if (!_IsMeshLoadSuccess)
        {
            HeightMapData _HeightMap = await _HeightMapTask; //OpenTopographyClient.CreateZeroHeightMap(308, 308); //await _HeightMapTask; 
            string _BuildingsResponse = await _BuildingsResponseTask;
            //string _ForestDataResponse = await _ForestDataResponseTask;
            //ForestGenerator _ForestGenerator = new ForestGenerator(_Terrain, _TreeSprite);
            BuildingsGenerator _BuildingsGenerator = new BuildingsGenerator(_Terrain);
            MeshData _ForestMeshData = null;
            await Task.Run(async() =>
            {
                Stopwatch _Timer = Stopwatch.StartNew();
                _MeshData = await _Terrain.HeightMapToMeshData(_HeightMap, true);
                BuildingsData _BuildingsData = JsonConvert.DeserializeObject<BuildingsData>(_BuildingsResponse) ?? new BuildingsData();
                //ForestData _ForestData = JsonConvert.DeserializeObject<ForestData>(_ForestDataResponse);
                //_ForestData ??= new ForestData();
                _BuildingsGenerator.AddToMeshData(_BuildingsData, ref _MeshData);
                //_ForestMeshData = _ForestGenerator.GenerateMeshData(_ForestData, -1);
                _Timer.Stop();
                UnityEngine.Debug.Log($"MeshData generating time: {_Timer.Elapsed.TotalMilliseconds}");
            });
            //_ForestGenerator.CreateGameObjectFromMeshData(_ForestMeshData, "υσι");
            //_Terrain.TrySaveMesh();
        }
        if (_MeshData == null)
            _MeshData = MeshData.FromMesh(_Terrain.Mesh);
        _Terrain.ApplyMeshData(_MeshData);
        return _Terrain;
    }

    private GeoPosition FindNearestMultipleCoordinates(GeoPosition Coordinates)
    {
        decimal DecimalSize = (decimal)EarthTerrain.SizeInDegrees;
        decimal LatitudeModulo = Math.Abs(Coordinates.Latitude.ExactDecimalDegrees) % DecimalSize;
        decimal LongitudeModulo = Math.Abs(Coordinates.Latitude.ExactDecimalDegrees) % DecimalSize;

        decimal Latitude = (Coordinates - LatitudeModulo).Latitude.ExactDecimalDegrees;
        if (Coordinates.Latitude.DecimalDegrees < 0)
            Latitude = (Coordinates + LatitudeModulo).Latitude.ExactDecimalDegrees;

        decimal Longitude = (Coordinates - LongitudeModulo).Longitude.ExactDecimalDegrees;
        if (Coordinates.Longitude.DecimalDegrees < 0)
            Longitude = (Coordinates + LongitudeModulo).Longitude.ExactDecimalDegrees;
        return new GeoPosition(Latitude, Longitude);
    }
}
