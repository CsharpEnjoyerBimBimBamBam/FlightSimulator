using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Jobs;

public class TreeGenerator : MeshDataGenerator<Vector3>
{
    public TreeGenerator()
    {
        RecalculateLayersPointsOffset();
        MeshDataParameters _Parameters = CalculateMeshDataParameters();
        AllVerticesCount = _Parameters.VerticesCount;
        AllTrianglesCount = _Parameters.TrianglesCount;
    }

    public int LayersCount = 2;
    public float BaseLayerHeight = 7.5f;
    public float RadiusReductionFactor = 0.1f;
    public float MaxLayerOffset = 0.3f;
    public float MaxLayerHeightOffset = 5f;
    public int BranchesCount = 5;
    public int BrancheLayersCount = 3;
    public int BrancheLayerPointsCount = 5;
    public float BranchesReductionFactor = 0.3f;
    public float TreeHeight { get; private set; }
    public int AllVerticesCount { get; private set; }
    public int AllTrianglesCount { get; private set; }
    public float BaseTreeRadius
    {
        get
        {
            return _FirstLayerRadius;
        }
        set
        {
            _FirstLayerRadius = value;
            RecalculateLayersPointsOffset();
        }
    }
    public int LayerPointsCount
    {
        get
        {
            return _LayerPointsCount;
        }
        set
        {
            _LayerPointsCount = value;
            RecalculateLayersPointsOffset();
        }
    }
    private float _FirstLayerRadius = 1;
    private int _LayerPointsCount = 5;
    private Dictionary<int, Vector3[]> _LayersPointsOffset = new Dictionary<int, Vector3[]>();
    private System.Random _Random = new System.Random();
    private Vector3 _GrowthVector = Vector3.up;

    public override MeshData GenerateMeshData(Vector3 _CenterPosition, int _LastVertexIndex)
    {
        MeshDataParameters _Parameters = CalculateMeshDataParameters();
        TreeHeight = 0;
        MeshData _TreeMeshData = new MeshData(_Parameters.VerticesCount, _Parameters.TrianglesCount);
        _TreeMeshData.LastVertexIndex = _LastVertexIndex;
        _TreeMeshData.CurrentVertexIndex = _LayerPointsCount;
        float _V = 0;
        for (int i = 0; i < _LayerPointsCount; i++)
        {
            Vector3 _CurrentVertex = _CenterPosition + _LayersPointsOffset[0][i];
            Vector3 _CurrentNormal = (_LayersPointsOffset[0][i] * 2) - _CurrentVertex;
            _TreeMeshData.Vertices[i] = _CurrentVertex;
            _TreeMeshData.Normals[i] = _CurrentNormal;
            float _U = i / (float)_LayerPointsCount;
            _TreeMeshData.UV[i] = new Vector2(_U, _V);
        }

        for (int i = 1; i <= LayersCount; i++)
        {
            _TreeMeshData.LastVertexIndex += _LayerPointsCount;
            AddLayer(ref _TreeMeshData, _CenterPosition, i);
        }
        if (BranchesCount != 0)
            AddBranches(ref _TreeMeshData, _CenterPosition);
        _TreeMeshData.Bounds = new Bounds(Vector3.zero, new Vector3(_FirstLayerRadius, TreeHeight, _FirstLayerRadius));
        return _TreeMeshData;
    }

    public override MeshDataParameters CalculateMeshDataParameters(Vector3 _Data = new Vector3())
    {
        int _BranchVerticesCount = BrancheLayerPointsCount * (BrancheLayersCount + 1);
        int _BranchTrianglesCount = BrancheLayerPointsCount * BrancheLayersCount * 6;
        int _VerticesCount = (LayerPointsCount * (LayersCount + 1)) + (CalculateBranchesCount() * _BranchVerticesCount);
        int _TrianglesCount = (LayerPointsCount * LayersCount * 6) + (CalculateBranchesCount() * _BranchTrianglesCount);
        return new MeshDataParameters(_VerticesCount, _TrianglesCount);
    }

