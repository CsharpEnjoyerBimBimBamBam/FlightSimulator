
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IPausable
{
    public static bool IsGamePaused = false;
    public static UnityEvent OnGamePaused = new UnityEvent();
    public static UnityEvent OnGameUnpaused = new UnityEvent();
    public void Pause();
    public void Unpause();
}
