using UnityEngine;

[CreateAssetMenu(menuName = "Mask/Effects/Spirit Sight Effect")]
public class SpiritSightEffect : MaskEffect
{
    [Header("Visuals")]
    [SerializeField] private GameObject overlayPrefab; // Drag your Green UI Prefab here

    // Internal reference to destroy the UI later
    private GameObject activeOverlay;
    
    // Store original settings to revert later
    private int originalCullingMask;
    private LayerMask spiritVisibleLayer;
    private LayerMask spiritHiddenLayer;

    public override void ActivateEffect(GameObject target)
    {
        // 1. SETUP LAYERS (Find them by name so IDs don't matter)
        int visibleID = LayerMask.NameToLayer("SpiritVisible");
        int hiddenID = LayerMask.NameToLayer("SpiritHidden");
        int playerLayerID = GameManager.Instance.playerMask;

        // 2. CAMERA LOGIC
        Camera cam = Camera.main;
        originalCullingMask = cam.cullingMask;

        // Bitwise Magic:
        // |= (1 << ID) ADDS a layer
        // &= ~(1 << ID) REMOVES a layer

        // Add SpiritVisible
        cam.cullingMask |= (1 << visibleID);
        // Remove SpiritHidden (The Door disappears)
        cam.cullingMask &= ~(1 << hiddenID);

        // 3. PHYSICS LOGIC (So you can walk through the door)
        // Ignore collision between Player and SpiritHidden objects
        Physics.IgnoreLayerCollision(playerLayerID, hiddenID, true);

        // (Optional) Ensure we collide with SpiritVisible (like a ghost bridge)
        Physics.IgnoreLayerCollision(playerLayerID, visibleID, false);

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
            }
        }

        Debug.Log("<color=green>SPIRIT WORLD ENTERED</color>");
    }

    public override void DeactivateEffect(GameObject target)
    {
        // 1. REVERT CAMERA
        if (Camera.main != null)
        {
            Camera.main.cullingMask = originalCullingMask;
        }

        // 2. REVERT PHYSICS
        int hiddenID = LayerMask.NameToLayer("SpiritHidden");
        int playerLayerID = GameManager.Instance.playerMask;

        // Re-enable collision with the door
        Physics.IgnoreLayerCollision(playerLayerID, hiddenID, false);

        // 3. DESTROY UI
        if (activeOverlay != null)
        {
            Destroy(activeOverlay);
        }

        Debug.Log("Spirit World Exited.");
    }
}