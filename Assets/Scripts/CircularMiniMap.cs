using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircularMiniMap : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public Transform navigationTargetParent;
    
    [Header("UI Components")]
    public RectTransform minimapMask;
    public RectTransform mapRotationPivot; // Rotates with camera
    public RectTransform mapContent; // Translates with player
    public RectTransform playerIcon; // Fixed at center
    
    [Header("Prefabs")]
    public GameObject targetMarkerPrefab;
    public GameObject edgeArrowPrefab;
    
    [Header("Settings")]
    public float mapScale = 10f;
    public float arrowPadding = 15f;
    public float smoothSpeed = 10f;
    
    private List<Transform> targets = new List<Transform>();
    private Dictionary<Transform, RectTransform> targetMarkers = new Dictionary<Transform, RectTransform>();
    
    private Transform activeTarget;
    private RectTransform activeEdgeArrow;
    
    void Start()
    {
        // Try find XR Origin / Main Camera if not assigned
        if (playerTransform == null)
        {
            if (Camera.main != null)
                playerTransform = Camera.main.transform;
            else
                Debug.LogWarning("CircularMiniMap: Player Transform not assigned and Main Camera not found.");
        }
        
        // Auto detect NavigationTarget
        if (navigationTargetParent == null)
        {
            GameObject navTargetObj = GameObject.Find("NavigationTarget");
            if (navTargetObj != null)
            {
                navigationTargetParent = navTargetObj.transform;
            }
        }
        
        // Setup edge arrow
        if (edgeArrowPrefab != null && minimapMask != null)
        {
            GameObject arrowObj = Instantiate(edgeArrowPrefab, minimapMask);
            activeEdgeArrow = arrowObj.GetComponent<RectTransform>();
            activeEdgeArrow.gameObject.SetActive(false);
        }

        RefreshTargets();
    }
    
    public void RefreshTargets()
    {
        if (navigationTargetParent == null) return;
        
        // Clear old markers
        foreach (var marker in targetMarkers.Values)
        {
            if (marker != null) Destroy(marker.gameObject);
        }
        targetMarkers.Clear();
        targets.Clear();
        
        // Add new children
        foreach (Transform child in navigationTargetParent)
        {
            targets.Add(child);
            if (targetMarkerPrefab != null && mapContent != null)
            {
                GameObject markerObj = Instantiate(targetMarkerPrefab, mapContent);
                RectTransform markerRect = markerObj.GetComponent<RectTransform>();
                
                // Position marker in UI space corresponding to world space
                markerRect.anchoredPosition = new Vector2(child.position.x * mapScale, child.position.z * mapScale);
                markerObj.SetActive(true); // By default show all markers
                targetMarkers.Add(child, markerRect);
            }
        }
    }
    
    public void SetTarget(Transform newTarget)
    {
        activeTarget = newTarget;
    }
    
    void LateUpdate()
    {
        if (playerTransform == null || mapContent == null || mapRotationPivot == null || minimapMask == null) return;
        
        // 1. Move MapContent so Player is at center
        // World coordinates to UI coordinates: X -> X, Z -> Y
        Vector2 targetPosition = new Vector2(-playerTransform.position.x * mapScale, -playerTransform.position.z * mapScale);
        mapContent.anchoredPosition = Vector2.Lerp(mapContent.anchoredPosition, targetPosition, Time.deltaTime * smoothSpeed);
        
        // 2. Rotate MapRotationPivot around Z axis inversely to Player's Y rotation
        float playerYRotation = playerTransform.eulerAngles.y;
        Quaternion targetRotation = Quaternion.Euler(0, 0, playerYRotation);
        mapRotationPivot.localRotation = Quaternion.Lerp(mapRotationPivot.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
        
        // 3. Keep target markers upright
        foreach (var kvp in targetMarkers)
        {
            if (kvp.Value != null)
            {
                // Counter-rotate markers so they stay upright
                kvp.Value.rotation = Quaternion.identity;
            }
        }
        
        // 4. Handle Active Target & Edge Arrow
        if (activeTarget != null && activeEdgeArrow != null)
        {
            // Vector from player to target in world 2D space (X, Z)
            Vector2 dirToTargetWorld = new Vector2(activeTarget.position.x - playerTransform.position.x, activeTarget.position.z - playerTransform.position.z);
            float distToTargetUI = dirToTargetWorld.magnitude * mapScale;
            
            float radius = (minimapMask.rect.width / 2f);
            
            if (distToTargetUI > radius)
            {
                // Target is outside minimap
                activeEdgeArrow.gameObject.SetActive(true);
                
                // Angle relative to player's forward
                float angleWorld = Mathf.Atan2(dirToTargetWorld.x, dirToTargetWorld.y) * Mathf.Rad2Deg;
                float relativeAngle = angleWorld - playerYRotation;
                
                float angleRad = relativeAngle * Mathf.Deg2Rad;
                Vector2 arrowPos = new Vector2(Mathf.Sin(angleRad), Mathf.Cos(angleRad)) * (radius - arrowPadding);
                
                activeEdgeArrow.anchoredPosition = arrowPos;
                activeEdgeArrow.localRotation = Quaternion.Euler(0, 0, -relativeAngle);
                
                if (targetMarkers.TryGetValue(activeTarget, out RectTransform activeMarker))
                {
                    activeMarker.gameObject.SetActive(false);
                }
            }
            else
            {
                // Target is inside minimap
                activeEdgeArrow.gameObject.SetActive(false);
                if (targetMarkers.TryGetValue(activeTarget, out RectTransform activeMarker))
                {
                    activeMarker.gameObject.SetActive(true);
                }
            }
        }
        else if (activeEdgeArrow != null)
        {
            activeEdgeArrow.gameObject.SetActive(false);
        }
    }
}
