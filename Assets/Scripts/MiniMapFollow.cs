using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapFollow : MonoBehaviour
{
    public Transform player; // Ini target Main Camera / XR Origin lu
    public float heightOffset = 10f; // Ketinggian kamera minimap

    void LateUpdate()
    {
        if (player != null)
        {
            // 1. Kamera ngikutin posisi player (X dan Z), tapi Y-nya di atas
            Vector3 newPosition = player.position;
            newPosition.y = player.position.y + heightOffset;
            transform.position = newPosition;

            // 2. INI KUNCINYA: Kamera minimap muter ngikutin arah hadap player
            // X tetep 90 (biar nunduk ke bawah)
            // Y ngikutin rotasi Y player (kanan-kiri)
            // Z dibikin 0
            transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
        }
    }
}