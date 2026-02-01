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

    [Header("References")]
    [SerializeField] private Transform mainCanvasTransform;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Tenta encontrar o canvas se n�o estiver atribu�do
        if (mainCanvasTransform == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null) mainCanvasTransform = canvas.transform;
        }
    }

    public void SetDisguise(bool state)
    {
        IsDisguised = state;
    }

    // --- SISTEMA DE DI�LOGO (VERS�O ACTUALIZADA) ---
    public void InteractWithMonster(Monster monster)
    {
        if (monster == null) return;

        // Se o di�logo j� estiver a decorrer, ignorar
        if (DialogManager.Instance.dialogueIsPlaying) return;

        // Desativa o movimento do jogador
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;

        // Inicia o di�logo usando a UI fixa da cena atrav�s do Manager
        DialogManager.Instance.EnterDialogueMode(monster);
    }

    // --- SISTEMA DE M�SCARAS ---
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
        // Remove efeitos da m�scara anterior
        if (currentMask != null && currentMask.MaskEffects != null)
        {
            foreach (MaskEffect effect in currentMask.MaskEffects)
                effect.DeactivateEffect(this.gameObject);
        }

        currentMask = maskData;

        // Ativa efeitos da nova m�scara
        if (currentMask != null && currentMask.MaskEffects != null)
        {
            foreach (MaskEffect effect in currentMask.MaskEffects)
                effect.ActivateEffect(this.gameObject);
        }
    }
}