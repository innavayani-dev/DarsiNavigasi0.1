using UnityEngine;
using UnityEditor;
using Immersal.Samples.Navigation;
using System.Collections.Generic;

public class WaypointAutoConnect : EditorWindow
{
    private float maxDistance = 3.0f;

    [MenuItem("Darsi/Waypoint Auto Connect")]
    public static void ShowWindow()
    {
        GetWindow<WaypointAutoConnect>("Auto Connect Waypoints");
    }

    void OnGUI()
    {
        GUILayout.Label("Auto Connect Navigation Waypoints", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Tools ini akan menghubungkan semua Waypoint yang berdekatan secara otomatis. Sangat berguna untuk menyambungkan titik-titik di tangga atau lorong tanpa harus klik manual satu per satu.", MessageType.Info);
        
        maxDistance = EditorGUILayout.FloatField("Maksimal Jarak (Meter)", maxDistance);

        if (GUILayout.Button("Connect All Nearby Waypoints"))
        {
            ConnectWaypoints();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Clear All Connections"))
        {
            ClearWaypoints();
        }
    }

    private void ConnectWaypoints()
    {
        Waypoint[] allWaypoints = FindObjectsOfType<Waypoint>();
        int connectionsMade = 0;

        foreach (Waypoint wp in allWaypoints)
        {
            if (wp.neighbours == null) wp.neighbours = new List<Waypoint>();
            
            foreach (Waypoint other in allWaypoints)
            {
                if (wp == other) continue;
                
                float dist = Vector3.Distance(wp.transform.position, other.transform.position);
                // Jika jarak di bawah batas maksimal, hubungkan
                if (dist <= maxDistance)
                {
                    if (!wp.neighbours.Contains(other))
                    {
                        wp.neighbours.Add(other);
                        EditorUtility.SetDirty(wp); // Simpan perubahan di scene
                        connectionsMade++;
                    }
                }
            }
        }
        
        Debug.Log($"[Darsi Navigasi] Berhasil menghubungkan {connectionsMade} jalur antar Waypoint dalam radius {maxDistance} meter!");
    }
    
    private void ClearWaypoints()
    {
        Waypoint[] allWaypoints = FindObjectsOfType<Waypoint>();
        foreach (Waypoint wp in allWaypoints)
        {
            if (wp.neighbours != null)
            {
                wp.neighbours.Clear();
                EditorUtility.SetDirty(wp);
            }
        }
        Debug.Log($"[Darsi Navigasi] Semua koneksi Waypoint telah direset / dihapus.");
    }
}
