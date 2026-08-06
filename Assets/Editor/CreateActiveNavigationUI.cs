using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class CreateActiveNavigationUI : EditorWindow
{
    [MenuItem("Tools/Darsi/Create Active Navigation UI")]
    public static void CreateUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas tidak ditemukan di scene! Pastikan ada Canvas sebelum membuat UI ini.");
            return;
        }

        // 1. Buat Container Utama (Panel)
        GameObject panelObj = new GameObject("ActiveNavigationPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        RectTransform panelRt = panelObj.AddComponent<RectTransform>();
        // Set anchor ke bawah layar, stretch horizontal
        panelRt.anchorMin = new Vector2(0, 0);
        panelRt.anchorMax = new Vector2(1, 0);
        panelRt.pivot = new Vector2(0.5f, 0);
        panelRt.anchoredPosition = new Vector2(0, 40); // Jarak 40 pixel dari bawah
        panelRt.sizeDelta = new Vector2(-60, 160); // Margin kiri kanan 30px, Tinggi 160px
        
        // Tambahkan Image sebagai background utama
        Image bg = panelObj.AddComponent<Image>();
        bg.color = new Color(1f, 1f, 1f, 0.98f); // Putih sedikit transparan
        
        // Ambil sprite bawaan Unity yang ujungnya melengkung
        Sprite roundedSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (roundedSprite != null)
        {
            bg.sprite = roundedSprite;
            bg.type = Image.Type.Sliced; // Memastikan sudutnya tetap melengkung walau ditarik
        }

        // 2. Buat Start Group (Separuh atas)
        GameObject startGroup = new GameObject("StartGroup");
        startGroup.transform.SetParent(panelObj.transform, false);
        RectTransform sgRt = startGroup.AddComponent<RectTransform>();
        sgRt.anchorMin = new Vector2(0, 0.5f);
        sgRt.anchorMax = new Vector2(1, 1);
        sgRt.offsetMin = new Vector2(10, 5); // Beri sedikit jarak (margin) dari tepi panel
        sgRt.offsetMax = new Vector2(-10, -5);
        
        // Tambahkan background transparan melengkung untuk StartGroup
        Image sgBg = startGroup.AddComponent<Image>();
        sgBg.color = new Color(0, 0, 0, 0.03f); // Abu-abu sangat tipis agar batasnya terlihat
        if (roundedSprite != null) { sgBg.sprite = roundedSprite; sgBg.type = Image.Type.Sliced; }

        // Ikon Start (Biru)
        GameObject startIconObj = new GameObject("StartIcon");
        startIconObj.transform.SetParent(startGroup.transform, false);
        RectTransform siRt = startIconObj.AddComponent<RectTransform>();
        siRt.anchorMin = new Vector2(0, 0.5f);
        siRt.anchorMax = new Vector2(0, 0.5f);
        siRt.pivot = new Vector2(0, 0.5f);
        siRt.anchoredPosition = new Vector2(30, 0);
        siRt.sizeDelta = new Vector2(35, 35);
        Image startIcon = startIconObj.AddComponent<Image>();
        startIcon.color = new Color(0.2f, 0.5f, 0.9f); // Biru

        // Teks Start
        GameObject startTextObj = new GameObject("StartText");
        startTextObj.transform.SetParent(startGroup.transform, false);
        RectTransform stRt = startTextObj.AddComponent<RectTransform>();
        stRt.anchorMin = new Vector2(0, 0);
        stRt.anchorMax = new Vector2(1, 1);
        stRt.offsetMin = new Vector2(85, 0);
        stRt.offsetMax = new Vector2(-20, 0);
        TextMeshProUGUI startText = startTextObj.AddComponent<TextMeshProUGUI>();
        startText.text = "Bank Syariah RSI";
        startText.color = new Color(0.3f, 0.3f, 0.3f); // Abu-abu gelap
        startText.fontSize = 24;
        startText.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Capline;

        // 3. Buat Destination Group (Separuh bawah)
        GameObject destGroup = new GameObject("DestGroup");
        destGroup.transform.SetParent(panelObj.transform, false);
        RectTransform dgRt = destGroup.AddComponent<RectTransform>();
        dgRt.anchorMin = new Vector2(0, 0);
        dgRt.anchorMax = new Vector2(1, 0.5f);
        dgRt.offsetMin = new Vector2(10, 5); // Beri sedikit jarak (margin) dari tepi panel
        dgRt.offsetMax = new Vector2(-10, -5);
        
        // Tambahkan background transparan melengkung untuk DestGroup
        Image dgBg = destGroup.AddComponent<Image>();
        dgBg.color = new Color(0, 0, 0, 0.03f); // Abu-abu sangat tipis agar batasnya terlihat
        if (roundedSprite != null) { dgBg.sprite = roundedSprite; dgBg.type = Image.Type.Sliced; }

        // Ikon Destination (Merah)
        GameObject destIconObj = new GameObject("DestIcon");
        destIconObj.transform.SetParent(destGroup.transform, false);
        RectTransform diRt = destIconObj.AddComponent<RectTransform>();
        diRt.anchorMin = new Vector2(0, 0.5f);
        diRt.anchorMax = new Vector2(0, 0.5f);
        diRt.pivot = new Vector2(0, 0.5f);
        diRt.anchoredPosition = new Vector2(30, 0);
        diRt.sizeDelta = new Vector2(35, 35);
        Image destIcon = destIconObj.AddComponent<Image>();
        destIcon.color = new Color(0.9f, 0.2f, 0.2f); // Merah

        // Teks Destination
        GameObject destTextObj = new GameObject("DestText");
        destTextObj.transform.SetParent(destGroup.transform, false);
        RectTransform dtRt = destTextObj.AddComponent<RectTransform>();
        dtRt.anchorMin = new Vector2(0, 0);
        dtRt.anchorMax = new Vector2(1, 1);
        dtRt.offsetMin = new Vector2(85, 0);
        dtRt.offsetMax = new Vector2(-20, 0);
        TextMeshProUGUI destText = destTextObj.AddComponent<TextMeshProUGUI>();
        destText.text = "Memuat Tujuan...";
        destText.color = Color.black;
        destText.fontSize = 32;
        destText.fontStyle = FontStyles.Bold;
        destText.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Capline;

        // 4. Buat Garis Penghubung (Connecting Line - Bentuk satu garis utuh tapi lonjong)
        GameObject lineObj = new GameObject("ConnectingLine");
        lineObj.transform.SetParent(panelObj.transform, false);
        RectTransform lineRt = lineObj.AddComponent<RectTransform>();
        lineRt.anchorMin = new Vector2(0, 0.5f);
        lineRt.anchorMax = new Vector2(0, 0.5f);
        lineRt.pivot = new Vector2(0, 0.5f);
        lineRt.anchoredPosition = new Vector2(45.5f, 0); // Di tengah-tengah ikon
        lineRt.sizeDelta = new Vector2(6, 45); // Lebar 6, tinggi 45 (tampak lonjong)
        
        Image line = lineObj.AddComponent<Image>();
        line.color = new Color(0.7f, 0.7f, 0.7f); // Abu-abu muda
        
        // Coba berikan bentuk ujung melengkung/lonjong menggunakan sprite bawaan Unity
        if (roundedSprite != null)
        {
            line.sprite = roundedSprite;
            line.type = Image.Type.Sliced; // Sliced agar rounded cornernya tidak penyok saat ditarik memanjang
        }
        
        // Pastikan garis berada di belakang grup text/icon agar tidak menimpa jika saling berdekatan
        lineObj.transform.SetAsFirstSibling(); 

        // 5. Tambahkan Script Logic
        ActiveNavigationUI script = panelObj.AddComponent<ActiveNavigationUI>();
        script.panel = panelObj;
        script.startLocationText = startText;
        script.destinationLocationText = destText;

        // Tandai sebagai objek baru di Undo system agar bisa di-undo (Ctrl+Z)
        Undo.RegisterCreatedObjectUndo(panelObj, "Create Active Navigation UI");

        // Seleksi object yang baru dibuat
        Selection.activeGameObject = panelObj;
        EditorGUIUtility.PingObject(panelObj);

        Debug.Log("✅ Active Navigation UI Panel berhasil dibuat di Canvas!");
    }
}
