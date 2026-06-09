using UnityEngine;
using UnityEngine.UI;

public class ScannerCameraController : MonoBehaviour
{
    public RawImage rawImage;
    private WebCamTexture camTexture;
    private AspectRatioFitter fitter;

    void Start()
    {
        if (WebCamTexture.devices.Length > 0)
        {
            camTexture = new WebCamTexture();
            rawImage.texture = camTexture;
            camTexture.Play();
            
            fitter = rawImage.GetComponent<AspectRatioFitter>();
            if (fitter == null) 
            {
                fitter = rawImage.gameObject.AddComponent<AspectRatioFitter>();
            }
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        }
    }

    void Update()
    {
        if (camTexture != null && camTexture.isPlaying)
        {
            // 1. Handle Aspect Ratio
            float ratio = (float)camTexture.width / (float)camTexture.height;
            int rotation = camTexture.videoRotationAngle;

            // Jika kamera diputar 90 atau 270 derajat (Portrait di Mobile)
            if (rotation == 90 || rotation == 270)
            {
                fitter.aspectRatio = 1f / ratio;
            }
            else
            {
                fitter.aspectRatio = ratio;
            }

            // 2. Handle Rotation (Z-axis)
            // Menggunakan -rotation untuk menyesuaikan arah rotasi Unity UI
            rawImage.rectTransform.localEulerAngles = new Vector3(0, 0, -rotation);
            
            // 3. Handle Mirroring & Inversion
            // Mirroring vertikal (untuk kamera depan atau sensor tertentu)
            float scaleY = camTexture.videoVerticallyMirrored ? -1f : 1f;
            
            // Balik secara horizontal (Mirror Mode) sesuai permintaan user
            float scaleX = -1f; 
            
            rawImage.rectTransform.localScale = new Vector3(scaleX, scaleY, 1f);
        }
    }

    void OnDisable()
    {
        if (camTexture != null)
        {
            camTexture.Stop();
        }
    }

    void OnDestroy()
    {
        if (camTexture != null)
        {
            camTexture.Stop();
        }
    }

    public void SimulateScanSuccess()
    {
        FindObjectOfType<SceneFlowManager>().GoToNavList();
    }
}
