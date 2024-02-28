using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] public Scrollbar ProgressBar;
    [SerializeField] public TMP_Text LoadingPercent;
    [SerializeField] public GameObject Loadingscreen;

    private void Start()
    {
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        AsyncOperation _Operation = SceneManager.LoadSceneAsync("Default");
        while (!_Operation.isDone) 
        {
            ProgressBar.value = _Operation.progress / 0.9f;
            LoadingPercent.text = (_Operation.progress / 0.9f * 100).ToString() + "%";
            yield return null;
        }
    }
}
