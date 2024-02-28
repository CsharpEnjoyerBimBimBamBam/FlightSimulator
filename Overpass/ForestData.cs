using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ForestData;

public class ForestData : OverpassData<ForestElement, ForestTags>
{
    public static string BaseLanduse = "cemetery";
    public static string BaseNatural = "wood";

    public class ForestTags : IOverpassTag
    {
        public string landuse;
        public string natural;

        public void SetDefaultValues()
        {
            landuse = BaseLanduse;
            natural = BaseNatural;
        }
    }

    public class ForestElement : OverpassElement<ForestTags>
    {
        public List<Member> members;
        public List<OverpassGeometry> geometry;
    }

    public class Member
    {
        public List<OverpassGeometry> geometry;
    }
}
