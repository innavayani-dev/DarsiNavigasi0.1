using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Text;

public class AuthManager : MonoBehaviour
{
    // Jalur langsung ke server Vercel Cloud lu (Tanpa perlu nyalain lokal lagi)
    private string baseUrl = "https://darsi-nav.hcm-lab.id/api";

    [Header("Input Fields (Login & Regis)")]
    public TMP_InputField emailField;
    public TMP_InputField passwordField;

    [Header("Input Fields (Khusus Regis)")]
    public TMP_InputField nameField; 
    public TMP_InputField phoneField; 
    public TMP_InputField bpjsField;  

    [Header("Lupa Password UI")]
    public GameObject forgotPasswordPanel;
    public GameObject phase1Panel; // Panel input email
    public GameObject phase2Panel; // Panel OTP dan Password Baru
    public TextMeshProUGUI successEmailText; // Teks "Kode telah dikirim ke aaaa"
    public TMP_InputField forgotEmailField;
    public TMP_InputField otpField;
    public TMP_InputField newPasswordField;

    [Header("UI Feedback")]
    public TextMeshProUGUI errorText; // Untuk Login & Regis
    public TextMeshProUGUI forgotPasswordErrorText; // Khusus untuk panel Lupa Password

    [Header("Scene Transition Options")]
    public float fadeDuration = 0.2f;

    private void Start()
    {
        if (errorText != null)
        {
            errorText.raycastTarget = false; // Mencegah teks error menutupi klik pada kolom input
            errorText.gameObject.SetActive(false);
        }

        if (forgotPasswordPanel != null)
        {
            forgotPasswordPanel.SetActive(false); // Sembunyikan panel lupa password saat mulai
        }

        if (forgotPasswordErrorText != null)
        {
            forgotPasswordErrorText.raycastTarget = false; // Mencegah teks error menutupi klik pada kolom input
            forgotPasswordErrorText.gameObject.SetActive(false);
        }

        // Otomatis hilangkan pesan error begitu user mulai mengetik ulang di kolom input
        if (emailField != null)
        {
            emailField.contentType = TMP_InputField.ContentType.EmailAddress;
            emailField.onValueChanged.AddListener((val) => { if (errorText != null) errorText.gameObject.SetActive(false); });
        }
        if (passwordField != null) passwordField.onValueChanged.AddListener((val) => { if (errorText != null) errorText.gameObject.SetActive(false); });
        if (forgotEmailField != null)
        {
            forgotEmailField.contentType = TMP_InputField.ContentType.EmailAddress;
            forgotEmailField.onValueChanged.AddListener((val) => { if (forgotPasswordErrorText != null) forgotPasswordErrorText.gameObject.SetActive(false); });
        }
        if (otpField != null) otpField.onValueChanged.AddListener((val) => { if (forgotPasswordErrorText != null) forgotPasswordErrorText.gameObject.SetActive(false); });
        if (newPasswordField != null) newPasswordField.onValueChanged.AddListener((val) => { if (forgotPasswordErrorText != null) forgotPasswordErrorText.gameObject.SetActive(false); });

        // Fitur Auto-fill / Remember Me
        if (PlayerPrefs.HasKey("SavedEmail") && PlayerPrefs.HasKey("SavedPassword"))
        {
            if (emailField != null) emailField.text = PlayerPrefs.GetString("SavedEmail");
            if (passwordField != null) passwordField.text = PlayerPrefs.GetString("SavedPassword");
        }
    }

