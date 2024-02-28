using UnityEngine;

public abstract class RoofGenerator : MeshDataGenerator<MeshData>
{
    protected abstract MeshData GenerateMeshDataForQuadrangularBuilding(int _StartIndex);
    protected abstract MeshData GenerateMeshDataForTriangularBuilding(int _StartIndex);

    public bool IsBuildingTriangular(Vector3[] _Vertices)
    {
        if (_Vertices.Length == 8) return true;
        return false;
    }

    public bool IsBuildingQuadrangular(Vector3[] _Vertices)
    {
        if (_Vertices.Length == 6) return true;
        return false;
    }

    public Vector3[] GetUpVertices(Vector3[] _Vertices)
    {
        int _Index = 0;
        Vector3[] _UpVertices = new Vector3[_Vertices.Length / 2];
        for (int i = 0; i < _Vertices.Length; i++)
        {
            if (i % 2 != 0)
            {
                _UpVertices[_Index] = _Vertices[i];
                _Index++;
            }
        }
        return _UpVertices;
    }
}
