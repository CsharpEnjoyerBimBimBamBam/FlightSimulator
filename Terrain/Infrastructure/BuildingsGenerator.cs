using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using static BuildingsData;

public class BuildingsGenerator : MeshDataGenerator<BuildingsData>
{
    public BuildingsGenerator(EarthTerrain Terrain)
    {
        _Terrain = Terrain;
    }

    private EarthTerrain _Terrain;
    private static Dictionary<RoofShape, Type> _RoofShapeTypes = new Dictionary<RoofShape, Type>()
    {
        {RoofShape.flat, typeof(FlatRoofGenerator)},
    };

    public override MeshData GenerateMeshData(BuildingsData _BuildingsData, int _LastVertexIndex = 0)
    {
        MeshDataParameters _MeshDataParameters = CalculateMeshDataParameters(_BuildingsData);
        MeshData _AllBuildingsMeshData = new MeshData(_MeshDataParameters.VerticesCount, _MeshDataParameters.TrianglesCount);
        _AllBuildingsMeshData.LastVertexIndex = _LastVertexIndex;
        for (int i = 0; i < _BuildingsData.elements.Count; i++)
        {
            BuildingElement _CurrentElement = _BuildingsData.elements[i];
            int _BuildingVecrticesCount = _CurrentElement.geometry.Count;
            int _TrianglesCount = (_BuildingVecrticesCount * 6) + ((_BuildingVecrticesCount - 2) * 3);
            MeshData _BuildingMeshData = new MeshData(_BuildingVecrticesCount * 2, _TrianglesCount);
            _BuildingMeshData.LastVertexIndex = _AllBuildingsMeshData.LastVertexIndex;
            int _VerticesIndex = 0;
            int _TrianglesIndex = 0;
            bool _IsGeometryClockWise = IsBuildingGeometryClockWise(_CurrentElement.geometry);
            for (int j = 0; j < _BuildingVecrticesCount; j++)
            {
                OverpassGeometry _BuildingGeometry = _CurrentElement.geometry[j];
                if (!_IsGeometryClockWise)
                {
                    _BuildingGeometry = _CurrentElement.geometry[_BuildingVecrticesCount - 1 - j];
                }
                Vector3 _VertexEarthLocalPosition = _BuildingGeometry.Coordinates.ToEarthLocalPosition();
                Vector3 _VertexTerrainLocalPosition = _Terrain.FromEarthToTerrainLocalPosition(_VertexEarthLocalPosition);
                float _SeaLevelHeight = _Terrain.FindNearestVertexInHeightMap(_VertexTerrainLocalPosition).y;
                float _FullBuildingHeight = _SeaLevelHeight + _CurrentElement.tags.Height;
                _VertexEarthLocalPosition = _VertexEarthLocalPosition.normalized * (Constants.EarthRadius + _SeaLevelHeight);
                _BuildingMeshData.Vertices[_VerticesIndex] = _Terrain.FromEarthToTerrainLocalPosition(_VertexEarthLocalPosition);
                _VertexEarthLocalPosition = _VertexEarthLocalPosition.normalized * (Constants.EarthRadius + _FullBuildingHeight);
                _BuildingMeshData.Vertices[_VerticesIndex + 1] = _Terrain.FromEarthToTerrainLocalPosition(_VertexEarthLocalPosition);

                _BuildingMeshData.Normals[_VerticesIndex] = Vector3.one;
                _BuildingMeshData.Normals[_VerticesIndex + 1] = Vector3.one;

                _BuildingMeshData.UV[_VerticesIndex] = _Terrain.CalcualteVertexUV(_BuildingMeshData.Vertices[_VerticesIndex]);
                _BuildingMeshData.UV[_VerticesIndex + 1] = _Terrain.CalcualteVertexUV(_BuildingMeshData.Vertices[_VerticesIndex]);

                if (j > 1)
                {
                    _BuildingMeshData.Normals[_VerticesIndex - 2] = EarthTerrain.CalculateTriangleNormal(_BuildingMeshData.Vertices[_VerticesIndex - 2],
                        _BuildingMeshData.Vertices[_VerticesIndex - 1],
                        _BuildingMeshData.Vertices[_VerticesIndex - 4]);
                    _BuildingMeshData.Normals[_VerticesIndex - 2] += EarthTerrain.CalculateTriangleNormal(_BuildingMeshData.Vertices[_VerticesIndex - 2],
                        _BuildingMeshData.Vertices[_VerticesIndex + 1],
                        _BuildingMeshData.Vertices[_VerticesIndex - 1]);
                    _BuildingMeshData.Normals[_VerticesIndex - 1] = EarthTerrain.CalculateTriangleNormal(_BuildingMeshData.Vertices[_VerticesIndex - 1],
                        _BuildingMeshData.Vertices[_VerticesIndex - 3],
                        _BuildingMeshData.Vertices[_VerticesIndex - 4]);
                    _BuildingMeshData.Normals[_VerticesIndex - 1] += EarthTerrain.CalculateTriangleNormal(_BuildingMeshData.Vertices[_VerticesIndex - 1],
                        _BuildingMeshData.Vertices[_VerticesIndex - 2],
                        _BuildingMeshData.Vertices[_VerticesIndex + 1]);
                    ReplaceZeroNormals(_BuildingMeshData.Normals, _VerticesIndex - 2);
                    ReplaceZeroNormals(_BuildingMeshData.Normals, _VerticesIndex - 1);
                }

                _BuildingMeshData.Triangles[_TrianglesIndex] = _AllBuildingsMeshData.LastVertexIndex + _VerticesIndex + 1;
                _BuildingMeshData.Triangles[_TrianglesIndex + 1] = _AllBuildingsMeshData.LastVertexIndex + _VerticesIndex + 3;
                _BuildingMeshData.Triangles[_TrianglesIndex + 2] = _AllBuildingsMeshData.LastVertexIndex + _VerticesIndex + 4;

                _BuildingMeshData.Triangles[_TrianglesIndex + 3] = _AllBuildingsMeshData.LastVertexIndex + _VerticesIndex + 1;
                _BuildingMeshData.Triangles[_TrianglesIndex + 4] = _AllBuildingsMeshData.LastVertexIndex + _VerticesIndex + 4;
                _BuildingMeshData.Triangles[_TrianglesIndex + 5] = _AllBuildingsMeshData.LastVertexIndex + _VerticesIndex + 2;
                if (j == _CurrentElement.geometry.Count - 1)
                {
                    _BuildingMeshData.Triangles[_TrianglesIndex + 1] = _AllBuildingsMeshData.LastVertexIndex + 1;
                    _BuildingMeshData.Triangles[_TrianglesIndex + 2] = _AllBuildingsMeshData.LastVertexIndex + 2;
                    _BuildingMeshData.Triangles[_TrianglesIndex + 4] = _AllBuildingsMeshData.LastVertexIndex + 2;
                }
                _VerticesIndex += 2;
                _TrianglesIndex += 6;
            }
            _BuildingMeshData.CurrentVertexIndex = _VerticesIndex;
            _BuildingMeshData.CurrentTrianglesIndex = _TrianglesIndex;
            RoofGenerator _RoofGenerator = (RoofGenerator)Activator.CreateInstance(_RoofShapeTypes[RoofShape.flat]);
            _RoofGenerator.AddToMeshData(_BuildingMeshData, ref _BuildingMeshData, false);
            _AllBuildingsMeshData.Connect(_BuildingMeshData, false);
            _AllBuildingsMeshData.LastVertexIndex += _BuildingMeshData.Vertices.Length;
        }
        return _AllBuildingsMeshData;
    }

