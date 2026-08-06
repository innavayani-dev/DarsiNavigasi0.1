using UnityEngine;
using Immersal.Samples.Navigation;

public class NavigationSelectionManager : MonoBehaviour
{
    public static NavigationSelectionManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Fungsi ini dipanggil saat user memilih lokasi di UI list ruangan
    public void SelectDestination(string targetName)
    {
        // 1. Cari semua objek yang punya script IsNavigationTarget di AR
        IsNavigationTarget[] allTargets = FindObjectsOfType<IsNavigationTarget>();

        foreach (var target in allTargets)
        {
            // 2. Jika namanya cocok, munculkan. Jika tidak, sembunyikan.
            if (target.targetName == targetName)
            {
                target.SetVisible(true);
                Debug.Log("✅ Munculkan target: " + targetName);

                // =========================================================
                // 🔥 MENCATAT KOORDINAT TUJUAN KE DALAM HP 🔥
                // =========================================================
                // Ambil posisi 3D (X, Y, Z) dari titik targetnya (dibulatkan 3 angka di belakang koma)
                string koordinat = $"{target.transform.position.x.ToString("F3")}, {target.transform.position.y.ToString("F3")}, {target.transform.position.z.ToString("F3")}";
                
                // Simpan ke "kantong memori" HP bernama KoordinatTujuan
                PlayerPrefs.SetString("KoordinatTujuan", koordinat);
                PlayerPrefs.Save(); 
            }
            else
            {
                target.SetVisible(false);
            }
        }
    }

    // Fungsi untuk reset (sembunyikan target)
    public void ClearDestination()
    {
        IsNavigationTarget[] allTargets = FindObjectsOfType<IsNavigationTarget>();
        foreach (var target in allTargets)
        {
            target.SetVisible(false);
        }

    }
}