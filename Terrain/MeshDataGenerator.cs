
using UnityEditor.Compilation;
using UnityEngine;

public abstract class MeshDataGenerator<T>
{
    protected Mesh _Mesh;
    protected MeshRenderer _MeshRenderer;

    public abstract MeshDataParameters CalculateMeshDataParameters(T _Data);
    public abstract MeshData GenerateMeshData(T _Data, int _LastVertexIndex);
    public void AddToMeshData(T _Data, ref MeshData _MeshData, bool _InitializeNewArrays = true)
    {
        MeshData _GeneratedData = GenerateMeshData(_Data, _MeshData.LastVertexIndex);
        _MeshData.Connect(_GeneratedData, _InitializeNewArrays);
        _MeshData.LastVertexIndex += _GeneratedData.Vertices.Length;
    }

    public virtual GameObject CreateGameObjectFromMeshData(MeshData _MeshData, string _Name, string _ShaderName = "Standard")
    {
        GameObject _GameObject = CreateGameObjectWithMesh(_Name, _ShaderName);
        _MeshData.Apply(_Mesh);
        return _GameObject;
    }

    private GameObject CreateGameObjectWithMesh(string _ObjectName, string _ShaderName)
    {
        GameObject _GameObject = new GameObject();
        _GameObject.name = _ObjectName;
        MeshFilter _MeshFilter = _GameObject.AddComponent<MeshFilter>();
        MeshRenderer MeshRenderer = _GameObject.AddComponent<MeshRenderer>();
        MeshRenderer.material = new Material(Shader.Find(_ShaderName));
        _Mesh = _MeshFilter.mesh;
        _MeshRenderer = MeshRenderer;
        return _GameObject;
    }
}
