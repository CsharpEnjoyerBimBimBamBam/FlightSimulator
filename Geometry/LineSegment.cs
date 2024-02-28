using UnityEngine;

public struct LineSegment
{
    public LineSegment(Vector2 _FirstPoint, Vector2 _SecondPoint)
    {
        FirstPoint = _FirstPoint;
        SecondPoint = _SecondPoint;
    }

    public Vector2 FirstPoint;
    public Vector2 SecondPoint;
    public float Magnitude { get { return Vector2.Distance(FirstPoint, SecondPoint); } }

    public static bool CheckForIntersection(LineSegment _FirstLineSegment, LineSegment _SecondLineSegment, out Vector2 _IntersectionPoint)
    {
        Vector2 _FirstLineVector = _FirstLineSegment.SecondPoint - _FirstLineSegment.FirstPoint;
        Vector2 _SecondLineFirstVertexLocal = _SecondLineSegment.FirstPoint - _FirstLineSegment.FirstPoint;
        Vector2 _SecondLineSecondVertexLocal = _SecondLineSegment.SecondPoint - _FirstLineSegment.FirstPoint;

        Vector3 _SecondToCurrentCrossProduct = Vector3.Cross(_FirstLineVector, _SecondLineFirstVertexLocal);
        Vector3 _NextToCurrentCrossProduct = Vector3.Cross(_FirstLineVector, _SecondLineSecondVertexLocal);

        Vector2 _SecondLineVector = _SecondLineSegment.SecondPoint - _SecondLineSegment.FirstPoint;
        Vector2 _FirstLineFirstVertexLocal = _FirstLineSegment.FirstPoint - _SecondLineSegment.FirstPoint;
        Vector2 _FirstLineSecondVertexLocal = _FirstLineSegment.SecondPoint - _SecondLineSegment.FirstPoint;

        Vector3 _NextToFirstCrossProduct = Vector3.Cross(_SecondLineVector, _FirstLineFirstVertexLocal);
        Vector3 _NextToSecondCrossProduct = Vector3.Cross(_SecondLineVector, _FirstLineSecondVertexLocal);

        _IntersectionPoint = Vector2.zero;

        if (((_SecondToCurrentCrossProduct.z > 0 && _NextToCurrentCrossProduct.z < 0) ||
        (_SecondToCurrentCrossProduct.z < 0 && _NextToCurrentCrossProduct.z > 0)) &&
        ((_NextToFirstCrossProduct.z > 0 && _NextToSecondCrossProduct.z < 0) ||
        (_NextToFirstCrossProduct.z < 0 && _NextToSecondCrossProduct.z > 0)))
        {
            _IntersectionPoint = Line.CalculateIntersactionPoint(_FirstLineSegment.ToLine(), _SecondLineSegment.ToLine());
            return true;
        }
        return false;
    }

    public bool CheckForIntersection(LineSegment _Other, out Vector2 _IntersectionPoint)
    {
        return CheckForIntersection(this, _Other, out _IntersectionPoint);
    }

    public static bool CheckForIntersectionWithLine(Line _Line, LineSegment _LineSegment, out Vector2 _IntersectionPoint)
    {
        float _Y1 = _Line.CalculateY(_LineSegment.FirstPoint.x);
        float _Y2 = _Line.CalculateY(_LineSegment.SecondPoint.x);
        _IntersectionPoint = Vector2.zero;
        if ((_Y1 < _LineSegment.FirstPoint.y && _Y2 > _LineSegment.SecondPoint.y) || 
        (_Y1 > _LineSegment.FirstPoint.y && _Y2 < _LineSegment.SecondPoint.y))
        {
            _IntersectionPoint = _LineSegment.ToLine().CalculateIntersactionPoint(_Line);
            return true;
        }
        return false;
    }

    public bool CheckForIntersectionWithLine(Line _Line, out Vector2 _IntersectionPoint)
    {
        return CheckForIntersectionWithLine(_Line, this, out _IntersectionPoint);
    }

    public static bool CheckForIntersection(LineSegment _CheckLineSegment, LineSegment[] _LineSegments)
    {
        foreach (LineSegment _LineSegment in _LineSegments)
        {
            if (CheckForIntersection(_CheckLineSegment, _LineSegment, out _))
            {
                return true;
            }
        }
        return false;
    }

    public bool CheckForIntersection(LineSegment[] _LineSegments)
    {
        return CheckForIntersection(this, _LineSegments);
    }

    public Line ToLine()
    {
        float _Slope = (SecondPoint.y - FirstPoint.y) / (SecondPoint.x - FirstPoint.x);
        float _Bias = FirstPoint.y - (_Slope * FirstPoint.x);
        return new Line(_Slope, _Bias);
    }
}
