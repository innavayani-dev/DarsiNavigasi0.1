using UnityEngine;
using Immersal.Samples.Navigation;

public class FilterUI : MonoBehaviour
{
    // Ini adalah kotak pop-up yang isinya tombol Graha & Tower
    public GameObject filterPopup; 
    
    // Variabel penyimpan filter aktif ("" berarti semua gedung)
    public string selectedFilter = "";

    // Fungsi untuk memunculkan atau menyembunyikan pop-up
    public void TogglePopup()
    {
        if (filterPopup != null)
        {
            bool willBeActive = !filterPopup.activeSelf;
            filterPopup.SetActive(willBeActive); 

            // CEGAH TUMPANG TINDIH: Jika Filter Popup aktif, sembunyikan daftar ruangan
            if (willBeActive)
            {
                NavigationManager navManager = Object.FindObjectOfType<NavigationManager>();
                if (navManager != null) navManager.CloseNavigationList();
            }
        }
    }

    // Fungsi ini dipanggil ketika salah satu opsi ditekan
    public void PilihOpsi(string namaGedung)
    {
        Debug.Log("Filter terpilih: " + (namaGedung == "" ? "Semua Gedung" : namaGedung));
        
        // Sesuai permintaan: Graha RSI dan Semua Gedung memunculkan semua data ("")
        // Sedangkan Tower RSI dibiarkan kosong (menggunakan filter "tower" yang belum ada datanya)
        if (namaGedung == "Tower")
        {
            selectedFilter = "tower";
        }
        else if (namaGedung == "Graha")
        {
            selectedFilter = "graha";
        }
        else
        {
            selectedFilter = "";
        }
        
        // Tutup kembali pop-up setelah disentuh
        if (filterPopup != null)

        {
            filterPopup.SetActive(false);
        }
        
        // Cari NavigationManager dan perbarui daftarnya
        NavigationManager navManager = GetComponent<NavigationManager>();
        if (navManager != null)
        {
            // Pastikan panel daftar ruangan (Scroll View) terbuka
            navManager.OpenNavigationList();
            
            // Panggil ulang pencarian agar filter diterapkan ke daftar yang sudah terbuka
            if (navManager.searchBar != null) {
                navManager.OnSearchChanged(navManager.searchBar.text);
            } else {
                navManager.OnSearchChanged("");
            }
        }
    }
}
