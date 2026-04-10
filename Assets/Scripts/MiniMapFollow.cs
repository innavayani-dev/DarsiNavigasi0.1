using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player; // Slot untuk memasukkan karakter pemain

    void LateUpdate()
    {
        // Menyalin posisi X dan Z pemain, tapi tinggi (Y) tetap di atas
        Vector3 newPosition = player.position;
        newPosition.y = transform.position.y;
        transform.position = newPosition;
    }
}