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
        }

        // Ensure popup is hidden on start
        if (filterPopup != null)
        {
            filterPopup.SetActive(false);
        }
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
            if (!isActive)
            {
                // Ensure it draws on top
                filterPopup.transform.SetAsLastSibling();
            }
        }
    }

    public void LoadScannerScene()
    {
        SceneManager.LoadScene("2_ScannerCamera");
    }

    public void OnTabClicked(string tabName)
    {
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
