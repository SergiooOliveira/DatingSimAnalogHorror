using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarouselInventory : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector2 ovalSize = new Vector2(200f, 100f); // Width/Height of the circle
    [SerializeField] private float rotateSpeed = 5f;
    [SerializeField] private float scrollSensitivity = 1f;

    [Header("Perspective")]
    [SerializeField] private float minScale = 0.5f; // Size of items in the back
    [SerializeField] private float maxScale = 1.0f; // Size of items in the front
    [SerializeField] private float xOffset = 50f;   // Moves the whole ring left/right
    [SerializeField] private float yOffset = 50f;   // Moves the whole ring up/down
    
    [Header("Data")]
    [SerializeField] private GameObject spritePrefab; // A simple UI Image prefab

    private List<CarouselItem> spawnedItems = new List<CarouselItem>();
    private float currentAngle = 0f;
    private float targetAngle = 0f;
    private int selectedIndex = 0;

    // Helper class to track the object and its "depth" for sorting
    private class CarouselItem
    {
        public GameObject obj;
        public RectTransform rect;
        public float currentDepth; // Used for sorting
        public MaskData maskData;
    }

    public void SpawnMaskVisual(MaskData data)
    {
        if (data == null || data.MaskPrefab == null)
        {
            Debug.Log($"No data or prefab found for {data.MaskName}");
            return;
        }

        // 1. Create the UI Image
        GameObject newItem = Instantiate(spritePrefab, transform);

        // 2. Assign the Sprite (Assuming your 3D prefab has a simple way to get a sprite, 
        // OR you can add a 'Sprite' field to your MaskData. For now, we'll try to grab an image)
        Image img = newItem.GetComponent<Image>();

        if  (data.MaskIcon != null) img.sprite = data.MaskIcon;
        else Debug.LogWarning($"MaskData for {data.MaskName} does not have a MaskIcon assigned.");

        img.preserveAspect = true;

        // *NOTE: If your MaskData has a sprite field, use: img.sprite = data.maskSprite;
        // If not, we can't easily turn a 3D prefab into a sprite without a separate texture.*
        // For testing, just assign a color or placeholder sprite here.
        newItem.name = data.MaskName;

        // 3. Add to list
        CarouselItem item = new CarouselItem
        {
            obj = newItem,
            rect = newItem.GetComponent<RectTransform>(),
            maskData = data
        };
        spawnedItems.Add(item);

        RefreshLayout();
    }

    public void Rotate(float scrollAmount)
    {
        if (spawnedItems.Count == 0) return;
        
        float adustedScroll = scrollAmount * scrollSensitivity;

        if (adustedScroll != 0)
        {
            ChangeSelection(adustedScroll > 0 ? -1 : 1);
        }
    }


    void Update()
    {
        // 2. Rotation Logic
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * rotateSpeed);

        // 3. Position & Sort
        UpdatePositions();
    }

    private void UpdatePositions()
    {
        if (spawnedItems.Count == 0) return;

        float angleStep = 360f / spawnedItems.Count;

        for (int i = 0; i < spawnedItems.Count; i++)
        {
            CarouselItem item = spawnedItems[i];

            // Calculate angle for this specific item
            float itemAngle = currentAngle + (i * angleStep);
            float rad = itemAngle * Mathf.Deg2Rad;

            // A. Calculate Oval Position (Trigonometry)
            // Cos = X (Width), Sin = Y (Depth/Height)
            float x = Mathf.Cos(rad) * ovalSize.x;

            // We use Sin for Y, but flatten it to make it look like a floor circle
            float y = Mathf.Sin(rad) * ovalSize.y;

            // B. Calculate "Depth" (0 to 1)
            // When Sin is -1 (Front), Depth is 1. When Sin is 1 (Back), Depth is 0.
            // This assumes -Y is "Close to camera" on screen, or we can use standard Z logic.
            // Let's say -90 degrees (Sin -1) is the FRONT.
            float depth = Mathf.InverseLerp(1f, -1f, Mathf.Sin(rad)); // 1.0 = Front, 0.0 = Back

            // C. Apply Position
            item.rect.anchoredPosition = new Vector2(x + xOffset, y + yOffset);

            // D. Apply Scale (Perspective)
            float scale = Mathf.Lerp(minScale, maxScale, depth);
            item.rect.localScale = Vector3.one * scale;

            // E. Store depth for sorting
            item.currentDepth = depth;

            // Optional: Fade out items in the back
            if (item.obj.TryGetComponent<CanvasGroup>(out var cg)) cg.alpha = Mathf.Lerp(0.5f, 1f, depth);
        }

        // 4. SORTING (The "See Behind" Magic)
        // We sort the list by depth so items in back are drawn first, items in front drawn last.        
        List<CarouselItem> sortedItems = new List<CarouselItem>(spawnedItems);

        sortedItems.Sort((a, b) =>
        {
            // If depths are nearly equal, sort by instance ID to ensure consistent order
            return a.currentDepth.CompareTo(b.currentDepth);
        });

        for (int i = 0; i < spawnedItems.Count; i++)
        {
            spawnedItems[i].rect.SetSiblingIndex(i);
        }
    }

    private void ChangeSelection(int direction)
    {
        if (spawnedItems.Count == 0) return;

        selectedIndex += direction;

        // Rotate the wheel so the selected item is at -90 degrees (Front)
        float angleStep = 360f / spawnedItems.Count;
        targetAngle = -selectedIndex * angleStep - 90f;
        // +90 offset ensures index 0 starts at the bottom (Front)
    }

    private void RefreshLayout()
    {
        // Reset target to align smoothly
        float angleStep = 360f / spawnedItems.Count;
        targetAngle = -selectedIndex * angleStep - 90f;
    }

    public MaskData GetSelectedMask()
    {
        if (spawnedItems.Count == 0) return null;

        int actualIndex = (selectedIndex % spawnedItems.Count + spawnedItems.Count) % spawnedItems.Count;

        Debug.Log($"actualIndex: {actualIndex} = selectedIndex: {selectedIndex} + spawnedItems.Count: {spawnedItems.Count}");

        return spawnedItems[actualIndex].maskData;
    }
}