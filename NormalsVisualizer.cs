using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.PackageManager.Requests;

public class NormalsVisualizer : MonoBehaviour
{
    private static Dictionary<GameObject, GameObject> _GameObjectsLines = new Dictionary<GameObject, GameObject>();
    private static float _NormalLineLength = 2f;
    private static float _NormalLineWidth = 0.2f;

    [MenuItem("Normals Tools/Visualize normals")]
    private static void VisualizeNormals()
    {
        GameObject _SelectedObject = GetSelectedGameObject();
        if (!_SelectedObject.TryGetComponent(out MeshFilter _SelectedObjectMeshFilter))
        {
            throw new Exception("Object must have mesh");
        }
        Mesh _SelectedObjectMesh = _SelectedObjectMeshFilter.sharedMesh;
        GameObject _CurrentLinesGameObject = new GameObject();
        for (int i = 0; i < _SelectedObjectMesh.normals.Length; i++)
        {
            Vector3 _VertexWorldPosition = _SelectedObject.transform.TransformPoint(_SelectedObjectMesh.vertices[i]);
            Vector3 _NormalLocalPosition = _SelectedObjectMesh.vertices[i] + (_SelectedObjectMesh.normals[i].normalized * _NormalLineLength);
            Vector3 _NormalWorldPosition = _SelectedObject.transform.TransformPoint(_NormalLocalPosition);
            GameObject _NormalLine = CreateLineRenderer(_VertexWorldPosition, _NormalWorldPosition, _NormalLineWidth, Color.cyan);
            _NormalLine.transform.parent = _CurrentLinesGameObject.transform;
        }
        _CurrentLinesGameObject.transform.parent = _SelectedObject.transform;
        _GameObjectsLines[_SelectedObject] = _CurrentLinesGameObject;
    }

    [MenuItem("Normals Tools/Remove visualized normals")]
    private static void RemoveVisualizedNormals()
    {
        GameObject _SelectedObject = GetSelectedGameObject();
        if (!_GameObjectsLines.ContainsKey(_SelectedObject) || _GameObjectsLines[_SelectedObject] == null)
        {
            throw new Exception("The visualized normals of the selected object have already been removed");
        }
        DestroyImmediate(_GameObjectsLines[_SelectedObject]);
    }

    [MenuItem("Normals Tools/Flip normals")]
    private static void FlipNormals()
    {
        GameObject _SelectedObject = GetSelectedGameObject();
        Mesh _SelectedObjectMesh = GetGameObjectMesh(_SelectedObject);
        Vector3[] _ObjectNormals = _SelectedObjectMesh.normals;
        for (int i = 0; i < _ObjectNormals.Length; i++)
        {
            _ObjectNormals[i] *= -1;
        }
        _SelectedObjectMesh.normals = _ObjectNormals;
    }

    private static GameObject CreateLineRenderer(Vector3 _StartPosition, Vector3 _EndPosition, float _Width, Color _Color)
    {
        GameObject _LineRendererGameObject = new GameObject();
        LineRenderer _LineRenderer = _LineRendererGameObject.AddComponent<LineRenderer>();
        _LineRenderer.SetPosition(0, _StartPosition);
        _LineRenderer.SetPosition(1, _EndPosition);
        _LineRenderer.widthMultiplier = _Width;
        _LineRenderer.material = new Material(Shader.Find("Legacy Shaders/Diffuse"));
        _LineRenderer.sharedMaterial.color = _Color;
        return _LineRendererGameObject;
    }

    private static GameObject GetSelectedGameObject()
    {
        if (Selection.gameObjects.Length != 1)
        {
            throw new Exception("Only one object must be selected");
        }
        return Selection.gameObjects[0];
    }

    private static Mesh GetGameObjectMesh(GameObject _GameObject)
    {
        if (!_GameObject.TryGetComponent(out MeshFilter _GameObjectMeshFilter))
        {
            throw new Exception("Object must have mesh");
        }
        return _GameObjectMeshFilter.sharedMesh;
    }
}
