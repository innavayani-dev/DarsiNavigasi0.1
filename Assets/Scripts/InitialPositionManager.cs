using UnityEngine;
using Immersal.Samples.Navigation;

public class InitialPositionManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Objek yang akan dipindah (Biasanya AR Session Origin atau XR Space). Jika kosong, akan otomatis mencari parent dari Main Camera.")]
    public Transform originToMove;

    [Tooltip("Ketinggian tambahan (opsional) agar kamera tidak berada tepat di lantai.")]
    public float heightOffset = 1.5f;

    void Start()
    {
        // 1. Cek apakah ada data ruangan yang disimpan dari Scanner Scene
        string startRoom = PlayerPrefs.GetString("InitialStartRoom", "");

        if (!string.IsNullOrEmpty(startRoom))
        {
            Debug.Log($"[InitialPositionManager] Mencari posisi awal untuk ruangan: {startRoom}");
            FindAndSetPosition(startRoom);
            
            // Opsional: Hapus memori setelah dipakai agar tidak terulang jika user langsung buka AR scene lagi
            // PlayerPrefs.DeleteKey("InitialStartRoom"); 
        }
        else
        {
            Debug.Log("[InitialPositionManager] Tidak ada start room spesifik dari QR Code.");
        }
    }

    private void FindAndSetPosition(string roomName)
    {
        // Cari semua IsNavigationTarget di dalam scene
        IsNavigationTarget[] allTargets = FindObjectsOfType<IsNavigationTarget>();
        bool found = false;

        foreach (var target in allTargets)
        {
            if (target.targetName == roomName)
            {
                found = true;
                
                // Tentukan objek yang akan dipindah
                Transform targetToMove = originToMove;
                if (targetToMove == null && Camera.main != null)
                {
                    targetToMove = Camera.main.transform.parent;
                }

                if (targetToMove != null)
                {
                    // Ambil posisi ruangan tersebut
                    Vector3 newPosition = target.transform.position;
                    
                    // Pindah secara menyeluruh (X,Y,Z) dan Rotasi
                    targetToMove.position = new Vector3(newPosition.x, newPosition.y + heightOffset, newPosition.z);
                    targetToMove.rotation = target.transform.rotation;

                    Debug.Log($"[InitialPositionManager] Berhasil memindahkan {targetToMove.name} ke koordinat penuh (X,Y,Z) di posisi {target.targetName}");
                }
                else
                {
                    Debug.LogWarning("[InitialPositionManager] Tidak dapat menemukan Origin/Kamera untuk dipindahkan!");
                }
                
                break;
            }
        }

        if (!found)
        {
            Debug.LogWarning($"[InitialPositionManager] Ruangan bernama '{roomName}' tidak ditemukan di daftar Navigation Target!");
        }
    }
}
