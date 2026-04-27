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
        if (searchBar != null)
        {
            searchBar.onValueChanged.AddListener(FilterRuangan);
        }
    }

    public void FilterRuangan(string kataKunci)
    {
        // Auto-open panel pas ngetik
        if (targetScrollView != null && kataKunci.Length > 0)
        {
            targetScrollView.SetActive(true);
        }

        kataKunci = kataKunci.ToLower();

        // Cek satu-satu tombol yang ada
        foreach (Transform tombolRuangan in contentContainer)
        {
            // RAHASIANYA DI SINI: Tambahin (true) biar bisa ngebaca teks dari tombol yang lagi ngilang!
            // Kita juga pakai GetComponentsInChildren (pakai 's') buat ngebaca semua teks di tombol (termasuk icon)
            TMP_Text[] semuaTeks = tombolRuangan.GetComponentsInChildren<TMP_Text>(true);
            bool adaYangCocok = false;

            foreach (TMP_Text teks in semuaTeks)
            {
                if (teks.text.ToLower().Contains(kataKunci))
                {
                    adaYangCocok = true;
                    break; // Kalau nemu 1 kecocokan, langsung stop nyari di tombol ini
                }
            }

            // Tampilkan tombol kalau ada teks yang cocok, sembunyikan kalau nggak ada
            tombolRuangan.gameObject.SetActive(adaYangCocok);
        }
    }
}