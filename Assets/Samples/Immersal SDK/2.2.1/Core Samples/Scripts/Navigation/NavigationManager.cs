/*===============================================================================
REVISI TOTAL: FIX UI, AUTO-DETECTION, & DATABASE INTEGRATION
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
        [Header("--- KONEKSI BACKEND VERCEL ---")]
        public string serverURL = "https://indoor-nav-backend.vercel.app";

        [Header("--- PENGATURAN UI (LIST RUANGAN) ---")]
        public GameObject listPanel;
        public Transform listContainer;
        public GameObject roomButtonPrefab;
        public GameObject showNavigationButton; // Pasang tombol utama lu di sini

        [Header("--- TARGET VISUAL (PHANTOM) ---")]
        public GameObject phantomTargetObject;

        [Header("--- VISUALISASI JALUR ---")]
        [SerializeField] private GameObject m_navigationPathPrefab = null;
        [SerializeField] private GameObject m_StopNavigationButton = null;

        [Header("--- PARAMETER NAVIGASI ---")]
        [SerializeField] private float m_ArrivedDistanceThreshold = 1.2f;
        [SerializeField] private float m_pathWidth = 0.35f;
        [SerializeField] private float m_heightOffset = 0.15f;

        // --- MODE KOMPATIBILITAS AGAR TIDAK ERROR ---
        [HideInInspector] public bool inEditMode = false;
        public void ToggleEditMode() { inEditMode = !inEditMode; }
        public void InitializeNavigation(NavigationTargetListButton button) { }
        public void ToggleTargetsList() { ToggleNavigationList(); }

        private XRSpace m_XRSpace = null;
        private bool m_managerInitialized = false;
        private IsNavigationTarget m_NavigationTarget = null;
        private Transform m_playerTransform = null;
        private GameObject m_navigationPathObject = null;
        private NavigationPath m_navigationPath = null;

        private enum NavigationState { NotNavigating, Navigating };
        private NavigationState m_navigationState = NavigationState.NotNavigating;

        private static NavigationManager instance = null;
        public static NavigationManager Instance { get { return instance; } }

        void Awake() { if (instance == null) instance = this; }

        private void Start()
        {
            InitializeNavigationManager();
            
            // Safety: Paksa phantom dapet component target
            if (phantomTargetObject != null) 
                m_NavigationTarget = phantomTargetObject.GetComponent<IsNavigationTarget>();

            // Sembunyikan panel list diawal
            if (listPanel != null) listPanel.SetActive(false);
            
            // Cek apakah tombol utama ada
            if (showNavigationButton == null) 
                Debug.LogWarning("⚠️ Tombol 'Show Navigation' belum ditarik ke Inspector!");
        }

        // FUNGSI UTAMA: Dipanggil saat tombol di klik
        public void ToggleNavigationList()
        {
            if (listPanel == null) {
                Debug.LogError("❌ listPanel kosong di Inspector! Tarik Scroll View ke sini.");
                return;
            }

            bool isShow = !listPanel.activeSelf;
            listPanel.SetActive(isShow);

            if (isShow) {
                Debug.Log("📡 Membuka daftar ruangan...");
                StartCoroutine(FetchRoomsFromDatabase());
            }
        }

        IEnumerator FetchRoomsFromDatabase()
        {
            // Bersihkan list lama
            foreach (Transform child in listContainer) Destroy(child.gameObject);

            string url = serverURL + "/api/get-room-list";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    RoomListResponse response = JsonUtility.FromJson<RoomListResponse>(request.downloadHandler.text);
                    if (response != null && response.status)
                    {
                        foreach (RoomItem room in response.data)
                        {
                            GameObject btn = Instantiate(roomButtonPrefab, listContainer);
                            btn.GetComponentInChildren<TMP_Text>().text = room.room_name;
                            string id = room.room_id;
                            btn.GetComponent<Button>().onClick.AddListener(() => OnRoomSelected(id));
                        }
                    }
                }
                else { Debug.LogError("❌ Gagal ambil data: " + request.error); }
            }
        }

        void OnRoomSelected(string roomId)
        {
            if (listPanel != null) listPanel.SetActive(false);
            StartCoroutine(GetRoomCoordinates(roomId));
        }

        IEnumerator GetRoomCoordinates(string roomId)
        {
            string url = serverURL + "/api/map/" + roomId;
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    MapResponse data = JsonUtility.FromJson<MapResponse>(request.downloadHandler.text);
                    if (data != null && !string.IsNullOrEmpty(data.coordinates))
                    {
                        string[] p = data.coordinates.Split(',');
                        float x = float.Parse(p[0], System.Globalization.CultureInfo.InvariantCulture);
                        float y = float.Parse(p[1], System.Globalization.CultureInfo.InvariantCulture);
                        float z = float.Parse(p[2], System.Globalization.CultureInfo.InvariantCulture);

                        if (phantomTargetObject != null)
                        {
                            phantomTargetObject.SetActive(true);
                            phantomTargetObject.transform.localPosition = new Vector3(x, y + 0.1f, z);

                            // --- TAMBAHAN BARU: JURUS PAKSA NYALAKAN VISUAL ---
                            // Ini bakal nyari semua Mesh Renderer di dalam Phantom dan mencentangnya!
                            MeshRenderer[] renderers = phantomTargetObject.GetComponentsInChildren<MeshRenderer>(true);
                            foreach (MeshRenderer mr in renderers)
                            {
                                mr.enabled = true;
                            }
                            // -------------------------------------------------

                            m_navigationState = NavigationState.Navigating;

                            // SIMPAN RIWAYAT (Nama diganti ke Alif)
                            SimpanRiwayat("Muchammad Alif", "Lobby Utama", data.room_name, data.coordinates);
                            UpdateNavigationUI(m_navigationState);
                        }

                        // SIMPAN RIWAYAT (Nama diganti ke Alif)
                        SimpanRiwayat("Muchammad Alif", "Lobby Utama", data.room_name, data.coordinates);
                        UpdateNavigationUI(m_navigationState);
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
            {
                DrawARPath();
            }
        }

        void DrawARPath()
        {
            if (m_playerTransform == null || m_NavigationTarget == null) return;

            Vector3 start = m_playerTransform.position;
            Vector3 target = m_NavigationTarget.transform.position;

            if (Vector3.Distance(start, target) < m_ArrivedDistanceThreshold)
            {
                StopNavigation();
                return;
            }

            Vector3 s = XRSpaceToUnity(m_XRSpace.transform, m_XRSpace.InitialPose, start);
            Vector3 t = XRSpaceToUnity(m_XRSpace.transform, m_XRSpace.InitialPose, target);

            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(s, t, NavMesh.AllAreas, path))
            {
                List<Vector3> corners = new List<Vector3>();
                foreach (var c in path.corners)
                {
                    corners.Add(UnityToXRSpace(m_XRSpace.transform, m_XRSpace.InitialPose, c + new Vector3(0, m_heightOffset, 0)));
                }
                m_navigationPath.GeneratePath(corners, m_XRSpace.transform.up);
                m_navigationPath.pathWidth = m_pathWidth;
            }
        }

        public void StopNavigation()
        {
            m_navigationState = NavigationState.NotNavigating;
            if (phantomTargetObject != null) phantomTargetObject.SetActive(false);

            UpdateNavigationUI(m_navigationState);
        }

        private void UpdateNavigationUI(NavigationState state)
        {
            if (m_StopNavigationButton) m_StopNavigationButton.SetActive(state == NavigationState.Navigating);
            if (m_navigationPathObject) m_navigationPathObject.SetActive(state == NavigationState.Navigating);
        }

        private void InitializeNavigationManager()
        {
            if (m_XRSpace == null) m_XRSpace = FindObjectOfType<XRSpace>();
            
            // WAJIB: Mencari kamera dengan tag MainCamera
            if (Camera.main != null)
                m_playerTransform = Camera.main.transform;
            else
                Debug.LogError("❌ Objek 'Main Camera' tidak ditemukan! Pastikan Tag-nya benar.");

            if (m_navigationPathPrefab != null)
            {
                m_navigationPathObject = Instantiate(m_navigationPathPrefab);
                m_navigationPathObject.SetActive(false);
                m_navigationPath = m_navigationPathObject.GetComponent<NavigationPath>();
            }
            m_managerInitialized = true;
        }

        private Vector3 XRSpaceToUnity(Transform s, Matrix4x4 o, Vector3 p) { return o.MultiplyPoint(s.worldToLocalMatrix.MultiplyPoint(p)); }
        private Vector3 UnityToXRSpace(Transform s, Matrix4x4 o, Vector3 p) { return s.localToWorldMatrix.MultiplyPoint(o.inverse.MultiplyPoint(p)); }
    }
}