using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class CreateArrivalNotificationUI : EditorWindow
{
    [MenuItem("Tools/Darsi/Create Arrival Notification UI")]
    public static void CreateUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas tidak ditemukan di scene!");
            return;
        }

        // 1. Container Utama (Panel Full Screen)
        GameObject panelObj = new GameObject("ArrivalNotificationPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        RectTransform panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0, 0);
        panelRt.anchorMax = new Vector2(1, 1);
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;
        
        Image bg = panelObj.AddComponent<Image>();
        bg.color = new Color(0.18f, 0.8f, 0.44f, 1f); // Warna hijau

        // 2. Lingkaran Centang (Checkmark)
        GameObject circleObj = new GameObject("CheckmarkCircle");
        circleObj.transform.SetParent(panelObj.transform, false);
        RectTransform circleRt = circleObj.AddComponent<RectTransform>();
        circleRt.anchorMin = new Vector2(0.5f, 0.5f);
        circleRt.anchorMax = new Vector2(0.5f, 0.5f);
        circleRt.anchoredPosition = new Vector2(0, 200);
        circleRt.sizeDelta = new Vector2(250, 250);
        
        Image circleImg = circleObj.AddComponent<Image>();
        circleImg.color = new Color(1f, 1f, 1f, 0.2f); // Lingkaran transparan luar
        Sprite knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        if (knobSprite != null) circleImg.sprite = knobSprite;

        GameObject innerCircleObj = new GameObject("InnerCircle");
        innerCircleObj.transform.SetParent(circleObj.transform, false);
        RectTransform innerCircleRt = innerCircleObj.AddComponent<RectTransform>();
        innerCircleRt.anchorMin = new Vector2(0.5f, 0.5f);
        innerCircleRt.anchorMax = new Vector2(0.5f, 0.5f);
        innerCircleRt.sizeDelta = new Vector2(200, 200);
        Image innerCircleImg = innerCircleObj.AddComponent<Image>();
        innerCircleImg.color = new Color(1f, 1f, 1f, 0.3f);
        if (knobSprite != null) innerCircleImg.sprite = knobSprite;

        // Teks Centang "✔" manual
        GameObject checkmarkObj = new GameObject("CheckmarkIcon");
        checkmarkObj.transform.SetParent(innerCircleObj.transform, false);
        RectTransform checkRt = checkmarkObj.AddComponent<RectTransform>();
        checkRt.anchorMin = new Vector2(0, 0);
        checkRt.anchorMax = new Vector2(1, 1);
        checkRt.offsetMin = Vector2.zero;
        checkRt.offsetMax = Vector2.zero;
        TextMeshProUGUI checkTxt = checkmarkObj.AddComponent<TextMeshProUGUI>();
        checkTxt.text = "✔";
        checkTxt.fontSize = 120;
        checkTxt.color = Color.white;
        checkTxt.alignment = TextAlignmentOptions.Center;

        // 3. Teks Judul "Anda Telah Sampai!"
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panelObj.transform, false);
        RectTransform titleRt = titleObj.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 0.5f);
        titleRt.anchorMax = new Vector2(0.5f, 0.5f);
        titleRt.anchoredPosition = new Vector2(0, -20);
        titleRt.sizeDelta = new Vector2(800, 150);
        TextMeshProUGUI titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
        titleTxt.text = "Anda Telah\nSampai!";
        titleTxt.fontSize = 72;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.color = Color.white;
        titleTxt.alignment = TextAlignmentOptions.Center;

        // 4. Info Group (Poli Anak, Jarak, Waktu)
        GameObject infoGroupObj = new GameObject("InfoGroup");
        infoGroupObj.transform.SetParent(panelObj.transform, false);
        RectTransform infoRt = infoGroupObj.AddComponent<RectTransform>();
        infoRt.anchorMin = new Vector2(0.5f, 0.5f);
        infoRt.anchorMax = new Vector2(0.5f, 0.5f);
        infoRt.anchoredPosition = new Vector2(0, -200);
        infoRt.sizeDelta = new Vector2(800, 200);

        // Teks Ruangan
        GameObject roomObj = new GameObject("RoomText");
        roomObj.transform.SetParent(infoGroupObj.transform, false);
        RectTransform roomRt = roomObj.AddComponent<RectTransform>();
        roomRt.anchorMin = new Vector2(0, 1);
        roomRt.anchorMax = new Vector2(1, 1);
        roomRt.anchoredPosition = new Vector2(0, -30);
        roomRt.sizeDelta = new Vector2(0, 50);
        TextMeshProUGUI roomTxt = roomObj.AddComponent<TextMeshProUGUI>();
        roomTxt.text = "Poli Anak";
        roomTxt.fontSize = 40;
        roomTxt.fontStyle = FontStyles.Bold;
        roomTxt.color = Color.white;
        roomTxt.alignment = TextAlignmentOptions.Center;

        // Teks Jarak
        GameObject distObj = new GameObject("DistanceText");
        distObj.transform.SetParent(infoGroupObj.transform, false);
        RectTransform distRt = distObj.AddComponent<RectTransform>();
        distRt.anchorMin = new Vector2(0, 1);
        distRt.anchorMax = new Vector2(1, 1);
        distRt.anchoredPosition = new Vector2(0, -90);
        distRt.sizeDelta = new Vector2(0, 50);
        TextMeshProUGUI distTxt = distObj.AddComponent<TextMeshProUGUI>();
        distTxt.text = "150 m";
        distTxt.fontSize = 36;
        distTxt.fontStyle = FontStyles.Bold;
        distTxt.color = Color.white;
        distTxt.alignment = TextAlignmentOptions.Center;

        // Teks Waktu
        GameObject timeObj = new GameObject("TimeText");
        timeObj.transform.SetParent(infoGroupObj.transform, false);
        RectTransform timeRt = timeObj.AddComponent<RectTransform>();
        timeRt.anchorMin = new Vector2(0, 1);
        timeRt.anchorMax = new Vector2(1, 1);
        timeRt.anchoredPosition = new Vector2(0, -150);
        timeRt.sizeDelta = new Vector2(0, 50);
        TextMeshProUGUI timeTxt = timeObj.AddComponent<TextMeshProUGUI>();
        timeTxt.text = "2 Menit";
        timeTxt.fontSize = 36;
        timeTxt.fontStyle = FontStyles.Bold;
        timeTxt.color = Color.white;
        timeTxt.alignment = TextAlignmentOptions.Center;

        // 5. Tombol "Selesai" (Pil di bawah)
        GameObject btnObj = new GameObject("SelesaiButton");
        btnObj.transform.SetParent(panelObj.transform, false);
        RectTransform btnRt = btnObj.AddComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0);
        btnRt.anchorMax = new Vector2(0.5f, 0);
        btnRt.anchoredPosition = new Vector2(0, 150); // Jarak 150px dari bawah
        btnRt.sizeDelta = new Vector2(800, 120);
        
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = Color.white;
        Sprite roundedSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (roundedSprite != null)
        {
            btnImg.sprite = roundedSprite;
            btnImg.type = Image.Type.Sliced;
        }

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        RectTransform btnTextRt = btnTextObj.AddComponent<RectTransform>();
        btnTextRt.anchorMin = new Vector2(0, 0);
        btnTextRt.anchorMax = new Vector2(1, 1);
        btnTextRt.offsetMin = Vector2.zero;
        btnTextRt.offsetMax = Vector2.zero;
        TextMeshProUGUI btnTxt = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnTxt.text = "Selesai";
        btnTxt.fontSize = 40;
        btnTxt.fontStyle = FontStyles.Bold;
        btnTxt.color = new Color(0.18f, 0.8f, 0.44f, 1f); // Teks hijau
        btnTxt.alignment = TextAlignmentOptions.Center;

        Button button = btnObj.AddComponent<Button>();

        // 6. Hubungkan komponen ke Script Logic
        ArrivalNotificationUI script = panelObj.AddComponent<ArrivalNotificationUI>();
        script.panel = panelObj;
        script.roomNameText = roomTxt;
        script.distanceText = distTxt;
        script.timeText = timeTxt;

        // Setup OnClick Event
        UnityEngine.Events.UnityAction action = new UnityEngine.Events.UnityAction(script.HideArrival);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, action);

        // Pastikan panel paling atas menutupi UI lain
        panelObj.transform.SetAsLastSibling();

        Undo.RegisterCreatedObjectUndo(panelObj, "Create Arrival Notification UI");
        Selection.activeGameObject = panelObj;
        EditorGUIUtility.PingObject(panelObj);

        Debug.Log("✅ Arrival Notification UI Panel berhasil dibuat!");
    }
}
