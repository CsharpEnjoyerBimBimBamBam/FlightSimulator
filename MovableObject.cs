using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MovableObject : MonoBehaviour
{
    public static IReadOnlyList<MovableObject> MovableObjects => _MovableObjects;
    private static List<MovableObject> _MovableObjects = new List<MovableObject>();

    private void Awake() => _MovableObjects.Add(this);

    private void OnDestroy() => _MovableObjects.Remove(this);
}