    public override GameObject CreateGameObjectFromMeshData(MeshData _MeshData, string _Name, string _ShaderName = "Standard")
    {
        GameObject _GameObject = base.CreateGameObjectFromMeshData(_MeshData, _Name);
        _GameObject.transform.parent = _Terrain.GameObject.transform;
        _GameObject.transform.localPosition = Vector3.zero;
        _GameObject.transform.rotation = _Terrain.GameObject.transform.rotation;
        MeshRenderer _MeshRenderer = _GameObject.AddComponent<MeshRenderer>();
        _MeshRenderer.material = new Material(Shader.Find("Standard"));
        _MeshRenderer.material.SetFloat("_Glossiness", 0.05f);
        _MeshRenderer.material.SetFloat("_Metallic", 0.15f);
        return _GameObject;
    }

    public bool IsBuildingGeometryClockWise(List<OverpassGeometry> _Geometry)
    {
        float _AnglesSum = 0;
        Vector3 _VertexInsideGeometry = (_Geometry[0].Coordinates.ToEarthLocalPosition() + _Geometry[2].Coordinates.ToEarthLocalPosition()) / 2;
        int _Index = 3;
        while (!IsVertexInsideGeometry(_Geometry, _VertexInsideGeometry))
        {
            if (_Index >= _Geometry.Count - 1)
            {
                break;
            }
            _VertexInsideGeometry = (_Geometry[0].Coordinates.ToEarthLocalPosition() + _Geometry[_Index].Coordinates.ToEarthLocalPosition()) / 2;
            _Index++;
        }
        for (int i = 1; i < _Geometry.Count; i++)
        {
            Vector3 _CurrentVertexLocalPosition = _Geometry[i - 1].Coordinates.ToEarthLocalPosition() - _VertexInsideGeometry;
            Vector3 _NextVertexLocalPosition = _Geometry[i].Coordinates.ToEarthLocalPosition() - _VertexInsideGeometry;
            float _Angle = Vector3.Angle(_CurrentVertexLocalPosition, _NextVertexLocalPosition);
            Vector3 _CrossProduct = Vector3.Cross(_CurrentVertexLocalPosition, _NextVertexLocalPosition);
            if (_CrossProduct.y > 0)
            {
                _AnglesSum += _Angle;
            }
            else
            {
                _AnglesSum -= _Angle;
            }
        }
        _AnglesSum /= 360;
        if (Mathf.RoundToInt(_AnglesSum) == 1)
            return true;
        return false;
    }

