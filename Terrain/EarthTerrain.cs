using JetBrains.Annotations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class EarthTerrain
{
    public EarthTerrain(Coordinates _SouthWestCornerCoordinates)
    {
        SouthWestCorner = _SouthWestCornerCoordinates;
        SetCoordinates(SouthWestCorner);
        _BaseAssetName = $"{SouthWestCorner.Latitude.DecimalDegrees};" +
                $"{SouthWestCorner.Longitude.DecimalDegrees};";
        HeigthInMeters = Coordinates.AngularDegreesToMeters(SizeInDegrees);
    }

    public EarthTerrain(float _SouthWestCornerLatitude, float _SouthWestCornerLongitude)
    {
        SouthWestCorner = new Coordinates(_SouthWestCornerLatitude, _SouthWestCornerLongitude);
        SetCoordinates(SouthWestCorner);
        _BaseAssetName = $"{SouthWestCorner.Latitude.DecimalDegrees};" +
                $"{SouthWestCorner.Longitude.DecimalDegrees};";
        HeigthInMeters = Coordinates.AngularDegreesToMeters(SizeInDegrees);
    }

    public static string BaseAssetsPath = "Assets/Resources/Terrains";
    public static float SizeInDegrees = 0.2f;
    public static Dictionary<Coordinates, EarthTerrain> TerrainsBySouthWestCorner = new Dictionary<Coordinates, EarthTerrain>();
    public Coordinates SouthWestCorner { get; private set; }
    public Coordinates NorthEastCorner { get; private set; }
    public Coordinates Center { get; private set; }
    public float SouthWidthInMeters { get; private set; } 
    public float NorthWidthInMeters { get; private set; }
    public float MaxWidth { get; private set; }
    public float HeigthInMeters { get; private set; }
    public GameObject GameObject { get; private set; }
    public Mesh Mesh { get; private set; }
    public MeshCollider MeshCollider { get; private set; }
    public Material Material { get; private set; }
    public Side NorthSide { get; private set; }
    public Side SouthSide { get; private set; }
    public Side WestSide { get; private set; }
    public Side EastSide { get; private set; }
    public EarthTerrain NorthNeighbour { get; private set; }
    public EarthTerrain SouthNeighbour { get; private set; }
    public EarthTerrain WestNeighbour { get; private set; }
    public EarthTerrain EastNeighbour { get; private set; }
    public MeshData MeshData { get; private set; }
    public HeightMapData HeightMapData { get; private set; }
    private Matrix4x4 _EarthLocalToWorldMatrix;
    private Matrix4x4 _WorldToLocalMatrix;
    private static List<EarthTerrain> _GeneratingTerrains = new List<EarthTerrain>();
    private readonly string _BaseAssetName;

    public void Instantiate()
    {
        GameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
        GameObject.name = SouthWestCorner.ToString();
        Mesh = GameObject.GetComponent<MeshFilter>().mesh;
        GameObject.AddComponent<LODGroup>();
        Mesh.Clear();
        Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        Material = GameObject.GetComponent<MeshRenderer>().material;
        GameObject.transform.parent = TerrainGenerator.Earth.transform;
        GameObject.transform.localPosition = Center.ToEarthLocalPosition();
        Rotate();
        _WorldToLocalMatrix = GameObject.transform.worldToLocalMatrix;
        _EarthLocalToWorldMatrix = TerrainGenerator.Earth.transform.localToWorldMatrix;
        TerrainsBySouthWestCorner[SouthWestCorner] = this;
        SetNeighbours();
    }

    public void Destroy()
    {
        MonoBehaviour.Destroy(GameObject);
    }

    public bool TryLoadMesh()
    {
        try
        {
            Mesh _LoadedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(BaseAssetsPath + $"/{_BaseAssetName}Mesh.asset");
            if (_LoadedMesh == null)
                return false;
            Mesh.vertices = _LoadedMesh.vertices;
            Mesh.triangles = _LoadedMesh.triangles;
            Mesh.normals = _LoadedMesh.normals;
            Mesh.uv = _LoadedMesh.uv;
            if (MeshCollider == null)
                MeshCollider = GameObject.AddComponent<MeshCollider>();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TryLoadMaterial()
    {
        try
        {
            Material _LoadedMaterial = AssetDatabase.LoadAssetAtPath<Material>(BaseAssetsPath + $"/{_BaseAssetName}Material.asset");
            if (_LoadedMaterial == null)
                return false;
            Material.mainTextureOffset = _LoadedMaterial.mainTextureOffset;
            Material.mainTextureScale = _LoadedMaterial.mainTextureScale;
            Material.SetFloat("_Metallic", _LoadedMaterial.GetFloat("_Metallic"));
            Material.SetFloat("_Glossiness", _LoadedMaterial.GetFloat("_Glossiness"));
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TryLoadTexture()
    {
        try
        {
            Texture _LoadedTexture = AssetDatabase.LoadAssetAtPath<Texture>(BaseAssetsPath + $"/{_BaseAssetName}Texture.asset");
            if (_LoadedTexture == null)
                return false;
            Material.mainTexture = _LoadedTexture;            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TrySaveMesh()
    {
        try
        {
            string _MeshAssetName = _BaseAssetName + "Mesh.asset";
            AssetDatabase.CreateAsset(Mesh, BaseAssetsPath + $"/{_MeshAssetName}");
            AssetDatabase.SaveAssets();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TrySaveMaterial()
    {
        try
        {
            string _MaterialAssetName = _BaseAssetName + "Material.asset";
            AssetDatabase.CreateAsset(Material, BaseAssetsPath + $"/{_MaterialAssetName}");
            AssetDatabase.SaveAssets();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TrySaveTexture(Texture _Texture=null)
    {
        try
        {
            string _TextureAssetName = _BaseAssetName + "Texture.asset";
            if (_Texture == null)
                _Texture = Material.mainTexture;
            AssetDatabase.CreateAsset(_Texture, BaseAssetsPath + $"/{_TextureAssetName}");
            AssetDatabase.SaveAssets();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void ApplyTexture(Texture _Texture)
    {
        Material.mainTexture = _Texture;
        Material.SetFloat("_Glossiness", 0.15f);
        Material.SetFloat("_Metallic", 0.4f);
    }

    public async Task<MeshData> HeightMapToMeshData(HeightMapData _HeightMapData, bool _ConnectToNeighbours)
    {
        short[][] _HeightMap = _HeightMapData.HeightMap;
        NorthSide = new Side();
        SouthSide = new Side(); 
        EastSide = new Side(); 
        WestSide = new Side();
        NorthSide.Vertices = new int[_HeightMap[0].Length];
        SouthSide.Vertices = new int[_HeightMap[_HeightMap.Length - 1].Length];
        EastSide.Vertices = new int[_HeightMap.Length];
        WestSide.Vertices = new int[_HeightMap.Length];
        int _RowCount = _HeightMap.Length;
        int _ColumnCount = _HeightMap[0].Length;
        long _VecrticesCount = _RowCount * _ColumnCount;
        Vector3[] _Vertices = new Vector3[_VecrticesCount];
        Vector2[] _UV = new Vector2[_VecrticesCount];
        int[] _Triangles = new int[(_ColumnCount - 1) * (_RowCount - 1) * 6];
        Vector3[] _Normals = new Vector3[_VecrticesCount];
        long _TrianglesIndex = 0;
        for (int i = 0; i < _RowCount; i++)
        {
            NorthSide.Vertices[i] = i;
            SouthSide.Vertices[i] = (_ColumnCount * _RowCount) - _ColumnCount + i;
            WestSide.Vertices[i] = _ColumnCount * i;
            EastSide.Vertices[i] = (_ColumnCount * i) + _ColumnCount - 1;
            float _RowLatitude = (NorthEastCorner - (SizeInDegrees * (i / ((float)_RowCount - 1)))).Latitude.DecimalDegrees;
            for (int j = 0; j < _ColumnCount; j++)
            {
                float _VertexLongitude = (SouthWestCorner + (SizeInDegrees * (j / ((float)_ColumnCount - 1)))).Longitude.DecimalDegrees;
                Vector3 _VertexEarthLocalPositon = new Coordinates(_RowLatitude, _VertexLongitude).ToEarthLocalPosition(_HeightMap[i][j]);
                Vector3 _VertexTerrainLocalPosition = FromEarthToTerrainLocalPosition(_VertexEarthLocalPositon);
                _Vertices[(_ColumnCount * i) + j] = _VertexTerrainLocalPosition;
                _UV[(_ColumnCount * i) + j] = CalcualteVertexUV(_VertexTerrainLocalPosition);
                //_Normals[(_ColumnCount * i) + j] = CalculateNormal(_Vertices, (_ColumnCount * i) + j, _RowCount, _ColumnCount);
                //if (i > 0 && i < _RowCount - 1 && j > 0 && j < _ColumnCount - 1)
                //{
                //    _Normals[(_ColumnCount * (i - 1)) + j] = CalculateNormal(_Vertices, (_ColumnCount * (i - 1)) + j, _RowCount, _ColumnCount);
                //}
                if (i > 0)
                {
                    Vector3 _UpVertexLocalPosition = _Vertices[(_ColumnCount * (i - 1)) + j] - _VertexTerrainLocalPosition;
                    if (j < _ColumnCount - 1)
                    {
                        _Triangles[_TrianglesIndex] = (_ColumnCount * i) + j;
                        _Triangles[_TrianglesIndex + 1] = (_ColumnCount * (i - 1)) + j;
                        _Triangles[_TrianglesIndex + 2] = (_ColumnCount * (i - 1)) + j + 1;

                        _Triangles[_TrianglesIndex + 3] = (_ColumnCount * i) + j;
                        _Triangles[_TrianglesIndex + 4] = (_ColumnCount * (i - 1)) + j + 1;
                        _Triangles[_TrianglesIndex + 5] = (_ColumnCount * i) + j + 1;
                        _TrianglesIndex += 6;

                        if (i == 2)
                        {
                            Vector3 _RightVertexLocalPosition = _Vertices[j + 1] - _Vertices[j];
                            Vector3 _DownVertexLocalPosition = _Vertices[_ColumnCount + j] - _Vertices[j];
                            _Normals[j] = Vector3.Cross(_RightVertexLocalPosition, _DownVertexLocalPosition);
                        }

                       Vector3 _UpRightVertexLocalPosition = _Vertices[(_ColumnCount * (i - 1)) + j + 1] - _VertexTerrainLocalPosition;
                       _Normals[(_ColumnCount * i) + j] = Vector3.Cross(_UpVertexLocalPosition, _UpRightVertexLocalPosition);
                    }
                    else
                    {
                       Vector3 _LeftVertexLocalPosition = _Vertices[(_ColumnCount * i) + j - 1] - _VertexTerrainLocalPosition;
                       _Normals[(_ColumnCount * i) + j] = Vector3.Cross(_LeftVertexLocalPosition, _UpVertexLocalPosition);
                    }
                }
            }
        }
        Vector3 _CurrentLeftVertexLocalPosition = _Vertices[_ColumnCount - 2] - _Vertices[_ColumnCount - 1];
        Vector3 _CurrentDownVertexLocalPosition = _Vertices[(_ColumnCount * 2) - 1] - _Vertices[_ColumnCount - 1];
        _Normals[_ColumnCount - 1] = Vector3.Cross(_CurrentLeftVertexLocalPosition, _CurrentDownVertexLocalPosition);
        MeshData _MeshData = new MeshData
        {
            Vertices = _Vertices,
            Normals = _Normals,
            Triangles = _Triangles,
            UV = _UV,
            Bounds = new Bounds(Vector3.zero, new Vector3(Mathf.Max(SouthWidthInMeters, NorthWidthInMeters), _HeightMapData.MaxHeight * 2, HeigthInMeters)),
            LastVertexIndex = _Vertices.Length - 1
        };
        if (_ConnectToNeighbours)
        {
            lock (_GeneratingTerrains)
            {
                _GeneratingTerrains.Add(this);
            }
            while (_GeneratingTerrains.IndexOf(this) != 0)
            {
                await Task.Delay(1);
            }
            ConnectToNeighbours(_MeshData);
            MeshData = _MeshData;
            SetAsNeighbour();
            lock (_GeneratingTerrains)
            {
                _GeneratingTerrains.Remove(this);
            }
            //await WaitForNormalsRecalculated();
        }
        MeshData = _MeshData;
        _HeightMapData.HeightMap = null;
        HeightMapData = _HeightMapData;
        return _MeshData;
    }

    public void ApplyMeshData(MeshData _MeshData)
    {
        _MeshData.Apply(Mesh);
    }

    public static async void AddCollider(EarthTerrain[] _Terrains, int _ThreadCount)
    {
        Mesh[] _Meshes = new Mesh[_Terrains.Length];
        for (int i = 0; i < _Terrains.Length; i++)
        {
            _Meshes[i] = _Terrains[i].Mesh;
        }
        JobHandle _MeshBakeJobHandle = new MeshBakeJob(_Meshes).Schedule(_Terrains.Length, _ThreadCount);
        while (!_MeshBakeJobHandle.IsCompleted)
        {
            await Task.Delay(1);
        }
        foreach (Mesh _Mesh in _Meshes)
        {
            _Mesh.AddComponent<MeshCollider>();
        }    
    }

    public Vector3 FromEarthToTerrainLocalPosition(Vector3 _Position)
    {
        Vector3 _WorldPosition = _EarthLocalToWorldMatrix.MultiplyPoint3x4(_Position);
        return _WorldToLocalMatrix.MultiplyPoint3x4(_WorldPosition);
    }

    public static Vector3 CalculateTriangleNormal(Vector3 _VertexA, Vector3 _VertexB, Vector3 _VertexC)
    {
        Vector3 _VertexBLocal = _VertexB - _VertexA;
        Vector3 _VertexCLocal = _VertexC - _VertexA;
        return Vector3.Cross(_VertexBLocal, _VertexCLocal).normalized;
    }

    public Vector2 CalcualteVertexUV(Vector3 _Vertex)
    {
        float _MaxWidth = Mathf.Max(NorthWidthInMeters, SouthWidthInMeters);
        float _U = (_Vertex.x + (_MaxWidth / 2)) / _MaxWidth;
        _U = Mathf.Clamp01(_U);
        float _V = (_Vertex.z + (HeigthInMeters / 2)) / HeigthInMeters;
        return new Vector2(_U, _V);
    }

    public Vector3 FindNearestVertexInHeightMap(Vector3 _Vertex)
    {
        _Vertex.z -= HeigthInMeters / 2;
        _Vertex.x += MaxWidth / 2;
        _Vertex.z *= -1;
        int _Row = Mathf.RoundToInt(HeightMapData.RowCount * (_Vertex.z / HeigthInMeters));
        int _Column = Mathf.RoundToInt(HeightMapData.ColumnCount * (_Vertex.x / MaxWidth));
        _Row = Mathf.Clamp(_Row, 0, HeightMapData.RowCount - 1);
        _Column = Mathf.Clamp(_Column, 0, HeightMapData.ColumnCount - 1);
        int _Index = (HeightMapData.ColumnCount * _Row) + _Column;
        return MeshData.Vertices[_Index];
    }

    public Vector3 FindNearestVertexInHeightMap(Coordinates _Coordinates)
    {
        return new Vector3();
    }

    private async Task WaitForNormalsRecalculated()
    {
        while (!WestSide.IsNormalsRecalculated && !EastSide.IsNormalsRecalculated) //!NorthSide.IsNormalsRecalculated && !SouthSide.IsNormalsRecalculated
        {
            await Task.Delay(1);
        }
    }

    private Vector3 CalculateNormal(Vector3[] _Vertices, int _Index, int _RowCount, int _ColumnCount)
    {
        int _VerticesCount = _RowCount * _ColumnCount;
        Vector3 _UpVertexLocalPosition = Vector3.zero;
        Vector3 _DownVertexLocalPosition = Vector3.zero;
        Vector3 _LeftVertexLocalPosition = Vector3.zero;
        Vector3 _RightVertexLocalPosition = Vector3.zero;
        Vector3 _UpRightVertexLocalPosition = Vector3.zero;
        Vector3 _DownLeftVertexLocalPosition = Vector3.zero;
        Vector3 _VertexPosition = _Vertices[_Index];
        if (_Index > _ColumnCount)
        {
            _UpVertexLocalPosition = _Vertices[_Index - _ColumnCount] - _VertexPosition;
            if (_Index + 1 % _ColumnCount != 0 && _Index - _ColumnCount + 1 < _Vertices.Length)
            {
                _UpRightVertexLocalPosition = _Vertices[_Index - _ColumnCount + 1] - _VertexPosition;
                _RightVertexLocalPosition = _Vertices[_Index + 1] - _VertexPosition;
            }
        }
        if (_Index < _VerticesCount - _ColumnCount && _Index + _ColumnCount < _Vertices.Length)
        {
            _DownVertexLocalPosition = _Vertices[_Index + _ColumnCount] - _VertexPosition;
            if (_Index % _ColumnCount != 0 && _Index + _ColumnCount - 1 < _Vertices.Length)
            {
                _DownLeftVertexLocalPosition = _Vertices[_Index + _ColumnCount - 1] - _VertexPosition;
                _LeftVertexLocalPosition = _Vertices[_Index - 1] - _VertexPosition;
            }
        }
        return Vector3.Cross(_UpVertexLocalPosition, _UpRightVertexLocalPosition).normalized +
            Vector3.Cross(_RightVertexLocalPosition, _DownVertexLocalPosition).normalized +
            Vector3.Cross(_LeftVertexLocalPosition, _DownLeftVertexLocalPosition).normalized;
    }

    private void ConnectToNeighbours(MeshData _MeshData)
    {
        Vector3[] _TerrainMeshVertices = _MeshData.Vertices;
        Vector3[] _TerrainMeshNormals = _MeshData.Normals;
        if (WestNeighbour != null)
        {
            Debug.Log("West");
            MeshData _WestTerrainMeshData = WestNeighbour.MeshData;
            for (int i = 0; i < WestSide.Vertices.Length; i++)
            {
                Vector3 _EastSideLocalPosition = _WestTerrainMeshData.Vertices[WestNeighbour.EastSide.Vertices[i]];
                _TerrainMeshVertices[WestSide.Vertices[i]] = new Vector3(-_EastSideLocalPosition.x, _EastSideLocalPosition.y, _EastSideLocalPosition.z);
                _TerrainMeshNormals[WestSide.Vertices[i]] += _WestTerrainMeshData.Normals[WestNeighbour.EastSide.Vertices[i]];
                lock (WestNeighbour)
                {
                    _WestTerrainMeshData.Normals[WestNeighbour.EastSide.Vertices[i]] += _TerrainMeshNormals[WestSide.Vertices[i]];
                }
            }
            lock (WestNeighbour)
            {
                WestNeighbour.EastSide.IsNormalsRecalculated = true;
            }
        }
        if (EastNeighbour != null)
        {
            Debug.Log("East");
            MeshData _EastTerrainMeshData = EastNeighbour.MeshData;
            for (int i = 0; i < EastSide.Vertices.Length; i++)
            {
                Vector3 _WestSideLocalPosition = _EastTerrainMeshData.Vertices[EastNeighbour.WestSide.Vertices[i]];
                _TerrainMeshVertices[EastSide.Vertices[i]] = new Vector3(-_WestSideLocalPosition.x, _WestSideLocalPosition.y, _WestSideLocalPosition.z);
                _TerrainMeshNormals[EastSide.Vertices[i]] += _EastTerrainMeshData.Normals[EastNeighbour.WestSide.Vertices[i]];
                lock (EastNeighbour)
                {
                    _EastTerrainMeshData.Normals[EastNeighbour.WestSide.Vertices[i]] += _TerrainMeshNormals[EastSide.Vertices[i]];
                }
            }
            lock (EastNeighbour)
            {
                EastNeighbour.WestSide.IsNormalsRecalculated = true;
            }
        }
        if (NorthNeighbour != null)
        {
            Debug.Log("North");
            MeshData _NorthTerrainMeshData = NorthNeighbour.MeshData;
            for (int i = 0; i < NorthSide.Vertices.Length; i++)
            {
                Vector3 _SouthSideLocalPosition = _NorthTerrainMeshData.Vertices[NorthNeighbour.SouthSide.Vertices[i]];
                _TerrainMeshVertices[NorthSide.Vertices[i]] = new Vector3(_SouthSideLocalPosition.x, _SouthSideLocalPosition.y, -_SouthSideLocalPosition.z);
            }
            lock (NorthNeighbour)
            {
                NorthNeighbour.SouthSide.IsNormalsRecalculated = true;
            }
        }
        if (SouthNeighbour != null)
        {
            Debug.Log("South");
            MeshData _SouthTerrainMeshData = SouthNeighbour.MeshData;
            for (int i = 0; i < SouthSide.Vertices.Length; i++)
            {
                Vector3 _NorthSideLocalPosition = _SouthTerrainMeshData.Vertices[SouthNeighbour.NorthSide.Vertices[i]];
                _TerrainMeshVertices[SouthSide.Vertices[i]] = new Vector3(_NorthSideLocalPosition.x, _NorthSideLocalPosition.y, -_NorthSideLocalPosition.z);
            }
            lock (SouthNeighbour)
            {
                SouthNeighbour.NorthSide.IsNormalsRecalculated = true;
            }
        }
    }

    private void Rotate()
    {
        Vector3 _EarthLocalPosition = GameObject.transform.InverseTransformPoint(TerrainGenerator.Earth.transform.position);
        Vector3 _TargetUpDirection = GameObject.transform.TransformDirection(-_EarthLocalPosition);
        Vector3 _EarthUpLocal = GameObject.transform.InverseTransformDirection(TerrainGenerator.Earth.transform.up);
        float _EarthUpRotationAngle = Vector3.Angle(_EarthUpLocal, _EarthLocalPosition) - 90;
        Vector3 _TargetForwardDirectionLocal = Vector3.RotateTowards(_EarthUpLocal, _EarthLocalPosition, _EarthUpRotationAngle * Mathf.Deg2Rad, 0);
        Vector3 _TargetForwardDirection = GameObject.transform.TransformDirection(_TargetForwardDirectionLocal);
        GameObject.transform.rotation = Quaternion.LookRotation(_TargetForwardDirection, _TargetUpDirection);
    }  

    private void SetCoordinates(Coordinates _SouthWestCornerCoordinates)
    {
        NorthEastCorner = _SouthWestCornerCoordinates + SizeInDegrees;
        Center = _SouthWestCornerCoordinates + (SizeInDegrees / 2);
        SouthWidthInMeters = Coordinates.AngularDegreesToMeters(SizeInDegrees, _SouthWestCornerCoordinates.Latitude.DecimalDegrees);
        NorthWidthInMeters = Coordinates.AngularDegreesToMeters(SizeInDegrees, NorthEastCorner.Latitude.DecimalDegrees);
        MaxWidth = Mathf.Max(SouthWidthInMeters, NorthWidthInMeters);
    }

    private void SetAsNeighbour()
    {
        Coordinates _WestTerrainCoordinates = SouthWestCorner - new Coordinates(0, SizeInDegrees);
        Coordinates _EastTerrainCoordinates = SouthWestCorner + new Coordinates(0, SizeInDegrees);
        Coordinates _NorthTerrainCoordinates = SouthWestCorner + new Coordinates(SizeInDegrees, 0);
        Coordinates _SouthTerrainCoordinates = SouthWestCorner - new Coordinates(SizeInDegrees, 0);
        if (TerrainsBySouthWestCorner.ContainsKey(_WestTerrainCoordinates))
        {
            TerrainsBySouthWestCorner[_WestTerrainCoordinates].EastNeighbour = this;
        }
        if (TerrainsBySouthWestCorner.ContainsKey(_EastTerrainCoordinates))
        {
            TerrainsBySouthWestCorner[_EastTerrainCoordinates].WestNeighbour = this;
        }
        if (TerrainsBySouthWestCorner.ContainsKey(_NorthTerrainCoordinates))
        {
            TerrainsBySouthWestCorner[_NorthTerrainCoordinates].SouthNeighbour = this;
        }
        if (TerrainsBySouthWestCorner.ContainsKey(_SouthTerrainCoordinates))
        {
            TerrainsBySouthWestCorner[_SouthTerrainCoordinates].NorthNeighbour = this;
        }
    }

    private void SetNeighbours()
    {
        Coordinates _WestTerrainCoordinates = SouthWestCorner - new Coordinates(0, SizeInDegrees);
        Coordinates _EastTerrainCoordinates = SouthWestCorner + new Coordinates(0, SizeInDegrees);
        Coordinates _NorthTerrainCoordinates = SouthWestCorner + new Coordinates(SizeInDegrees, 0);
        Coordinates _SouthTerrainCoordinates = SouthWestCorner - new Coordinates(SizeInDegrees, 0);
        if (TerrainsBySouthWestCorner.ContainsKey(_WestTerrainCoordinates) && TerrainsBySouthWestCorner[_WestTerrainCoordinates].MeshData != null)
        {
            WestNeighbour = TerrainsBySouthWestCorner[_WestTerrainCoordinates];
        }
        if (TerrainsBySouthWestCorner.ContainsKey(_EastTerrainCoordinates) && TerrainsBySouthWestCorner[_EastTerrainCoordinates].MeshData != null)
        {
            EastNeighbour = TerrainsBySouthWestCorner[_EastTerrainCoordinates];
        }
        if (TerrainsBySouthWestCorner.ContainsKey(_NorthTerrainCoordinates) && TerrainsBySouthWestCorner[_NorthTerrainCoordinates].MeshData != null)
        {
            NorthNeighbour = TerrainsBySouthWestCorner[_NorthTerrainCoordinates];
        }
        if (TerrainsBySouthWestCorner.ContainsKey(_SouthTerrainCoordinates) && TerrainsBySouthWestCorner[_SouthTerrainCoordinates].MeshData != null)
        {
            SouthNeighbour = TerrainsBySouthWestCorner[_SouthTerrainCoordinates];
        }
    }

    public class Side
    {
        public int[] Vertices;
        public bool IsNormalsRecalculated = false;
    }

    private struct MeshBakeJob : IJobParallelFor
    {
        public MeshBakeJob(Mesh[] _Meshes)
        {
            _MeshesIDs = new NativeArray<int>(_Meshes.Length, Allocator.TempJob);
            SetMeshetIDs(_Meshes);
        }

        private NativeArray<int> _MeshesIDs;

        private void SetMeshetIDs(Mesh[] _Meshes)
        {
            for (int i = 0; i < _Meshes.Length; i++)
            {
                _MeshesIDs[i] = _Meshes[i].GetInstanceID();
            }
        }

        public void Execute(int _Index)
        {
            Physics.BakeMesh(_MeshesIDs[_Index], false);
        }
    }
}