    // ==========================================
    // FUNGSI VALIDASI FORMAT EMAIL (ANDROID & IOS)
    // ==========================================
    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Regex super ketat: mencegah @ salah tempat, @ ganda, titik ganda (..), atau karakter spesial di awal/akhir
            return System.Text.RegularExpressions.Regex.IsMatch(email.Trim(),
                @"^(?!.*\.\.)[a-zA-Z0-9](?:[a-zA-Z0-9._%+-]*[a-zA-Z0-9])?@[a-zA-Z0-9](?:[a-zA-Z0-9.-]*[a-zA-Z0-9])?\.[a-zA-Z]{2,}$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    // ==========================================
    // 1. FUNGSI TOMBOL LOGIN
    // ==========================================
    public void OnLoginButtonClicked()
    {
        if (string.IsNullOrEmpty(emailField.text) || string.IsNullOrEmpty(passwordField.text))
        {
            ShowError("Email atau Password tidak boleh kosong!");
            return;
        }

        if (!IsValidEmail(emailField.text))
        {
            ShowError("❌ Format email tidak valid! (Contoh: nama@domain.com)");
            return;
        }

        StartCoroutine(ProsesLoginServer());
    }

    private IEnumerator ProsesLoginServer()
    {
        ShowError("Memverifikasi data...");

        string emailClean = emailField.text.Trim();
        string passClean = passwordField.text.Trim();
        string jsonInput = "{\"email\":\"" + emailClean + "\",\"password\":\"" + passClean + "\"}";

        UnityWebRequest request = new UnityWebRequest(baseUrl + "/login", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonInput);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string respon = request.downloadHandler.text;
            
            if (respon.Contains("\"status\":true"))
            {
                errorText.gameObject.SetActive(false);
                Debug.Log("Login sukses via server cloud.");

                // 🔥 JURUS BARU: Simpan email user ke memori HP biar dibaca saat klik tombol Selesai
                PlayerPrefs.SetString("LoggedInUser", emailClean); 
                // Simpan juga untuk fitur auto-fill login selanjutnya
                PlayerPrefs.SetString("SavedEmail", emailClean);
                PlayerPrefs.SetString("SavedPassword", passClean);
                PlayerPrefs.Save();

                StartCoroutine(FadeAndLoad("3_halaman utama"));
            }
            else 
            {
                ShowError("❌ Login Gagal! Periksa Email dan Password.");
            }
        }
        else
        {
            ShowError("❌ Server tidak merespon/Offline.");
        }
    }

    // ==========================================
    // 2. FUNGSI TOMBOL REGISTRASI
    // ==========================================
    public void OnRegisterButtonClicked()
    {
        if (string.IsNullOrEmpty(emailField.text) || string.IsNullOrEmpty(passwordField.text) || string.IsNullOrEmpty(nameField.text))
        {
            ShowError("Data wajib tidak boleh kosong!");
            return;
        }

        if (!IsValidEmail(emailField.text))
        {
            ShowError("❌ Format email tidak valid! (Contoh: nama@domain.com)");
            return;
        }

        StartCoroutine(ProsesRegistrasiServer());
    }

