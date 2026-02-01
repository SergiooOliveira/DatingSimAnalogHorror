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
        // Se o diálogo estiver a decorrer, pausamos a corrupção
        if (DialogManager.Instance != null && DialogManager.Instance.dialogueIsPlaying) return;

        if (currentMask != null)
        {
            // Aumenta a corrupção
            currentCorruption += currentMask.CorruptionRate * Time.deltaTime;

            if (currentCorruption >= maxCorruption)
            {
                currentCorruption = maxCorruption;
                OnCorruptionFull();
            }
        }
        else
        {
            // Recupera sanidade se estiver sem máscara
            if (currentCorruption > 0)
            {
                currentCorruption -= recoveryRate * Time.deltaTime;
                currentCorruption = Mathf.Max(currentCorruption, 0);
            }
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
        // Podes adicionar aqui um efeito visual de "flash" ou som de susto
    }

    public void SetDisguise(bool state)
    {
        IsDisguised = state;
    }

    public void InteractWithMonster(Monster monster)
    {
        if (monster == null) return;
        if (DialogManager.Instance.dialogueIsPlaying) return;

        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;

        DialogManager.Instance.EnterDialogueMode(monster);
    }

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
        // Se já tiver uma, remove efeitos
        if (currentMask != null && currentMask.MaskEffects != null)
        {
            foreach (MaskEffect effect in currentMask.MaskEffects)
                effect.DeactivateEffect(this.gameObject);
        }

        currentMask = maskData;

        // Ativa novos efeitos
        if (currentMask != null)
        {
            if (currentMask.MaskEffects != null)
            {
                foreach (MaskEffect effect in currentMask.MaskEffects)
                    effect.ActivateEffect(this.gameObject);
            }
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
}