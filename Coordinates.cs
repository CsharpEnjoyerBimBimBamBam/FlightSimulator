using System;
using Unity.VisualScripting;
using UnityEngine;

public struct Coordinates
{
    public Coordinates(float _Latitude, float _Longitude)
    {
        Latitude = new LatLng();
        Longitude = new LatLng();
        SetCoordinates((decimal)_Latitude, (decimal)_Longitude);
    }

    public Coordinates(decimal _Latitude, decimal _Longitude)
    {
        Latitude = new LatLng();
        Longitude = new LatLng();
        SetCoordinates(_Latitude, _Longitude);
    }

    public LatLng Latitude;
    public LatLng Longitude;
    public bool InNorthernHemisphere { get { if (Latitude.DecimalDegrees > 0) return true; return false; } }
    public bool InWesternHemisphere { get { if (Longitude.DecimalDegrees < 0) return true; return false; } }

    public static Coordinates operator + (Coordinates _Left, Coordinates _Right)
    {
        decimal _RightLatAbs = _Right.Latitude.ExactDecimalDegrees;
        decimal _RightLngAbs = _Right.Longitude.ExactDecimalDegrees;
        if (_RightLatAbs < 0)
            _RightLatAbs = -_RightLatAbs;
        if (_RightLngAbs < 0)
            _RightLngAbs = -_RightLngAbs;
        decimal _Latitude = _Left.Latitude.ExactDecimalDegrees + _RightLatAbs;
        decimal _Longitude = _Left.Longitude.ExactDecimalDegrees + _RightLngAbs;

        if (_Latitude > 90)
        {
            _Latitude = 90 - _RightLatAbs + (90 - (decimal)Mathf.Abs(_Left.Latitude.DecimalDegrees));
        }

        if (_Longitude > 180)
        {
            _Longitude = -180 + _RightLngAbs - (180 - (decimal)Mathf.Abs(_Left.Longitude.DecimalDegrees));
        }

        return new Coordinates(_Latitude, _Longitude);
    }

    public static Coordinates operator - (Coordinates _Left, Coordinates _Right)
    {
        decimal _RightLatAbs = _Right.Latitude.ExactDecimalDegrees;
        decimal _RightLngAbs = _Right.Longitude.ExactDecimalDegrees;
        if (_RightLatAbs < 0)
            _RightLatAbs = -_RightLatAbs;
        if (_RightLngAbs < 0)
            _RightLngAbs = -_RightLngAbs;
        decimal _Latitude = _Left.Latitude.ExactDecimalDegrees - _RightLatAbs;
        decimal _Longitude = _Left.Longitude.ExactDecimalDegrees - _RightLngAbs;

        if (_Latitude < -90)
        {
            _Latitude = -90 + _RightLatAbs - (90 - (decimal)Mathf.Abs(_Left.Latitude.DecimalDegrees));
        }

        if (_Longitude < -180)
        {
            _Longitude = 180 - _RightLngAbs + (180 - (decimal)Mathf.Abs(_Left.Longitude.DecimalDegrees));
        }

        return new Coordinates(_Latitude, _Longitude);
    }

    public static Coordinates operator + (Coordinates _Left, float _Right)
    {
        return _Left + new Coordinates((decimal)_Right, (decimal)_Right);
    }

    public static Coordinates operator - (Coordinates _Left, float _Right)
    {
        return _Left - new Coordinates((decimal)_Right, (decimal)_Right);
    }

    public static float Distance(Coordinates _FirstCoordinates, Coordinates _SecondCoordinates)
    {
        decimal _FirstLatNormalized = _FirstCoordinates.Latitude.ExactDecimalDegrees + 90;
        decimal _FirstLngNormalized = _FirstCoordinates.Longitude.ExactDecimalDegrees + 180;
        decimal _SecondLatNormalized = _SecondCoordinates.Latitude.ExactDecimalDegrees + 90;
        decimal _SecondLngNormalized = _SecondCoordinates.Longitude.ExactDecimalDegrees + 180;
        decimal _FirstLeg = (decimal)Mathf.Abs((float)(_SecondLngNormalized - _FirstLngNormalized));
        decimal _SecondLeg = (decimal)Mathf.Abs((float)(_SecondLatNormalized - _FirstLatNormalized));
        return Mathf.Sqrt((float)((_FirstLeg * _FirstLeg) + (_SecondLeg * _SecondLeg)));
    }