    private IEnumerator ProsesRegistrasiServer()
    {
        ShowError("Mendaftarkan ke server...");

        string nameClean = nameField.text.Trim();
        string emailClean = emailField.text.Trim();
        string passClean = passwordField.text.Trim();
        string phoneClean = phoneField != null ? phoneField.text.Trim() : "";
        string bpjsClean = bpjsField != null ? bpjsField.text.Trim() : "";

        string jsonInput = "{\"username\":\"" + nameClean + "\"," +
                           "\"gmail\":\"" + emailClean + "\"," +
                           "\"password\":\"" + passClean + "\"," +
                           "\"mobile_number\":\"" + phoneClean + "\"," +
                           "\"BPJS_number\":\"" + bpjsClean + "\"}";

        UnityWebRequest request = new UnityWebRequest(baseUrl + "/register", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonInput);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string respon = request.downloadHandler.text;
            if (respon.Contains("\"status\":true"))
            {
                Debug.Log("Registrasi Sukses! Mengarahkan ke form Login.");
                GoToLoginScene(); 
            }
            else
            {
                ShowError("❌ Gagal! Email mungkin sudah dipakai.");
            }
        }
        else
        {
            ShowError("❌ Server tidak merespon/Offline.");
        }
    }

    // ==========================================
    // 3. FUNGSI PINDAH SCENE BAWAAN LU
    // ==========================================
    public void GoToLoginScene()
    {
        StartCoroutine(FadeAndLoad("1_Login"));
    }

    public void GoToRegisterScene()
    {
        StartCoroutine(FadeAndLoad("2_regis"));
    }

    // ==========================================
    // 4. FUNGSI LUPA KATA SANDI (FORGOT PASSWORD)
    // ==========================================
    public void OnForgotPasswordClicked()
    {
        if (forgotPasswordPanel != null)
        {
            forgotPasswordPanel.SetActive(true);
            if (phase1Panel != null) phase1Panel.SetActive(true);
            if (phase2Panel != null) phase2Panel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Panel Lupa Password belum di-assign di Inspector!");
        }
    }

    public void OnCloseForgotPasswordClicked()
    {
        if (forgotPasswordPanel != null)
        {
            forgotPasswordPanel.SetActive(false);
        }
    }

    public void OnSendOTPClicked()
    {
        if (string.IsNullOrEmpty(forgotEmailField.text))
        {
            ShowForgotPasswordError("Email untuk reset password tidak boleh kosong!");
            return;
        }

        if (!IsValidEmail(forgotEmailField.text))
        {
            ShowForgotPasswordError("❌ Format email tidak valid! (Contoh: nama@domain.com)");
            return;
        }

        StartCoroutine(ProsesKirimOTPServer());
    }

    private IEnumerator ProsesKirimOTPServer()
    {
        ShowForgotPasswordError("Mengirim OTP ke email...");

        string emailClean = forgotEmailField.text.Trim();

        // Kirim key 'email' dan 'gmail' sekaligus agar kompatibel dengan database backend Vercel
        string jsonInput = "{\"email\":\"" + emailClean + "\",\"gmail\":\"" + emailClean + "\"}";
        
        UnityWebRequest request = new UnityWebRequest(baseUrl + "/forgot-password", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonInput);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string respon = request.downloadHandler.text;
            Debug.Log("[Forgot Password] Respons Server: " + respon);

            if (respon.Contains("\"status\":true"))
            {
                ShowForgotPasswordError("OTP telah dikirim ke email Anda.");
                if (phase1Panel != null) phase1Panel.SetActive(false);
                if (phase2Panel != null) phase2Panel.SetActive(true);
                if (successEmailText != null) successEmailText.text = "Kode telah dikirim ke " + emailClean;
            }
            else
            {
                ShowForgotPasswordError("❌ Gagal: Email tidak terdaftar di aplikasi atau server gagal mengirim OTP.");
            }
        }
        else
        {
            ShowForgotPasswordError("❌ Gagal terhubung ke server / gangguan koneksi.");
        }
    }

    public void OnVerifyOTPAndResetPasswordClicked()
    {
        if (string.IsNullOrEmpty(forgotEmailField.text) || string.IsNullOrEmpty(otpField.text) || string.IsNullOrEmpty(newPasswordField.text))
        {
            ShowForgotPasswordError("Email, OTP, dan Password Baru tidak boleh kosong!");
            return;
        }

        if (!IsValidEmail(forgotEmailField.text))
        {
            ShowForgotPasswordError("❌ Format email tidak valid!");
            return;
        }

        StartCoroutine(ProsesResetPasswordServer());
    }

    private IEnumerator ProsesResetPasswordServer()
    {
        ShowForgotPasswordError("Mereset password...");

        string emailClean = forgotEmailField.text.Trim();
        string otpClean = otpField.text.Trim();
        string passClean = newPasswordField.text.Trim();

        string jsonInput = "{\"email\":\"" + emailClean + "\",\"gmail\":\"" + emailClean + "\",\"otp\":\"" + otpClean + "\",\"newPassword\":\"" + passClean + "\"}";
        
        UnityWebRequest request = new UnityWebRequest(baseUrl + "/reset-password", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonInput);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string respon = request.downloadHandler.text;
            Debug.Log("[Reset Password] Respons Server: " + respon);

            if (respon.Contains("\"status\":true"))
            {
                ShowForgotPasswordError("Password berhasil direset! Silakan login.");
                if (forgotPasswordPanel != null) forgotPasswordPanel.SetActive(false);
                if (passwordField != null) passwordField.text = ""; // Kosongkan field password lama
            }
            else
            {
                ShowForgotPasswordError("❌ Gagal mereset password. Pastikan kode OTP benar.");
            }
        }
        else
        {
            ShowForgotPasswordError("❌ Gagal terhubung ke server.");
        }
    }

    private void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Error Text UI is not assigned: " + message);
        }
    }

    private void ShowForgotPasswordError(string message)
    {
        if (forgotPasswordErrorText != null)
        {
            forgotPasswordErrorText.text = message;
            forgotPasswordErrorText.gameObject.SetActive(true);
        }
        else
        {
            // Jika belum di-assign, pakai errorText yang lama sebagai fallback
            ShowError(message);
        }
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        SceneFadeManager fadeManager = FindObjectOfType<SceneFadeManager>();
        if (fadeManager != null)
        {
            fadeManager.fadeDuration = this.fadeDuration;
            yield return fadeManager.DoFadeOut();
        }
        SceneManager.LoadScene(sceneName);
    }
}