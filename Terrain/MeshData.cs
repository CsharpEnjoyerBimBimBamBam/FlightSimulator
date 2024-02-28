using UnityEngine;
using System;
using UnityEngine.Scripting;

public class MeshData : IDisposable
{
    public MeshData(int _VerticesCount, int _TrianglesCount)
    {
        Vertices = new Vector3[_VerticesCount];
        UV = new Vector2[_VerticesCount];
        Triangles = new int[_TrianglesCount];
        Normals = new Vector3[_VerticesCount];
        Bounds = new Bounds();
    }

    public MeshData()
    {
        Vertices = new Vector3[0];
        UV = new Vector2[0];
        Triangles = new int[0];
        Normals = new Vector3[0];
        Bounds = new Bounds();
    }

    public Vector3[] Vertices;
    public Vector2[] UV;
    public int[] Triangles;
    public Vector3[] Normals;
    public Bounds Bounds;
    public int CurrentVertexIndex;
    public int LastVertexIndex;
    public int CurrentTrianglesIndex;
    public float CompressionRatio;
    public MeshData CompressedMeshData;

    public static MeshData FromMesh(Mesh _Mesh)
    {
        return new MeshData
        {
            Vertices = _Mesh.vertices,
            UV = _Mesh.uv,
            Normals = _Mesh.normals,
            Triangles = _Mesh.triangles,
            Bounds = _Mesh.bounds,
        };
    }

    public void Connect(MeshData _OtherMeshData, bool _InitializeNewArrays=true)
    {
        if (_InitializeNewArrays)
        {
            Vertices = ConnectArraysByCreatingNewArray(Vertices, _OtherMeshData.Vertices);
            UV = ConnectArraysByCreatingNewArray(UV, _OtherMeshData.UV);
            Triangles = ConnectArraysByCreatingNewArray(Triangles, _OtherMeshData.Triangles);
            Normals = ConnectArraysByCreatingNewArray(Normals, _OtherMeshData.Normals);
        }
        else
        {
            ConnectArrays(ref Vertices, _OtherMeshData.Vertices, CurrentVertexIndex);
            ConnectArrays(ref UV, _OtherMeshData.UV, CurrentVertexIndex);
            ConnectArrays(ref Triangles, _OtherMeshData.Triangles, CurrentTrianglesIndex);
            ConnectArrays(ref Normals, _OtherMeshData.Normals, CurrentVertexIndex);
        }
        CurrentVertexIndex += _OtherMeshData.Vertices.Length;
        CurrentTrianglesIndex += _OtherMeshData.Triangles.Length;
        Bounds = new Bounds(Vector3.zero, new Vector3(Mathf.Max(Bounds.size.x, _OtherMeshData.Bounds.size.x), 
            Mathf.Max(Bounds.size.y, _OtherMeshData.Bounds.size.y), 
            Mathf.Max(Bounds.size.z, _OtherMeshData.Bounds.size.z)));
    }

    public void Apply(Mesh _Mesh)
    {
        _Mesh.vertices = Vertices;  
        _Mesh.normals = Normals;
        _Mesh.triangles = Triangles;
        _Mesh.uv = UV;
        _Mesh.bounds = Bounds;
    }

    public void ApplyAsSubMesh(Mesh _Mesh, int _SubMeshIndex, int _UVChannel)
    {
        _Mesh.SetVertices(Vertices);
        _Mesh.SetTriangles(Triangles, _SubMeshIndex);
        _Mesh.SetNormals(Normals);
        _Mesh.SetUVs(_UVChannel, UV);
        _Mesh.bounds = Bounds;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private T[] ConnectArraysByCreatingNewArray<T>(T[] _FirstArray, T[] _SecondArray)
    {
        T[] _ConnectedArrays = new T[_FirstArray.Length + _SecondArray.Length];
        int _Index = 0;
        for (int i = 0; i < _FirstArray.Length; i++)
        {
            _ConnectedArrays[_Index] = _FirstArray[i];
            _Index++;
        }

        for (int i = 0; i < _SecondArray.Length; i++)
        {
            _ConnectedArrays[_Index] = _SecondArray[i];
            _Index++;
        }
        return _ConnectedArrays;
    }

    private void ConnectArrays<T>(ref T[] _FirstArray, T[] _SecondArray, int _StartIndex)
    {
        for (int i = 0; i < _SecondArray.Length; i++)
        {
            _FirstArray[_StartIndex + i] = _SecondArray[i];
        }
    }
}
