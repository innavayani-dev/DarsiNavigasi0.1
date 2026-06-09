using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class SetupNavUI
{
    [MenuItem("Darsi/Build Nav UI")]
    public static void BuildUI()
    {
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        // Clean up previous children
        for (int i = canvasObj.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(canvasObj.transform.GetChild(i).gameObject);
        }

        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0f; // Match Width exactly
        }

        // Colors
        Color cBg = ColorUtility.TryParseHtmlString("#28C25E", out Color bg) ? bg : new Color(0.15f, 0.76f, 0.36f);
        Color cWhite = Color.white;
        Color cSearchBg = ColorUtility.TryParseHtmlString("#F4F9F4", out Color sb) ? sb : Color.white;
        Color cTextDark = ColorUtility.TryParseHtmlString("#333333", out Color td) ? td : Color.black;
        Color cTextLight = ColorUtility.TryParseHtmlString("#888888", out Color tl) ? tl : Color.gray;

        // Sprites
        Sprite roundedRect = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/RoundedRect.png");
        Sprite circle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");

        // 1. Background
        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
        bgObj.GetComponent<Image>().color = cBg;

        // 2. Title
        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(canvasObj.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1); titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0, 1);
        titleRt.offsetMin = new Vector2(60, -180); titleRt.offsetMax = new Vector2(-60, -80);
        TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
        titleText.text = "Darsi Navigasi";
        titleText.fontSize = 75;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = cWhite;

        // 3. Search Bar Area
        GameObject searchArea = new GameObject("SearchArea", typeof(RectTransform));
        searchArea.transform.SetParent(canvasObj.transform, false);
        RectTransform searchAreaRt = searchArea.GetComponent<RectTransform>();
        searchAreaRt.anchorMin = new Vector2(0, 1); searchAreaRt.anchorMax = new Vector2(1, 1);
        searchAreaRt.pivot = new Vector2(0.5f, 1);
        searchAreaRt.offsetMin = new Vector2(60, -320); searchAreaRt.offsetMax = new Vector2(-60, -200);

        // Search Input
        GameObject searchObj = new GameObject("SearchBar", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        searchObj.transform.SetParent(searchArea.transform, false);
        RectTransform searchRt = searchObj.GetComponent<RectTransform>();
        searchRt.anchorMin = new Vector2(0, 0); searchRt.anchorMax = new Vector2(1, 1);
        searchRt.offsetMin = new Vector2(0, 0); searchRt.offsetMax = new Vector2(-140, 0); 
        Image searchImg = searchObj.GetComponent<Image>();
        searchImg.sprite = roundedRect; searchImg.type = Image.Type.Sliced; searchImg.color = cSearchBg;

        GameObject placeholderObj = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        placeholderObj.transform.SetParent(searchObj.transform, false);
        RectTransform placeRt = placeholderObj.GetComponent<RectTransform>();
        placeRt.anchorMin = Vector2.zero; placeRt.anchorMax = Vector2.one;
        placeRt.offsetMin = new Vector2(120, 0); placeRt.offsetMax = new Vector2(-40, 0);
        TextMeshProUGUI placeText = placeholderObj.GetComponent<TextMeshProUGUI>();
        placeText.text = "Cari ruangan atau poliklinik...";
        placeText.fontSize = 35; placeText.color = cTextLight;
        placeText.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Midline;

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(searchObj.transform, false);
        RectTransform txtRt = textObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = new Vector2(120, 0); txtRt.offsetMax = new Vector2(-40, 0);
        TextMeshProUGUI inputTxt = textObj.GetComponent<TextMeshProUGUI>();
        inputTxt.fontSize = 35; inputTxt.color = cTextDark;
        inputTxt.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Midline;

        TMP_InputField inputField = searchObj.GetComponent<TMP_InputField>();
        inputField.textComponent = inputTxt;
        inputField.placeholder = placeText;

        Sprite searchIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SearchIcon.png");

        GameObject searchIconObj = new GameObject("SearchIcon", typeof(RectTransform), typeof(Image));
        searchIconObj.transform.SetParent(searchObj.transform, false);
        RectTransform sIconRt = searchIconObj.GetComponent<RectTransform>();
        sIconRt.anchorMin = new Vector2(0, 0.5f); sIconRt.anchorMax = new Vector2(0, 0.5f);
        sIconRt.pivot = new Vector2(0, 0.5f);
        sIconRt.sizeDelta = new Vector2(60, 60); sIconRt.anchoredPosition = new Vector2(40, 0);
        Image sIconImg = searchIconObj.GetComponent<Image>();
        sIconImg.sprite = searchIconSprite;
        sIconImg.color = cTextLight;

        // Filter Button
        GameObject filterBtnObj = new GameObject("FilterBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        filterBtnObj.transform.SetParent(searchArea.transform, false);
        RectTransform filterRt = filterBtnObj.GetComponent<RectTransform>();
        filterRt.anchorMin = new Vector2(1, 0.5f); filterRt.anchorMax = new Vector2(1, 0.5f);
        filterRt.pivot = new Vector2(1, 0.5f);
        filterRt.sizeDelta = new Vector2(120, 120); filterRt.anchoredPosition = new Vector2(0, 0);
        Image filterImg = filterBtnObj.GetComponent<Image>();
        filterImg.sprite = roundedRect; filterImg.type = Image.Type.Sliced; filterImg.color = cBg;

        GameObject filterIconInner = new GameObject("Icon", typeof(RectTransform), typeof(TextMeshProUGUI));
        filterIconInner.transform.SetParent(filterBtnObj.transform, false);
        RectTransform fIconRt = filterIconInner.GetComponent<RectTransform>();
        fIconRt.anchorMin = Vector2.zero; fIconRt.anchorMax = Vector2.one;
        fIconRt.offsetMin = Vector2.zero; fIconRt.offsetMax = Vector2.zero;
        TextMeshProUGUI fIconTxt = filterIconInner.GetComponent<TextMeshProUGUI>();
        fIconTxt.text = "="; // Mockup icon
        fIconTxt.fontSize = 60; fIconTxt.color = cWhite;
        fIconTxt.alignment = TextAlignmentOptions.Center | TextAlignmentOptions.Midline;

        // 4. Tabs Area (Horizontal ScrollView for tabs to avoid squishing)
        GameObject tabsScrollObj = new GameObject("TabsScroll", typeof(RectTransform), typeof(ScrollRect));
        tabsScrollObj.transform.SetParent(canvasObj.transform, false);
        RectTransform tsRt = tabsScrollObj.GetComponent<RectTransform>();
        tsRt.anchorMin = new Vector2(0, 1); tsRt.anchorMax = new Vector2(1, 1);
        tsRt.pivot = new Vector2(0.5f, 1);
        tsRt.offsetMin = new Vector2(60, -440); tsRt.offsetMax = new Vector2(-60, -350); // Height 90

        GameObject tsViewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        tsViewport.transform.SetParent(tabsScrollObj.transform, false);
        RectTransform tsvRt = tsViewport.GetComponent<RectTransform>();
        tsvRt.anchorMin = Vector2.zero; tsvRt.anchorMax = Vector2.one;
        tsvRt.offsetMin = Vector2.zero; tsvRt.offsetMax = Vector2.zero;
        tsViewport.GetComponent<Image>().color = new Color(1, 1, 1, 0.01f);
        tsViewport.GetComponent<Mask>().showMaskGraphic = false;

        GameObject tabsContent = new GameObject("Content", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        tabsContent.transform.SetParent(tsViewport.transform, false);
        RectTransform tcRt = tabsContent.GetComponent<RectTransform>();
        tcRt.anchorMin = new Vector2(0, 0); tcRt.anchorMax = new Vector2(0, 1);
        tcRt.pivot = new Vector2(0, 0.5f);
        tcRt.offsetMin = Vector2.zero; tcRt.offsetMax = Vector2.zero;

        HorizontalLayoutGroup tabsHlg = tabsContent.GetComponent<HorizontalLayoutGroup>();
        tabsHlg.spacing = 20; tabsHlg.childControlWidth = false; tabsHlg.childControlHeight = true;
        tabsHlg.childForceExpandWidth = false;

        ContentSizeFitter tcsf = tabsContent.GetComponent<ContentSizeFitter>();
        tcsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect tsScrollRect = tabsScrollObj.GetComponent<ScrollRect>();
        tsScrollRect.content = tcRt; tsScrollRect.viewport = tsvRt;
        tsScrollRect.horizontal = true; tsScrollRect.vertical = false; 
        tsScrollRect.movementType = ScrollRect.MovementType.Elastic;

        string[] tabNames = { "Semua", "Lantai 1", "Lantai 2", "Lantai 3", "Lantai 4" };
        for (int i = 0; i < tabNames.Length; i++)
        {
            GameObject tab = new GameObject("Tab_" + tabNames[i], typeof(RectTransform), typeof(Image), typeof(Button));
            tab.transform.SetParent(tabsContent.transform, false);
            RectTransform tRt = tab.GetComponent<RectTransform>();
            tRt.sizeDelta = new Vector2(180, 0);
            Image tImg = tab.GetComponent<Image>();
            tImg.sprite = roundedRect; tImg.type = Image.Type.Sliced;
            
            GameObject tTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            tTxtObj.transform.SetParent(tab.transform, false);
            RectTransform tTxtRt = tTxtObj.GetComponent<RectTransform>();
            tTxtRt.anchorMin = Vector2.zero; tTxtRt.anchorMax = Vector2.one;
            tTxtRt.offsetMin = Vector2.zero; tTxtRt.offsetMax = Vector2.zero;
            TextMeshProUGUI tTxt = tTxtObj.GetComponent<TextMeshProUGUI>();
            tTxt.text = tabNames[i];
            tTxt.fontSize = 32; tTxt.alignment = TextAlignmentOptions.Center | TextAlignmentOptions.Midline;

            if (i == 0) // "Semua" is active
            {
                tImg.color = cWhite;
                tTxt.color = cBg;
                tTxt.fontStyle = FontStyles.Bold;
            }
            else
            {
                tImg.color = new Color(cBg.r, cBg.g, cBg.b, 0.4f); // Semi-transparent
                tTxt.color = cWhite;
            }
        }

        // 5. ScrollView for Main List
        GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
        scrollObj.transform.SetParent(canvasObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0); scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(0, 180); // above bottom nav
        scrollRt.offsetMax = new Vector2(0, -460); // below tabs

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform viewRt = viewport.GetComponent<RectTransform>();
        viewRt.anchorMin = Vector2.zero; viewRt.anchorMax = Vector2.one;
        viewRt.offsetMin = Vector2.zero; viewRt.offsetMax = Vector2.zero;
        viewport.GetComponent<Image>().color = new Color(1, 1, 1, 0.01f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRt = content.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1); contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.offsetMin = new Vector2(0, 0); contentRt.offsetMax = new Vector2(0, 0);

        VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(60, 60, 20, 100); vlg.spacing = 25;
        vlg.childControlHeight = false; vlg.childControlWidth = true; vlg.childForceExpandHeight = false;
        
        ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scrollRect = scrollObj.GetComponent<ScrollRect>();
        scrollRect.content = contentRt; scrollRect.viewport = viewRt;
        scrollRect.horizontal = false; scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.scrollSensitivity = 10; 

        // Navigation Items
        string[] names = { "Mandiri BPJS", "Bank Mega Syariah", "Loket Pendaftaran", "BPJS Center", "Layanan Transportasi", "Resepsionis", "Lift" };
        string[] dists = { "50m", "120m", "30m", "80m", "10m", "200m", "45m" };
        string[] times = { "3 mnt", "6 mnt", "1 mnt", "4 mnt", "1 mnt", "10 mnt", "2 mnt" };
        string[] iconsText = { "M", "B", "L", "C", "T", "R", "L" }; 

        for (int i = 0; i < names.Length; i++)
        {
            GameObject item = new GameObject("Item_" + i, typeof(RectTransform), typeof(Image), typeof(Button));
            item.transform.SetParent(content.transform, false);
            RectTransform itemRt = item.GetComponent<RectTransform>();
            itemRt.sizeDelta = new Vector2(0, 180); // reduced height
            Image itemImg = item.GetComponent<Image>();
            itemImg.sprite = roundedRect; itemImg.type = Image.Type.Sliced; itemImg.color = cWhite;

            GameObject iconBg = new GameObject("IconBg", typeof(RectTransform), typeof(Image));
            iconBg.transform.SetParent(item.transform, false);
            RectTransform iBgRt = iconBg.GetComponent<RectTransform>();
            iBgRt.anchorMin = new Vector2(0, 0.5f); iBgRt.anchorMax = new Vector2(0, 0.5f);
            iBgRt.pivot = new Vector2(0, 0.5f);
            iBgRt.sizeDelta = new Vector2(80, 80); iBgRt.anchoredPosition = new Vector2(40, 0);
            iconBg.GetComponent<Image>().sprite = circle;
            iconBg.GetComponent<Image>().color = ColorUtility.TryParseHtmlString("#F5F5F5", out Color icBg) ? icBg : Color.white;

            GameObject iTxtObj = new GameObject("ITxt", typeof(RectTransform), typeof(TextMeshProUGUI));
            iTxtObj.transform.SetParent(iconBg.transform, false);
            RectTransform iTxtRt = iTxtObj.GetComponent<RectTransform>();
            iTxtRt.anchorMin = Vector2.zero; iTxtRt.anchorMax = Vector2.one;
            iTxtRt.offsetMin = Vector2.zero; iTxtRt.offsetMax = Vector2.zero;
            TextMeshProUGUI iTxt = iTxtObj.GetComponent<TextMeshProUGUI>();
            iTxt.text = iconsText[i]; iTxt.fontSize = 35; iTxt.color = cTextDark;
            iTxt.alignment = TextAlignmentOptions.Center | TextAlignmentOptions.Midline;

            GameObject nameObj = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameObj.transform.SetParent(item.transform, false);
            RectTransform nameRt = nameObj.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0, 1); nameRt.anchorMax = new Vector2(1, 1);
            nameRt.pivot = new Vector2(0, 1);
            nameRt.offsetMin = new Vector2(150, -90); nameRt.offsetMax = new Vector2(-200, -30);
            TextMeshProUGUI nameTxt = nameObj.GetComponent<TextMeshProUGUI>();
            nameTxt.text = names[i]; nameTxt.fontSize = 38; nameTxt.fontStyle = FontStyles.Bold; nameTxt.color = cTextDark;

            GameObject subObj = new GameObject("Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            subObj.transform.SetParent(item.transform, false);
            RectTransform subRt = subObj.GetComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0, 1); subRt.anchorMax = new Vector2(1, 1);
            subRt.pivot = new Vector2(0, 1);
            subRt.offsetMin = new Vector2(150, -140); subRt.offsetMax = new Vector2(-200, -90);
            TextMeshProUGUI subTxt = subObj.GetComponent<TextMeshProUGUI>();
            subTxt.text = "Lantai Dasar"; subTxt.fontSize = 28; subTxt.color = cTextLight;

            GameObject distObj = new GameObject("Distance", typeof(RectTransform), typeof(TextMeshProUGUI));
            distObj.transform.SetParent(item.transform, false);
            RectTransform distRt = distObj.GetComponent<RectTransform>();
            distRt.anchorMin = new Vector2(1, 1); distRt.anchorMax = new Vector2(1, 1);
            distRt.pivot = new Vector2(1, 1);
            distRt.offsetMin = new Vector2(-200, -90); distRt.offsetMax = new Vector2(-40, -30);
            TextMeshProUGUI distTxt = distObj.GetComponent<TextMeshProUGUI>();
            distTxt.text = dists[i]; distTxt.fontSize = 38; distTxt.fontStyle = FontStyles.Bold; distTxt.color = cTextDark;
            distTxt.alignment = TextAlignmentOptions.Right | TextAlignmentOptions.Bottom;

            GameObject timeObj = new GameObject("Time", typeof(RectTransform), typeof(TextMeshProUGUI));
            timeObj.transform.SetParent(item.transform, false);
            RectTransform timeRt = timeObj.GetComponent<RectTransform>();
            timeRt.anchorMin = new Vector2(1, 1); timeRt.anchorMax = new Vector2(1, 1);
            timeRt.pivot = new Vector2(1, 1);
            timeRt.offsetMin = new Vector2(-200, -140); timeRt.offsetMax = new Vector2(-40, -90);
            TextMeshProUGUI timeTxt = timeObj.GetComponent<TextMeshProUGUI>();
            timeTxt.text = times[i]; timeTxt.fontSize = 28; timeTxt.color = cTextLight;
            timeTxt.alignment = TextAlignmentOptions.Right | TextAlignmentOptions.Top;
        }

        // 6. Bottom Nav Bar
        GameObject bottomObj = new GameObject("BottomNavArea", typeof(RectTransform), typeof(Image));
        bottomObj.transform.SetParent(canvasObj.transform, false);
        RectTransform bRt = bottomObj.GetComponent<RectTransform>();
        bRt.anchorMin = new Vector2(0, 0); bRt.anchorMax = new Vector2(1, 0);
        bRt.offsetMin = new Vector2(0, 0); bRt.offsetMax = new Vector2(0, 180);
        bottomObj.GetComponent<Image>().color = cWhite;

        string[] navTabs = { "Beranda", "Pindai", "Profil" };
        for (int i = 0; i < 3; i++)
        {
            GameObject tab = new GameObject("NavTab_" + navTabs[i], typeof(RectTransform), typeof(Button));
            tab.transform.SetParent(bottomObj.transform, false);
            RectTransform tRt = tab.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(i / 3f, 0); tRt.anchorMax = new Vector2((i + 1) / 3f, 1);
            tRt.offsetMin = Vector2.zero; tRt.offsetMax = Vector2.zero;

            GameObject tIcon = new GameObject("Icon", typeof(RectTransform), typeof(TextMeshProUGUI));
            tIcon.transform.SetParent(tab.transform, false);
            RectTransform tiRt = tIcon.GetComponent<RectTransform>();
            tiRt.anchorMin = new Vector2(0, 0.4f); tiRt.anchorMax = new Vector2(1, 1);
            tiRt.offsetMin = Vector2.zero; tiRt.offsetMax = new Vector2(0, -10);
            TextMeshProUGUI tiTxt = tIcon.GetComponent<TextMeshProUGUI>();
            tiTxt.text = i == 0 ? "H" : (i == 1 ? "QR" : "U");
            tiTxt.fontSize = 60; tiTxt.alignment = TextAlignmentOptions.Bottom | TextAlignmentOptions.Center;
            tiTxt.color = i == 0 ? cBg : cTextLight;

            GameObject tText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            tText.transform.SetParent(tab.transform, false);
            RectTransform ttRt = tText.GetComponent<RectTransform>();
            ttRt.anchorMin = new Vector2(0, 0); ttRt.anchorMax = new Vector2(1, 0.4f);
            ttRt.offsetMin = Vector2.zero; ttRt.offsetMax = Vector2.zero;
            TextMeshProUGUI ttTxt = tText.GetComponent<TextMeshProUGUI>();
            ttTxt.text = navTabs[i]; ttTxt.fontSize = 30;
            ttTxt.alignment = TextAlignmentOptions.Top | TextAlignmentOptions.Center;
            ttTxt.color = tiTxt.color;
        }

        // 7. Filter Popup (must be created last to be on top)
        GameObject filterPopup = new GameObject("FilterPopup", typeof(RectTransform), typeof(Image));
        filterPopup.transform.SetParent(canvasObj.transform, false);
        RectTransform popupRt = filterPopup.GetComponent<RectTransform>();
        popupRt.anchorMin = new Vector2(1, 1); popupRt.anchorMax = new Vector2(1, 1);
        popupRt.pivot = new Vector2(1, 1);
        popupRt.sizeDelta = new Vector2(450, 250);
        popupRt.anchoredPosition = new Vector2(-60, -320); // Aligned with bottom of filter button
        Image popupImg = filterPopup.GetComponent<Image>();
        popupImg.sprite = roundedRect; popupImg.type = Image.Type.Sliced; popupImg.color = cWhite;

        // Shadow/Border (optional mockup)
        Outline outline = filterPopup.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.15f); outline.effectDistance = new Vector2(0, -4);

        string[] filterOpts = { "Graha RSI", "IGD RSI" };
        GameObject opt1 = CreateFilterOption(filterPopup.transform, filterOpts[0], 0, cBg);
        GameObject opt2 = CreateFilterOption(filterPopup.transform, filterOpts[1], -125, cBg);

        filterPopup.SetActive(false); // Hide by default

        // 8. Connect to Manager
        var navManager = canvasObj.GetComponent("NavListManager") as MonoBehaviour;
        if (navManager != null)
        {
            SerializedObject so = new SerializedObject(navManager);
            so.Update();
            so.FindProperty("contentParent").objectReferenceValue = content.transform;
            so.FindProperty("searchField").objectReferenceValue = inputField;
            so.FindProperty("filterPopup").objectReferenceValue = filterPopup;
            so.FindProperty("mainScrollRect").objectReferenceValue = scrollRect;
            so.ApplyModifiedProperties();

            Button fBtn = filterBtnObj.GetComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(fBtn.onClick, new UnityEngine.Events.UnityAction(((NavListManager)navManager).ToggleFilterPopup));

            Button pindaiBtn = bottomObj.transform.Find("NavTab_Pindai").GetComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(pindaiBtn.onClick, new UnityEngine.Events.UnityAction(((NavListManager)navManager).LoadScannerScene));
            
            for (int i = 0; i < tabNames.Length; i++)
            {
                Button tabBtn = tabsContent.transform.Find("Tab_" + tabNames[i]).GetComponent<Button>();
                string tName = tabNames[i];
                UnityEditor.Events.UnityEventTools.AddStringPersistentListener(tabBtn.onClick, new UnityEngine.Events.UnityAction<string>(((NavListManager)navManager).OnTabClicked), tName);
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }

    private static GameObject CreateFilterOption(Transform parent, string text, float yOffset, Color iconColor)
    {
        GameObject opt = new GameObject("Option_" + text, typeof(RectTransform), typeof(Button));
        opt.transform.SetParent(parent, false);
        RectTransform rt = opt.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.sizeDelta = new Vector2(0, 125); rt.anchoredPosition = new Vector2(0, yOffset);

        GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(TextMeshProUGUI));
        iconObj.transform.SetParent(opt.transform, false);
        RectTransform iRt = iconObj.GetComponent<RectTransform>();
        iRt.anchorMin = new Vector2(0, 0.5f); iRt.anchorMax = new Vector2(0, 0.5f);
        iRt.pivot = new Vector2(0, 0.5f);
        iRt.sizeDelta = new Vector2(80, 80); iRt.anchoredPosition = new Vector2(30, 0);
        TextMeshProUGUI iTxt = iconObj.GetComponent<TextMeshProUGUI>();
        iTxt.text = text.Contains("Graha") ? "H" : "*"; // Mock icons
        iTxt.fontSize = 40; iTxt.color = iconColor;
        iTxt.alignment = TextAlignmentOptions.Center | TextAlignmentOptions.Midline;

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(opt.transform, false);
        RectTransform tRt = textObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0, 0); tRt.anchorMax = new Vector2(1, 1);
        tRt.offsetMin = new Vector2(130, 0); tRt.offsetMax = new Vector2(0, 0);
        TextMeshProUGUI tTxt = textObj.GetComponent<TextMeshProUGUI>();
        tTxt.text = text; tTxt.fontSize = 35; tTxt.color = ColorUtility.TryParseHtmlString("#333333", out Color ctd) ? ctd : Color.black;
        tTxt.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Midline;

        return opt;
    }
}
