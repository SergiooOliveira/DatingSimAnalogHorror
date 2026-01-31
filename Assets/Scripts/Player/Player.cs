using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    [SerializeField] private PlayerData playerData = new PlayerData();
    public PlayerData PlayerData => playerData;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
        }
    }
}