    public static Coordinates FromWorldPosition(Vector3 _Position)
    {
        Vector3 _EarthPositionLocal = TerrainGenerator.Earth.transform.InverseTransformPoint(_Position);
        float _Latitude = 90 - Vector3.Angle(_EarthPositionLocal, Vector3.up);
        Vector3 _UpProjection = Vector3.ProjectOnPlane(_EarthPositionLocal, Vector3.up);
        float _Longitude = Vector3.Angle(_UpProjection, Vector3.right);
        float _RigidbodyForwardAngle = Vector3.Angle(_UpProjection, Vector3.forward);
        if (_RigidbodyForwardAngle > 90)
            _Longitude *= -1;
        return new Coordinates(_Latitude, _Longitude);
    }

    public static Coordinates FromWorldPosition(float _X, float _Y, float _Z)
    {
        return FromWorldPosition(new Vector3(_X, _Y, _Z));
    }

    public Vector3 ToEarthLocalPosition(float _Altitude = 0f)
    {
        decimal _LatitudeRadians = Latitude.ExactDecimalDegrees * (decimal)Mathf.Deg2Rad;
        decimal _LongitudeRadians = Longitude.ExactDecimalDegrees * (decimal)Mathf.Deg2Rad;
        decimal _FullRadius = (decimal)Constants.EarthRadius + (decimal)_Altitude;
        decimal _Hypotinuse = (decimal)Mathf.Abs(Mathf.Cos((float)_LatitudeRadians)) * _FullRadius;
        float _X = Mathf.Cos((float)_LongitudeRadians) * (float)_Hypotinuse;
        float _Y = Mathf.Sin((float)_LatitudeRadians) * (float)_FullRadius;
        float _Z = Mathf.Sin((float)_LongitudeRadians) * (float)_Hypotinuse;
        return new Vector3(_X, _Y, _Z);
    }

    public Vector3 ToWorldPosition(float _Altitude = 0)
    {
        return TerrainGenerator.Earth.transform.TransformPoint(ToEarthLocalPosition(_Altitude));
    }

    public static float AngularDegreesToMeters(float _AngularDegrees, float _Latitude = 0)
    {
        float _Radius = Mathf.Abs(Mathf.Cos(_Latitude * Mathf.Deg2Rad)) * Constants.EarthRadius;
        float _Circumference = 2f * Mathf.PI * _Radius;
        return _AngularDegrees / 360 * _Circumference;
    }

    public static float MetersToAngularDegrees(float _Meters, float _Latitude = 0)
    {
        float _Radius = Mathf.Abs(Mathf.Cos(_Latitude * Mathf.Deg2Rad)) * Constants.EarthRadius;
        float _Circumference = 2f * Mathf.PI * _Radius;
        return _Meters / _Circumference * 360;
    }

    public override string ToString()
    {
        return $"{Latitude.DecimalDegrees};{Longitude.DecimalDegrees}";
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is Coordinates))
        {
            return false;
        }
        Coordinates _Coordinates = (Coordinates)obj;
        return (_Coordinates.Latitude.ExactDecimalDegrees == Latitude.ExactDecimalDegrees) && 
            (_Coordinates.Longitude.ExactDecimalDegrees == Longitude.ExactDecimalDegrees);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    private void SetCoordinates(decimal _Latitude, decimal _Longitude)
    {
        Latitude.ExactDecimalDegrees = _Latitude;
        Longitude.ExactDecimalDegrees = _Longitude;
        Latitude.DecimalDegrees = (float)_Latitude;
        Longitude.DecimalDegrees = (float)_Longitude;
    }

    public struct LatLng
    {
        public decimal ExactDecimalDegrees;
        public float DecimalDegrees;
        public float AbsDecimalDegrees { get { return Mathf.Abs(DecimalDegrees); } }
        public int Degrees { get { return Mathf.FloorToInt(AbsDecimalDegrees); } }
        public int Minutes { get { return Mathf.FloorToInt((AbsDecimalDegrees - Mathf.FloorToInt(AbsDecimalDegrees)) * 60); } }
        public int Seconds 
        {
            get 
            {
                float _Minutes = (AbsDecimalDegrees - Mathf.FloorToInt(AbsDecimalDegrees)) * 60;
                return Mathf.FloorToInt((_Minutes - Mathf.FloorToInt(_Minutes)) * 60);
            } 
        }

        public string ToDMS()
        {
            return $"{Degrees}° {Minutes}' {Seconds}\"";
        }

        public string ToDDM()
        {
            return $"{Degrees}° {Math.Round((double)Minutes, 3)}\"";
        }
    }
}
