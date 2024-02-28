using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDataParameters
{
    public MeshDataParameters(int _VerticesCount, int _TrianglesCount)
    {
        VerticesCount = _VerticesCount;
        TrianglesCount = _TrianglesCount;
    }

    public MeshDataParameters()
    {
        
    }

    public int VerticesCount;
    public int TrianglesCount;
}
