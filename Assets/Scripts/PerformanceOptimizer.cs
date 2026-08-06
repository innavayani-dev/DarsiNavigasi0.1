using UnityEngine;
using UnityEngine.Rendering;

public class PerformanceOptimizer : MonoBehaviour
{
    void Awake()
    {
        // Targetkan 60 FPS agar pergerakan kamera AR dan Minimap terasa mulus (tidak pusing)
        Application.targetFrameRate = 60;
        
        // Matikan vSync agar tidak dipaksa sinkron dengan layar (bisa menyebabkan drop FPS)
        QualitySettings.vSyncCount = 0;

        // --- OPTIMASI SHADOW MASIF ---
        // Cari seluruh komponen MeshRenderer yang ada di dalam scene (terutama BIM Graha & Tower)
        MeshRenderer[] allRenderers = FindObjectsOfType<MeshRenderer>(true);
        int optimizedCount = 0;
        foreach (MeshRenderer renderer in allRenderers)
        {
            // Matikan fitur memantulkan bayangan
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            // Matikan fitur menerima bayangan
            renderer.receiveShadows = false;
            optimizedCount++;
        }
        Debug.Log($"Performance Optimizer: Shadows disabled on {optimizedCount} 3D objects.");
    }

    void Start()
    {
        // Jalankan pengecekan setiap 1 detik untuk menyembunyikan point cloud yang di-load secara dinamis
        InvokeRepeating("HidePointClouds", 0.5f, 1.0f);
    }

    void HidePointClouds()
    {
        GameObject xrSpace = GameObject.Find("XR Space");
        Transform xrSpaceTransform = xrSpace != null ? xrSpace.transform : null;

        if (xrSpaceTransform == null) return;

        MeshRenderer[] allRenderers = xrSpace.GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer renderer in allRenderers)
        {
            string objName = renderer.gameObject.name.ToLower();
            // Hanya sembunyikan jika namanya mengandung vis, bytes, pointcloud, atau bagian dari Immersal Map
            if (objName.Contains(".vis") || objName.Contains(".bytes") || objName.Contains("pointcloud") || renderer.gameObject.GetComponent("Immersal.XR.XRMap") != null)
            {
                if (renderer.enabled)
                {
                    renderer.enabled = false;
                }
            }
        }
    }
}