    public bool IsVertexInsideGeometry(List<OverpassGeometry> _Geometry, Vector3 _Vertex)
    {
        float _AnglesSum = 0;
        for (int i = 1; i < _Geometry.Count; i++)
        {
            Vector3 _PreviousVertexLocalPosition = _Geometry[i - 1].Coordinates.ToEarthLocalPosition() - _Vertex;
            Vector3 _NextVertexLocalPosition = _Geometry[i].Coordinates.ToEarthLocalPosition() - _Vertex;
            float _Angle = Vector3.Angle(_PreviousVertexLocalPosition, _NextVertexLocalPosition);
            Vector3 _CrossProduct = Vector3.Cross(_PreviousVertexLocalPosition, _NextVertexLocalPosition);
            if (_CrossProduct.y > 0)
            {
                _AnglesSum += _Angle;
            }
            else
            {
                _AnglesSum -= _Angle;
            }
        }
        _AnglesSum /= 360;
        if (Mathf.RoundToInt(_AnglesSum) == 0)
            return false;
        return true;
    }

    public override MeshDataParameters CalculateMeshDataParameters(BuildingsData _Data)
    {
        int _VerticesCount = 0;
        int _TrianglesCount = 0;
        for (int i = 0; i < _Data.elements.Count; i++)
        {
            //RoofGenerator _RoofGenerator = (RoofGenerator)Activator.CreateInstance(_RoofShapeTypes[RoofShape.flat]);
            int _GeometryCount = _Data.elements[i].geometry.Count;
            _VerticesCount += _GeometryCount * 2;
            _TrianglesCount += (_GeometryCount * 6) + ((_GeometryCount - 2) * 3);
        }
        return new MeshDataParameters(_VerticesCount, _TrianglesCount);
    }

    private void ReplaceZeroNormals(Vector3[] _Normals, int _Index)
    {
        if (_Normals[_Index] == Vector3.zero)
            _Normals[_Index] = Vector3.one;
    }
}
