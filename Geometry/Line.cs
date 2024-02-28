using UnityEngine;
using System;

public struct Line
{
    public Line(float _Slope, float _Bias)
    {
        Slope = _Slope;
        Bias = _Bias;
    }

    public float Slope;
    public float Bias;

    public static Vector2 CalculateIntersactionPoint(Line _FirstLine, Line _SecondLine)
    {
        if (_FirstLine.Slope == _SecondLine.Slope)
        {
            throw new Exception("Lines must not be parallel");
        }

        float _X = (_SecondLine.Bias - _FirstLine.Bias) / (_FirstLine.Slope - _SecondLine.Slope);
        float _Y = (_SecondLine.Slope * _X) + _SecondLine.Bias;
        return new Vector2(_X, _Y);
    }

    public Vector2 CalculateIntersactionPoint(Line _OtherLine)
    {
        return CalculateIntersactionPoint(this, _OtherLine);
    }

    public float CalculateY(float _X)
    {
        return (Slope * _X) + Bias;
    }

    public float CalculateX(float _Y)
    {
        return (_Y - Bias) / Slope;
    }
}