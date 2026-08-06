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
        StartCoroutine(FadeAndLoad("1_Login"));
    }

    public void GoToRegister()
    {
        StartCoroutine(FadeAndLoad("2_regis"));
    }

    public void GoToScanner()
    {
        PlayerPrefs.SetString("PrevScene", SceneManager.GetActiveScene().name);
        StartCoroutine(FadeAndLoad("4_Scan Screen"));
    }

    public void GoToNavList()
    {
        StartCoroutine(FadeAndLoad("3_halaman utama"));
    }

    public void GoBack()
    {
        string prevScene = PlayerPrefs.GetString("PrevScene", "1_Login");
        StartCoroutine(FadeAndLoad(prevScene));
    }

    public void GoBackToNavList()
    {
        StartCoroutine(FadeAndLoad("3_halaman utama"));
    }

    public void GoToHalamanUtama()
    {
        StartCoroutine(FadeAndLoad("3_halaman utama"));
    }

    public void GoToARNavigasi()
    {
        StartCoroutine(FadeAndLoad("6_AR Navigasi"));
    }

    public void GoToTentangAplikasi()
    {
        StartCoroutine(FadeAndLoad("5_Tentang Aplikasi"));
    }
}
