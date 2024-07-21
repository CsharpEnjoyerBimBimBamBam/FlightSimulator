using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class Polygon
{
    public Polygon(Vector2[] _Vertices)
    {
        Initialize(_Vertices);
    }

    public Polygon(Vector2[] _Vertices, bool _SortVerticesClockWise)
    {
        Initialize(_Vertices);
        if (_SortVerticesClockWise)
            SortClockWise();
    }

    public IReadOnlyList<Vector2> Vertices { get; private set; }
    public IReadOnlyList<LineSegment> Edges { get; private set; }
    public float SignedArea { get; private set; }
    public float Area { get; private set; }
    public bool IsClockWise { get; private set; } 

    public bool CheckIfPointInsidePolygon(Vector2 _Point)
    {
        Vector2 _GuideVector = Vector2.right;
        Line _RayLine = new LineSegment(_Point, _Point + _GuideVector).ToLine();
        int _IntersectionsCount = 0;
        for (int i = 0; i < Edges.Count; i++)
        {
            if (Edges[i].CheckForIntersectionWithLine(_RayLine, out Vector2 _IntersectionPoint))
            {
                if (Vector2.Angle(_GuideVector, _IntersectionPoint - _Point) == 0)
                {
                    _IntersectionsCount++;
                }
            }
        }

        if (_IntersectionsCount % 2 == 0)
            return false;
        return true;
    }

    public void SortClockWise()
    {
        if (SignedArea < 0)
        {
            Vertices = Vertices.Reverse().ToList();
            Edges = VerticesToEdges(Vertices.ToArray());
            SignedArea *= -1;
            IsClockWise = true;
        }
    }

    private float CalculateSignedArea()
    {
        float _SignedArea = 0;
        for (int i = 0; i < Edges.Count; i++)
        {
            Vector3 _CrossProduct = Vector3.Cross(Edges[i].FirstPoint.ToVector3(), Edges[i].SecondPoint.ToVector3());
            float _TriangleSignedArea = _CrossProduct.magnitude / 2;
            if (_CrossProduct.y < 0)
                _TriangleSignedArea *= -1;
            _SignedArea += _TriangleSignedArea;
        }
        return _SignedArea;
    }

    private LineSegment[] VerticesToEdges(Vector2[] _Vertices)
    {
        LineSegment[] _Edges = new LineSegment[_Vertices.Length];
        Vector2 _PreviousVertex = _Vertices[_Vertices.Length - 1];
        for (int i = 0; i < _Vertices.Length; i++)
        {
            Vector2 _CurrentVeretex = _Vertices[i];
            _Edges[i] = new LineSegment(_PreviousVertex, _CurrentVeretex);
            _PreviousVertex = _Vertices[i];
        }
        return _Edges;
    }

    private void Initialize(Vector2[] _Vertices)
    {
        Vertices = _Vertices;
        Edges = VerticesToEdges(_Vertices);
        SignedArea = CalculateSignedArea();
        Area = Mathf.Abs(SignedArea);
        IsClockWise = SignedArea > 0;
    }
}
