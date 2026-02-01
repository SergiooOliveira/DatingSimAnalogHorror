using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player Instance;

    [Header("Player Data")]
    [SerializeField] private PlayerData playerData = new PlayerData();
    public PlayerData PlayerData => playerData;

    [Header("Mask System")]
    public bool IsDisguised { get; private set; } = false;
    private MaskData currentMask;
    public MaskData CurrentMask => currentMask;

    [Header("Corruption System")]
    [SerializeField] private Slider corruptionSlider;
    [SerializeField] private float maxCorruption = 100f;
    [SerializeField] private float recoveryRate = 2.0f; // Recupera sem máscara
    private float currentCorruption = 0f;

    [Header("References")]
    [SerializeField] private Transform mainCanvasTransform;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (mainCanvasTransform == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null) mainCanvasTransform = canvas.transform;
        }
    }

    private void Start()
    {
        // Inicialização do Inventário baseada no PlayerData
        if (playerData.playerMasks.Count == 0)
        {
            Debug.LogWarning("Player has no masks in PlayerData at start.");
        }

        if (InventoryManager.Instance != null)
        {
            foreach (MaskData mask in playerData.playerMasks)
            {
                InventoryManager.Instance.AddMaskVisual(mask);
            }
        }
        else
        {
            Debug.LogWarning("InventoryManager instance not found at Player Start.");
        }

        // Configuração inicial do Slider
        if (corruptionSlider != null)
        {
            corruptionSlider.maxValue = maxCorruption;
            corruptionSlider.value = 0f;
        }
    }

    private void Update()
    {
        HandleMaskCorruption();
    }

    private void HandleMaskCorruption()
    {
        // Pausa a corrupção se o diálogo estiver a decorrer
        if (DialogManager.Instance != null && DialogManager.Instance.dialogueIsPlaying) return;

        if (currentMask != null)
        {
            // Aplica a taxa da máscara (Positiva corrompe, Negativa como a Humana limpa)
            currentCorruption += currentMask.CorruptionRate * Time.deltaTime;
        }
        else
        {
            // Recupera sanidade passivamente apenas se estiver SEM máscara
            if (currentCorruption > 0)
            {
                currentCorruption -= recoveryRate * Time.deltaTime;
            }
        }

        // Garante que a corrupção nunca seja menor que 0 nem maior que o máximo
        currentCorruption = Mathf.Clamp(currentCorruption, 0, maxCorruption);

        if (currentCorruption >= maxCorruption)
        {
            OnCorruptionFull();
        }

        // Atualiza a UI se existir
        if (corruptionSlider != null)
        {
            corruptionSlider.value = currentCorruption;
        }
    }

    private void OnCorruptionFull()
    {
        Debug.LogWarning("Corrupção máxima atingida! Retirando máscara...");
        UnequipMask();
    }

    public void SetDisguise(bool state)
    {
        IsDisguised = state;
    }

    // --- SISTEMA DE DIÁLOGO ---
    public void InteractWithMonster(Monster monster)
    {
        if (monster == null) return;
        if (DialogManager.Instance.dialogueIsPlaying) return;

        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;

        DialogManager.Instance.EnterDialogueMode(monster);
    }

    // --- SISTEMA DE MÁSCARAS ---
    public void ReceiveMask(MaskData maskData)
    {
        if (playerData.AddMask(maskData))
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.AddMaskVisual(maskData);
        }
    }

    public void EquipMask(MaskData maskData)
    {
        // Remove efeitos da máscara anterior
        if (currentMask != null && currentMask.MaskEffects != null)
        {
            foreach (MaskEffect effect in currentMask.MaskEffects)
                effect.DeactivateEffect(this.gameObject);
        }

        currentMask = maskData;

        // Ativa efeitos da nova máscara
        if (currentMask != null)
        {
            if (currentMask.MaskEffects != null)
            {
                foreach (MaskEffect effect in currentMask.MaskEffects)
                    effect.ActivateEffect(this.gameObject);
            }

            // A máscara Humana limpa a corrupção mas NÃO disfarça o jogador dos monstros
            SetDisguise(currentMask.MaskCorruptionLevel != MaskCorruption.Humana);
        }
        else
        {
            SetDisguise(false);
        }
    }

    public void UnequipMask()
    {
        EquipMask(null);
    }

    #region Diary
    // --- DIARY SYSTEM ---
    public void InteractWithDiaryItem(DiaryItem item)
    {
        if (item != null) item.Interact();
    }

    public void CollectDiaryCover()
    {
        playerData.hasDiaryCover = true;
    }

    public void CollectDiaryPage(int pageNumber)
    {
        playerData.AddPage();

        if (playerData.pagesCollected >= PlayerData.totalPages)
        {
            Debug.Log("DIARY COMPLETED! You found the truth.");
        }
    }
    #endregion
}