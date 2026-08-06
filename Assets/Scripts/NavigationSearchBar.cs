using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NavigationSearchBar : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown floorDropdown;
    public TMP_InputField searchInputField;
    public GameObject scrollableListPanel;
    public Transform contentContainer;
    public GameObject locationItemPrefab; // Assign a prefab with a TextMeshProUGUI component

    [Header("Floor Databases")]
    private List<string> groundFloorLocations = new List<string>()
    {
        "Mandiri BPJS", "Bank Mega Syariah", "Loket Pendaftaran", "BPJS Center", 
        "Layanan Transportasi", "Resepsionis", "Lift", "Poli Gigi", "Toilet Umum", 
        "R. Oprasi Gigi", "Poli Bedah", "Poli THT", "Ruang Gizi", "Poli Ortopedi", 
        "Kasir", "Farmasi", "R. Poli Kandungan", "R. Poli Anak", "R. Poli Anak Laktasi", 
        "Masjid", "Minimarket Kantin", "R. TB DOTS", "R. Multazam", "Bilik Dahak", 
        "Kamar Jenazah", "Poli Spesialis Jantung", "Poli Spesialis Paru", 
        "Rehabilitasi Medik", "Poli Penyakit Dalam", "Poli Syaraf", "Poli Mata"
    };

    private List<GameObject> instantiatedLocationItems = new List<GameObject>();

    void Start()
    {
        // Setup Dropdown
        SetupFloorDropdown();

        // Hide List initially
        if (scrollableListPanel != null) scrollableListPanel.SetActive(false);

        // Bind Inputs
        if (searchInputField != null)
        {
            searchInputField.onValueChanged.AddListener(OnSearchValueChanged);
            searchInputField.onSelect.AddListener(OnSearchSelected);
        }

        // Initialize default floor data
        PopulateList(groundFloorLocations);
    }

    private void OnSearchSelected(string text)
    {
        // a. Open scrollable vertical list when input is focused
        if (scrollableListPanel != null)
        {
            scrollableListPanel.SetActive(true);
        }
    }

    private void SetupFloorDropdown()
    {
        if (floorDropdown != null)
        {
            floorDropdown.ClearOptions();
            List<string> floors = new List<string> { "Ground Floor", "Floor 1", "Floor 2", "Floor 3", "Floor 4" };
            floorDropdown.AddOptions(floors);
            
            // Listen for floor changes
            floorDropdown.onValueChanged.AddListener(OnFloorSelected);
        }
    }

    private void OnFloorSelected(int index)
    {
        // For now, index 0 is Ground Floor. 1-4 are empty.
        if (index == 0)
        {
            PopulateList(groundFloorLocations);
        }
        else
        {
            // Empty floors
            PopulateList(new List<string>());
        }
    }



    private void OnSearchValueChanged(string searchText)
    {
        if (scrollableListPanel != null && !scrollableListPanel.activeSelf)
        {
            scrollableListPanel.SetActive(true);
        }

        string query = searchText.ToLower();

        foreach (GameObject item in instantiatedLocationItems)
        {
            if (item == null) continue;

            TMP_Text textComp = item.GetComponentInChildren<TMP_Text>(true);
            if (textComp != null)
            {
                if (string.IsNullOrEmpty(query) || textComp.text.ToLower().Contains(query))
                {
                    item.SetActive(true);
                }
                else
                {
                    item.SetActive(false);
                }
            }
        }
    }

    private void PopulateList(List<string> locations)
    {
        if (contentContainer == null || locationItemPrefab == null) return;

        // Clear previous items
        foreach (GameObject item in instantiatedLocationItems)
        {
            Destroy(item);
        }
        instantiatedLocationItems.Clear();

        // Instantiate new items
        foreach (string loc in locations)
        {
            GameObject newItem = Instantiate(locationItemPrefab, contentContainer);
            TMP_Text textComp = newItem.GetComponentInChildren<TMP_Text>();
            if (textComp != null)
            {
                textComp.text = loc;
            }
            newItem.SetActive(true);
            instantiatedLocationItems.Add(newItem);

            Button btn = newItem.GetComponent<Button>();
            string targetRoomName = loc;
            if (btn != null)
            {
                btn.onClick.AddListener(() => {
                    Immersal.Samples.Navigation.IsNavigationTarget[] allTargets = Resources.FindObjectsOfTypeAll<Immersal.Samples.Navigation.IsNavigationTarget>();
                    Immersal.Samples.Navigation.IsNavigationTarget foundTarget = null;
                    
                    // Pass 1: Exact Match
                    foreach (var t in allTargets)
                    {
                        if (t != null && t.gameObject.scene.isLoaded)
                        {
                            string tName = !string.IsNullOrEmpty(t.targetName) ? t.targetName : t.gameObject.name;
                            if (string.Equals(tName, targetRoomName, System.StringComparison.OrdinalIgnoreCase))
                            {
                                foundTarget = t;
                                break;
                            }
                        }
                    }

                    // Pass 2: Smart Substring Match (jika Exact Match tidak ketemu)
                    if (foundTarget == null)
                    {
                        foreach (var t in allTargets)
                        {
                            if (t != null && t.gameObject.scene.isLoaded)
                            {
                                string tName = !string.IsNullOrEmpty(t.targetName) ? t.targetName : t.gameObject.name;
                                if (tName.Length >= 5 && targetRoomName != null &&
                                   !string.Equals(tName, "Poli", System.StringComparison.OrdinalIgnoreCase) &&
                                   !string.Equals(tName, "Farmasi", System.StringComparison.OrdinalIgnoreCase))
                                {
                                    if (targetRoomName.IndexOf(tName, System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        tName.IndexOf(targetRoomName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        foundTarget = t;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (foundTarget != null)
                    {
                        Immersal.Samples.Navigation.NavigationTargetListButton dummyBtn = foundTarget.gameObject.GetComponent<Immersal.Samples.Navigation.NavigationTargetListButton>();
                        if (dummyBtn == null) dummyBtn = foundTarget.gameObject.AddComponent<Immersal.Samples.Navigation.NavigationTargetListButton>();
                        dummyBtn.targetObject = foundTarget.gameObject;
                        Immersal.Samples.Navigation.NavigationManager.Instance?.InitializeNavigation(dummyBtn);
                    }
                    if (scrollableListPanel != null) scrollableListPanel.SetActive(false);
                });
            }
        }
        
        // Reset search filter if search bar is active
        if (searchInputField != null && searchInputField.gameObject.activeInHierarchy)
        {
            OnSearchValueChanged(searchInputField.text);
        }
    }
}
