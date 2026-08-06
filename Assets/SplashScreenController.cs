using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashScreenController : MonoBehaviour
{
    public float delayTime = 5f;
    public string nextScene = "1_Login";
    public Image fadeOverlay;
    public float fadeDuration = 0.3f;

    void Awake()
    {
        if (fadeOverlay != null) fadeOverlay.color = new Color(0, 0, 0, 1); // Start black for fade in immediately
    }

    void Start()
    {
        StartCoroutine(LoadingToLogin());
    }

    IEnumerator LoadingToLogin()
    {
        if (fadeOverlay != null)
        {
            // Fade In
            float timer = fadeDuration;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                fadeOverlay.color = new Color(0, 0, 0, timer / fadeDuration);
                yield return null;
            }
            fadeOverlay.color = new Color(0, 0, 0, 0);
        }

        yield return new WaitForSeconds(delayTime);

        if (fadeOverlay != null)
        {
            // Fade Out
            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                fadeOverlay.color = new Color(0, 0, 0, timer / fadeDuration);
                yield return null;
            }
            fadeOverlay.color = new Color(0, 0, 0, 1);
            yield return new WaitForSeconds(0.1f); // Brief pause to ensure black screen is seen
        }

        SceneManager.LoadScene(nextScene);
    }
}