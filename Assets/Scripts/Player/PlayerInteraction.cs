using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform playerCamera; // Drag your Main Camera here
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private float viewAngle = 60f; // How wide the cone is
    [SerializeField] private LayerMask interactLayer; // Set to "Interactable"
    [SerializeField] private LayerMask obstructionLayer; // Set to "Default" (walls)

    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    private void OnEnable() => interactAction.action.Enable();
    private void OnDisable() => interactAction.action.Disable();

    private void Update()
    {
        if (interactAction.action.WasPressedThisFrame())
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        // 1. Get all objects within range (Sphere Check)
        Collider[] hits = Physics.OverlapSphere(playerCamera.position, interactRange, interactLayer);

        float closestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            // 2. Check Direction (Cone Check)
            Vector3 directionToTarget = (hit.transform.position - playerCamera.position).normalized;

            // Calculate angle between Camera Forward and Object Direction
            float angleToTarget = Vector3.Angle(playerCamera.forward, directionToTarget);

            // If object is within our field of view (half of the total angle)
            if (angleToTarget < viewAngle / 2)
            {
                // 3. Check Line of Sight (Raycast Check)
                // Prevents picking up items through walls
                float distanceToTarget = Vector3.Distance(playerCamera.position, hit.transform.position);

                if (!Physics.Raycast(playerCamera.position, directionToTarget, distanceToTarget, obstructionLayer))
                {
                    // Valid Target! Let's find the closest one if there are multiple.
                    if (distanceToTarget < closestDistance)
                    {
                        closestDistance = distanceToTarget;
                        if (hit.TryGetComponent<Monster>(out Monster monster))
                            DialogManager.Instance.EnterDialogueMode(monster.InkJson);
                        else if (hit.TryGetComponent<Mask>(out Mask mask))
                        {
                            Player.Instance.ReceiveMask(mask.MaskData);
                            Destroy(mask.gameObject);
                        }
                    }
                }
            }
        }
    }

    // --- VISUALIZATION (DEBUG) ---
    private void OnDrawGizmosSelected()
    {
        if (playerCamera == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerCamera.position, interactRange);

        // Draw the edges of the cone
        Vector3 leftRay = Quaternion.Euler(0, -viewAngle / 2, 0) * playerCamera.forward;
        Vector3 rightRay = Quaternion.Euler(0, viewAngle / 2, 0) * playerCamera.forward;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(playerCamera.position, leftRay * interactRange);
        Gizmos.DrawRay(playerCamera.position, rightRay * interactRange);
    }
}