using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;

public class OpenTopographyClient : WebServiceClient
{
    public OpenTopographyClient(List<string> _ApiKeys) : base(_ApiKeys)
    {
        
    }

    public OpenTopographyClient(List<string> _ApiKeys, List<WebProxy> _Proxys) : base(_ApiKeys, _Proxys)
    {
        
    }

    public DemType demType = DemType.SRTMGL1;
    public OutputFormat outputFormat = OutputFormat.AAIGrid;
    public IReadOnlyDictionary<DemType, int> DemTypePointDistance { get { return _DemTypePointDistance; } }
    public int HeightMapRowCount = 308;
    public int HeightMapColumnCount = 308;
    private Dictionary<DemType, int> _DemTypePointDistance;

    public async Task<HeightMapData> GetHeightMap(GeoPosition _SouthWestCorner, GeoPosition _NorthEastCorner, bool _Cut = true)
    {
        _UriBuilder.Query = $"demtype={demType}&API_Key={ApiKeys[CurrentApiKeyIndex]}&outputFormat={outputFormat}&" +
            $"south={_SouthWestCorner.Latitude.DecimalDegrees}&" +
            $"north={_NorthEastCorner.Latitude.DecimalDegrees}&" +
            $"west={_SouthWestCorner.Longitude.DecimalDegrees}&" +
            $"east={_NorthEastCorner.Longitude.DecimalDegrees}&";
        HttpResponseMessage _Response;
        try
        {
            _Response = await GetResponse();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e);
            return CreateZeroHeightMap(HeightMapRowCount, HeightMapColumnCount);
        }
        HeightMapData _HeightMapData;
        CurrentApiKeyIndex = NextCollectionIndex(ApiKeys, CurrentApiKeyIndex);

        Stopwatch _Timer = new Stopwatch();
        _Timer.Start();
        string _ResponseString = await _Response.Content.ReadAsStringAsync();
        try
        {
            _HeightMapData = ParseAAIGridResponse(ref _ResponseString);
        }
        catch
        {
            _HeightMapData = CreateZeroHeightMap(HeightMapRowCount, HeightMapColumnCount);
        }
        _Timer.Stop();
        UnityEngine.Debug.Log($"Open topography response parse time: {_Timer.ElapsedMilliseconds}");
        return _HeightMapData;
    }

    public static HeightMapData CreateZeroHeightMap(int _RowCount, int _ColumnCount)
    {
        short[][] _ZerosArray = new short[_RowCount][];
        for (int i = 0; i < _ZerosArray.Length; i++)
        {
            _ZerosArray[i] = new short[_ColumnCount];
        }
        return new HeightMapData
        {
            HeightMap = _ZerosArray,
            RowCount = _RowCount,
            ColumnCount = _ColumnCount,
            MaxHeight = 0
        };
    }

    protected override void Initialize()
    {
        _DemTypePointDistance = new Dictionary<DemType, int>
        {
            { DemType.SRTMGL3, 90},
            { DemType.SRTMGL1, 30},
            { DemType.SRTMGL1_E, 30},
            { DemType.AW3D30, 30},
            { DemType.AW3D30_E, 30},
            { DemType.COP30, 30},
            { DemType.COP90, 90},
            { DemType.EU_DTM, 30},
            { DemType.GEDI_L3, 1000}
        };
        _BaseUri = "https://portal.opentopography.org/API/globaldem";
        _UriBuilder = new UriBuilder(_BaseUri);
    }

    private unsafe HeightMapData ParseAAIGridResponse(ref string _Response)
    {
        string[] _Rows = _Response.Split("\n");
        short[][] _HeightMap = new short[_Rows.Length - 7][];
        int _FirstRowLength = _Rows[6].Split(" ").Length - 1;
        short _MaxHeight = 0;
        for (int i = 6; i < _Rows.Length - 1; i++)
        {
            string[] _CurrentRowString = _Rows[i].Trim().Split(" ");
            short[] _CurrentRow = new short[_FirstRowLength];
            for (int j = 0; j < _FirstRowLength; j++)
            {
                if (j > _CurrentRowString.Length - 1)
                {
                    _CurrentRow[j] = 0;
                }
                else
                {
                    short.TryParse(_CurrentRowString[j], out short _CurrentHeight);
                    _MaxHeight = (short)Mathf.Max(_MaxHeight, _CurrentHeight);
                    _CurrentRow[j] = _CurrentHeight;
                }
            }
            GCHandle.Alloc(_CurrentRowString).Free();
            _HeightMap[i - 6] = _CurrentRow;
        }

        HeightMapData Data = new HeightMapData
        {
            HeightMap = _HeightMap,
            RowCount = _HeightMap.Length,
            ColumnCount = _HeightMap[0].Length,
            MaxHeight = _MaxHeight
        };

        GCHandle.Alloc(_Rows).Free();
        return Data;
    }

    public enum DemType
    {
        SRTMGL3,
        SRTMGL1,
        SRTMGL1_E,
        AW3D30,
        AW3D30_E,
        COP30,
        COP90,
        EU_DTM,
        GEDI_L3
    }

    public enum OutputFormat
    {
        GTiff,
        AAIGrid,
        HFA
    }
}
