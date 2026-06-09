using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFadeManager : MonoBehaviour
{
    public Image fadeOverlay;
    public float fadeDuration = 1.0f;
    public bool fadeInOnStart = true;

    void Awake()
    {
        if (fadeInOnStart && fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.color = Color.black;
        }
    }

    void Start()
    {
        if (fadeInOnStart && fadeOverlay != null)
        {
            StartCoroutine(DoFadeIn());
        }
    }

    public IEnumerator DoFadeIn()
    {
        float timer = fadeDuration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            fadeOverlay.color = new Color(0, 0, 0, Mathf.Clamp01(timer / fadeDuration));
            yield return null;
        }
        fadeOverlay.color = new Color(0, 0, 0, 0);
        fadeOverlay.gameObject.SetActive(false); // Hide to prevent blocking clicks
    }

    public IEnumerator DoFadeOut()
    {
        fadeOverlay.gameObject.SetActive(true);
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeOverlay.color = new Color(0, 0, 0, Mathf.Clamp01(timer / fadeDuration));
            yield return null;
        }
        fadeOverlay.color = Color.black;
    }
}
