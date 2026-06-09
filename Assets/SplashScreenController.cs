using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashScreenController : MonoBehaviour
{
    public float delayTime = 3f;
    public string nextScene = "0_Login";
    public Image fadeOverlay;
    public float fadeDuration = 1.0f;

    void Start()
    {
        if (fadeOverlay != null) fadeOverlay.color = new Color(0, 0, 0, 0);
        StartCoroutine(LoadingToLogin());
    }

    IEnumerator LoadingToLogin()
    {
        yield return new WaitForSeconds(delayTime);

        if (fadeOverlay != null)
        {
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