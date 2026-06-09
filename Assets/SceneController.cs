using UnityEngine;
using UnityEngine.SceneManagement; // Library wajib untuk navigasi scene
using System.Collections;

public class SceneController : MonoBehaviour
{
    // Fungsi untuk pindah ke scene berdasarkan nama
    public void ChangeSceneByName(string sceneName)
    {
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
        SceneManager.LoadScene(sceneName);
    }

    // Fungsi untuk memuat ulang scene yang sedang aktif
    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Fungsi untuk keluar dari aplikasi
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game ditutup (Hanya berfungsi setelah di-build)");
    }
}