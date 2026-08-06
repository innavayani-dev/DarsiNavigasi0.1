/*===============================================================================
REVISI TOTAL: FIX UI, AUTO-DETECTION, & DATABASE INTEGRATION + SEARCH FILTER
Dibuat untuk: Alif (Tugas Akhir Navigasi Indoor PENS - RSI Ahmad Yani)
===============================================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Networking;
using Immersal.XR;
using Immersal.Samples.Util;
using TMPro;

namespace Immersal.Samples.Navigation
{
    [System.Serializable]
    public class RoomItem { public string room_id; public string room_name; }

    [System.Serializable]
    public class RoomListResponse { public bool status; public RoomItem[] data; }

    [System.Serializable]
    public class MapResponse { public string room_id; public string coordinates; public string room_name; public string Floor_ID; }

    public class NavigationManager : MonoBehaviour
    {
        // --- DAFTAR RUANGAN GRAHA DAN TOWER ---
        private HashSet<string> daftarGraha = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "Mandiri BPJS", "Bank Mega Syariah", "Loket Pendaftaran", "BPJS Center", "Layanan Transportasi", 
            "Resepsionis", "Lift", "Poli Gigi", "Toilet Umum", 
            "R. Oprasi Gigi", "Poli Bedah", "Poli THT", "Ruang Gizi", "Poli Ortopedi", 
            "Kasir", "Farmasi", "R. Poli Kandungan", "R. Poli Anak", 
            "R. Poli Anak Laktasi", "Masjid", "Minimarket Kantin", "R. TB DOTS", 
            "R. Multazam", "Bilik Dahak", "Kamar Jenazah", "Poli Spesialis Jantung", 
            "Poli Spesialis Paru", "Rehabilitasi Medik", "Poli Penyakit Dalam", "Poli Syaraf", 
            "Poli Mata", "Poli 1", "Poli 2", "Hemodialisis LT1", "Klinik Vaksin LT1", "Irna Bedah LT2", 
            "R. Thaif LT2", "Irna Anak LT3", "R. Madinah LT3", "Poli Urologi LT3", 
            "Irna Dewasa LT4", "R. Makkah LT3", "R. Direksi LT5", "R. Managemen LT5", "R. Komdik LT5"
        };

        private HashSet<string> daftarTower = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "Gedung Tower LT1", "IGD LT2", "Farmasi LT2", "Laboratium LT3", "Radiologi LT3", 
            "R. Rawat Intensif", "R. Operasi LT5", "CSSD LT5", "Poli Eksekutif LT6", 
            "Coffe Bean LT6", "Farmasi LT6", "An-Nisa Irna Bersalin LT7", "Al-Kautsar Irna Anak LT8", 
            "Ar-Rayyan Irna Dewasa LT9", "Ar-Radhiin Irna Dewasa LT10", "Ar-Raudhah Irna Eksekutif LT11", 
            "Kordik LT12", "Yarsis LT13"
        };
        [Header("--- KONEKSI BACKEND VERCEL ---")]
        public string serverURL = "https://indoor-nav-backend.vercel.app";

        [Header("--- PENGATURAN UI (LIST RUANGAN) ---")]
        public GameObject listPanel;
        public Transform listContainer;
        public GameObject roomButtonPrefab;
        public GameObject showNavigationButton;
        // 1. TAMBAHKAN VARIABEL SEARCH BAR DI SINI
        public TMP_InputField searchBar;

        [Header("--- TARGET VISUAL (PHANTOM) ---")]
        public GameObject phantomTargetObject;

        [Header("--- VISUALISASI JALUR ---")]
        [SerializeField] private GameObject m_navigationPathPrefab = null;
        [SerializeField] private GameObject m_StopNavigationButton = null;

        [Header("--- ELEVATOR POPUP UI ---")]
        public GameObject panelElevatorPopup;
        [Tooltip("Ambang batas perbedaan tinggi (meter) untuk mendeteksi lift/tangga")]
        [SerializeField] private float m_elevatorHeightThreshold = 3.5f;
        [Tooltip("Jarak dekat (meter) pemain ke lift agar Pop-up muncul")]
        [SerializeField] private float m_elevatorActivationDistance = 2.5f;
        private Vector3 m_elevatorPosition;
        private bool m_hasElevatorInPath = false;

        [Header("--- PARAMETER NAVIGASI ---")]
        [SerializeField] private float m_ArrivedDistanceThreshold = 1.2f;
        [SerializeField] private float m_pathWidth = 0.2f;
        [SerializeField] private float m_heightOffset = 0.5f;

        [HideInInspector] public bool inEditMode = false;
        public void ToggleEditMode() { inEditMode = !inEditMode; }

        public void InitializeNavigation(NavigationTargetListButton button)
        {
            if (button == null || button.targetObject == null) return;

            m_NavigationTarget = button.targetObject.GetComponent<IsNavigationTarget>();
            if (m_NavigationTarget != null)
            {
                // Sembunyikan target lain dan nyalakan target yang dipilih
                IsNavigationTarget[] allTargets = Resources.FindObjectsOfTypeAll<IsNavigationTarget>();
                foreach (var t in allTargets)
                {
                    if (t != null && t.gameObject.scene.isLoaded)
                    {
                        t.SetVisible(t == m_NavigationTarget);
                    }
                }

                if (phantomTargetObject != null)
                {
                    phantomTargetObject.SetActive(true);
                    phantomTargetObject.transform.position = m_NavigationTarget.transform.position;
                }

                m_navigationState = NavigationState.Navigating;
                m_currentDestinationName = !string.IsNullOrEmpty(m_NavigationTarget.targetName) ? m_NavigationTarget.targetName : m_NavigationTarget.gameObject.name;
                m_navigationStartTime = Time.time;
                if (m_playerTransform != null)
                {
                    m_navigationStartDistance = Vector3.Distance(m_playerTransform.position, m_NavigationTarget.transform.position);
                }

                UpdateNavigationUI(m_navigationState);
                DoorbellARPopup.Instance.CheckAndShow(m_currentDestinationName, m_NavigationTarget.transform);

                string startRoom = PlayerPrefs.GetString("InitialStartRoom", "Lokasi Saat Ini");
                if (ActiveNavigationUI.Instance != null)
                {
                    ActiveNavigationUI.Instance.ShowNavigation(startRoom, m_currentDestinationName);
                }
                CloseNavigationList();
            }
        }

        public void ToggleTargetsList() { ToggleNavigationList(); }

        private XRSpace m_XRSpace = null;
        private bool m_managerInitialized = false;
        private IsNavigationTarget m_NavigationTarget = null;
        private Transform m_playerTransform = null;
        private GameObject m_navigationPathObject = null;
        private NavigationPath m_navigationPath = null;

        // --- ARRIVAL NOTIFICATION TRACKING ---
        private string m_currentDestinationName = "";
        private float m_navigationStartTime = 0f;
        private float m_navigationStartDistance = 0f;
        // -------------------------------------

        private enum NavigationState { NotNavigating, Navigating };
        private NavigationState m_navigationState = NavigationState.NotNavigating;

        private static NavigationManager instance = null;
        public static NavigationManager Instance { get { return instance; } }

        void Awake() { if (instance == null) instance = this; }

        private void Start()
        {
            InitializeNavigationManager();

            if (phantomTargetObject != null)
                m_NavigationTarget = phantomTargetObject.GetComponent<IsNavigationTarget>();

            if (listPanel != null) listPanel.SetActive(false);

            if (showNavigationButton == null)
                Debug.LogWarning("⚠️ Tombol 'Show Navigation' belum ditarik ke Inspector!");

            // 2. TAMBAHKAN LISTENER AGAR SAAT MENGETIK LANGSUNG FILTER DAN SAAT DI-TAP MEMBUKA DAFTAR
            if (searchBar != null)
            {
                searchBar.onValueChanged.AddListener(OnSearchChanged);
                searchBar.onSelect.AddListener((string s) => OpenNavigationList());
            }

            // Load data otomatis saat mulai agar bisa langsung dicari
            Debug.Log("📡 Membuka daftar ruangan saat start...");
            StartCoroutine(FetchRoomsFromDatabase());
        }

        // 3. FUNGSI BARU: LOGIKA FILTER SEARCH BAR (SUDAH DI-FIX)
        public void OnSearchChanged(string searchText)
        {
            if (listContainer == null) return;

            // Pastikan daftar ruangan & scroll view selalu terbuka dan aktif saat user mengetik di search bar
            OpenNavigationList();

            string searchLower = searchText.ToLower();
            
            // Ambil filter gedung yang sedang aktif (contoh: "graha" atau "tower" atau "")
            string activeFilter = "";
            FilterUI filterUI = GetComponent<FilterUI>();
            if (filterUI != null) activeFilter = filterUI.selectedFilter;

            foreach (Transform child in listContainer)
            {
                // RAHASIANYA DI SINI: Pakai GetComponentsInChildren dengan (true) 
                // agar sistem tetap bisa membaca text meskipun tombol sedang disembunyikan / SetActive(false)
                TMP_Text[] semuaTeks = child.GetComponentsInChildren<TMP_Text>(true);
                
                // Gabungkan semua teks yang ada di tombol ruangan ini
                string fullText = "";
                foreach (TMP_Text txt in semuaTeks)
                {
                    if (txt != null) fullText += txt.text.ToLower() + " ";
                }

                // Cek apakah cocok dengan teks yang diketik di search bar
                bool isSearchMatch = fullText.Contains(searchLower);
                
                // Ambil nama ruangan asli (tanpa di-lower case) untuk pencocokan list
                string namaRuanganAsli = "";
                Transform nameObj = child.Find("Name");
                if (nameObj != null)
                {
                    TMP_Text nameText = nameObj.GetComponent<TMP_Text>();
                    if (nameText != null) namaRuanganAsli = nameText.text.Trim();
                }
                
                // Jika tidak ada objek "Name", fallback pakai string kosong
                if (string.IsNullOrEmpty(namaRuanganAsli) && semuaTeks.Length > 0)
                {
                    namaRuanganAsli = semuaTeks[0].text.Trim();
                }

                // Cek apakah cocok dengan filter gedung
                bool isFilterMatch = true;
                if (!string.IsNullOrEmpty(activeFilter))
                {
                    if (activeFilter == "graha")
                    {
                        isFilterMatch = false;
                        foreach (string graha in daftarGraha)
                        {
                            if (namaRuanganAsli.Contains(graha, System.StringComparison.OrdinalIgnoreCase))
                            {
                                isFilterMatch = true;
                                break;
                            }
                        }
                    }
                    else if (activeFilter == "tower")
                    {
                        isFilterMatch = false;
                        foreach (string tower in daftarTower)
                        {
                            if (namaRuanganAsli.Contains(tower, System.StringComparison.OrdinalIgnoreCase))
                            {
                                isFilterMatch = true;
                                break;
                            }
                        }
                    }
                }

                // Tampilkan hanya jika cocok KEDUANYA (Search DAN Filter Gedung)
                child.gameObject.SetActive(isSearchMatch && isFilterMatch);
            }

            // Reset scroll ke paling atas
            UnityEngine.UI.ScrollRect scrollRect = listContainer.GetComponentInParent<UnityEngine.UI.ScrollRect>();
            if (scrollRect != null)
            {
                // Force canvas update so the layout reflects the hidden items before setting position
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        public void ToggleNavigationList()
        {
            if (listPanel == null) return;
            if (listPanel.activeSelf) CloseNavigationList();
            else OpenNavigationList();
        }

        public void OpenNavigationList()
        {
            if (listPanel != null)
            {
                listPanel.SetActive(true);
                // Pastikan seluruh anak di dalam listPanel (seperti Scroll View) ikut aktif kembali
                for (int i = 0; i < listPanel.transform.childCount; i++)
                {
                    Transform child = listPanel.transform.GetChild(i);
                    if (child != null && child.name.IndexOf("Blocker", System.StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        child.gameObject.SetActive(true);
                    }
                }
            }
            if (searchBar != null && searchBar.gameObject != null) searchBar.gameObject.SetActive(true);

            // Aktifkan kembali semua NavigationTargetListControl yang berada di dalam listPanel agar daftar scroll muncul
            foreach (var tl in Resources.FindObjectsOfTypeAll<Immersal.Samples.Navigation.NavigationTargetListControl>())
            {
                if (tl != null && tl.gameObject != null)
                {
                    if (listPanel == null || tl.transform.IsChildOf(listPanel.transform) || tl.gameObject == listPanel)
                    {
                        tl.gameObject.SetActive(true);
                    }
                }
            }

            // CEGAH TUMPANG TINDIH: Sembunyikan Filter Popup jika sedang terbuka
            FilterUI filterUI = Object.FindObjectOfType<FilterUI>();
            if (filterUI != null && filterUI.filterPopup != null)
            {
                filterUI.filterPopup.SetActive(false);
            }

            // Hanya fetch jika list masih kosong untuk mencegah spam
            if (listContainer != null && listContainer.childCount == 0)
            {
                Debug.Log("📡 Memuat daftar ruangan...");
                StartCoroutine(FetchRoomsFromDatabase());
            }
        }

        public void CloseNavigationList()
        {
            if (listPanel != null) listPanel.SetActive(false);

            // Tutup panel NavigationTargetListControl (Navigation Targets Scroll View) yang aktif
            foreach (var tl in Object.FindObjectsOfType<Immersal.Samples.Navigation.NavigationTargetListControl>())
            {
                if (tl != null && tl.gameObject.activeInHierarchy)
                {
                    tl.gameObject.SetActive(false);
                }
            }
        }

        private void PopulateListFromSceneTargets(HashSet<string> existingNames)
        {
            if (listContainer == null || roomButtonPrefab == null) return;

            IsNavigationTarget[] targets = Resources.FindObjectsOfTypeAll<IsNavigationTarget>();
            foreach (IsNavigationTarget target in targets)
            {
                if (target == null || !target.gameObject.scene.isLoaded) continue;
                if (target.gameObject == phantomTargetObject) continue;
                string roomName = !string.IsNullOrEmpty(target.targetName) ? target.targetName : target.gameObject.name;
                if (string.IsNullOrEmpty(roomName) || existingNames.Contains(roomName)) continue;
                if (roomName.IndexOf("Phantom", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    target.gameObject.name.IndexOf("Phantom", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }
                existingNames.Add(roomName);

                GameObject btn = Instantiate(roomButtonPrefab, listContainer);

                // Set teks pada tombol
                TMP_Text[] allTexts = btn.GetComponentsInChildren<TMP_Text>(true);
                if (allTexts != null && allTexts.Length > 0)
                {
                    allTexts[0].text = roomName;
                }

                NavigationTargetListButton navBtn = btn.GetComponent<NavigationTargetListButton>();
                if (navBtn != null)
                {
                    navBtn.SetText(roomName);
                    navBtn.SetIcon(target.icon);
                    navBtn.SetTarget(target.gameObject);
                }

                Button btnComp = btn.GetComponent<Button>();
                if (btnComp != null)
                {
                    string capturedName = roomName;
                    btnComp.onClick.RemoveAllListeners();
                    btnComp.onClick.AddListener(() => OnRoomSelected(capturedName));
                }
            }
        }

        IEnumerator FetchRoomsFromDatabase()
        {
            foreach (Transform child in listContainer) Destroy(child.gameObject);

            HashSet<string> addedRoomNames = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

            // 1. SELALU isi daftar ruangan langsung dari objek 3D IsNavigationTarget di scene Unity terlebih dahulu
            PopulateListFromSceneTargets(addedRoomNames);

            // 2. Coba ambil dari database backend jika tersedia
            string url = serverURL + "/api/get-room-list";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    RoomListResponse response = JsonUtility.FromJson<RoomListResponse>(request.downloadHandler.text);
                    if (response != null && response.status && response.data != null)
                    {
                        foreach (RoomItem room in response.data)
                        {
                            if (string.IsNullOrEmpty(room.room_name) || addedRoomNames.Contains(room.room_name)) continue;
                            if (room.room_name.IndexOf("Phantom", System.StringComparison.OrdinalIgnoreCase) >= 0) continue;
                            addedRoomNames.Add(room.room_name);

                            GameObject btn = Instantiate(roomButtonPrefab, listContainer);
                            TMP_Text[] allTexts = btn.GetComponentsInChildren<TMP_Text>(true);
                            if (allTexts != null && allTexts.Length > 0) allTexts[0].text = room.room_name;

                            string id = !string.IsNullOrEmpty(room.room_id) ? room.room_id : room.room_name;
                            string nameForClick = room.room_name;
                            btn.GetComponent<Button>().onClick.AddListener(() => OnRoomSelected(nameForClick));
                        }
                    }
                }
            }

            // Jika search bar sedang aktif / ada teks pencarian, langsung aplikasikan filter ke tombol-tombol yang baru terbuat
            if (searchBar != null)
            {
                TMP_InputField inputField = searchBar.GetComponent<TMP_InputField>();
                if (inputField != null && !string.IsNullOrEmpty(inputField.text))
                {
                    OnSearchChanged(inputField.text);
                }
            }
        }

        void OnRoomSelected(string roomId)
        {
            CloseNavigationList();
            StartCoroutine(GetRoomCoordinates(roomId));
        }

        IEnumerator GetRoomCoordinates(string roomId)
        {
            IsNavigationTarget foundTarget = null;
            IsNavigationTarget[] allTargets = Resources.FindObjectsOfTypeAll<IsNavigationTarget>();

            // Pass 1: Cari Exact Match di Scene Unity terlebih dahulu
            foreach (var t in allTargets)
            {
                if (t != null && t.gameObject.scene.isLoaded)
                {
                    if (t.gameObject == phantomTargetObject) continue;
                    string tName = !string.IsNullOrEmpty(t.targetName) ? t.targetName : t.gameObject.name;
                    if (tName.IndexOf("Phantom", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                        t.gameObject.name.IndexOf("Phantom", System.StringComparison.OrdinalIgnoreCase) >= 0)
                        continue;

                    if (string.Equals(tName, roomId, System.StringComparison.OrdinalIgnoreCase))
                    {
                        foundTarget = t;
                        break;
                    }
                }
            }

            // Pass 2: Smart Substring Match di Scene Unity jika Exact Match tidak ketemu
            if (foundTarget == null)
            {
                foreach (var t in allTargets)
                {
                    if (t != null && t.gameObject.scene.isLoaded)
                    {
                        if (t.gameObject == phantomTargetObject) continue;
                        string tName = !string.IsNullOrEmpty(t.targetName) ? t.targetName : t.gameObject.name;
                        if (tName.IndexOf("Phantom", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                            t.gameObject.name.IndexOf("Phantom", System.StringComparison.OrdinalIgnoreCase) >= 0)
                            continue;

                        if (tName.Length >= 4 && roomId != null)
                        {
                            if (roomId.IndexOf(tName, System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                                tName.IndexOf(roomId, System.StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                foundTarget = t;
                                break;
                            }
                        }
                    }
                }
            }

            // Jika target ditemukan langsung di Scene Unity, langsung mulai navigasi!
            if (foundTarget != null)
            {
                foreach (var t in allTargets)
                {
                    if (t != null && t.gameObject.scene.isLoaded)
                    {
                        t.SetVisible(t == foundTarget);
                    }
                }

                m_NavigationTarget = foundTarget;
                if (phantomTargetObject != null)
                {
                    phantomTargetObject.SetActive(true);
                    phantomTargetObject.transform.position = foundTarget.transform.position;
                    MeshRenderer[] renderers = phantomTargetObject.GetComponentsInChildren<MeshRenderer>(true);
                    foreach (MeshRenderer mr in renderers) mr.enabled = true;
                }

                if (m_navigationPathObject == null) InitializeNavigationManager();

                m_navigationState = NavigationState.Navigating;
                string startRoom = PlayerPrefs.GetString("InitialStartRoom", "Lokasi Saat Ini");
                SimpanRiwayat("Muchammad Alif", startRoom, roomId, $"{foundTarget.transform.position.x},{foundTarget.transform.position.y},{foundTarget.transform.position.z}");
                UpdateNavigationUI(m_navigationState);
                CloseNavigationList();
                DoorbellARPopup.Instance.CheckAndShow(roomId, foundTarget.transform);

                m_currentDestinationName = roomId;
                m_navigationStartTime = Time.time;
                if (m_playerTransform != null) m_navigationStartDistance = Vector3.Distance(m_playerTransform.position, foundTarget.transform.position);
                else m_navigationStartDistance = 0f;

                if (ActiveNavigationUI.Instance != null) ActiveNavigationUI.Instance.ShowNavigation(startRoom, roomId);

                yield break;
            }

            string url = serverURL + "/api/map/" + roomId;
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    MapResponse data = JsonUtility.FromJson<MapResponse>(request.downloadHandler.text);
                    if (data != null)
                    {
                        string targetRoomName = data.room_name;
                        foundTarget = null;

                        // Pass 1: Cari Exact Match (Cocok 100% sama persis) terlebih dahulu agar tidak salah pilih antar Poli/Farmasi
                        allTargets = Resources.FindObjectsOfTypeAll<IsNavigationTarget>();
                        foreach (var t in allTargets)
                        {
                            if (t != null && t.gameObject.scene.isLoaded)
                            {
                                string tName = !string.IsNullOrEmpty(t.targetName) ? t.targetName : t.gameObject.name;
                                if (string.Equals(tName, targetRoomName, System.StringComparison.OrdinalIgnoreCase))
                                {
                                    foundTarget = t;
                                    break;
                                }
                            }
                        }

                        // Pass 2: Jika Exact Match tidak ditemukan, baru cari dengan Smart Substring Match
                        if (foundTarget == null)
                        {
                            foreach (var t in allTargets)
                            {
                                if (t != null && t.gameObject.scene.isLoaded)
                                {
                                    string tName = !string.IsNullOrEmpty(t.targetName) ? t.targetName : t.gameObject.name;
                                    // Hindari pencocokan kata pendek umum (< 5 huruf) atau kata generik seperti "Poli" / "Farmasi" saja
                                    if (tName.Length >= 5 && targetRoomName != null &&
                                       !string.Equals(tName, "Poli", System.StringComparison.OrdinalIgnoreCase) &&
                                       !string.Equals(tName, "Farmasi", System.StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (targetRoomName.IndexOf(tName, System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                                            tName.IndexOf(targetRoomName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                                        {
                                            foundTarget = t;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        // Update visibilitas target yang terpilih
                        foreach (var t in allTargets)
                        {
                            if (t != null && t.gameObject.scene.isLoaded)
                            {
                                t.SetVisible(t == foundTarget);
                            }
                        }

                        Vector3 targetPos = Vector3.zero;
                        bool hasValidSceneTarget = (foundTarget != null);

                        if (hasValidSceneTarget)
                        {
                            m_NavigationTarget = foundTarget;
                            targetPos = foundTarget.transform.position;
                        }
                        else if (!string.IsNullOrEmpty(data.coordinates))
                        {
                            string[] p = data.coordinates.Split(',');
                            float x = float.Parse(p[0], System.Globalization.CultureInfo.InvariantCulture);
                            float y = float.Parse(p[1], System.Globalization.CultureInfo.InvariantCulture);
                            float z = float.Parse(p[2], System.Globalization.CultureInfo.InvariantCulture);

                            // Proteksi Anti-Koordinat Xeokit Web BIM: Jika Y > 50 atau X > 500, ini adalah koordinat piksel 2D denah web.
                            if (Mathf.Abs(y) > 50f || Mathf.Abs(x) > 500f)
                            {
                                Debug.LogWarning($"[Shield] Koordinat {data.coordinates} untuk '{targetRoomName}' adalah format Web BIM Xeokit. Diabaikan di Unity agar tidak melompat ke langit.");
                            }
                            else
                            {
                                targetPos = new Vector3(x, y + 0.1f, z);
                            }
                        }

                        if (phantomTargetObject != null)
                        {
                            phantomTargetObject.SetActive(true);
                            if (hasValidSceneTarget)
                                phantomTargetObject.transform.position = targetPos;
                            else
                                phantomTargetObject.transform.localPosition = targetPos;

                            if (!hasValidSceneTarget)
                            {
                                m_NavigationTarget = phantomTargetObject.GetComponent<IsNavigationTarget>();
                            }

                            MeshRenderer[] renderers = phantomTargetObject.GetComponentsInChildren<MeshRenderer>(true);
                            foreach (MeshRenderer mr in renderers) mr.enabled = true;

                            m_navigationState = NavigationState.Navigating;
                            
                            string startRoom = PlayerPrefs.GetString("InitialStartRoom", "Lokasi Saat Ini");
                            
                            SimpanRiwayat("Muchammad Alif", startRoom, data.room_name, data.coordinates);
                            UpdateNavigationUI(m_navigationState);
                            CloseNavigationList();
                            DoorbellARPopup.Instance.CheckAndShow(data.room_name, phantomTargetObject.transform);
                            
                            m_currentDestinationName = data.room_name;
                            m_navigationStartTime = Time.time;
                            if (m_playerTransform != null)
                            {
                                m_navigationStartDistance = Vector3.Distance(m_playerTransform.position, phantomTargetObject.transform.position);
                            }
                            else
                            {
                                m_navigationStartDistance = 0f;
                            }
                            
                            if (ActiveNavigationUI.Instance != null)
                            {
                                ActiveNavigationUI.Instance.ShowNavigation(startRoom, data.room_name);
                            }
                        }
                    }
                }
            }
        }

        public void SimpanRiwayat(string userId, string dari, string ke, string pos)
        {
            StartCoroutine(PostHistoryToVercel(userId, dari, ke, pos));
        }

        IEnumerator PostHistoryToVercel(string userId, string dari, string ke, string pos)
        {
            string url = serverURL + "/api/save-history";
            WWWForm form = new WWWForm();
            form.AddField("user_id", userId);
            form.AddField("mulai", dari);
            form.AddField("tujuan", ke);
            form.AddField("koordinat", pos);

            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                    Debug.Log("✅ Riwayat Berhasil Dicatat ke TiDB!");
            }
        }

        private void Update()
        {
            if (m_managerInitialized && m_navigationState == NavigationState.Navigating)
                DrawARPath();
        }

        void DrawARPath()
        {
            if (m_playerTransform == null || m_NavigationTarget == null) return;
            Vector3 start = m_playerTransform.position;
            Vector3 target = m_NavigationTarget.transform.position;

            if (Vector3.Distance(start, target) < m_ArrivedDistanceThreshold)
            {
                // SHOW ARRIVAL NOTIFICATION
                if (ArrivalNotificationUI.Instance != null)
                {
                    float timeElapsed = Time.time - m_navigationStartTime;
                    ArrivalNotificationUI.Instance.ShowArrival(m_currentDestinationName, m_navigationStartDistance, timeElapsed);
                }

                StopNavigation();
                return;
            }

            List<Vector3> corners = new List<Vector3>();

            // Coba pakai algoritma A* dari NavigationGraphManager jika ada waypoint
            if (NavigationGraphManager.Instance != null)
            {
                List<Vector3> aStarPath = NavigationGraphManager.Instance.FindPath(start, target);
                if (aStarPath != null && aStarPath.Count > 1)
                {
                    foreach (var c in aStarPath)
                    {
                        // aStarPath menggunakan koordinat global (World Space), tambahkan heightOffset sesuai orientasi XRSpace
                        corners.Add(c + m_XRSpace.transform.up * m_heightOffset);
                    }
                }
            }

            // Fallback ke NavMesh jika A* tidak menemukan jalur atau tidak digunakan
            if (corners.Count == 0)
            {
                Vector3 s = XRSpaceToUnity(m_XRSpace.transform, m_XRSpace.InitialPose, start);
                Vector3 t = XRSpaceToUnity(m_XRSpace.transform, m_XRSpace.InitialPose, target);

                NavMeshPath path = new NavMeshPath();
                if (NavMesh.CalculatePath(s, t, NavMesh.AllAreas, path))
                {
                    foreach (var c in path.corners)
                    {
                        corners.Add(UnityToXRSpace(m_XRSpace.transform, m_XRSpace.InitialPose, c + new Vector3(0, m_heightOffset, 0)));
                    }
                }
            }

            // Fallback terakhir: Garis lurus ke tujuan jika NavMesh dan A* gagal (sehingga AR Path selalu muncul)
            if (corners.Count < 2)
            {
                corners.Clear();
                Vector3 upDir = m_XRSpace != null ? m_XRSpace.transform.up : Vector3.up;
                corners.Add(start + upDir * m_heightOffset);
                corners.Add(target + upDir * m_heightOffset);
            }

            if (corners.Count > 0)
            {
                // -- LIFT LOGIC: POTONG JALUR VERTIKAL --
                m_hasElevatorInPath = false;
                for (int i = 0; i < corners.Count - 1; i++)
                {
                    if (Mathf.Abs(corners[i].y - corners[i + 1].y) > m_elevatorHeightThreshold)
                    {
                        m_elevatorPosition = corners[i];
                        m_hasElevatorInPath = true;
                        corners.RemoveRange(i + 1, corners.Count - (i + 1));
                        break;
                    }
                }

                Vector3 upDir = m_XRSpace != null ? m_XRSpace.transform.up : Vector3.up;
                m_navigationPath.GeneratePath(corners, upDir);
                m_navigationPath.pathWidth = m_pathWidth;
            }

            // -- LIFT LOGIC: PROXIMITY CHECK --
            if (panelElevatorPopup != null)
            {
                if (m_hasElevatorInPath && Vector3.Distance(start, m_elevatorPosition) < m_elevatorActivationDistance)
                {
                    if (!panelElevatorPopup.activeSelf)
                    {
                        panelElevatorPopup.SetActive(true);
                        if (m_StopNavigationButton) m_StopNavigationButton.transform.SetAsLastSibling();
                    }
                }
                else
                {
                    if (panelElevatorPopup.activeSelf) panelElevatorPopup.SetActive(false);
                }
            }
        }

        public void StopNavigation()
        {
            Debug.Log("🛑 Stop Navigation Button ditekan!");
            m_navigationState = NavigationState.NotNavigating;
            if (phantomTargetObject != null) phantomTargetObject.SetActive(false);
            if (panelElevatorPopup != null) panelElevatorPopup.SetActive(false);
            UpdateNavigationUI(m_navigationState);
            DoorbellARPopup.Instance.HidePopup(); // SEMBUNYIKAN TEKAN BEL 3D POPUP
            
            // MENYEMBUNYIKAN UI PERJALANAN SAAT STOP
            if (ActiveNavigationUI.Instance != null)
            {
                ActiveNavigationUI.Instance.HideNavigation();
            }

            // SEMBUNYIKAN SELURUH TARGET RUANGAN DI SCENE
            foreach (var t in Object.FindObjectsOfType<IsNavigationTarget>())
            {
                t.SetVisible(false);
            }
        }

        private void UpdateNavigationUI(NavigationState state)
        {
            if (m_StopNavigationButton)
            {
                m_StopNavigationButton.SetActive(state == NavigationState.Navigating);
                if (state == NavigationState.Navigating)
                {
                    m_StopNavigationButton.transform.SetAsLastSibling();
                }
            }
            if (m_navigationPathObject) m_navigationPathObject.SetActive(state == NavigationState.Navigating);
        }

        private void InitializeNavigationManager()
        {
            if (m_XRSpace == null) m_XRSpace = FindObjectOfType<XRSpace>();
            if (Camera.main != null) m_playerTransform = Camera.main.transform;
            if (m_navigationPathPrefab != null)
            {
                m_navigationPathObject = Instantiate(m_navigationPathPrefab);
                m_navigationPathObject.SetActive(false);
                m_navigationPath = m_navigationPathObject.GetComponent<NavigationPath>();
            }

            if (m_StopNavigationButton != null)
            {
                Button stopBtn = m_StopNavigationButton.GetComponent<Button>();
                if (stopBtn != null)
                {
                    stopBtn.onClick.RemoveListener(StopNavigation);
                    stopBtn.onClick.AddListener(StopNavigation);
                }
            }

            m_managerInitialized = true;
        }

        private Vector3 XRSpaceToUnity(Transform s, Matrix4x4 o, Vector3 p) { return o.MultiplyPoint(s.worldToLocalMatrix.MultiplyPoint(p)); }
        private Vector3 UnityToXRSpace(Transform s, Matrix4x4 o, Vector3 p) { return s.localToWorldMatrix.MultiplyPoint(o.inverse.MultiplyPoint(p)); }
    }
}