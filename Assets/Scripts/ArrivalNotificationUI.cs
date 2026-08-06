using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using UnityEngine.UI; // Wajib untuk ngebaca Tombol

public class ArrivalNotificationUI : MonoBehaviour
{
    public static ArrivalNotificationUI Instance { get; private set; }

    [Header("Sambungan UI Inspector")]
    public GameObject panel;
    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI timeText;

    private string apiUrl = "https://darsi-nav.hcm-lab.id/api/save-history";
    private string currentTujuan = "";
    private bool isSending = false; // Pelindung biar gak ngirim data double

    private void Awake()
    {
        // 1. Aturan Penguasa Tunggal (Singleton)
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
        
        if (panel == null) panel = gameObject;

        // 2. Setup proteksi raycast & prioritas layar
        SetupCanvasPriorityAndRaycasts();

        // 3. Sembunyikan panel di awal permainan
        if (panel != null) panel.SetActive(false);
    }

    private void SetupCanvasPriorityAndRaycasts()
    {
        if (panel == null) panel = gameObject;

        // 1. Atur Canvas agar pop-up ini 100% berada di lapisan paling atas (di atas Canvas lain dalam mode Simulator/HP)
        Canvas popCanvas = panel.GetComponent<Canvas>();
        if (popCanvas == null) popCanvas = panel.AddComponent<Canvas>();
        popCanvas.overrideSorting = true;
        popCanvas.sortingOrder = 999; // Sangat tinggi agar tidak terhalang UI/Canvas lain yang memiliki sortingOrder 0

        // 2. Pastikan GraphicRaycaster aktif untuk mendeteksi sentuhan jari / klik mouse
        if (panel.GetComponent<GraphicRaycaster>() == null)
        {
            panel.AddComponent<GraphicRaycaster>();
        }

        // 3. Matikan raycast penghalang pada dekorasi (teks judul, ikon centang, dsb) agar tidak menyerap klik
        Graphic[] allGraphics = panel.GetComponentsInChildren<Graphic>(true);
        foreach (Graphic g in allGraphics)
        {
            Button parentBtn = g.GetComponentInParent<Button>();
            if (parentBtn != null)
            {
                // Hanya Image utama tombol yang menerima raycast, teks di dalam tombol tidak
                g.raycastTarget = (g.gameObject == parentBtn.gameObject);
            }
            else if (g.gameObject != panel)
            {
                // Elemen dekorasi lain tidak boleh menghalangi klik tombol Selesai
                g.raycastTarget = false;
            }
        }

        // 4. Pastikan fungsi tombol Selesai terpasang
        Button[] semuaTombol = panel.GetComponentsInChildren<Button>(true);
        foreach (Button btn in semuaTombol)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(HideArrival);
        }
    }

    // Fungsi ini bakal dipanggil otomatis sama script Navigasi lu pas sampai tujuan
    public void ShowArrival(string destinationName, float distanceMeters, float timeSeconds)
    {
        if (panel == null) panel = gameObject;

        currentTujuan = destinationName;
        isSending = false; // Buka gerbang pengiriman lagi

        if (roomNameText != null)
        {
            if (DoorbellARPopup.IsRoomRequiringDoorbell(destinationName))
            {
                roomNameText.text = destinationName + "\n<color=#FFD700><size=70%>Harap Tekan Bel Sebelum Masuk</size></color>";
            }
            else
            {
                roomNameText.text = destinationName;
            }
        }
        if (distanceText != null)
        {
            distanceText.enableWordWrapping = false;
            distanceText.text = $"{Mathf.RoundToInt(distanceMeters)} m";
        }

        if (timeText != null)
        {
            timeText.enableWordWrapping = false;
            int minutes = Mathf.FloorToInt(timeSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeSeconds % 60f);
            if (minutes > 0)
            {
                timeText.text = seconds > 0 ? $"{minutes} Menit {seconds} Detik" : $"{minutes} Menit";
            }
            else
            {
                timeText.text = $"{seconds} Detik";
            }
        }

        panel.SetActive(true);
        SetupCanvasPriorityAndRaycasts(); // Pastikan selalu jadi teratas & tidak terhalang saat dimunculkan
        panel.transform.SetAsLastSibling(); // Paksa pop-up tampil paling depan
    }

    // Fungsi pas Tombol Selesai diklik
    public void HideArrival()
    {
        Debug.Log("🔔 Tombol Selesai diklik! Tutup panel & kirim riwayat...");
        
        // 1. LANGSUNG TUTUP PANEL UI AGAR TOMBOL SELESAI TERASA SANGAT RESPONSIF (0 DETIK!)
        if (panel != null) panel.SetActive(false);
        else gameObject.SetActive(false);

        // Pelindung agar tidak kirim data double jika tombol tertekan 2 kali
        if (isSending) return;
        isSending = true;

        // 2. Kirim data di latar belakang menggunakan kurir independen agar tidak mati walau panel dimatikan
        GameObject kurirObj = new GameObject("KurirRiwayatVercel");
        DontDestroyOnLoad(kurirObj);
        KurirVercel kurir = kurirObj.AddComponent<KurirVercel>();
        kurir.MulaiKirim(apiUrl, currentTujuan);
    }
}

public class KurirVercel : MonoBehaviour
{
    public void MulaiKirim(string url, string tujuan)
    {
        StartCoroutine(ProsesKirim(url, tujuan));
    }

    private IEnumerator ProsesKirim(string url, string tujuan)
    {
        string userId = PlayerPrefs.GetString("LoggedInUser", "Guest_User"); 
        string mulai = PlayerPrefs.GetString("InitialStartRoom", "Lokasi Saat Ini"); 
        string koordinatAwal = PlayerPrefs.GetString("KoordinatAwal", "0,0,0");
        string koordinatTujuan = PlayerPrefs.GetString("KoordinatTujuan", "-");

        string jsonInput = "{" +
            "\"user_id\":\"" + userId + "\"," +
            "\"mulai\":\"" + mulai + "\"," +
            "\"tujuan\":\"" + tujuan + "\"," +
            "\"koordinat_awal\":\"" + koordinatAwal + "\"," +
            "\"koordinat_tujuan\":\"" + koordinatTujuan + "\"" +
        "}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonInput);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 5; // Batasi maksimal 5 detik agar tidak tunggu 1 menit jika jaringan lambat!

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ [KurirVercel] Sukses kirim riwayat ke server: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogWarning("⚠️ [KurirVercel] Pengiriman riwayat gagal/timeout (tidak mengganggu UI): " + request.error);
        }

        Destroy(gameObject); // Hapus diri sendiri setelah tugas selesai
    }
}