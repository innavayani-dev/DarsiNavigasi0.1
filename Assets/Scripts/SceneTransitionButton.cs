using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SceneTransitionButton : MonoBehaviour
{
    public string targetSceneName;

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() => {
                if (!string.IsNullOrEmpty(targetSceneName))
                {
                    SceneManager.LoadScene(targetSceneName);
                }
                else
                {
                    Debug.LogWarning("Target scene name is empty on " + gameObject.name);
                }
            });
        }
    }
}