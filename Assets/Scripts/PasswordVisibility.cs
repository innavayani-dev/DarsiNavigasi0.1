using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PasswordVisibility : MonoBehaviour
{
    public TMP_InputField passwordField;
    public Button toggleButton;
    public Image toggleIconImage;
    public Sprite eyeOpenSprite;
    public Sprite eyeClosedSprite;

    private bool isVisible = false;

    void Start()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleVisibility);
        }
        UpdateUI();
    }

    public void ToggleVisibility()
    {
        isVisible = !isVisible;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (passwordField != null)
        {
            passwordField.contentType = isVisible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
            passwordField.ForceLabelUpdate();
        }

        if (toggleIconImage != null)
        {
            toggleIconImage.sprite = isVisible ? eyeOpenSprite : eyeClosedSprite;
        }
    }
}
