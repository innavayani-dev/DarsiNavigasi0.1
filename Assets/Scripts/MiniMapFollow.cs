using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapFollow : MonoBehaviour
{
    public Transform player; // Ini target Main Camera / XR Origin lu
    public float heightOffset = 10f; // Ketinggian kamera minimap
    
    [Header("Smoothing")]
    public float moveSmoothSpeed = 10f; // Kecepatan menghaluskan posisi
    public float rotationSmoothSpeed = 8f; // Kecepatan menghaluskan rotasi

    void LateUpdate()
    {
        if (player != null)
        {
            // 1. Hitung target posisi
            Vector3 targetPosition = player.position;
            targetPosition.y = player.position.y + heightOffset;
            
            // Haluskan posisi dengan Lerp (mencegah patah-patah/jitter)
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSmoothSpeed);

            // 2. Hitung target rotasi (X selalu 90 menunduk, Y mengikuti player)
            Quaternion targetRotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
            
            // Haluskan rotasi dengan Slerp
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);
        }
    }
}