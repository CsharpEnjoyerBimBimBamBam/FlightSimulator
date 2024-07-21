using System;
using UnityEngine;

public struct GeoPosition
{
    public GeoPosition(float Latitude, float Longitude)
    {
        _Latitude = new Lat();
        _Longitude = new Lng();
        SetCoordinates((decimal)Latitude, (decimal)Longitude);
    }

    public GeoPosition(decimal Latitude, decimal Longitude)
    {
        _Latitude = new Lat();
        _Longitude = new Lng();
        SetCoordinates(Latitude, Longitude);
    }

    public ICoordinates Latitude => _Latitude;
    public ICoordinates Longitude => _Longitude;
    public bool InNorthernHemisphere => _Latitude.InNorthernHemisphere;
    public bool InWesternHemisphere => _Longitude.InWesternHemisphere;
    private Lat _Latitude;
    private Lng _Longitude;

    public static GeoPosition operator + (GeoPosition _Left, GeoPosition _Right)
    {
        decimal _RightLatAbs = _Right._Latitude.ExactDecimalDegrees;
        decimal _RightLngAbs = _Right._Longitude.ExactDecimalDegrees;
        if (_RightLatAbs < 0)
            _RightLatAbs = -_RightLatAbs;
        if (_RightLngAbs < 0)
            _RightLngAbs = -_RightLngAbs;
        decimal _Latitude = _Left._Latitude.ExactDecimalDegrees + _RightLatAbs;
        decimal _Longitude = _Left._Longitude.ExactDecimalDegrees + _RightLngAbs;

        if (_Latitude > 90)
        {
            _Latitude = 90 - _RightLatAbs + (90 - (decimal)Mathf.Abs(_Left._Latitude.DecimalDegrees));
        }

        if (_Longitude > 180)
        {
            _Longitude = -180 + _RightLngAbs - (180 - (decimal)Mathf.Abs(_Left._Longitude.DecimalDegrees));
        }

        return new GeoPosition(_Latitude, _Longitude);
    }

    public static GeoPosition operator - (GeoPosition _Left, GeoPosition _Right)
    {
        decimal _RightLatAbs = _Right._Latitude.ExactDecimalDegrees;
        decimal _RightLngAbs = _Right._Longitude.ExactDecimalDegrees;
        if (_RightLatAbs < 0)
            _RightLatAbs = -_RightLatAbs;
        if (_RightLngAbs < 0)
            _RightLngAbs = -_RightLngAbs;
        decimal _Latitude = _Left._Latitude.ExactDecimalDegrees - _RightLatAbs;
        decimal _Longitude = _Left._Longitude.ExactDecimalDegrees - _RightLngAbs;

        if (_Latitude < -90)
        {
            _Latitude = -90 + _RightLatAbs - (90 - (decimal)Mathf.Abs(_Left._Latitude.DecimalDegrees));
        }

        if (_Longitude < -180)
        {
            _Longitude = 180 - _RightLngAbs + (180 - (decimal)Mathf.Abs(_Left._Longitude.DecimalDegrees));
        }

        return new GeoPosition(_Latitude, _Longitude);
    }

    public static GeoPosition operator + (GeoPosition _Left, float _Right) => _Left + new GeoPosition((decimal)_Right, (decimal)_Right);

    public static GeoPosition operator - (GeoPosition _Left, float _Right) => _Left - new GeoPosition((decimal)_Right, (decimal)_Right);

    public static GeoPosition operator + (GeoPosition _Left, decimal _Right) => _Left + new GeoPosition(_Right, _Right);

    public static GeoPosition operator - (GeoPosition _Left, decimal _Right) => _Left - new GeoPosition(_Right, _Right);

    public static float Distance(GeoPosition From, GeoPosition To)
    {
        decimal _FirstLatNormalized = From._Latitude.ExactDecimalDegrees + 90;
        decimal _FirstLngNormalized = From._Longitude.ExactDecimalDegrees + 180;
        decimal _SecondLatNormalized = To._Latitude.ExactDecimalDegrees + 90;
        decimal _SecondLngNormalized = To._Longitude.ExactDecimalDegrees + 180;
        decimal _FirstLeg = (decimal)Mathf.Abs((float)(_SecondLngNormalized - _FirstLngNormalized));
        decimal _SecondLeg = (decimal)Mathf.Abs((float)(_SecondLatNormalized - _FirstLatNormalized));
        return Mathf.Sqrt((float)((_FirstLeg * _FirstLeg) + (_SecondLeg * _SecondLeg)));
    }

    public static GeoPosition FromEarthLocalPosition(Vector3 _Position)
    {
        float _Latitude = 90 - Vector3.Angle(_Position, Vector3.up);
        Vector3 _UpProject = Vector3.ProjectOnPlane(_Position, Vector3.up);
        float _Longitude = Vector3.Angle(_UpProject, Vector3.right);
        float _RigidbodyForwardAngle = Vector3.Angle(_UpProject, Vector3.forward);
        if (_RigidbodyForwardAngle > 90)
            _Longitude *= -1;
        return new GeoPosition(_Latitude, _Longitude);
    }

