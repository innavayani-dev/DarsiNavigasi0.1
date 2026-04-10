using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;

public class ARPathDistance : MonoBehaviour
{
    [Header("Referensi Posisi")]
    [Tooltip("Masukkan Main Camera dari dalam XR Origin")]
    public Transform userCamera;

    [Tooltip("Masukkan objek target tujuan ruangan")]
    public Transform destinationTarget;

    [Header("Referensi UI")]
    public TextMeshProUGUI distanceText;

    [Header("Pengaturan")]
    public float updateInterval = 0.5f;

    // Variabel untuk menyimpan data rute
    private NavMeshPath calculatedPath;

    void Start()
    {
        // Inisialisasi path
        calculatedPath = new NavMeshPath();

        StartCoroutine(CalculateDistanceRoutine());
    }

    IEnumerator CalculateDistanceRoutine()
    {
        while (true)
        {
            // Pastikan kamera dan target sudah ada
            if (userCamera != null && destinationTarget != null)
            {
                // Meminta NavMesh untuk membuat rute dari Kamera ke Target
                bool pathFound = NavMesh.CalculatePath(userCamera.position, destinationTarget.position, NavMesh.AllAreas, calculatedPath);

                if (pathFound && calculatedPath.status == NavMeshPathStatus.PathComplete)
                {
                    // Hitung total jarak dari rute yang didapat
                    float totalDistance = CalculatePathLength(calculatedPath);

                    // Tampilkan ke UI dengan pembulatan
                    int displayDistance = Mathf.RoundToInt(totalDistance);
                    distanceText.text = displayDistance.ToString() + " m";
                }
                else
                {
                    // Jika rute belum ketemu (misal tracking AR belum pas)
                    distanceText.text = "Mencari rute...";
                }
            }

            // Jeda sebelum menghitung ulang
            yield return new WaitForSeconds(updateInterval);
        }
    }

    float CalculatePathLength(NavMeshPath path)
    {
        float distance = 0.0f;

        if (path.corners.Length < 2)
            return distance;

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            distance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
        }

        return distance;
    }
}