using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFlowManager : MonoBehaviour
{
    private IEnumerator FadeAndLoad(string sceneName)
    {
        SceneFadeManager fadeManager = FindObjectOfType<SceneFadeManager>();
        if (fadeManager != null)
        {
            yield return fadeManager.DoFadeOut();
        }
        SceneManager.LoadScene(sceneName);
    }

    public void GoToLogin()
    {
        StartCoroutine(FadeAndLoad("0_Login"));
    }

    public void GoToRegister()
    {
        StartCoroutine(FadeAndLoad("1_Register"));
    }

    public void GoToScanner()
    {
        PlayerPrefs.SetString("PrevScene", SceneManager.GetActiveScene().name);
        StartCoroutine(FadeAndLoad("2_ScannerCamera"));
    }

    public void GoToNavList()
    {
        StartCoroutine(FadeAndLoad("3_NavList"));
    }

    public void GoBack()
    {
        string prevScene = PlayerPrefs.GetString("PrevScene", "0_Login");
        StartCoroutine(FadeAndLoad(prevScene));
    }

    public void GoBackToNavList()
    {
        StartCoroutine(FadeAndLoad("3_NavList"));
    }
}
