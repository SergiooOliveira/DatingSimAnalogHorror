using UnityEngine;
using TMPro; // Necessário para TextMeshProUGUI
using System.Collections.Generic; // Necessário para Listas
using UnityEngine.UI; // Necessário para Button

public class Player : MonoBehaviour
{
    public static Player Instance;

    [SerializeField] private PlayerData playerData = new PlayerData();
    public PlayerData PlayerData => playerData;

    public bool IsDisguised { get; private set; } = false;

    private MaskData currentMask;

    // Referência ao Canvas onde vamos criar o HUD do monstro
    [SerializeField] private Transform mainCanvasTransform;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Tenta encontrar o Canvas automaticamente se não tiveres arrastado no Inspector
        if (mainCanvasTransform == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null) mainCanvasTransform = canvas.transform;
        }
    }

    public void SetDisguise(bool state)
    {
        IsDisguised = state;
        Debug.Log(state ? "Player is now disguised." : "Player is no longer disguised.");
    }

    public void InteractWithMonster(Monster monster)
    {
        // 1. Verifica se o monstro tem um HUD específico e se temos onde o pôr
        if (monster.HudPrefab != null && mainCanvasTransform != null)
        {
            // 2. Instancia o HUD visual do monstro no Canvas
            GameObject spawnedHUD = Instantiate(monster.HudPrefab, mainCanvasTransform);

            // 3. Procura os componentes automaticamente dentro do HUD novo
            // Assume-se que o primeiro TMP_Text é o do diálogo
            TextMeshProUGUI foundText = spawnedHUD.GetComponentInChildren<TextMeshProUGUI>();

            // Assume-se que os botões têm o componente Button do Unity UI
            Button[] foundButtonsComponents = spawnedHUD.GetComponentsInChildren<Button>(true);
            List<GameObject> foundButtonsGO = new List<GameObject>();

            foreach (var btn in foundButtonsComponents)
            {
                foundButtonsGO.Add(btn.gameObject);
            }

            if (foundText != null && foundButtonsGO.Count > 0)
            {
                // 4. Envia a nova UI para o DialogManager
                // O spawnedHUD é passado como 'dialoguePanel' e como 'rootObject' para ser destruído no fim
                DialogManager.Instance.SwapDialogueUI(spawnedHUD, foundText, foundButtonsGO.ToArray(), spawnedHUD);
            }
            else
            {
                Debug.LogWarning($"Player: Instanciei o HUD de {monster.MonsterName}, mas não encontrei Texto ou Botões suficientes lá dentro.");
            }
        }
        else
        {
            // Se não houver prefab, usa a UI padrão que já está no DialogManager
            Debug.Log("Player: A usar UI padrão (Monstro sem prefab ou Canvas não encontrado).");
        }

        // 5. Inicia o diálogo com a história do monstro
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