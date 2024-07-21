using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ForestData;

public class ForestGenerator : MeshDataGenerator<ForestData>
{
    public ForestGenerator(EarthTerrain Terrain, Sprite TreeSprite)
    {
        _Terrain = Terrain;
        _TreeSprite = TreeSprite;
        _TreeGenerator = new TreeGenerator();
        _TreeGenerator.BranchesCount = 0;
    }

    public int DistanceBetweenTrees = 10;
    public float MaxShiftBetweenTrees = 4;
    private EarthTerrain _Terrain;
    private System.Random _Random = new System.Random();
    private TreeGenerator _TreeGenerator = new TreeGenerator();
    private Sprite _TreeSprite;

    public override MeshData GenerateMeshData(ForestData _ForestData, int _LastVertexIndex)
    {
        ForestMeshDataParameters _MeshDataParameters = (ForestMeshDataParameters)CalculateMeshDataParameters(_ForestData);
        List<Vector2> _IntersectionPoints = _MeshDataParameters.IntersectionPoints;
        MeshData _ForestMeshData = new MeshData(_MeshDataParameters.VerticesCount, _MeshDataParameters.TrianglesCount);
        _ForestMeshData.LastVertexIndex = _LastVertexIndex;
        float _MaxTreeHeight = 0;
        for (int i = 0; i < _IntersectionPoints.Count - 1; i += 2)
        {
            Vector3 _TreesGeneratingStartPoint = _IntersectionPoints[i].ToVector3();
            Vector3 _TreesGeneratingEndPoint = _IntersectionPoints[i + 1].ToVector3();
            int _TreesCount = Mathf.FloorToInt(Vector3.Distance(_TreesGeneratingStartPoint, _TreesGeneratingEndPoint) / DistanceBetweenTrees);
            for (int j = 0; j < _TreesCount; j++)
            {
                Vector3 _CurrentTreePosition = _TreesGeneratingStartPoint;
                _CurrentTreePosition.x += DistanceBetweenTrees * j;
                _CurrentTreePosition.y = _Terrain.FindNearestVertexInHeightMap(_CurrentTreePosition).y;
                float _XShift = _Random.GenerateRandomFloat(MaxShiftBetweenTrees);
                float _ZShift = _Random.GenerateRandomFloat(MaxShiftBetweenTrees);
                _CurrentTreePosition += new Vector3(_XShift, 0, _ZShift);
                _TreeGenerator.AddToMeshData(_CurrentTreePosition, ref _ForestMeshData, false);
                _MaxTreeHeight = Mathf.Max(_MaxTreeHeight, _TreeGenerator.TreeHeight + _CurrentTreePosition.y);
            }
        }
        _ForestMeshData.Bounds.center = Vector3.zero;
        _ForestMeshData.Bounds.size = new Vector3(_Terrain.MaxWidth, _MaxTreeHeight, _Terrain.HeigthInMeters);
        return _ForestMeshData;
    }

    public override GameObject CreateGameObjectFromMeshData(MeshData _ForestMeshData, string _Name, string _ShaderName = "Standard")
    {
        GameObject _GameObject = base.CreateGameObjectFromMeshData(_ForestMeshData, _Name);
        _GameObject.transform.parent = _Terrain.GameObject.transform;
        _GameObject.transform.localPosition = Vector3.zero;
        _GameObject.transform.rotation = _Terrain.GameObject.transform.rotation;
        _MeshRenderer.material.mainTexture = _TreeSprite.texture;
        _MeshRenderer.material.SetFloat("_Glossiness", 0.05f);
        _MeshRenderer.material.SetFloat("_Metallic", 0.15f);
        LODGroup _LODGroup = _GameObject.AddComponent<LODGroup>();
        LOD[] _Lods = new LOD[1];
        _Lods[0] = new LOD(0.5f, new Renderer[] { _MeshRenderer });
        _LODGroup.SetLODs(_Lods);
        return _GameObject;
    }

    private List<OverpassGeometry> LimitAreaWithinTerrain(List<OverpassGeometry> _Geometry)
    {

        float _MinLatitude = Mathf.Min(_Terrain.SouthWestCorner.Latitude.DecimalDegrees, _Terrain.NorthEastCorner.Latitude.DecimalDegrees);
        float _MaxLatitude = Mathf.Max(_Terrain.SouthWestCorner.Latitude.DecimalDegrees, _Terrain.NorthEastCorner.Latitude.DecimalDegrees);
        float _MinLongitude = Mathf.Min(_Terrain.SouthWestCorner.Longitude.DecimalDegrees, _Terrain.NorthEastCorner.Longitude.DecimalDegrees);
        float _MaxLongitude = Mathf.Max(_Terrain.SouthWestCorner.Longitude.DecimalDegrees, _Terrain.NorthEastCorner.Longitude.DecimalDegrees);
        for (int i = 0; i < _Geometry.Count; i++)
        {
            _Geometry[i].lat = Mathf.Clamp(_Geometry[i].lat, _MinLatitude, _MaxLatitude);
            _Geometry[i].lon = Mathf.Clamp(_Geometry[i].lon, _MinLongitude, _MaxLongitude);
        }
        return _Geometry;
    }

