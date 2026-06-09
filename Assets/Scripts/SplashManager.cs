using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashManager : MonoBehaviour
{
    public Image fadeOverlay;
    public float fadeDuration = 1.0f;
    public float waitTime = 2.0f;
    public string nextSceneName = "0_Login";

    void Start()
    {
        if (fadeOverlay != null)
        {
            StartCoroutine(DoSplash());
        }
    }

    IEnumerator DoSplash()
    {
        // Start visible (no Fade In as requested)
        if (fadeOverlay != null) fadeOverlay.color = new Color(0, 0, 0, 0);

        // 2. Wait
        yield return new WaitForSeconds(waitTime);

        // 3. Fade Out (Transparent to Black)
        yield return StartCoroutine(Fade(0, 1));

        // 4. Load Next Scene
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            fadeOverlay.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeOverlay.color = new Color(0, 0, 0, endAlpha);
    }
}
