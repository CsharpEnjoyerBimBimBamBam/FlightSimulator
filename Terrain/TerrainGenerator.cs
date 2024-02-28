using UnityEngine;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json;
using System.Diagnostics;
using static ForestData;

public class TerrainGenerator : MonoBehaviour
{
    public static GameObject Earth { get; private set; }
    private OpenTopographyClient _OpenTopographyClient = new OpenTopographyClient("adff7584c456e30146509371e206dbb5");
    private BingMapsClient _BingMapsClient = new BingMapsClient("AmOi2SRCelXEddtqCBKcSKzN6irlmjH2USCL909rw7ZkPIDOzBChKD1v_pz_gagq");
    private OverpassClient _OverpassClient = new OverpassClient();
    [SerializeField] private Sprite _TreeSprite;

    private void Awake()
    {
        Earth = transform.gameObject;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
    }

    private async void Start()
    {
        //27.171923m, 90.174719m
        //56.797351m, 60.468231m
        decimal _TerrainSize = 0.01m;
        EarthTerrain.SizeInDegrees = (float)_TerrainSize;
        Coordinates _Coordinates = new Coordinates(56.797351m, 60.468231m);
        for (int i = 0; i < 1; i++)
        {
            for (int j = 0; j < 1; j++)
            {
                Coordinates _CurrentCoordinates = _Coordinates + new Coordinates(_TerrainSize * i, _TerrainSize * j);
                CreateNewTerrain(_CurrentCoordinates);
                await Task.Delay(100);
            }
        }
    }

    private async void CreateNewTerrain(Coordinates _SouthWestCorner)
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
            //_HeightMapTask = Task.Run(() => _OpenTopographyClient.GetHeightMap(_Terrain.SouthWestCorner, _Terrain.NorthEastCorner));
            _OverpassClient.SetQueryToBuildingsGeometry(_Terrain.SouthWestCorner, _Terrain.NorthEastCorner);
            _BuildingsResponseTask = _OverpassClient.GetStringResponse();
            _OverpassClient.SetQueryToForestData(_Terrain.SouthWestCorner, _Terrain.NorthEastCorner);
            _ForestDataResponseTask = _OverpassClient.GetStringResponse();
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
            HeightMapData _HeightMap = OpenTopographyClient.CreateZeroHeightMap(308, 308); //await _HeightMapTask;
            string _BuildingsResponse = await _BuildingsResponseTask;
            string _ForestDataResponse = await _ForestDataResponseTask;
            ForestGenerator _ForestGenerator = new ForestGenerator(_Terrain, _TreeSprite);
            BuildingsGenerator _BuildingsGenerator = new BuildingsGenerator(_Terrain);
            MeshData _ForestMeshData = null;
            await Task.Run(async() => 
            {
                Stopwatch _Timer = Stopwatch.StartNew();
                _MeshData = await _Terrain.HeightMapToMeshData(_HeightMap, true);
                BuildingsData _BuildingsData = JsonConvert.DeserializeObject<BuildingsData>(_BuildingsResponse);
                ForestData _ForestData = JsonConvert.DeserializeObject<ForestData>(_ForestDataResponse);
                _BuildingsGenerator.AddToMeshData(_BuildingsData, ref _MeshData);
                //_ForestGenerator.AddToMeshData(_ForestData, ref _MeshData);
                //_ForestMeshData = _ForestGenerator.GenerateMeshData(_ForestData, -1);
                //_ForestGenerator.AddToMeshData(_ForestData, ref _MeshData);
                _Timer.Stop();
                UnityEngine.Debug.Log($"MeshData generating time: {_Timer.Elapsed.TotalMilliseconds}");
            });
            TreeGenerator treeGenerator = new TreeGenerator();
            GameObject gameObject = treeGenerator.CreateGameObjectFromMeshData(treeGenerator.GenerateMeshData(Vector3.zero, -1), "ισυ");
            gameObject.transform.position = _Terrain.GameObject.transform.position;
            gameObject.transform.rotation = _Terrain.GameObject.transform.rotation;
            //_ForestGenerator.CreateGameObjectFromMeshData(_ForestMeshData, "υσι");
            //_Terrain.TrySaveMesh();
        }
        if (_MeshData == null)
            _MeshData = MeshData.FromMesh(_Terrain.Mesh);
        _Terrain.ApplyMeshData(_MeshData);
        //return _Terrain;
    }
}