    private void AddLayer(ref MeshData _TreeMeshData, Vector3 _CenterPosition, int _LayerNumber)
    {
        MeshData _LayerMeshData = new MeshData(_LayerPointsCount, _LayerPointsCount * 6);
        int _LastIndex = _TreeMeshData.LastVertexIndex;
        int _TrianglesIndex = 0;
        float _XShift = _Random.GenerateRandomFloat(MaxLayerOffset);
        float _YShift = _Random.GenerateRandomFloat(MaxLayerOffset);
        float _ZShift = _Random.GenerateRandomFloat(MaxLayerOffset);
        Vector3 _Shift = new Vector3(_XShift, _YShift, _ZShift);
        if (_LayerNumber == 0)
            _Shift = Vector3.zero;
        Vector3[] _LayerPointsOffset = _LayersPointsOffset[_LayerNumber];
        int _PreviousLayerFirstVertexIndex = _LastIndex - _LayerPointsCount + 1;
        float _Height = (BaseLayerHeight * _LayerNumber) + _Random.GenerateRandomFloat(MaxLayerHeightOffset);
        if (_LayerNumber == LayersCount)
            TreeHeight = _Height;
        float _V = _LayerNumber / (float)LayersCount;
        for (int i = 0; i < _LayerPointsCount; i++)
        {
            int _PreviousLayerCurrentVertexIndex = _PreviousLayerFirstVertexIndex + i;
            Vector3 _Offset = _LayerPointsOffset[i] + _Shift;
            Vector3 _CurrentVertex = _CenterPosition + _Offset;
            _CurrentVertex += _GrowthVector * _Height;
            Vector3 _CurrentNormal = (_Offset * 2) - _CurrentVertex;

            _LayerMeshData.Vertices[i] = _CurrentVertex;

            _LayerMeshData.Normals[i] = _CurrentNormal;

            float _U = i / (float)_LayerPointsCount;
            _LayerMeshData.UV[i] = new Vector2(_U, _V);

            _LayerMeshData.Triangles[_TrianglesIndex] = _LastIndex + i + 1;
            _LayerMeshData.Triangles[_TrianglesIndex + 1] = _PreviousLayerCurrentVertexIndex;
            _LayerMeshData.Triangles[_TrianglesIndex + 2] = _PreviousLayerCurrentVertexIndex + 1;

            _LayerMeshData.Triangles[_TrianglesIndex + 3] = _LastIndex + i + 1;
            _LayerMeshData.Triangles[_TrianglesIndex + 4] = _PreviousLayerCurrentVertexIndex + 1;
            _LayerMeshData.Triangles[_TrianglesIndex + 5] = _LastIndex + i + 2;

            if (i == _LayerPointsCount - 1)
            {
                _LayerMeshData.Triangles[_TrianglesIndex + 2] = _PreviousLayerFirstVertexIndex;
                _LayerMeshData.Triangles[_TrianglesIndex + 4] = _PreviousLayerFirstVertexIndex;
                _LayerMeshData.Triangles[_TrianglesIndex + 5] = _LastIndex + 1;
            }
            _TrianglesIndex += 6;
        }
        _TreeMeshData.Connect(_LayerMeshData, false);
    }

