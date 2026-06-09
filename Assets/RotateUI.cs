using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateUI : MonoBehaviour
{
    public float kecepatanPutar = -200f; // Angka minus biar muter searah jarum jam

    void Update()
    {
        // Perintah untuk memutar objek ini di sumbu Z (sumbu depan-belakang)
        transform.Rotate(0, 0, kecepatanPutar * Time.deltaTime);
    }
}