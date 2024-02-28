using UnityEngine;

public static class Extensions 
{
    public static Vector2 ToVector2(this Vector3 _Vector)
    {
        return new Vector2(_Vector.x, _Vector.z);
    }

    public static Vector3 ToVector3(this Vector2 _Vector)
    {
        return new Vector3(_Vector.x, 0, _Vector.y);
    }

    public static float GenerateRandomFloat(this System.Random _Random, float _AbsoluteMaxValue)
    {
        float _RandomFloat = (float)_Random.NextDouble() * _AbsoluteMaxValue;
        if (_Random.Next(0, 2) == 1)
        {
            _RandomFloat *= -1;
        }
        return _RandomFloat;
    }
}
