using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Choices UI")]
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;

    [SerializeField] private InputActionReference submitAction;

    private Story currentStory;

    // Flags
    private bool isWaitingForExit = false;
    private bool waitingForClick = false;
    private bool inputCooldown = false;

    // Variável para guardar a UI instanciada (HUD do Monstro) e destruí-la no final
    private GameObject currentCustomUI;

    public bool dialogueIsPlaying { get; private set; }

    #region Monobehaviour Methods
    private void OnEnable()
    {
        if (submitAction != null) submitAction.action.Enable();
    }

    private void OnDisable()
    {
        if (submitAction != null) submitAction.action.Disable();
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        dialogueIsPlaying = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;

        InitializeChoicesText();
    }

    // Função auxiliar para encontrar os componentes de texto nos botões atuais
    private void InitializeChoicesText()
    {
        if (choices == null) return;

        choicesText = new TextMeshProUGUI[choices.Length];
        int index = 0;
        foreach (GameObject choice in choices)
        {
            if (choice != null)
                choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }

    private void Update()
    {
        if (!dialogueIsPlaying || inputCooldown) return;

        // Suporte para Rato OU Tecla de Ação (ex: Espaço/Enter)
        bool submitPressed = false;
        if (submitAction != null && submitAction.action != null)
        {
            if (submitAction.action.WasPressedThisFrame()) submitPressed = true;
        }
        if (Input.GetMouseButtonDown(0)) submitPressed = true;

        if (waitingForClick && submitPressed)
        {
            ContinueStory();
        }
    }
    #endregion

    #region Methods

    // --- NOVO MÉTODO: Permite ao Player trocar a UI dinamicamente ---
    public void SwapDialogueUI(GameObject panel, TextMeshProUGUI text, GameObject[] choiceBtns, GameObject rootObject)
    {
        // 1. Guarda as novas referências visuais
        dialoguePanel = panel;
        dialogueText = text;
        choices = choiceBtns;

        // 2. Guarda a raiz do objeto para o destruir no final
        currentCustomUI = rootObject;

        // 3. Reinicializa os textos dos botões para a nova UI
        InitializeChoicesText();
    }
    // ---------------------------------------------------------------

    public void EnterDialogueMode(TextAsset inkJson)
    {
        if (inkJson == null)
        {
            Debug.LogError("DialogManager: InkJSON é nulo!");
            return;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        currentStory = new Story(inkJson.text);
        dialogueIsPlaying = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        // Verifica se a história tem conteúdo antes de começar
        if (!currentStory.canContinue && currentStory.currentChoices.Count == 0)
        {
            Debug.LogError("DialogManager: A história está vazia ou acabou imediatamente!");
            StartCoroutine(ExitDialogueMode());
            return;
        }

        StartCoroutine(InputCooldownRoutine());

        ContinueStory();
    }

    private IEnumerator InputCooldownRoutine()
    {
        inputCooldown = true;
        yield return new WaitForSeconds(0.2f);
        inputCooldown = false;
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            if (dialogueText != null)
                dialogueText.text = currentStory.Continue();

            HideChoices();

            bool isAutoAdvance = currentStory.currentTags.Contains("auto");

            if (isAutoAdvance)
            {
                waitingForClick = false;
                StartCoroutine(WaitAndAutoAdvance());
            }
            else if (currentStory.currentChoices.Count > 0)
            {
                waitingForClick = false;
                DisplayChoices();
            }
            else
            {
                waitingForClick = true;
            }
        }
        else
        {
            // CORREÇÃO: Verifica se há escolhas pendentes antes de sair!
            if (currentStory.currentChoices.Count > 0)
            {
                waitingForClick = false;
                DisplayChoices();
            }
            else
            {
                StartCoroutine(ExitDialogueMode());
            }
        }
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        if (currentChoices.Count > choices.Length)
        {
            Debug.LogError("More choices were given than the UI can support.");
        }

        int index = 0;
        foreach (Choice choice in currentChoices)
        {
            // Verifica se o array tem tamanho suficiente e se o objeto existe
            if (index < choices.Length && choices[index] != null)
            {
                choices[index].SetActive(true);
                if (choicesText[index] != null)
                    choicesText[index].text = choice.text;
            }
            index++;
        }

        for (int i = index; i < choices.Length; i++)
        {
            if (choices[i] != null)
                choices[i].gameObject.SetActive(false);
        }
    }

    private void HideChoices()
    {
        if (choices == null) return;

        foreach (GameObject choice in choices)
        {
            if (choice != null)
                choice.SetActive(false);
        }
    }

    public void MakeChoice(int choiceIndex)
    {
        if (inputCooldown) return;

        currentStory.ChooseChoiceIndex(choiceIndex);
        ContinueStory();
    }
    #endregion

    #region Enumerators
    private IEnumerator ExitDialogueMode()
    {
        isWaitingForExit = false;
        yield return new WaitForSeconds(0.2f);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        dialogueIsPlaying = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (dialogueText != null)
            dialogueText.text = "";

        // --- LIMPEZA DO HUD CUSTOMIZADO ---
        // Se instanciámos uma UI específica para o monstro, destruímo-la aqui
        if (currentCustomUI != null)
        {
            Destroy(currentCustomUI);
            currentCustomUI = null;
            // Opcional: Limpar referências para evitar erros se tentarmos aceder depois
            choices = new GameObject[0];
            dialogueText = null;
            dialoguePanel = null;
        }

        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null)
        {
            // CORREÇÃO DE ROTAÇÃO:
            // Alinha o Corpo do Jogador com a direção atual da Câmara (Monstro)
            if (pc.cameraJogador != null)
            {
                Vector3 currentCamEuler = pc.cameraJogador.eulerAngles;

                // 1. Roda o Corpo (Y) para coincidir com a Câmara Global
                pc.transform.rotation = Quaternion.Euler(0, currentCamEuler.y, 0);

                // 2. Define a Câmara Local (X) para manter a inclinação (Pitch) mas zera o Y local
                pc.cameraJogador.localRotation = Quaternion.Euler(currentCamEuler.x, 0, 0);
            }

            pc.enabled = true;
            pc.SyncCameraRotation(); // Sincroniza a variável interna para não haver salto
        }
    }

    private IEnumerator WaitAndAutoAdvance()
    {
        yield return new WaitForSeconds(1f);
        ContinueStory();
    }
    #endregion
}