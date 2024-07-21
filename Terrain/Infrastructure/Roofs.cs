using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class FlatRoofGenerator : RoofGenerator
{
    private bool[] _DeletedVertices;
    public override MeshData GenerateMeshData(MeshData _BuildingData, int _LastVertexIndex = 0)
    {
        MeshDataParameters _MeshDataParameters = CalculateMeshDataParameters(_BuildingData);
        MeshData _RoofMeshData = new MeshData(_MeshDataParameters.VerticesCount, _MeshDataParameters.TrianglesCount);
        if (IsBuildingQuadrangular(_BuildingData.Vertices))
        {
            return GenerateMeshDataForQuadrangularBuilding(_LastVertexIndex + 1);
        }
        else if (IsBuildingTriangular(_BuildingData.Vertices))
        {
            return GenerateMeshDataForTriangularBuilding(_LastVertexIndex + 1);
        }

        Vector2[] _UpVerticesArray = Vector3ArrayToVector2(GetUpVertices(_BuildingData.Vertices));
        List<Vector2> _UpVertices = new List<Vector2>(_UpVerticesArray);
        Polygon _Roof = new Polygon(_UpVertices.ToArray(), true);
        _DeletedVertices = new bool[_UpVerticesArray.Length];

        _RoofMeshData.Triangles = new int[(_UpVerticesArray.Length - 2) * 3];
        int _TrianglesIndex = 0;
        int _CurrentIndex = 0;
        int _NextIndex = 1;
        int _LastIndex = 2;
        int _CurrentTrianglesCount = 0;
        while (_CurrentTrianglesCount < _UpVerticesArray.Length - 2)
        {
            Vector3 _FirstEdgeVertex = _UpVerticesArray[_CurrentIndex];
            Vector3 _SecondEdgeVertex = _UpVerticesArray[_LastIndex];
            if (!ValidateEdge(_Roof, new LineSegment(_FirstEdgeVertex, _SecondEdgeVertex)))
            {
                _LastIndex = FindPreviousUndeletedVertexIndex(_CurrentIndex);
                _SecondEdgeVertex = _UpVerticesArray[_LastIndex];
                if (!ValidateEdge(_Roof, new LineSegment(_FirstEdgeVertex, _SecondEdgeVertex)))
                {
                    _NextIndex = FindPreviousUndeletedVertexIndex(_LastIndex);
                    _DeletedVertices[_LastIndex] = true;
                    _UpVertices.RemoveAt(_UpVertices.Count - 1);
                }
                else
                {
                    _DeletedVertices[_CurrentIndex] = true;
                    _UpVertices.RemoveAt(0);
                }
            }
            else
            {
                _DeletedVertices[_NextIndex] = true;
                _UpVertices.RemoveAt(1);
            }
            _RoofMeshData.Triangles[_TrianglesIndex] = _LastVertexIndex + (_CurrentIndex * 2) + 2;
            _RoofMeshData.Triangles[_TrianglesIndex + 1] = _LastVertexIndex + (_NextIndex * 2) + 2;
            _RoofMeshData.Triangles[_TrianglesIndex + 2] = _LastVertexIndex + (_LastIndex * 2) + 2;
            Polygon _Triangle = new Polygon(new Vector2[] { _UpVerticesArray[_CurrentIndex], _UpVerticesArray[_NextIndex], _UpVerticesArray[_LastIndex] });
            if (!_Triangle.IsClockWise)
            {
                _RoofMeshData.Triangles[_TrianglesIndex] = _LastVertexIndex + (_LastIndex * 2) + 2;
                _RoofMeshData.Triangles[_TrianglesIndex + 2] = _LastVertexIndex + (_CurrentIndex * 2) + 2;
            }
            _CurrentIndex = FindFirstUndeletedVertexIndex();
            _NextIndex = FindNextUndeletedVertexIndex(_CurrentIndex);
            _LastIndex = FindNextUndeletedVertexIndex(_NextIndex);
            _TrianglesIndex += 3;
            _CurrentTrianglesCount++;
        }
        return _RoofMeshData;
    }

    public override MeshDataParameters CalculateMeshDataParameters(MeshData _BuildingData)
    {
        int _VerticesCount = 0;
        int _TrianglesCount = ((_BuildingData.Vertices.Length / 2) - 2) * 3;
        return new MeshDataParameters(_VerticesCount, _TrianglesCount);
    }

    private int FindFirstUndeletedVertexIndex()
    {
        int _Index = 0;
        if (!_DeletedVertices[_Index])
        {
            return _Index;
        }
        while (_DeletedVertices[_Index])
        {
            _Index++;
        }
        return _Index;
    }

    private int FindNextUndeletedVertexIndex(int _CurrentVertexIndex)
    {
        int _UndeletedVertexIndex = _CurrentVertexIndex + 1;
        if (_UndeletedVertexIndex > _DeletedVertices.Length - 1)
        {
            _UndeletedVertexIndex = 0;
        }

        while (_DeletedVertices[_UndeletedVertexIndex])
        {
            _UndeletedVertexIndex++;
            if (_UndeletedVertexIndex > _DeletedVertices.Length - 1)
            {
                _UndeletedVertexIndex = 0;
            }
        }
        return _UndeletedVertexIndex;
    }

    private int FindPreviousUndeletedVertexIndex(int _CurrentVertexIndex)
    {
        int _UndeletedVertexIndex = _CurrentVertexIndex - 1;
        if (_UndeletedVertexIndex < 0)
        {
            _UndeletedVertexIndex = _DeletedVertices.Length - 1;
        }

        while (_DeletedVertices[_UndeletedVertexIndex])
        {
            _UndeletedVertexIndex--;
            if (_UndeletedVertexIndex < 0)
            {
                _UndeletedVertexIndex = _DeletedVertices.Length - 1;
            }
        }
        return _UndeletedVertexIndex;
    }

    private Vector2[] Vector3ArrayToVector2(Vector3[] _Vector2List)
    {
        Vector2[] _Vectors = new Vector2[_Vector2List.Length];
        for (int i = 0; i < _Vector2List.Length; i++)
        {
            _Vectors[i] = _Vector2List[i].ToVector2();
        }
        return _Vectors;
    }

    private bool ValidateEdge(Polygon _RoofPolygon, LineSegment _Edge)
    {
        bool _IsEdgeIntersection = _Edge.CheckForIntersection(_RoofPolygon.Edges.ToArray());
        bool _IsEdgeInsidePolygon = _RoofPolygon.CheckIfPointInsidePolygon((_Edge.FirstPoint + _Edge.SecondPoint) / 2);
        if (!_IsEdgeInsidePolygon || _IsEdgeIntersection)
        {
            return false;
        }
        return true;
    }

    protected override MeshData GenerateMeshDataForQuadrangularBuilding(int _StartIndex)
    {
        int[] _Triangles = new int[6];
        _Triangles[0] = _StartIndex;
        _Triangles[1] = _StartIndex + 2;
        _Triangles[2] = _StartIndex + 4;

        _Triangles[3] = _StartIndex + 4;
        _Triangles[4] = _StartIndex + 6;
        _Triangles[5] = _StartIndex;
        MeshData _MeshData = new MeshData();
        _MeshData.Triangles = _Triangles;
        return _MeshData;
    }

    protected override MeshData GenerateMeshDataForTriangularBuilding(int _StartIndex)
    {
        int[] _Triangles = new int[3];
        _Triangles[0] = _StartIndex;
        _Triangles[1] = _StartIndex + 2;
        _Triangles[2] = _StartIndex + 4;
        MeshData _MeshData = new MeshData();
        _MeshData.Triangles = _Triangles;
        return _MeshData;
    }
}
