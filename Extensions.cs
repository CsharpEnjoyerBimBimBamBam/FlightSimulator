using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions 
{
    private static Dictionary<Rigidbody, RigidbodyParameters> _RigidbodiesParameters = new Dictionary<Rigidbody, RigidbodyParameters>();

    public static Vector2 ToVector2(this Vector3 _Vector) => new Vector2(_Vector.x, _Vector.z);

    public static Vector3 ToVector3(this Vector2 _Vector) => new Vector3(_Vector.x, 0, _Vector.y);

    public static float GenerateRandomFloat(this System.Random _Random, float _AbsoluteMaxValue)
    {
        float _RandomFloat = (float)_Random.NextDouble() * _AbsoluteMaxValue;
        if (_Random.Next(0, 2) == 1)
        {
            _RandomFloat *= -1;
        }
        return _RandomFloat;
    }

    public static void Pause(this Rigidbody _Rigidbody)
    {
        if (_RigidbodiesParameters.ContainsKey(_Rigidbody))
            return;

        _RigidbodiesParameters[_Rigidbody] = new RigidbodyParameters(_Rigidbody.velocity, _Rigidbody.angularVelocity);
        _Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }

    public static void Unpause(this Rigidbody _Rigidbody)
    {
        if (!_RigidbodiesParameters.ContainsKey(_Rigidbody))
            return;

        RigidbodyParameters Parameters = _RigidbodiesParameters[_Rigidbody];
        _Rigidbody.constraints = RigidbodyConstraints.None;
        _Rigidbody.velocity = Parameters.Velocity;
        _Rigidbody.angularVelocity = Parameters.AngularVelocity;

        _RigidbodiesParameters.Remove(_Rigidbody);
    }

    private struct RigidbodyParameters
    {
        public RigidbodyParameters(Vector3 _Velocity, Vector3 _AngularVelocity)
        {
            Velocity = _Velocity;
            AngularVelocity = _AngularVelocity;
        }

        public Vector3 Velocity { get; private set; }
        public Vector3 AngularVelocity { get; private set; }
    }
}
