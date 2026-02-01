using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    [SerializeField] private PlayerData playerData = new PlayerData();
    public PlayerData PlayerData => playerData;

    public bool IsDisguised { get; private set; } = false;

    private MaskData currentMask;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetDisguise (bool state)
    {
        IsDisguised = state;
        Debug.Log(state ? "Player is now disguised." : "Player is no longer disguised.");
    }

    public void InteractWithMonster(Monster monster)
    {
        DialogManager.Instance.EnterDialogueMode(monster.InkJson);
    }

    public void ReceiveMask(MaskData maskData)
    {
        bool wasAdded = playerData.AddMask(maskData);

        if (wasAdded)
        {
            InventoryManager.Instance.AddMaskVisual(maskData);
            Debug.Log($"Added a new Mask: {maskData.MaskName}");
        }
    }

    public void EquipMask(MaskData maskData)
    {
        if (currentMask != null && currentMask.MaskEffects != null)
        {
            foreach (MaskEffect effect in currentMask.MaskEffects)
            {
                effect.DeactivateEffect(this.gameObject);
            }
        }

        currentMask = maskData;

        if (currentMask != null && currentMask.MaskEffects != null)
        {
            foreach (MaskEffect effect in currentMask.MaskEffects)
            {
                effect.ActivateEffect(this.gameObject);
            }
        }
    }
}