    public static GeoPosition FromWorldPosition(Vector3 _Position)
    {
        Vector3 _EarthLocalPosition = TerrainGenerator.Earth.transform.InverseTransformPoint(_Position);
        return FromEarthLocalPosition(_EarthLocalPosition);
    }

    public static GeoPosition FromWorldPosition(float _X, float _Y, float _Z) => FromWorldPosition(new Vector3(_X, _Y, _Z));

    public Vector3 ToEarthLocalPosition(float _Altitude = 0f)
    {
        float _LatitudeRadians = (float)_Latitude.ExactDecimalDegrees * Mathf.Deg2Rad;
        float _LongitudeRadians = (float)_Longitude.ExactDecimalDegrees * Mathf.Deg2Rad;
        float _FullRadius = Constants.EarthRadius + _Altitude;
        float _Hypotinuse = Mathf.Abs(Mathf.Cos(_LatitudeRadians)) * _FullRadius;
        float _X = Mathf.Cos(_LongitudeRadians) * _Hypotinuse;
        float _Y = Mathf.Sin(_LatitudeRadians) * _FullRadius;
        float _Z = Mathf.Sin(_LongitudeRadians) * _Hypotinuse;
        return new Vector3(_X, _Y, _Z);
    }

    public Vector3 ToWorldPosition(float _Altitude = 0) => TerrainGenerator.Earth.transform.TransformPoint(ToEarthLocalPosition(_Altitude));

    public static float AngularDegreesToMeters(float _AngularDegrees, float Latitude = 0) => _AngularDegrees / 360 * CalculateCircumference(Latitude);

    public static float MetersToAngularDegrees(float _Meters, float Latitude = 0) => _Meters / CalculateCircumference(Latitude) * 360;

    public override string ToString() => $"{_Latitude.DecimalDegrees};{_Longitude.DecimalDegrees}";

    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is GeoPosition))
            return false;

        GeoPosition Coordinates = (GeoPosition)obj;
        return (Coordinates._Latitude.ExactDecimalDegrees == _Latitude.ExactDecimalDegrees) && 
            (Coordinates._Longitude.ExactDecimalDegrees == _Longitude.ExactDecimalDegrees);
    }

    public override int GetHashCode() => base.GetHashCode();

    private static float CalculateCircumference(float Latitude)
    {
        float _Radius = Mathf.Abs(Mathf.Cos(Latitude * Mathf.Deg2Rad)) * Constants.EarthRadius;
        return 2f * Mathf.PI * _Radius;
    }

    private void SetCoordinates(decimal Latitude, decimal Longitude)
    {
        if (Math.Abs(Latitude) > 90)
            throw new ArgumentOutOfRangeException(nameof(Latitude), "Latitude must be greater then -90 and less then 90");

        if (Math.Abs(Longitude) > 180)
            throw new ArgumentOutOfRangeException(nameof(Longitude), "Longitude must be greater then -180 and less then 180");

        _Latitude.ExactDecimalDegrees = Latitude;
        _Longitude.ExactDecimalDegrees = Longitude;
        _Latitude.DecimalDegrees = (float)Latitude;
        _Longitude.DecimalDegrees = (float)Longitude;
    }

    public struct Lat : ICoordinates
    {
        public decimal ExactDecimalDegrees { get; set; }
        public float DecimalDegrees { get; set; }
        public bool InNorthernHemisphere => DecimalDegrees >= 0;
        public string Hemisphere => InNorthernHemisphere ? "N" : "S";
    }

    public struct Lng : ICoordinates
    {
        public decimal ExactDecimalDegrees { get; set; }
        public float DecimalDegrees { get; set; }
        public bool InWesternHemisphere => DecimalDegrees <= 0;
        public string Hemisphere => InWesternHemisphere ? "W" : "E";
    }

    public interface ICoordinates
    {
        public decimal ExactDecimalDegrees { get; }
        public float DecimalDegrees { get; }
        public float AbsDecimalDegrees => Mathf.Abs(DecimalDegrees);
        public int Degrees => Mathf.FloorToInt(AbsDecimalDegrees);
        public float DecimalMinutes => (AbsDecimalDegrees - Mathf.FloorToInt(AbsDecimalDegrees)) * 60;
        public int Minutes => Mathf.FloorToInt(DecimalMinutes);
        public int Seconds
        {
            get
            {
                float _Minutes = (AbsDecimalDegrees - Mathf.FloorToInt(AbsDecimalDegrees)) * 60;
                return Mathf.FloorToInt((_Minutes - Mathf.FloorToInt(_Minutes)) * 60);
            }
        }
        public string Hemisphere { get; }

        public string ToDMS() => $"{Degrees}° {Minutes}' {Seconds}\"{Hemisphere}";

        public string ToDDM() => $"{Degrees}° {Math.Round((double)DecimalMinutes, 3)}\"{Hemisphere}";
    }
}
