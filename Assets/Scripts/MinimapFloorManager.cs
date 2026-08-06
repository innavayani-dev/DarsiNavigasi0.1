using UnityEngine;

[System.Serializable]
public class FloorLayerInfo
{
    [Tooltip("Nama lantai (hanya label)")]
    public string floorName;
    
    [Tooltip("Tinggi (Y) minimal untuk lantai ini")]
    public float yThresholdMin;
    
    [Tooltip("Tinggi (Y) maksimal untuk lantai ini")]
    public float yThresholdMax;
    
    [Tooltip("Index Layer Unity untuk lantai ini (misal LG = 9, Lantai 1 = 10)")]
    public int layerIndex;
}

public class MinimapFloorManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Kamera AR Utama (XR Origin / Main Camera)")]
    public Transform playerTransform;
    
    [Tooltip("Kamera Minimap")]
    public Camera minimapCamera;
    
    [Header("Settings")]
    [Tooltip("Layer Index untuk Jalur Navigasi (contoh: 8) yang harus selalu terlihat")]
    public int basePathLayerIndex = 8;
    
    [Header("Floors Configuration")]
    public FloorLayerInfo[] floors;

    private int currentFloorLayer = -1;

    void Start()
    {
        if (minimapCamera == null) minimapCamera = GetComponent<Camera>();
        if (playerTransform == null && Camera.main != null) playerTransform = Camera.main.transform;
    }

    void Update()
    {
        if (playerTransform == null || minimapCamera == null || floors == null || floors.Length == 0) return;

        float playerY = playerTransform.position.y;

        int detectedLayer = -1;
        foreach (var floor in floors)
        {
            if (playerY >= floor.yThresholdMin && playerY < floor.yThresholdMax)
            {
                detectedLayer = floor.layerIndex;
                break;
            }
        }

        // Jika user berada di range lantai tertentu dan layernya berbeda dari sebelumnya
        if (detectedLayer != -1 && detectedLayer != currentFloorLayer)
        {
            currentFloorLayer = detectedLayer;
            
            // Perbaikan: Selalu tampilkan SEMUA layer gedung (Default + Jalur + Semua Layer Lantai)
            // Mengapa? Karena kamera minimap sudah berada di ketinggian +3 meter dan menunduk ke bawah.
            // Secara otomatis, atap dan lantai di atasnya akan terpotong oleh kamera, jadi kita tidak perlu
            // secara agresif menyembunyikan layer. Hal ini mengatasi masalah BIM yang hilang karena belum di-assign.
            int mask = (1 << 0) | (1 << basePathLayerIndex);
            foreach (var floor in floors)
            {
                mask |= (1 << floor.layerIndex);
            }
            
            minimapCamera.cullingMask = mask;
            
            Debug.Log($"[MinimapFloorManager] Posisi: Lantai {currentFloorLayer}. Menampilkan seluruh BIM secara otomatis.");
        }
    }
}
