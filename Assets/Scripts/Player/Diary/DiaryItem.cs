using UnityEngine;

public class DiaryItem : MonoBehaviour
{
    public enum ItemType { Cover, Page }

    [Header("Settings")]
    [SerializeField] private ItemType itemType;
    [SerializeField] private int pageNumber; // Optional (e.g., "Page 3")

    [Header("Feedback")]
    // Assign your Ink JSON or simple string here for the "failure" message
    [TextArea] public string lockedMessage = "I need the Diary Cover first to store this page.";
    [TextArea] public string successMessage = "Collected!";

    public int PageNumber => pageNumber;
    public void Interact()
    {
        // 1. Logic for the COVER
        if (itemType == ItemType.Cover)
        {
            Player.Instance.CollectDiaryCover();

            // Visual feedback (Sound, particle, etc.)
            Debug.Log(successMessage);

            // Destroy or Hide the object
            Destroy(gameObject);
        }
        // 2. Logic for a PAGE
        else
        {
            // Check if player HAS the cover
            if (Player.Instance.PlayerData.hasDiaryCover)
            {
                Player.Instance.CollectDiaryPage(pageNumber);
                Debug.Log($"Page {pageNumber} collected! ({Player.Instance.PlayerData.pagesCollected}/8)");
                Destroy(gameObject);
            }
            else
            {
                // Player does NOT have the cover.
                // SHOW DIALOGUE HERE ("I can't carry this loose page...")
                Debug.Log(lockedMessage);

                // Do NOT destroy the object. It stays in the scene.
            }
        }
    }
}