public class Runway : IReadOnlyRunway
{
    public Runway(GeoPosition _Coordinates, float _MagneticCourse, string _Name, float _Length, float _Elevation)
    {
        Coordinates = _Coordinates;
        Heading = _MagneticCourse;
        Name = _Name;
        Length = _Length;
        Elevation = _Elevation;
    }

    public Runway()
    {

    }

    public GeoPosition Coordinates { get; set; }
    public float Heading { get; set; }
    public string Name { get; set; }
    public float Length { get; set; }
    public float Elevation { get; set; }
}

public interface IReadOnlyRunway
{
    public GeoPosition Coordinates { get; }
    public float Heading { get; }
    public string Name { get; }
    public float Length { get; }
    public float Elevation { get; }
}
