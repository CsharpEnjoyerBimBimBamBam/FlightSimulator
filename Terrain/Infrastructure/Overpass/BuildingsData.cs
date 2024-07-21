using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using static BuildingsData;

public class BuildingsData : OverpassData<BuildingElement, BuildingTags>
{
    public static int BaseBuildingHeight = 30;
    public static int BaseBuildingLevels = 5;
    public static RoofShape BaseRoofShape = RoofShape.flat;

    [Serializable]
    public class BuildingTags : IOverpassTag
    {
        public BuildingTags()
        {
            SetDefaultValues();
        }
        [JsonProperty("building:levels")] public int building_levels { get; set; }
        [JsonProperty("roof:shape")] public RoofShape roof_shape { get; set; }
        [JsonProperty("height")] private string _height { get; set; }
        public int Height 
        {
            get 
            {
                if (int.TryParse(_height, out int _Height))
                    return _Height;
                return BaseBuildingHeight;
            }
        }

        public void SetDefaultValues()
        {
            building_levels = BaseBuildingLevels;
            roof_shape = BaseRoofShape;
        }
    }

    [Serializable]
    public class BuildingElement : OverpassElement<BuildingTags>
    {
        public List<OverpassGeometry> geometry = new List<OverpassGeometry>();
    }

    [Serializable]
    public enum RoofShape
    {
        [EnumMember(Value = "half-hipped")] half_hipped,
        [EnumMember(Value = "hipped-and-gabled")] hipped_and_gabled,
        [EnumMember(Value = "side_half-hipped")] side_half_hipped,
        [EnumMember(Value = "skillion;hipped")] skillion_hipped,
        gabled,
        flat,
        hipped,
        pyramidal,
        skillion,
        butterfly,
        round,   
        gambrel,
        mansard, 
        dome,    
        onion,   
        sawtooth,
        cone,    
        crosspitched,    
        side_hipped,  
        gabled_height_moved,
        hitched,
        double_saltbox,
        saltbox,
        triple_saltbox,
        quadruple_saltbox,
        conical,
        lean_to,
        shed,
        gabled_row,
        many,
        Mix,
        mix
    }
}
