using UnityEngine;
using UnityEngine.UI;
using ZXing;
using UnityEngine.Android;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class QRCodeMapping
{
    [Tooltip("Teks QR Code yang di-scan")]
    public string qrCodeText;
    [Tooltip("Nama ruangan awal di AR (harus sama persis dengan Target Name)")]
    public string targetRoomName;
}

public class ScannerCameraController : MonoBehaviour
{
    [Header("UI References")]
    public RawImage rawImage;
    public AspectRatioFitter aspectRatioFitter;

    [Header("QR Code Settings")]
    [Tooltip("Daftar pemetaan teks QR Code ke Nama Ruangan di AR")]
    public List<QRCodeMapping> qrMappings = new List<QRCodeMapping>();
    
    [Tooltip("Teks QR fallback jika tidak ada di daftar mapping")]
    public string expectedQRCodeText = "START_NAV_RSI_AYANI";
    public float scanInterval = 0.5f;

    private WebCamTexture webCamTexture;
    private BarcodeReader barcodeReader;
    private float scanTimer = 0f;
    private bool isScanning = false;
    private bool isCameraReady = false;
    private bool isDecoding = false;
    private Color32[] pixelBuffer;
    private string decodedResult = null;

    void Start()
    {
        // 1. Sembunyikan layar putih di awal
        if (rawImage != null)
        {
            rawImage.enabled = false;
            // Pastikan warna RawImage adalah putih polos agar tidak mengubah warna kamera
            rawImage.color = Color.white;
            
            // Dapatkan atau tambahkan AspectRatioFitter agar tidak lonjong/gepeng
            if (aspectRatioFitter == null)
            {
                aspectRatioFitter = rawImage.GetComponent<AspectRatioFitter>();
                if (aspectRatioFitter == null)
                {
                    aspectRatioFitter = rawImage.gameObject.AddComponent<AspectRatioFitter>();
                }
            }
            aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        }

        // 2. Minta Izin Kamera (Android)
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
#endif

        // 3. Konfigurasi ZXing Reader
        barcodeReader = new BarcodeReader();
        barcodeReader.AutoRotate = true;
        barcodeReader.Options.TryHarder = true;

        // 4. Mulai Kamera
        StartCoroutine(StartCameraCoroutine());
    }

    private IEnumerator StartCameraCoroutine()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Tunggu sistem Android memunculkan prompt izin jika baru diminta di Start()
        int maxWaitSeconds = 15;
        while (!Permission.HasUserAuthorizedPermission(Permission.Camera) && maxWaitSeconds > 0)
        {
            yield return new WaitForSeconds(1f);
            maxWaitSeconds--;
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Debug.LogError("User denied camera permission.");
            yield break; // Hentikan coroutine jika tidak diizinkan
        }
#else
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.LogWarning("Editor/PC WebCam authorization failed, but continuing anyway as it is often ignored on Windows.");
        }
