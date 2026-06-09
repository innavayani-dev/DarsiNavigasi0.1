using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using TMPro;
using System.IO;

public class UIBuilderEditor : EditorWindow
{
    [MenuItem("Tools/Build UI Scenes")]
    public static void BuildScenes()
    {
        CreateGradientTexture();
        EnsureSprites();
        // BuildSplashScene(); // Disable this to prevent overwriting user's manual design
        BuildLoginScene();
        BuildRegisterScene();
        BuildScannerScene();
        BuildNavListScene();
        
        // Add to build settings
        var newScenes = new EditorBuildSettingsScene[5];
        newScenes[0] = new EditorBuildSettingsScene("Assets/Scenes/SplashScene.unity", true);
        newScenes[1] = new EditorBuildSettingsScene("Assets/Scenes/0_Login.unity", true);
        newScenes[2] = new EditorBuildSettingsScene("Assets/Scenes/1_Register.unity", true);
        newScenes[3] = new EditorBuildSettingsScene("Assets/Scenes/2_ScannerCamera.unity", true);
        newScenes[4] = new EditorBuildSettingsScene("Assets/Scenes/3_NavList.unity", true);
        EditorBuildSettings.scenes = newScenes;

        Debug.Log("Scenes generated and added to Build Settings!");
    }

