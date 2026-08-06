using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class NavListManager : MonoBehaviour
{
    public Transform contentParent;
    public TMP_InputField searchField;
    public GameObject filterPopup; // Reference to the popup
    public ScrollRect mainScrollRect;

    private List<GameObject> listItems = new List<GameObject>();

    void Start()
    {
        // Populate listItems with existing children
        if (contentParent != null)
        {
            for (int i = 0; i < contentParent.childCount; i++)
            {
                listItems.Add(contentParent.GetChild(i).gameObject);
            }
        }

        if (searchField != null)
        {
            searchField.onValueChanged.AddListener(OnSearchValueChanged);
            // CEGAH TUMPANG TINDIH: Ketika bar pencarian diklik/fokus, tutup popup filter
            searchField.onSelect.AddListener((_) => {
                if (filterPopup != null && filterPopup.activeSelf) filterPopup.SetActive(false);
                GameObject scrollView = GetScrollViewObject();
                if (scrollView != null) scrollView.SetActive(true);
            });
        }

        // Ensure popup is hidden on start
        if (filterPopup != null)
        {
            filterPopup.SetActive(false);
        }
    }

    private GameObject GetScrollViewObject()
    {
        if (mainScrollRect != null) return mainScrollRect.gameObject;
        if (contentParent != null && contentParent.parent != null && contentParent.parent.parent != null)
        {
            return contentParent.parent.parent.gameObject;
        }
        return null;
    }

    public void OnSearchValueChanged(string query)
    {
        query = query.ToLower().Trim();
        for (int i = 0; i < listItems.Count; i++)
        {
            if (string.IsNullOrEmpty(query))
            {
                listItems[i].SetActive(true);
            }
            else
            {
                // Search by reading the "Name" text component of the item
                Transform nameTransform = listItems[i].transform.Find("Name");
                if (nameTransform != null)
                {
                    TextMeshProUGUI tmp = nameTransform.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        bool match = tmp.text.ToLower().Contains(query);
                        listItems[i].SetActive(match);
                        continue;
                    }
                }
                listItems[i].SetActive(true);
            }
        }
    }

    public void ToggleFilterPopup()
    {
        if (filterPopup != null)
        {
            bool isActive = filterPopup.activeSelf;
            filterPopup.SetActive(!isActive);
            GameObject scrollView = GetScrollViewObject();
            if (!isActive)
            {
                // Ensure it draws on top
                filterPopup.transform.SetAsLastSibling();
                // CEGAH TUMPANG TINDIH: Saat popup filter terbuka, sembunyikan list ruangan
                if (scrollView != null) scrollView.SetActive(false);
            }
            else
            {
                // Saat popup filter ditutup, munculkan kembali list ruangan
                if (scrollView != null) scrollView.SetActive(true);
            }
        }
    }

    public void LoadScannerScene()
    {
        SceneManager.LoadScene("4_Scan Screen");
    }

    public void OnTabClicked(string tabName)
    {
        // Tutup popup filter & munculkan list ruangan yang sudah difilter
        if (filterPopup != null) filterPopup.SetActive(false);
        GameObject scrollView = GetScrollViewObject();
        if (scrollView != null) scrollView.SetActive(true);

        string filter = tabName.ToLower();

        for (int i = 0; i < listItems.Count; i++)
        {
            if (tabName == "Semua")
            {
                listItems[i].SetActive(true);
            }
            else
            {
                // Cari kompenen "Subtitle" yang menyimpan informasi lantai
                Transform subTransform = listItems[i].transform.Find("Subtitle");
                if (subTransform != null)
                {
                    TextMeshProUGUI tmp = subTransform.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        // "Lantai 1" akan cocok dengan "Lantai 1" di subtitle
                        bool match = tmp.text.ToLower().Contains(filter);
                        listItems[i].SetActive(match);
                        continue;
                    }
                }
                listItems[i].SetActive(false); // Sembunyikan jika tidak cocok
            }
        }

        if (mainScrollRect != null)
        {
            mainScrollRect.verticalNormalizedPosition = 1f; // Scrolldown/reset ke posisi paling atas
        }
    }
}
