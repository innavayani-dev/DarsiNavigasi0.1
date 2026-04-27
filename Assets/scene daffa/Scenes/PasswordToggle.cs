using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Wajib dipanggil untuk TextMeshPro
using UnityEngine.UI;

public class PasswordToggle : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField passwordInput;
    public Image eyeIconImage; // Komponen Image dari ikon matamu

    [Header("Sprites")]
    public Sprite eyeClosedSprite; // Ikon mata dicoret
    public Sprite eyeOpenSprite;   // Ikon mata terbuka

    private bool isPasswordHidden = true;

    public void TogglePassword()
    {
        isPasswordHidden = !isPasswordHidden;

        if (isPasswordHidden)
        {
            // Ubah ke mode password (bintang/titik)
            passwordInput.contentType = TMP_InputField.ContentType.Password;
            if (eyeIconImage != null && eyeClosedSprite != null)
                eyeIconImage.sprite = eyeClosedSprite;
        }
        else
        {
            // Ubah ke mode teks biasa
            passwordInput.contentType = TMP_InputField.ContentType.Standard;
            if (eyeIconImage != null && eyeOpenSprite != null)
                eyeIconImage.sprite = eyeOpenSprite;
        }

        // PERINTAH WAJIB: Memaksa Unity me-refresh tampilan teks saat itu juga
        passwordInput.ForceLabelUpdate();
    }
}