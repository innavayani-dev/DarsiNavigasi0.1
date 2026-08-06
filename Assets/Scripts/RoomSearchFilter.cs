using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoomSearchFilter : MonoBehaviour
{
    [Header("Komponen UI")]
    public TMP_InputField searchBar;
    public Transform contentContainer;
    public GameObject targetScrollView;

    void Start()
    {
        // Hapus fungsi ganda karena NavigationManager.cs sudah punya fungsi search yang lebih sempurna
        // dan terhubung langsung dengan database.
        
        // AUTO-HOOKUP: Cari semua objek bernama "Filter Popup" (termasuk yang disembunyikan/inactive)
        Transform filterPopup = null;
        foreach (Transform t in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (t.name == "Filter Popup" && t.gameObject.scene.isLoaded)
            {
                filterPopup = t;
                break;
            }
        }

        if (filterPopup != null)
        {
            FilterUI filterUI = Object.FindObjectOfType<FilterUI>();
            if (filterUI == null) return;

            // Ambil semua tombol yang ada di dalam Filter Popup
            UnityEngine.UI.Button[] tombolFilter = filterPopup.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            foreach (var btn in tombolFilter)
            {
                TMP_Text teks = btn.GetComponentInChildren<TMP_Text>(true);
                if (teks != null)
                {
                    string namaTombol = teks.text.ToLower();
                    
                    // Bersihkan listener lama
                    btn.onClick.RemoveAllListeners();

                    // Pasangkan fungsi langsung ke FilterUI bawaan sistem
                    if (namaTombol.Contains("semua"))
                    {
                        btn.onClick.AddListener(() => filterUI.PilihOpsi("Semua"));
                    }
                    else if (namaTombol.Contains("graha"))
                    {
                        btn.onClick.AddListener(() => filterUI.PilihOpsi("Graha"));
                    }
                    else if (namaTombol.Contains("tower"))
                    {
                        btn.onClick.AddListener(() => filterUI.PilihOpsi("Tower"));
                    }
                }
            }
        }
    }
}