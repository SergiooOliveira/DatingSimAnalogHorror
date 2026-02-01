using UnityEngine;

[CreateAssetMenu(menuName = "Mask/Effects/Spirit Sight Effect")]
public class SpiritSightEffect : MaskEffect
{
    [Header("Visuals")]
    [SerializeField] private GameObject overlayPrefab; // Drag your Green UI Prefab here

    [Header("Layer Configuration")]
    [SerializeField] private LayerMask spiritVisibleLayer;
    [SerializeField] private LayerMask spiritHiddenLayer;

    // Internal reference to destroy the UI later
    private GameObject activeOverlay;
    
    // Store original settings to revert later
    private int originalCullingMask;
    
    public override void ActivateEffect(GameObject target)
    {
        // 2. CAMERA LOGIC
        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError("No main camera found in the scene.");
            return;
        }

        originalCullingMask = cam.cullingMask;
        Debug.Log($"2. Camera Found. Old Mask: {originalCullingMask}");

        // Bitwise Magic:
        // |= (1 << ID) ADDS a layer
        // &= ~(1 << ID) REMOVES a layer

        // Add SpiritVisible
        cam.cullingMask |= spiritVisibleLayer.value;
        // Remove SpiritHidden (The Door disappears)
        cam.cullingMask &= ~spiritHiddenLayer.value;

        Debug.Log("3. Camera Mask Updated.");

        int hiddenLayerID = GetLayerIndexFromMask(spiritHiddenLayer);
        int playerID = GetLayerIndexFromMask(GameManager.Instance.playerMask);

        if (hiddenLayerID != -1)
        {
            Debug.Log($"4. Disabling collision between Player ({playerID}) and Layer {hiddenLayerID}");
            Physics.IgnoreLayerCollision(playerID, hiddenLayerID, true);
        }
        else
        {
            Debug.LogError("ERROR: 'Hidden Layer' is not assigned in the Mask Data Inspector!");
        }

        //// 3. PHYSICS LOGIC (So you can walk through the door)
        //// Ignore collision between Player and SpiritHidden objects
        //Physics.IgnoreLayerCollision(playerLayerID, hiddenID, true);

        //// (Optional) Ensure we collide with SpiritVisible (like a ghost bridge)
        //Physics.IgnoreLayerCollision(playerLayerID, visibleID, false);

        // 4. UI OVERLAY
        if (overlayPrefab != null)
        {
            // Instantiate inside the player's canvas or just roughly on screen
            // For simplicity, we assume there is a Canvas tagged "MainCanvas" or we just spawn it
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                activeOverlay = Instantiate(overlayPrefab, canvas.transform);
                // Ensure it's behind other UI like health bars (optional)
                activeOverlay.transform.SetAsFirstSibling();
                Debug.Log("5. Flare Spawned.");
            }
        }

        Debug.Log("<color=green>SPIRIT WORLD ENTERED</color>");
    }

    public override void DeactivateEffect(GameObject target)
    {
        // 1. REVERT CAMERA
        if (Camera.main != null) Camera.main.cullingMask = originalCullingMask;        
        if (activeOverlay != null) Destroy(activeOverlay);

        // 2. REVERT PHYSICS
        int hiddenID = GetLayerIndexFromMask(spiritHiddenLayer);
        int playerID = GetLayerIndexFromMask(GameManager.Instance.playerMask);

        if (hiddenID != -1)
        {
            // Re-enable collision with the door
            Physics.IgnoreLayerCollision(playerID, hiddenID, false);
        }

        Debug.Log("Spirit World Exited.");
    }

    private int GetLayerIndexFromMask(LayerMask mask)
    {
        int value = mask.value;
        if (value == 0) return -1; // Nothing selected
        for (int i = 0; i < 32; i++)
        {
            if ((value & (1 << i)) != 0) return i;
        }
        return -1;
    }
}