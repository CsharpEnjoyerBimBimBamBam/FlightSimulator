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
    [SerializeField] private Scrollbar ProgressBar;
    [SerializeField] private TMP_Text LoadingPercent;
    [SerializeField] private GameObject Loadingscreen;

    private void Start() => StartCoroutine(LoadScene());

    private IEnumerator LoadScene()
    {
        AsyncOperation _Operation = SceneManager.LoadSceneAsync("Earth");
        while (!_Operation.isDone) 
        {
            ProgressBar.value = _Operation.progress / 0.9f;
            LoadingPercent.text = (_Operation.progress / 0.9f * 100).ToString() + "%";
            yield return null;
        }
    }
}
