using UnityEngine;
using TMPro;

public class ActiveNavigationUI : MonoBehaviour
{
    public static ActiveNavigationUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject panel;
    public TextMeshProUGUI startLocationText;
    public TextMeshProUGUI destinationLocationText;

    private void Awake()
    {
        // Setup Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Pastikan panel tersembunyi saat awal mula
        HideNavigation();
    }

    public void ShowNavigation(string startName, string destName)
    {
        if (panel != null) 
            panel.SetActive(true);
            
        if (startLocationText != null) 
            startLocationText.text = startName;
            
        if (destinationLocationText != null) 
            destinationLocationText.text = destName;
    }

    public void HideNavigation()
    {
        if (panel != null) 
            panel.SetActive(false);
    }
}