    private LineSegment[] GeometryToLineSegments(List<OverpassGeometry> _Geometry)
    {
        LineSegment[] _LineSegments = new LineSegment[_Geometry.Count];
        Vector3 _FirstTerrainPosition = _Terrain.FromEarthToTerrainLocalPosition(_Geometry[0].Coordinates.ToEarthLocalPosition());
        Vector3 _LastTerrainPosition = _Terrain.FromEarthToTerrainLocalPosition(_Geometry[_Geometry.Count - 1].Coordinates.ToEarthLocalPosition());
        _LineSegments[_LineSegments.Length - 1] = new LineSegment(_FirstTerrainPosition.ToVector2(), _LastTerrainPosition.ToVector2());
        for (int i = 1; i < _Geometry.Count; i++)
        {
            Vector3 _CurrentTerrainPosition = _Terrain.FromEarthToTerrainLocalPosition(_Geometry[i].Coordinates.ToEarthLocalPosition());
            Vector3 _PreviousTerrainPosition = _Terrain.FromEarthToTerrainLocalPosition(_Geometry[i - 1].Coordinates.ToEarthLocalPosition());
            _LineSegments[i - 1] = new LineSegment(_PreviousTerrainPosition.ToVector2(), _CurrentTerrainPosition.ToVector2());
        }
        return _LineSegments;
    }

    private int FindNorthernmostCoordinatesIndex(List<OverpassGeometry> _Geometries)
    {
        GeoPosition _NorthernmostCoordinates = _Geometries[0].Coordinates;
        int _Index = 0;
        for (int i = 0; i < _Geometries.Count; i++)
        {
            if (_Geometries[i].Coordinates.Latitude.DecimalDegrees > _NorthernmostCoordinates.Latitude.DecimalDegrees)
            {
                _NorthernmostCoordinates = _Geometries[i].Coordinates;
                _Index = i;
            }
        }
        return _Index;
    }

    public override MeshDataParameters CalculateMeshDataParameters(ForestData _ForestData)
    {
        if (_ForestData.elements == null || _ForestData.elements.Count == 0)
            return new ForestMeshDataParameters();

        List<ForestElement> _Elements = _ForestData.elements;
        int _VerticesCount = 0;
        int _TrianglesCount = 0;
        List<Vector2> _IntersectionPoints = new List<Vector2>();
        for (int i = 0; i < _Elements.Count; i++)
        {
            List<OverpassGeometry> _Geometry = _Elements[i].geometry;
            if (_Geometry == null)
            {
                continue;
            }
            _Geometry = LimitAreaWithinTerrain(_Geometry);
            ForestMeshDataParameters _GeometryData = CalculateMeshDataParametersForGeometry(_Geometry);
            _VerticesCount += _GeometryData.VerticesCount;
            _TrianglesCount += _GeometryData.TrianglesCount;
            _IntersectionPoints.AddRange(_GeometryData.IntersectionPoints);
        }
        ForestMeshDataParameters _ForestMeshDataParameters = new ForestMeshDataParameters(_VerticesCount, _TrianglesCount);
        _ForestMeshDataParameters.IntersectionPoints = _IntersectionPoints;
        return _ForestMeshDataParameters;
    }

    private ForestMeshDataParameters CalculateMeshDataParametersForGeometry(List<OverpassGeometry> _Geometry)
    {
        int NorthernmostCoordinatesIndex = FindNorthernmostCoordinatesIndex(_Geometry);
        Vector3 _NorthernmostPosition = _Terrain.FromEarthToTerrainLocalPosition(_Geometry[NorthernmostCoordinatesIndex].Coordinates.ToEarthLocalPosition());
        LineSegment[] _GeometryLineSegments = GeometryToLineSegments(_Geometry);
        Line _TreesGeneratingLine = new Line(0, _NorthernmostPosition.z - DistanceBetweenTrees);
        List<Vector2> _CurrentLineIntersectionPoints;
        ForestMeshDataParameters _ForestMeshDataParameters = new ForestMeshDataParameters();
        while (true)
        {
            _CurrentLineIntersectionPoints = new List<Vector2>(2);
            for (int j = 0; j < _GeometryLineSegments.Length; j++)
            {
                if (_GeometryLineSegments[j].CheckForIntersectionWithLine(_TreesGeneratingLine, out Vector2 _IntersectionPoint))
                {
                    _CurrentLineIntersectionPoints.Add(_IntersectionPoint);
                }
            }
            if (_CurrentLineIntersectionPoints.Count == 0)
            {
                break;
            }
            _CurrentLineIntersectionPoints = _CurrentLineIntersectionPoints.OrderBy(IntersectionPoint => IntersectionPoint.x).ToList();
            _ForestMeshDataParameters.IntersectionPoints.AddRange(_CurrentLineIntersectionPoints);
            for (int j = 0; j < _CurrentLineIntersectionPoints.Count - 1; j += 2)
            {
                Vector3 _TreesGeneratingStartPoint = _CurrentLineIntersectionPoints[j].ToVector3();
                Vector3 _TreesGeneratingEndPoint = _CurrentLineIntersectionPoints[j + 1].ToVector3();
                int _TreesCount = Mathf.FloorToInt(Vector3.Distance(_TreesGeneratingStartPoint, _TreesGeneratingEndPoint) / DistanceBetweenTrees);
                _ForestMeshDataParameters.VerticesCount += _TreesCount * _TreeGenerator.AllVerticesCount;
                _ForestMeshDataParameters.TrianglesCount += _TreesCount * _TreeGenerator.AllTrianglesCount;
            }
            _TreesGeneratingLine.Bias -= DistanceBetweenTrees;
        };
        return _ForestMeshDataParameters;
    }

    private class ForestMeshDataParameters : MeshDataParameters
    {
        public ForestMeshDataParameters(int _VerticesCount, int _TrianglesCount) : base(_VerticesCount, _TrianglesCount) { }

        public ForestMeshDataParameters()
        {

        }

        public List<Vector2> IntersectionPoints = new List<Vector2>();
    }
}