#endif

        // Tunggu sedikit ekstra agar sistem OS merefresh device list kamera setelah izin diberikan
        yield return new WaitForSeconds(0.5f);

        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length > 0)
        {
            // Gunakan kamera belakang jika ada
            string backCamName = devices[0].name;
            for (int i = 0; i < devices.Length; i++)
            {
                if (!devices[i].isFrontFacing)
                {
                    backCamName = devices[i].name;
                    break;
                }
            }

            // Gunakan resolusi standar HD (1280x720) atau Full HD. 
            // Jika kita gunakan Screen.width/height, Android sering kali melakukan ZOOM (crop) 
            // secara paksa dan menurunkan FPS drastis menjadi patah-patah.
            webCamTexture = new WebCamTexture(backCamName, 1280, 720, 30);
            if (rawImage != null) rawImage.texture = webCamTexture;
            webCamTexture.Play();

            // Tunggu sampai resolusi kamera terbaca dengan benar (bukan 16x16)
            while (webCamTexture.width <= 16 || webCamTexture.height <= 16)
            {
                yield return null;
            }

            // Setup tampilan kamera agar proporsional dan tidak gepeng/lonjong
            SetupCameraView();

            // Tampilkan kamera sekarang karena sudah siap
            if (rawImage != null) rawImage.enabled = true;
            
            isCameraReady = true;
            isScanning = true;
        }
        else
        {
            Debug.LogError("Tidak ada kamera terdeteksi di perangkat ini!");
        }
    }

    private void SetupCameraView()
    {
        if (rawImage == null || webCamTexture == null) return;

        // 1. Atur Rotasi Kamera
        int videoRotationAngle = webCamTexture.videoRotationAngle;
        rawImage.rectTransform.localEulerAngles = new Vector3(0, 0, -videoRotationAngle);

        // 2. Atur Mirroring (Jika kamera depan)
        bool isMirrored = webCamTexture.videoVerticallyMirrored;
        rawImage.rectTransform.localScale = new Vector3(1, isMirrored ? -1 : 1, 1);

        // 3. Atur Aspect Ratio (Penting agar tidak oval/lonjong)
        // Karena AspectRatioFitter menempel pada RawImage yang sudah dirotasi, 
        // rasio lokalnya harus tetap width/height dari tekstur asli tanpa dibalik.
        float videoRatio = (float)webCamTexture.width / (float)webCamTexture.height;
        aspectRatioFitter.aspectRatio = videoRatio;
    }

    void Update()
    {
        // Proses hasil decode di Main Thread agar aman mengeksekusi fungsi Unity
        if (!string.IsNullOrEmpty(decodedResult))
        {
            string resultText = decodedResult;
            decodedResult = null;
            if (isScanning) // Pastikan masih dalam mode scanning
            {
                ProcessQRCode(resultText);
            }
        }

        if (isScanning && isCameraReady && webCamTexture != null && webCamTexture.isPlaying && !isDecoding)
        {
            scanTimer += Time.deltaTime;
            if (scanTimer >= scanInterval)
            {
                scanTimer = 0f;
                ScanQRCode();
            }
        }
    }

    private void ScanQRCode()
    {
        if (isDecoding) return;

        try
        {
            isDecoding = true;

            int width = webCamTexture.width;
            int height = webCamTexture.height;

            if (pixelBuffer == null || pixelBuffer.Length != width * height)
            {
                pixelBuffer = new Color32[width * height];
            }

            // Membaca pixel tanpa alokasi memori berulang (hindari GC Spike penyebab patah-patah)
            webCamTexture.GetPixels32(pixelBuffer);

            // Jalankan proses decode berat di background thread (hindari freeze 0.1-0.2 detik)
            System.Threading.ThreadPool.QueueUserWorkItem(_ => 
            {
                try 
                {
                    var result = barcodeReader.Decode(pixelBuffer, width, height);
                    if (result != null)
                    {
                        decodedResult = result.Text.Trim();
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("Decode error: " + ex.Message);
                }
                finally 
                {
                    isDecoding = false;
                }
            });
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Error reading pixels: " + ex.Message);
            isDecoding = false;
        }
    }

    private void ProcessQRCode(string scannedText)
    {
        Debug.Log("QR Code Detected: " + scannedText);

        bool matchFound = false;
        string targetRoom = "";

        // Bersihkan teks: hapus newline, spasi, dan underscore agar lebih tahan terhadap typo
        string cleanScanned = scannedText.Replace("\r", "").Replace("\n", "").Replace(" ", "").Replace("_", "").Trim();

        // --- HARDCODED QR MAPPINGS ---
        Dictionary<string, string> builtInMappings = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "Poli Tengah", "Minimarket Kantin" },
            { "Poli Tengahe", "Minimarket Kantin" }, // Alias untuk typo
            { "Farmasi Graha", "Farmasi" },
            { "Farmmasi Graha", "Farmasi" }, // Alias untuk typo
            { "farmasi tower lantai 6", "Farmasi LT6" },
            { "graha lantai 1", "Hemodialisis LT1" },
            { "graha lantai 2", "Irna Bedah LT2" },
            { "graha lantai 3", "Irna Anak LT3" },
            { "graha lantai 4", "Irna Dewasa LT4" },
            { "graha lantai 5", "R. Managemen LT5" },
            { "tower lantai 1", "Gedung Tower LT1" },
            { "tower lantai 2", "IGD LT2" },
            { "tower lantai 3", "Laboratium LT3" },
            { "tower lantai 4", "R. Rawat Intensif" },
            { "tower lantai 5", "R. Operasi LT5" },
            { "tower lantai 6", "Poli Eksekutif LT6" },
            { "tower lantai 7", "An-Nisa Irna Bersalin LT7" },
            { "tower lantai 8", "Al-Kautsar Irna Anak LT8" },
            { "tower lantai 9", "Ar-Rayyan Irna Dewasa LT9" },
            { "tower lantai 10", "Ar-Radhiin Irna Dewasa LT10" },
            { "tower lantai 11", "Ar-Raudhah Irna Eksekutif LT11" },
            { "tower lantai 12", "Kordik LT12" },
            { "tower lantai 13", "Yarsis LT13" }
        };

        // Cek di hardcoded mapping (abaikan huruf besar/kecil, spasi, dan underscore)
        foreach (var mapping in builtInMappings)
        {
            string cleanMapping = mapping.Key.Replace("\r", "").Replace("\n", "").Replace(" ", "").Replace("_", "").Trim();
            if (string.Equals(cleanScanned, cleanMapping, System.StringComparison.OrdinalIgnoreCase))
            {
                matchFound = true;
                targetRoom = mapping.Value;
                break;
            }
        }

        // Cek di daftar mapping Inspector sebagai tambahan (jika ada)
        if (!matchFound)
        {
            foreach (var mapping in qrMappings)
            {
                string cleanMapping = mapping.qrCodeText.Replace("\r", "").Replace("\n", "").Replace(" ", "").Replace("_", "").Trim();
                if (string.Equals(cleanScanned, cleanMapping, System.StringComparison.OrdinalIgnoreCase))
                {
                    matchFound = true;
                    targetRoom = mapping.targetRoomName;
                    break;
                }
            }
        }

        // Cek backward compatibility (fallback terakhir)
        string cleanExpected = expectedQRCodeText.Replace("\r", "").Replace("\n", "").Replace(" ", "").Replace("_", "").Trim();
        if (!matchFound && string.Equals(cleanScanned, cleanExpected, System.StringComparison.OrdinalIgnoreCase))
        {
            matchFound = true;
        }

        if (matchFound)
        {
            isScanning = false;
            Debug.Log("Valid QR Code! Navigating to AR Navigasi...");
            
            // Simpan nama ruangan target ke PlayerPrefs
            if (!string.IsNullOrEmpty(targetRoom))
            {
                PlayerPrefs.SetString("InitialStartRoom", targetRoom);
                PlayerPrefs.Save();
                Debug.Log("Menyimpan Start Room: " + targetRoom);
            }
            else
            {
                // Hapus memori jika pakai QR lama tanpa target
                PlayerPrefs.DeleteKey("InitialStartRoom");
            }

            if (webCamTexture != null && webCamTexture.isPlaying)
            {
                webCamTexture.Stop();
            }
            SimulateScanSuccess();
        }
        else
        {
            Debug.LogWarning("QR Code tidak dikenali/tidak cocok: " + scannedText);
            ResumeScanning();
        }
    }

    public void SimulateScanSuccess()
    {
        Debug.Log("Simulation: Loading AR Navigasi Scene...");
        StartCoroutine(TransitionToARScene());
    }

    private IEnumerator TransitionToARScene()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
            Destroy(webCamTexture);
            webCamTexture = null;
        }
        
        // Beri waktu OS Android untuk benar-benar menutup kamera (hardware release)
        // sebelum ARCore mencoba membukanya di scene berikutnya.
        yield return new WaitForSeconds(0.5f);
        
        SceneManager.LoadScene("6_AR Navigasi");
    }

    public void OpenGallery()
    {
        // Jeda scanner kamera saat buka galeri
        isScanning = false;
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Pause();
        }

        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                Debug.Log("Image path: " + path);
                
                // Load gambar dari galeri ke Texture2D
                Texture2D texture = NativeGallery.LoadImageAtPath(path, -1, false);
                if (texture == null)
                {
                    Debug.LogError("Gagal meload gambar dari " + path);
                    ResumeScanning();
                    return;
                }

                try
                {
                    // Scan QR dari Texture
                    Color32[] pixels = texture.GetPixels32();
                    var result = barcodeReader.Decode(pixels, texture.width, texture.height);

                    if (result != null)
                    {
                        Debug.Log("QR Code dari Galeri Detected: " + result.Text);
                        ProcessQRCode(result.Text.Trim());
                    }
                    else
                    {
                        Debug.LogWarning("Tidak ada QR Code yang ditemukan di gambar tersebut.");
                        ResumeScanning();
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("Error decoding QR Code dari galeri: " + ex.Message);
                    ResumeScanning();
                }
                finally
                {
                    Destroy(texture); // Pastikan memory dibersihkan
                }
            }
            else
            {
                // Jika user batal memilih gambar
                ResumeScanning();
            }
        });
    }

    private void ResumeScanning()
    {
        isScanning = true;
        if (webCamTexture != null && !webCamTexture.isPlaying)
        {
            webCamTexture.Play();
        }
    }

    void OnDestroy()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
            Destroy(webCamTexture);
            webCamTexture = null;
        }
    }
}