    private void AddBranches(ref MeshData _TreeMeshData, Vector3 _CenterPosition)
    {
        if (BranchesCount == 0)
            return;
        int _BranchesCount = Mathf.FloorToInt(BranchesCount * BranchesReductionFactor);
        //if (_BranchesCount < 1)
        //    _BranchesCount = 0;
        TreeGenerator _BranchesGenerator = new TreeGenerator();
        _BranchesGenerator.BranchesCount = _BranchesCount;
        _BranchesGenerator.LayersCount = BrancheLayersCount;
        _BranchesGenerator.LayerPointsCount = BrancheLayerPointsCount;
        _BranchesGenerator.BaseTreeRadius = 0.2f;
        _BranchesGenerator.RadiusReductionFactor = 1f / BrancheLayersCount;
        _BranchesGenerator.MaxLayerHeightOffset = 0.3f;
        _BranchesGenerator.MaxLayerOffset = 0;
        _BranchesGenerator.BaseLayerHeight = 1.5f;
        MeshDataParameters _Parameters = _BranchesGenerator.CalculateMeshDataParameters();
        //MeshData _BranchesMeshData = new MeshData(_Parameters.VerticesCount * BranchesCount, _Parameters.TrianglesCount * BranchesCount);
        MeshData _BranchesMeshData = new MeshData();
        _BranchesMeshData.LastVertexIndex = _TreeMeshData.LastVertexIndex;
        Vector3 _StartBrenchPosition = _CenterPosition;
        _StartBrenchPosition += _GrowthVector * (TreeHeight * 0.7f);
        float _RotationAngle = 2 * Mathf.PI / BranchesCount;
        for (int i = 0; i < BranchesCount; i++)
        {
            Vector3 _CurrentBrenchPosition = _StartBrenchPosition;
            //_CurrentBrenchPosition += _GrowthVector * _Random.GenerateRandomFloat(TreeHeight * 0.2f);
            float _X = Mathf.Sin(_RotationAngle * i) * _FirstLayerRadius;
            float _Z = Mathf.Cos(_RotationAngle * i) * _FirstLayerRadius;
            Vector3 _Vector = new Vector3(_X, 0, _Z);
            _BranchesGenerator._GrowthVector = Vector3.RotateTowards(_GrowthVector, _Vector, Mathf.PI / 4, 0);
            _BranchesGenerator.RecalculateLayersPointsOffset();
            _BranchesGenerator.AddToMeshData(_CurrentBrenchPosition, ref _BranchesMeshData, true);
        }
        _TreeMeshData.Connect(_BranchesMeshData, true);
    }

    private void RecalculateLayersPointsOffset()
    {
        _LayersPointsOffset.Clear();
        float _CurrentRadius = _FirstLayerRadius;
        for (int i = 0; i <= LayersCount; i++)
        {
            _LayersPointsOffset[i] = CalculateLayerPointsOffset(_CurrentRadius);
            _CurrentRadius -= _FirstLayerRadius * RadiusReductionFactor;
            _CurrentRadius = Mathf.Clamp(_CurrentRadius, 0, _CurrentRadius);
        }
    }

    private Vector3[] CalculateLayerPointsOffset(float _Radius)
    {
        Vector3[] _PointsOffset = new Vector3[_LayerPointsCount];
        float _RotationAngle = 2 * Mathf.PI / _LayerPointsCount;
        for (int i = 0; i < _PointsOffset.Length; i++)
        {
            float _X = Mathf.Sin(_RotationAngle * i) * _Radius;
            float _Z = Mathf.Cos(_RotationAngle * i) * _Radius;
            Vector3 _NotRotatedVector = new Vector3(_X, 0, _Z);
            float _VectorRotationAngle = (Vector3.Angle(_NotRotatedVector, _GrowthVector) - 90) * Mathf.Deg2Rad;
            _PointsOffset[i] = Vector3.RotateTowards(_NotRotatedVector, _GrowthVector, _VectorRotationAngle, 0);
        }
        return _PointsOffset;
    }

    private int CalculateBranchesCount()
    {
        int _BranchesCount = BranchesCount;
        int _ChildrensCount = 0;
        while (_BranchesCount > 0)
        {
            _BranchesCount = Mathf.FloorToInt(_BranchesCount * BranchesReductionFactor);
            _ChildrensCount += _BranchesCount;
        }
        return (_ChildrensCount * BranchesCount) + BranchesCount;
    }
}
