using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DoorbellARPopup : MonoBehaviour
{
    private static DoorbellARPopup instance;
    public static DoorbellARPopup Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("DoorbellARPopupManager");
                instance = go.AddComponent<DoorbellARPopup>();
            }
            return instance;
        }
    }

    private GameObject worldPopupObj;
    private Transform targetTransform;

    // Daftar ruangan di berbagai lantai yang mewajibkan tekan bel sebelum masuk
    private readonly HashSet<string> doorbellRooms = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
    {
        "R. Rawat Intensif LT4", "R. Operasi LT5", "CSSD LT5", 
        "An-Nisa Irna Bersalin LT7", "Al-Kautsar Irna Anak LT8", 
        "Ar-Rayyan Irna Dewasa LT9", "Ar-Radhiin Irna Dewasa LT10", 
        "Ar-Raudhah Irna Eksekutif LT11"
    };

    public static bool IsRoomRequiringDoorbell(string roomName)
    {
        if (string.IsNullOrEmpty(roomName)) return false;
        
        if (roomName.Contains("Rawat Intensif", System.StringComparison.OrdinalIgnoreCase) ||
            roomName.Contains("Operasi", System.StringComparison.OrdinalIgnoreCase) ||
            roomName.Contains("CSSD", System.StringComparison.OrdinalIgnoreCase) ||
            roomName.Contains("An-Nisa", System.StringComparison.OrdinalIgnoreCase) ||
            roomName.Contains("Al-Kautsar", System.StringComparison.OrdinalIgnoreCase) ||
            roomName.Contains("Ar-Rayyan", System.StringComparison.OrdinalIgnoreCase) ||
            roomName.Contains("Ar-Radhiin", System.StringComparison.OrdinalIgnoreCase) ||
            roomName.Contains("Ar-Raudhah", System.StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        return false;
    }

    public void CheckAndShow(string roomName, Transform targetParent)
    {
        HidePopup();

        if (!IsRoomRequiringDoorbell(roomName) || targetParent == null) return;

        targetTransform = targetParent;
        CreateWorldSpacePopup();
    }

    public void HidePopup()
    {
        if (worldPopupObj != null) Destroy(worldPopupObj);
        worldPopupObj = null;
        targetTransform = null;
    }

    private static Sprite roundedSprite;
    private static Sprite GetRoundedSprite()
    {
        if (roundedSprite != null) return roundedSprite;

        // Ambil resource bawaan Unity untuk UI rounded kotak
        Sprite builtin = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        if (builtin != null)
        {
            roundedSprite = builtin;
            return roundedSprite;
        }

        // Failsafe generator: Buat tekstur rounded 64x64 secara dinamis via C#
        int size = 64;
        int radius = 18;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color32[] colors = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool inside = true;
                if (x < radius && y < radius)
                    inside = Vector2.Distance(new Vector2(x, y), new Vector2(radius - 1, radius - 1)) <= radius;
                else if (x > size - radius && y < radius)
                    inside = Vector2.Distance(new Vector2(x, y), new Vector2(size - radius, radius - 1)) <= radius;
                else if (x < radius && y > size - radius)
                    inside = Vector2.Distance(new Vector2(x, y), new Vector2(radius - 1, size - radius)) <= radius;
                else if (x > size - radius && y > size - radius)
                    inside = Vector2.Distance(new Vector2(x, y), new Vector2(size - radius, size - radius)) <= radius;

                colors[y * size + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
            }
        }
        tex.SetPixels32(colors);
        tex.Apply();

        roundedSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        return roundedSprite;
    }

    // --- WORLD SPACE 3D POP-UP (Rounded Box Elegan, Presisi Di Atas Pin) ---
    private void CreateWorldSpacePopup()
    {
        worldPopupObj = new GameObject("FloatingDoorbellWorldPopup");
        worldPopupObj.transform.SetParent(targetTransform, false);
        worldPopupObj.transform.localPosition = new Vector3(0f, 0.6f, 0f); // Tepat di atas objek pin AR

        Canvas canvas = worldPopupObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform rect = worldPopupObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(360, 110);
        worldPopupObj.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);

        // Background Kotak Merah Rounded (9-Sliced)
        Image bg = worldPopupObj.AddComponent<Image>();
        bg.sprite = GetRoundedSprite();
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0.82f, 0.12f, 0.12f, 0.96f); // Merah medis

        // Outline Emas Melingkari Sudut Rounded
        Outline outline = worldPopupObj.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.85f, 0f, 1f); // Kuning emas
        outline.effectDistance = new Vector2(2.5f, -2.5f);

        GameObject textObj = new GameObject("WorldText");
        textObj.transform.SetParent(worldPopupObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12, 6);
        textRect.offsetMax = new Vector2(-12, -6);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "<b>SILAHKAN TEKAN BEL</b>\n<size=80%>Sebelum Memasuki Ruangan</size>";
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.enableWordWrapping = true;
    }

    private void LateUpdate()
    {
        // Kunci orientasi pop-up 3D sejajar sempurna dengan rotasi kamera HP pasien
        if (worldPopupObj != null)
        {
            Camera cam = Camera.main;
            if (cam == null) cam = Object.FindObjectOfType<Camera>();
            
            if (cam != null)
            {
                worldPopupObj.transform.rotation = cam.transform.rotation;
            }
        }
    }
}
