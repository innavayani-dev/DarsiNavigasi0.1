using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Immersal.Samples.Navigation
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(ScrollRect))]
    public class NavigationTargetListControl : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_ButtonTemplate = null;
        [SerializeField]
        private RectTransform m_ContentParent = null;
        [SerializeField]
        private int m_MaxButtonsOnScreen = 4;
        private List<GameObject> m_Buttons = new List<GameObject>();

        public void GenerateButtons()
        {
            if (m_Buttons.Count > 0)
            {
                DestroyButtons();
            }

            // loops through all navigation categories
            foreach (KeyValuePair<NavigationTargets.NavigationCategory, List<GameObject>> entry in NavigationTargets.NavigationTargetsDict)
            {
                // loops through all targets in each category
                foreach (GameObject go in entry.Value)
                {
                    if (go == null) continue;
                    if (go.name.IndexOf("Phantom", System.StringComparison.OrdinalIgnoreCase) >= 0) continue;
                    IsNavigationTarget isNavigationTarget = go.GetComponent<IsNavigationTarget>();
                    if (isNavigationTarget == null) continue;
                    string targetName = isNavigationTarget.targetName;
                    if (!string.IsNullOrEmpty(targetName) && targetName.IndexOf("Phantom", System.StringComparison.OrdinalIgnoreCase) >= 0) continue;
                    Sprite icon = isNavigationTarget.icon;

                    GameObject button = Instantiate(m_ButtonTemplate, m_ContentParent);
                    m_Buttons.Add(button);
                    button.SetActive(true);
                    button.name = string.Format("button {0}", targetName);

                    NavigationTargetListButton navigationTargetListButton = button.GetComponent<NavigationTargetListButton>();
                    navigationTargetListButton.SetText(targetName);
                    navigationTargetListButton.SetIcon(icon);
                    navigationTargetListButton.SetTarget(go);

                    // --- MODIFIKASI DIMULAI DISINI ---
                    // Menambahkan listener agar saat tombol diklik, hanya target ini yang muncul
                    Button btnComponent = button.GetComponent<Button>();
                    if (btnComponent != null)
                    {
                        btnComponent.onClick.AddListener(() => {
                            HandleTargetVisibility(targetName);
                        });
                    }
                    // --- MODIFIKASI SELESAI ---
                }
            }

            // calculate lists RectTransform size
            float x = m_ButtonTemplate.GetComponent<RectTransform>().sizeDelta.x;
            float y = m_ButtonTemplate.GetComponent<RectTransform>().sizeDelta.y * Mathf.Min(m_Buttons.Count, m_MaxButtonsOnScreen);
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(x, y);

            ScrollToTop();
        }

        // Fungsi baru untuk mengatur siapa yang boleh muncul di layar AR
        private void HandleTargetVisibility(string selectedName)
        {
            // Cari semua script IsNavigationTarget yang ada di scene (termasuk yang inactive)
            IsNavigationTarget[] allTargets = FindObjectsOfType<IsNavigationTarget>(true);

            foreach (var target in allTargets)
            {
                bool isChosenOne = (target.targetName == selectedName);
                target.SetVisible(isChosenOne);
                if (isChosenOne)
                {
                    DoorbellARPopup.Instance.CheckAndShow(selectedName, target.transform);
                }
            }

            // TUTUP OTOMATIS PANEL LIST RUANGAN
            NavigationManager navMgr = Object.FindObjectOfType<NavigationManager>();
            if (navMgr != null)
            {
                navMgr.CloseNavigationList();
            }

            // Sebagai pengaman ganda jika NavigationTargetListControl berada di panel tersendiri:
            gameObject.SetActive(false);
            if (transform.parent != null && (transform.parent.name.Contains("Panel") || transform.parent.name.Contains("List") || transform.parent.name.Contains("Scroll") || transform.parent.name.Contains("Dropdown")))
            {
                transform.parent.gameObject.SetActive(false);
            }
        }

        private void DestroyButtons()
        {
            foreach (GameObject button in m_Buttons)
            {
                Destroy(button);
            }
            m_Buttons.Clear();
        }

        private void ScrollToTop()
        {
            if (transform.GetComponent<ScrollRect>() != null)
                transform.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
        }
    }
}