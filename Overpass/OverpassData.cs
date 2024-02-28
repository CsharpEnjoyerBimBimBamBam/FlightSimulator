using System;
using System.Collections.Generic;

public class OverpassData<ElementType, TagType> where ElementType : OverpassElement<TagType> where TagType : IOverpassTag
{
    public List<ElementType> elements;
}

public interface IOverpassTag
{
    public void SetDefaultValues();
}
