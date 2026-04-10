using UnityEngine;
using Immersal.Samples.Navigation;

public class NavigationSelectionManager : MonoBehaviour
{
    public static NavigationSelectionManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Fungsi ini dipanggil saat user memilih lokasi di UI
    public void SelectDestination(string targetName)
    {
        // 1. Cari semua objek yang punya script IsNavigationTarget
        IsNavigationTarget[] allTargets = FindObjectsOfType<IsNavigationTarget>();

        foreach (var target in allTargets)
        {
            // 2. Jika namanya cocok, munculkan. Jika tidak, sembunyikan.
            if (target.targetName == targetName)
            {
                target.SetVisible(true);
                Debug.Log("Munculkan target: " + targetName);
            }
            else
            {
                target.SetVisible(false);
            }
        }
    }

    // Fungsi untuk reset (sembunyikan semua lagi pas klik 'Stop Navigation')
    public void ClearDestination()
    {
        IsNavigationTarget[] allTargets = FindObjectsOfType<IsNavigationTarget>();
        foreach (var target in allTargets)
        {
            target.SetVisible(false);
        }
    }
}