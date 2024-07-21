using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditorInternal;
using System.IO;
using UnityEngine;
using UnityEngine.Windows;
using System.Globalization;

public class Airac : IReadOnlyAirac
{
    private Airac()
    {

    }

    public IReadOnlyList<IReadOnlyAirport> Airports => _Airports;
    public DistanceUnits Units { get; set; }
    private List<IReadOnlyAirport> _Airports = new List<IReadOnlyAirport>();
    private static Airac? _Instance = null;

    public static IReadOnlyAirac GetInstance()
    {
        if (_Instance != null)
            return _Instance;

        DirectoryInfo Info = new DirectoryInfo(AbsolutePath.Airac);

        if (!Info.Exists)
            throw new FileNotFoundException("Airac path not found");

        AiracParser Parser = GetParser(Info);

        if (!Parser.ValidateFiles())
            throw new Exception("Airac files is incorrect");

        _Instance = Parser.Parse();

        return _Instance;
    }

    private static AiracParser GetParser(DirectoryInfo Info)
    {
        return new AerobaskSkyViewParser(Info);
    }

    private abstract class AiracParser
    {
        public AiracParser(DirectoryInfo Info) => _Info = Info;

        protected DirectoryInfo _Info;
        protected RequiredDirectoryFiles _RequiredDirectoryFiles;

        public abstract Airac Parse();
        public bool ValidateFiles() => _RequiredDirectoryFiles.CheckForRequiredFiles(_Info);
    }

    private class AerobaskSkyViewParser : AiracParser
    {
        public AerobaskSkyViewParser(DirectoryInfo Info) : base(Info)
        {
            _RequiredDirectoryFiles = new RequiredDirectoryFiles(new List<string> 
            {
                "Airports.txt",
                "ATS.txt",
                "Navaids.txt",
                "Waypoints.txt"
            }, 
            new Dictionary<string, RequiredDirectoryFiles> 
            {
                { "Proc", new RequiredDirectoryFiles(null, null) }
            });
        }

        private FileInfo[] _Files;

        public override Airac Parse()
        {
            _Files = _Info.GetFiles();
            return new Airac 
            { 
                _Airports = ParseAirports(), 
                Units = DistanceUnits.Feet 
            };
        }

        private List<IReadOnlyAirport> ParseAirports()
        {
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";

            string AirportsData = _Files.FirstOrDefault(X => X.Name == "Airports.txt").OpenText().ReadToEnd();
            string[] SplitedData = AirportsData.Split(Environment.NewLine + Environment.NewLine);
            
            List<IReadOnlyAirport> Airports = new List<IReadOnlyAirport>();
            for (int i = 1; i < SplitedData.Length; i++)
            {
                string[] CurrentData = SplitedData[i].Split(Environment.NewLine);
                string[] CurrentAirportData = CurrentData[0].Split(",");

                if (!float.TryParse(CurrentAirportData[3], NumberStyles.Any, ci, out float Latitude))
                    throw new Exception($"Can not parse airport latitude {CurrentAirportData[3]}");

                if (!float.TryParse(CurrentAirportData[4], NumberStyles.Any, ci, out float Longitude))
                    throw new Exception($"Can not parse airport longitude {CurrentAirportData[4]}");

                if (!float.TryParse(CurrentAirportData[5], NumberStyles.Any, ci, out float Elevation))
                    throw new Exception($"Can not parse airport elevation {CurrentAirportData[5]}");

                if (!float.TryParse(CurrentAirportData[6], NumberStyles.Any, ci, out float TransitionAltitude))
                    throw new Exception($"Can not parse transition altitude {CurrentAirportData[6]}");

                Airport CurrentAirport = new Airport
                {
                    ICAO = CurrentAirportData[1],
                    Name = CurrentAirportData[2],
                    Coordinates = new GeoPosition(Latitude, Longitude),
                    Elevation = Elevation,
                    TransitionAltitude = TransitionAltitude
                };

                List<Runway> AirportRunways = new List<Runway>();
                for (int j = 1; j < CurrentData.Length; j++)
                {
                    string[] RunwayData = CurrentData[j].Split(",");

                    if (RunwayData.Length < 10)
                        continue;

                    if (!float.TryParse(RunwayData[2], NumberStyles.Any, ci, out float MagneticCourse))
                        throw new Exception($"Can not parse magnetic course {RunwayData[2]}");

                    if (!float.TryParse(RunwayData[3], NumberStyles.Any, ci, out float Length))
                        throw new Exception($"Can not parse length {RunwayData[3]}");

                    if (!float.TryParse(RunwayData[8], NumberStyles.Any, ci, out float RunwayLatitude))
                        throw new Exception($"Can not parse runway latitude {RunwayData[8]}");

                    if (!float.TryParse(RunwayData[9], NumberStyles.Any, ci, out float RunwayLongitude))
                        throw new Exception($"Can not parse runway longitude {RunwayData[9]}");

                    Runway CurrentRunway = new Runway
                    {
                        Name = RunwayData[1],
                        Heading = MagneticCourse,
                        Length = Length,
                        Coordinates = new GeoPosition(RunwayLatitude, RunwayLongitude)
                    };
                    AirportRunways.Add(CurrentRunway);
                }

                CurrentAirport.Runways = AirportRunways;
                Airports.Add(CurrentAirport);
            }
            return Airports;
        }
    }
}

public interface IReadOnlyAirac
{
    public IReadOnlyList<IReadOnlyAirport> Airports { get; }
    public DistanceUnits Units { get; }
}
