using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseSwithcer : MonoBehaviour
{
    public static event Action OnGamePaused, OnGameUnpaused;
    public static bool IsGamePaused { get; private set; } = false;

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (IsGamePaused)
        {
            IsGamePaused = false;
            OnGameUnpaused.Invoke();
            return;
        }

        IsGamePaused = true;
        OnGamePaused.Invoke();
    }
}
