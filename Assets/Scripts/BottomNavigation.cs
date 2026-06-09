using UnityEngine;

public class BottomNavigation : MonoBehaviour
{
    private SceneFlowManager flowManager;

    void Start()
    {
        flowManager = FindObjectOfType<SceneFlowManager>();
    }

    public void OnHomeClicked()
    {
        // Currently does nothing as Scene 4 is the Home
    }

    public void OnScanClicked()
    {
        if (flowManager != null)
        {
            flowManager.GoToScanner();
        }
    }

    public void OnProfileClicked()
    {
        // Placeholder for Profile
    }
}
