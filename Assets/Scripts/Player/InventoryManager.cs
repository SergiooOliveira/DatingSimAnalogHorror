using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [SerializeField] private InputActionReference toggleInventoryAction;
    [SerializeField] private InputActionReference scrollAction;

    [Header("Visual References")]
    [SerializeField] private GameObject SpriteCarousel; // The parent object of the ring
    [SerializeField] private CarouselInventory carouselScript; // The script that spins things

    private bool isOpen = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        SpriteCarousel.SetActive(false); // Hide at start
    }

    private void OnEnable()
    {
        toggleInventoryAction.action.Enable();
        if (scrollAction != null) scrollAction.action.Enable();
    }
    private void OnDisable()
    {
        toggleInventoryAction.action.Disable();
        if (scrollAction != null) scrollAction.action.Disable();
    }

    private void Update()
    {
        // Toggle with Tab
        if (toggleInventoryAction.action.WasPressedThisFrame())
        {
            isOpen = !isOpen;
            SpriteCarousel.SetActive(isOpen);

            if (!isOpen)
            {
                MaskData selected = carouselScript.GetSelectedMask();

                if (selected != null)
                {
                    Debug.Log($"Closed Inventory. Selected Mask: {selected.MaskName}");
                }
            }

            // Optional: Pause game or unlock cursor here
            Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isOpen;
        }

        if (isOpen && scrollAction != null)
        {
            float scrollValue = scrollAction.action.ReadValue<Vector2>().y;

            if (scrollValue != 0)
            {
                carouselScript.Rotate(scrollValue);
            }

        }
    }

    // This is the method Player calls!
    public void AddMaskVisual(MaskData data)
    {
        carouselScript.SpawnMaskVisual(data);
    }
}