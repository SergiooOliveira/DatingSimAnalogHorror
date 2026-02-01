using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player Instance;

    [SerializeField] private PlayerData playerData = new PlayerData();
    public PlayerData PlayerData => playerData;

    public bool IsDisguised { get; private set; } = false;
    private MaskData currentMask;

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

    public void SetDisguise(bool state) { IsDisguised = state; }

    public void InteractWithMonster(Monster monster)
    {
        if (DialogManager.Instance.dialogueIsPlaying) return;

        // 1. INSTANCIAR HUD
        if (monster.HudPrefab != null && mainCanvasTransform != null)
        {
            GameObject spawnedHUD = Instantiate(monster.HudPrefab, mainCanvasTransform);

            // 2. BUSCA AUTOMÁTICA (Sem scripts de interface)

            // A. Encontrar Texto (Assume que o primeiro TMP que encontrar é o dialogo)
            TextMeshProUGUI foundText = spawnedHUD.GetComponentInChildren<TextMeshProUGUI>();

            // B. Encontrar Botões (Todos os botões dentro do prefab)
            Button[] foundButtonsComponents = spawnedHUD.GetComponentsInChildren<Button>(true);
            List<GameObject> foundButtonsGO = new List<GameObject>();
            foreach (var btn in foundButtonsComponents) foundButtonsGO.Add(btn.gameObject);

            // C. Encontrar Retrato (Procura um objeto chamado "Portrait" que tenha uma Imagem)
            Image foundPortrait = null;
            Image[] allImages = spawnedHUD.GetComponentsInChildren<Image>(true);
            foreach (var img in allImages)
            {
                if (img.gameObject.name == "Portrait") // <--- REGRA DE NOME
                {
                    foundPortrait = img;
                    break;
                }
            }

            // 3. Enviar para o Manager
            if (foundText != null && foundButtonsGO.Count > 0)
            {
                DialogManager.Instance.SwapDialogueUI(
                    spawnedHUD,
                    foundText,
                    foundPortrait, // Pode ser nulo se não encontrares, o Manager lida com isso
                    foundButtonsGO.ToArray(),
                    spawnedHUD
                );
            }
            else
            {
                Debug.LogWarning($"Player: O HUD de {monster.MonsterName} foi criado mas não encontrei TextMeshPro ou Botões lá dentro.");
            }
        }
        else
        {
            Debug.Log("Player: A usar UI Padrão.");
        }

        // 4. Iniciar
        DialogManager.Instance.EnterDialogueMode(monster);
    }

    public void ReceiveMask(MaskData maskData)
    {
        if (playerData.AddMask(maskData)) InventoryManager.Instance.AddMaskVisual(maskData);
    }

    public void EquipMask(MaskData maskData)
    {
        if (currentMask != null && currentMask.MaskEffects != null)
            foreach (MaskEffect effect in currentMask.MaskEffects) effect.DeactivateEffect(this.gameObject);

        currentMask = maskData;

        if (currentMask != null && currentMask.MaskEffects != null)
            foreach (MaskEffect effect in currentMask.MaskEffects) effect.ActivateEffect(this.gameObject);
    }
}