    private static void CreateGradientTexture()
    {
        if (!Directory.Exists("Assets/GeneratedAssets"))
        {
            AssetDatabase.CreateFolder("Assets", "GeneratedAssets");
        }

        int height = 512;
        Texture2D tex = new Texture2D(1, height);
        Color topColor = ColorUtility.TryParseHtmlString("#1ED760", out Color t) ? t : Color.green;
        Color bottomColor = ColorUtility.TryParseHtmlString("#118C3F", out Color b) ? b : Color.green;

        for (int y = 0; y < height; y++)
        {
            tex.SetPixel(0, y, Color.Lerp(bottomColor, topColor, (float)y / height));
        }
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes("Assets/GeneratedAssets/GreenGradient.png", bytes);
        AssetDatabase.Refresh();

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath("Assets/GeneratedAssets/GreenGradient.png");
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }
    }

    private static void EnsureSprites()
    {
        string[] paths = { 
            "Assets/Sprites/eye_open.png", 
            "Assets/Sprites/eye_closed.png",
            "Assets/Sprites/btn_masuk.png",
            "Assets/Sprites/btn_daftar.png",
            "Assets/Sprites/btn_back.png",
            "Assets/Sprites/nav_home.png",
            "Assets/Sprites/nav_scan.png",
            "Assets/Sprites/nav_profile.png",
            "Assets/Sprites/icon_spinner.png"
        };
        foreach (string path in paths)
        {
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }
        }
    }


    private static GameObject CreateCanvas(string name)
    {
        GameObject canvasObj = new GameObject(name);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();
        return canvasObj;
    }

    private static GameObject CreateEventSystem()
    {
        GameObject esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        return esObj;
    }

    private static GameObject CreateBackground(Transform parent, bool useGradient = true)
    {
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(parent, false);
        Image img = bgObj.AddComponent<Image>();
        RectTransform rt = bgObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        
        if (useGradient)
        {
            Sprite gradientSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GeneratedAssets/GreenGradient.png");
            if (gradientSprite != null) img.sprite = gradientSprite;
        }
        else
        {
            img.color = new Color(0.1f, 0.1f, 0.1f); // Dark background for scanner
        }
        return bgObj;
    }

    private static GameObject CreateText(string name, Transform parent, string text, int fontSize, Color color, Vector2 pos, Vector2 size, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        GameObject txtObj = new GameObject(name);
        txtObj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.fontStyle = FontStyles.Bold;
        
        RectTransform rt = txtObj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return txtObj;
    }

    private static GameObject CreateInputField(string name, Transform parent, string placeholderText, Vector2 pos, Vector2 size, bool isPassword = false)
    {
        GameObject inputObj = new GameObject(name);
        inputObj.transform.SetParent(parent, false);
        Image bgImg = inputObj.AddComponent<Image>();
        bgImg.color = Color.white;
        RectTransform rt = inputObj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();

        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputObj.transform, false);
        RectTransform textRt = textArea.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(40, 10);
        textRt.offsetMax = new Vector2(-40, -10);
        textArea.AddComponent<RectMask2D>();

        GameObject placeholder = new GameObject("Placeholder");
        placeholder.transform.SetParent(textArea.transform, false);
        TextMeshProUGUI placeholderTmp = placeholder.AddComponent<TextMeshProUGUI>();
        placeholderTmp.text = placeholderText;
        placeholderTmp.fontSize = 45;
        placeholderTmp.color = ColorUtility.TryParseHtmlString("#A0A0A0", out Color pColor) ? pColor : Color.gray;
        placeholderTmp.alignment = TextAlignmentOptions.Left;
        RectTransform phRt = placeholder.GetComponent<RectTransform>();
        phRt.anchorMin = Vector2.zero; phRt.anchorMax = Vector2.one; phRt.sizeDelta = Vector2.zero;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(textArea.transform, false);
        TextMeshProUGUI textTmp = textObj.AddComponent<TextMeshProUGUI>();
        textTmp.fontSize = 45;
        textTmp.color = Color.black;
        textTmp.alignment = TextAlignmentOptions.Left;
        RectTransform txtRt = textObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.sizeDelta = Vector2.zero;

        inputField.textViewport = textRt;
        inputField.textComponent = textTmp;
        inputField.placeholder = placeholderTmp;

        if (isPassword)
        {
            inputField.contentType = TMP_InputField.ContentType.Password;
        }

        return inputObj;
    }

    private static GameObject CreateButton(string name, Transform parent, string text, Vector2 pos, Vector2 size, Color bgColor, Color textColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;
        Button btn = btnObj.AddComponent<Button>();
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 50;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.sizeDelta = Vector2.zero;

        return btnObj;
    }

    private static GameObject CreateImageButton(string name, Transform parent, Sprite sprite, Vector2 pos, Vector2 size)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = pos;
        rectTransform.sizeDelta = size;

        Image image = buttonObj.AddComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = true;

        buttonObj.AddComponent<Button>();

        return buttonObj;
    }


    // SCENES GENERATION
    private static void BuildLoginScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = "0_Login";

        CreateEventSystem();
        GameObject flowManagerObj = new GameObject("SceneFlowManager");
        flowManagerObj.AddComponent<SceneFlowManager>();

        GameObject canvas = CreateCanvas("Canvas");
        CreateBackground(canvas.transform, true);

        CreateText("HospitalName", canvas.transform, "RS Ahmad Yani Surabaya", 50, Color.white, new Vector2(0, 500), new Vector2(800, 100));
        CreateText("Title", canvas.transform, "Login Darsi Navigasi", 80, Color.white, new Vector2(0, 400), new Vector2(1000, 120));

        GameObject emailField = CreateInputField("EmailField", canvas.transform, "Email", new Vector2(0, 100), new Vector2(900, 140));
        GameObject passFieldObj = CreateInputField("PasswordField", canvas.transform, "Password", new Vector2(0, -100), new Vector2(900, 140), true);

        GameObject eyeToggle = new GameObject("EyeToggle");
        eyeToggle.transform.SetParent(passFieldObj.transform, false);
        RectTransform eyeRt = eyeToggle.AddComponent<RectTransform>();
        eyeRt.anchorMin = new Vector2(1, 0.5f); eyeRt.anchorMax = new Vector2(1, 0.5f);
        eyeRt.anchoredPosition = new Vector2(-70, 0); eyeRt.sizeDelta = new Vector2(80, 80);
        Button eyeBtn = eyeToggle.AddComponent<Button>();
        Image eyeImg = eyeToggle.AddComponent<Image>();
        eyeImg.preserveAspect = true; // Fix for stretched/lonjong icons
        
        Sprite eyeClosed = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/eye_closed.png");
        Sprite eyeOpen = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/eye_open.png");
        eyeImg.sprite = eyeClosed;
        eyeImg.color = Color.gray;

        PasswordVisibility passVis = passFieldObj.AddComponent<PasswordVisibility>();
        passVis.passwordField = passFieldObj.GetComponent<TMP_InputField>();
        passVis.toggleButton = eyeBtn;
        passVis.toggleIconImage = eyeImg;
        passVis.eyeOpenSprite = eyeOpen;
        passVis.eyeClosedSprite = eyeClosed;

        Sprite btnMasukSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/btn_masuk.png");
        GameObject loginBtnObj = CreateImageButton("LoginButton", canvas.transform, btnMasukSprite, new Vector2(0, -350), new Vector2(900, 220));
        UnityEngine.Events.UnityAction action = new UnityEngine.Events.UnityAction(flowManagerObj.GetComponent<SceneFlowManager>().GoToScanner);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(loginBtnObj.GetComponent<Button>().onClick, action);

        GameObject regBtnObj = CreateButton("RegisterLink", canvas.transform, "Belum punya akun? <color=white><u>Daftar Sekarang</u></color>", new Vector2(0, -600), new Vector2(900, 100), Color.clear, new Color(0.9f, 0.9f, 0.9f));
        regBtnObj.transform.Find("Text").GetComponent<TextMeshProUGUI>().fontSize = 40;
        action = new UnityEngine.Events.UnityAction(flowManagerObj.GetComponent<SceneFlowManager>().GoToRegister);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(regBtnObj.GetComponent<Button>().onClick, action);

        // Fade Overlay & Manager
        AddFadeOverlay(canvas.transform);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/0_Login.unity");
    }

    private static void BuildRegisterScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = "1_Register";

        CreateEventSystem();
        GameObject flowManagerObj = new GameObject("SceneFlowManager");
        flowManagerObj.AddComponent<SceneFlowManager>();

        GameObject canvas = CreateCanvas("Canvas");
        CreateBackground(canvas.transform, true);

        CreateText("HospitalName", canvas.transform, "RS Ahmad Yani Surabaya", 50, Color.white, new Vector2(0, 500), new Vector2(800, 100));
        CreateText("Title", canvas.transform, "Daftar Darsi Navigasi", 80, Color.white, new Vector2(0, 400), new Vector2(1000, 120));

        CreateInputField("NameField", canvas.transform, "Nama Lengkap", new Vector2(0, 150), new Vector2(900, 140));
        CreateInputField("EmailField", canvas.transform, "Email", new Vector2(0, -20), new Vector2(900, 140));
        GameObject passFieldObj = CreateInputField("PasswordField", canvas.transform, "Password", new Vector2(0, -190), new Vector2(900, 140), true);

        GameObject eyeToggle = new GameObject("EyeToggle");
        eyeToggle.transform.SetParent(passFieldObj.transform, false);
        RectTransform eyeRt = eyeToggle.AddComponent<RectTransform>();
        eyeRt.anchorMin = new Vector2(1, 0.5f); eyeRt.anchorMax = new Vector2(1, 0.5f);
        eyeRt.anchoredPosition = new Vector2(-70, 0); eyeRt.sizeDelta = new Vector2(80, 80);
        Button eyeBtn = eyeToggle.AddComponent<Button>();
        Image eyeImg = eyeToggle.AddComponent<Image>();
        eyeImg.preserveAspect = true; // Fix for stretched/lonjong icons
        
        Sprite eyeClosed = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/eye_closed.png");
        Sprite eyeOpen = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/eye_open.png");
        eyeImg.sprite = eyeClosed;
        eyeImg.color = Color.gray;

        PasswordVisibility passVis = passFieldObj.AddComponent<PasswordVisibility>();
        passVis.passwordField = passFieldObj.GetComponent<TMP_InputField>();
        passVis.toggleButton = eyeBtn;
        passVis.toggleIconImage = eyeImg;
        passVis.eyeOpenSprite = eyeOpen;
        passVis.eyeClosedSprite = eyeClosed;

        Sprite btnDaftarSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/btn_daftar.png");
        GameObject regBtnObj = CreateImageButton("RegisterButton", canvas.transform, btnDaftarSprite, new Vector2(0, -450), new Vector2(900, 220));
        UnityEngine.Events.UnityAction action = new UnityEngine.Events.UnityAction(flowManagerObj.GetComponent<SceneFlowManager>().GoToScanner);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(regBtnObj.GetComponent<Button>().onClick, action);

        GameObject loginBtnObj = CreateButton("LoginLink", canvas.transform, "Sudah punya akun? <color=white><u>Masuk</u></color>", new Vector2(0, -650), new Vector2(900, 100), Color.clear, new Color(0.9f, 0.9f, 0.9f));
        loginBtnObj.transform.Find("Text").GetComponent<TextMeshProUGUI>().fontSize = 40;
        action = new UnityEngine.Events.UnityAction(flowManagerObj.GetComponent<SceneFlowManager>().GoToLogin);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(loginBtnObj.GetComponent<Button>().onClick, action);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/1_Register.unity");
    }

    private static void BuildScannerScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = "2_ScannerCamera";

        CreateEventSystem();
        GameObject flowManagerObj = new GameObject("SceneFlowManager");
        flowManagerObj.AddComponent<SceneFlowManager>();

        GameObject canvas = CreateCanvas("Canvas");
        CreateBackground(canvas.transform, false);

        GameObject feedObj = new GameObject("CameraFeed");
        feedObj.transform.SetParent(canvas.transform, false);
        RawImage ri = feedObj.AddComponent<RawImage>();
        ri.color = Color.white; // Needs to be white to show texture properly
        RectTransform riRt = feedObj.GetComponent<RectTransform>();
        riRt.anchorMin = Vector2.zero; riRt.anchorMax = Vector2.one;
        riRt.sizeDelta = Vector2.zero;

        ScannerCameraController camCtrl = feedObj.AddComponent<ScannerCameraController>();
        camCtrl.rawImage = ri;

        // Focus Frame Container
        GameObject frameObj = new GameObject("FocusFrame");
        frameObj.transform.SetParent(canvas.transform, false);
        RectTransform frameRt = frameObj.AddComponent<RectTransform>();
        frameRt.anchoredPosition = new Vector2(0, 100);
        frameRt.sizeDelta = new Vector2(700, 700);

        // Frame Borders
        Color frameColor = Color.green;
        float thickness = 10f;

        // Top Border
        GameObject top = new GameObject("TopBorder"); top.transform.SetParent(frameObj.transform, false);
        Image tImg = top.AddComponent<Image>(); tImg.color = frameColor;
        RectTransform tRt = top.GetComponent<RectTransform>(); tRt.anchorMin = new Vector2(0,1); tRt.anchorMax = new Vector2(1,1); tRt.sizeDelta = new Vector2(0, thickness);

        // Bottom Border
        GameObject bot = new GameObject("BottomBorder"); bot.transform.SetParent(frameObj.transform, false);
        Image bImg = bot.AddComponent<Image>(); bImg.color = frameColor;
        RectTransform bRt = bot.GetComponent<RectTransform>(); bRt.anchorMin = new Vector2(0,0); bRt.anchorMax = new Vector2(1,0); bRt.sizeDelta = new Vector2(0, thickness);

        // Left Border
        GameObject left = new GameObject("LeftBorder"); left.transform.SetParent(frameObj.transform, false);
        Image lImg = left.AddComponent<Image>(); lImg.color = frameColor;
        RectTransform lRt = left.GetComponent<RectTransform>(); lRt.anchorMin = new Vector2(0,0); lRt.anchorMax = new Vector2(0,1); lRt.sizeDelta = new Vector2(thickness, 0);

        // Right Border
        GameObject right = new GameObject("RightBorder"); right.transform.SetParent(frameObj.transform, false);
        Image rImg = right.AddComponent<Image>(); rImg.color = frameColor;
        RectTransform rRt = right.GetComponent<RectTransform>(); rRt.anchorMin = new Vector2(1,0); rRt.anchorMax = new Vector2(1,1); rRt.sizeDelta = new Vector2(thickness, 0);

        CreateText("ScannerInstruction", canvas.transform, "find qr code, scan qr code at a distance of 0,5 meters", 40, Color.white, new Vector2(0, -350), new Vector2(800, 150));

        Sprite backBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/btn_back.png");
        GameObject backBtnObj = CreateImageButton("BackButton", canvas.transform, backBtnSprite, new Vector2(-420, 820), new Vector2(140, 140));
        UnityEngine.Events.UnityAction action = new UnityEngine.Events.UnityAction(flowManagerObj.GetComponent<SceneFlowManager>().GoBack);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(backBtnObj.GetComponent<Button>().onClick, action);

        GameObject dummyBtnObj = CreateButton("DummyScanBtn", canvas.transform, "Simulate Scan Success", new Vector2(0, -700), new Vector2(600, 120), new Color(0,0,0,0.5f), Color.white);
        action = new UnityEngine.Events.UnityAction(camCtrl.SimulateScanSuccess);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(dummyBtnObj.GetComponent<Button>().onClick, action);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/2_ScannerCamera.unity");
    }

    private static void BuildNavListScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = "3_NavList";

        CreateEventSystem();
        GameObject flowManagerObj = new GameObject("SceneFlowManager");
        flowManagerObj.AddComponent<SceneFlowManager>();

        GameObject canvas = CreateCanvas("Canvas");
        CreateBackground(canvas.transform, true);

        CreateText("Title", canvas.transform, "Darsi Navigasi", 80, Color.white, new Vector2(-100, 800), new Vector2(800, 120), TextAlignmentOptions.Left);

        GameObject searchBar = CreateInputField("SearchBar", canvas.transform, "   Cari ruangan atau poliklinik...", new Vector2(0, 650), new Vector2(900, 120));
        
        // --- SCROLL VIEW SETUP ---
        GameObject scrollView = new GameObject("ScrollView");
        scrollView.transform.SetParent(canvas.transform, false);
        RectTransform svRt = scrollView.AddComponent<RectTransform>();
        svRt.anchorMin = new Vector2(0, 0); svRt.anchorMax = new Vector2(1, 1);
        svRt.offsetMin = new Vector2(0, 200); svRt.offsetMax = new Vector2(0, -400);
        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.scrollSensitivity = 50f;

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        RectTransform vpRt = viewport.AddComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one;
        vpRt.offsetMin = Vector2.zero; vpRt.offsetMax = Vector2.zero;
        viewport.AddComponent<Image>().color = Color.clear; // mask needs image
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform cRt = content.AddComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0, 1); cRt.anchorMax = new Vector2(1, 1);
        cRt.pivot = new Vector2(0.5f, 1);
        cRt.anchoredPosition = Vector2.zero;
        cRt.sizeDelta = new Vector2(0, 0);

        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(90, 90, 20, 20);
        vlg.spacing = 30;
        vlg.childControlHeight = false;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;

        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = vpRt;
        scrollRect.content = cRt;
        // -------------------------

        NavListManager navManager = canvas.AddComponent<NavListManager>();
        navManager.contentParent = content.transform;
        navManager.searchField = searchBar.GetComponent<TMP_InputField>();

        GameObject bottomBar = new GameObject("BottomNavArea");
        bottomBar.transform.SetParent(canvas.transform, false);
        Image bbImg = bottomBar.AddComponent<Image>();
        bbImg.color = Color.white;
        RectTransform bbRt = bottomBar.GetComponent<RectTransform>();
        bbRt.anchorMin = new Vector2(0, 0); bbRt.anchorMax = new Vector2(1, 0);
        bbRt.anchoredPosition = new Vector2(0, 80);
        bbRt.sizeDelta = new Vector2(0, 160);

        BottomNavigation bNav = bottomBar.AddComponent<BottomNavigation>();

        GameObject btnHome = CreateButton("BtnHome", bottomBar.transform, "Beranda", new Vector2(-350, 0), new Vector2(300, 160), Color.clear, new Color(0.1f, 0.7f, 0.3f));
        btnHome.transform.Find("Text").GetComponent<TextMeshProUGUI>().fontSize = 35;
        btnHome.transform.Find("Text").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -45);
        GameObject iHomeObj = new GameObject("Icon"); iHomeObj.transform.SetParent(btnHome.transform, false);
        Image iHome = iHomeObj.AddComponent<Image>(); iHome.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/nav_home.png");
        iHome.preserveAspect = true; iHomeObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 35);
        iHomeObj.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
        UnityEngine.Events.UnityAction action1 = new UnityEngine.Events.UnityAction(bNav.OnHomeClicked);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnHome.GetComponent<Button>().onClick, action1);

        GameObject btnScan = CreateButton("BtnScan", bottomBar.transform, "Pindai", new Vector2(0, 0), new Vector2(300, 160), Color.clear, Color.gray);
        btnScan.transform.Find("Text").GetComponent<TextMeshProUGUI>().fontSize = 35;
        btnScan.transform.Find("Text").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -45);
        GameObject iScanObj = new GameObject("Icon"); iScanObj.transform.SetParent(btnScan.transform, false);
        Image iScan = iScanObj.AddComponent<Image>(); iScan.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/nav_scan.png");
        iScan.preserveAspect = true; iScanObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 35);
        iScanObj.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
        UnityEngine.Events.UnityAction action2 = new UnityEngine.Events.UnityAction(bNav.OnScanClicked);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnScan.GetComponent<Button>().onClick, action2);

        GameObject btnProfile = CreateButton("BtnProfile", bottomBar.transform, "Profil", new Vector2(350, 0), new Vector2(300, 160), Color.clear, Color.gray);
        btnProfile.transform.Find("Text").GetComponent<TextMeshProUGUI>().fontSize = 35;
        btnProfile.transform.Find("Text").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -45);
        GameObject iProfObj = new GameObject("Icon"); iProfObj.transform.SetParent(btnProfile.transform, false);
        Image iProf = iProfObj.AddComponent<Image>(); iProf.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/nav_profile.png");
        iProf.preserveAspect = true; iProfObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 35);
        iProfObj.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
        UnityEngine.Events.UnityAction action3 = new UnityEngine.Events.UnityAction(bNav.OnProfileClicked);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnProfile.GetComponent<Button>().onClick, action3);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/3_NavList.unity");
    }

    private static void BuildSplashScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        GameObject canvas = CreateCanvas("SplashCanvas");
        
        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvas.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GeneratedAssets/GreenGradient.png");
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.sizeDelta = Vector2.zero;

        // Title
        CreateText("LogoTitle", canvas.transform, "RS AHMAD YANI\nSURABAYA", 100, Color.white, new Vector2(0, 100), new Vector2(1000, 400));

        // Spinner Logo
        GameObject spinObj = new GameObject("SpinnerLogo");
        spinObj.transform.SetParent(canvas.transform, false);
        Image spinImg = spinObj.AddComponent<Image>();
        spinImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/icon_spinner.png");
        spinImg.preserveAspect = true;
        spinObj.AddComponent<Spinner>(); // Add the rotation script
        RectTransform spinRt = spinObj.GetComponent<RectTransform>();
        spinRt.anchoredPosition = new Vector2(0, -250);
        spinRt.sizeDelta = new Vector2(150, 150);

        // Fade Overlay
        GameObject fadeObj = new GameObject("FadeOverlay");
        fadeObj.transform.SetParent(canvas.transform, false);
        Image fadeImg = fadeObj.AddComponent<Image>();
        fadeImg.color = new Color(0, 0, 0, 0); // Start transparent
        RectTransform fadeRt = fadeObj.GetComponent<RectTransform>();
        fadeRt.anchorMin = Vector2.zero; fadeRt.anchorMax = Vector2.one; fadeRt.sizeDelta = Vector2.zero;

        // Manager
        SplashManager manager = canvas.AddComponent<SplashManager>();
        manager.fadeOverlay = fadeImg;
        manager.fadeDuration = 1.0f;
        manager.waitTime = 3.0f;
        manager.nextSceneName = "0_Login";

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/SplashScene.unity");
    }
    private static void AddFadeOverlay(Transform parent)
    {
        GameObject fadeObj = new GameObject("FadeOverlay");
        fadeObj.transform.SetParent(parent, false);
        Image fadeImg = fadeObj.AddComponent<Image>();
        fadeImg.color = Color.black;
        fadeImg.raycastTarget = true; // Block clicks during fade
        RectTransform fadeRt = fadeObj.GetComponent<RectTransform>();
        fadeRt.anchorMin = Vector2.zero; fadeRt.anchorMax = Vector2.one; fadeRt.sizeDelta = Vector2.zero;

        SceneFadeManager manager = parent.gameObject.AddComponent<SceneFadeManager>();
        manager.fadeOverlay = fadeImg;
        manager.fadeDuration = 1.0f;
        manager.fadeInOnStart = true;
    }
}
