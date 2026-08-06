using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeOutOnStart : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1.0f;
    public float startDelay = 0.5f;

    void Start()
    {
        if (fadeImage == null)
            fadeImage = GetComponent<Image>();
            
        if (fadeImage != null)
        {
            // Ensure image is fully black and blocks clicks at start
            fadeImage.color = Color.black;
            fadeImage.raycastTarget = true;
            
            StartCoroutine(FadeOutRoutine());
        }
    }

    IEnumerator FadeOutRoutine()
    {
        yield return new WaitForSeconds(startDelay);
        
        float time = 0;
        Color startColor = fadeImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeImage.color = Color.Lerp(startColor, endColor, time / fadeDuration);
            yield return null;
        }

        // Disable object completely to save resources
        gameObject.SetActive(false);
    }
}
