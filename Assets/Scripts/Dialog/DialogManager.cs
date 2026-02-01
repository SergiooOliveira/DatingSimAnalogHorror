using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;

    // --- CONFIGURAÇÃO PADRÃO (Inspector) ---
    [Header("UI Padrão (Fallback)")]
    [SerializeField] private GameObject defaultDialoguePanel;
    [SerializeField] private TextMeshProUGUI defaultDialogueText;
    [SerializeField] private Image defaultPortraitImage;
    [SerializeField] private GameObject[] defaultChoices;

    [Header("Settings")]
    [SerializeField] private float bobSpeed = 2.0f;
    [SerializeField] private float bobDistance = 10.0f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private InputActionReference submitAction;

    // --- VARIÁVEIS ATIVAS ---
    private GameObject activePanel;
    private TextMeshProUGUI activeText;
    private Image activePortrait;
    private GameObject[] activeChoicesObj;
    private TextMeshProUGUI[] activeChoicesText;

    private Story currentStory;
    private bool waitingForClick = false;
    private bool inputCooldown = false;

    private Vector3 portraitOriginalPos;
    private Coroutine musicCoroutine;
    private GameObject currentCustomUI;

    public bool dialogueIsPlaying { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (defaultChoices == null) defaultChoices = new GameObject[0];
        dialogueIsPlaying = false;

        if (defaultDialoguePanel != null)
            defaultDialoguePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        ResetToDefaultUI();
    }

    private void ResetToDefaultUI()
    {
        activePanel = defaultDialoguePanel;
        activeText = defaultDialogueText;
        activePortrait = defaultPortraitImage;
        activeChoicesObj = defaultChoices;
        currentCustomUI = null;

        UpdatePortraitPos();
        InitializeChoicesText();
        SetupButtonClicks();
    }

    // CHAMADO PELO PLAYER
    public void SwapDialogueUI(GameObject panel, TextMeshProUGUI text, Image portrait, GameObject[] choiceBtns, GameObject rootObject)
    {
        activePanel = panel;
        activeText = text;
        activePortrait = portrait;
        activeChoicesObj = choiceBtns;
        currentCustomUI = rootObject;

        UpdatePortraitPos();
        InitializeChoicesText();
        SetupButtonClicks();
    }

    private void UpdatePortraitPos()
    {
        if (activePortrait != null)
            portraitOriginalPos = activePortrait.rectTransform.anchoredPosition;
    }

    private void InitializeChoicesText()
    {
        if (activeChoicesObj == null) return;
        activeChoicesText = new TextMeshProUGUI[activeChoicesObj.Length];
        for (int i = 0; i < activeChoicesObj.Length; i++)
        {
            if (activeChoicesObj[i] != null)
                activeChoicesText[i] = activeChoicesObj[i].GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    private void SetupButtonClicks()
    {
        if (activeChoicesObj == null) return;
        for (int i = 0; i < activeChoicesObj.Length; i++)
        {
            if (activeChoicesObj[i] != null)
            {
                Button btn = activeChoicesObj[i].GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    int index = i;
                    btn.onClick.AddListener(() => MakeChoice(index));
                }
            }
        }
    }

    private void Update()
    {
        if (!dialogueIsPlaying) return;

        // --- ANIMAÇÃO (BOBBING) ---
        if (activePortrait != null)
        {
            float newY = portraitOriginalPos.y + Mathf.Sin(Time.time * bobSpeed) * bobDistance;
            activePortrait.rectTransform.anchoredPosition = new Vector2(portraitOriginalPos.x, newY);
        }

        if (inputCooldown) return;

        bool submitPressed = false;
        if (submitAction != null && submitAction.action != null && submitAction.action.WasPressedThisFrame())
            submitPressed = true;
        if (Input.GetMouseButtonDown(0))
            submitPressed = true;

        if (waitingForClick && submitPressed)
        {
            ContinueStory();
        }
    }

    public void EnterDialogueMode(Monster monster)
    {
        if (monster == null || monster.InkJson == null)
        {
            Debug.LogError("DialogManager: InkJSON ou Monstro nulo!");
            return;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        currentStory = new Story(monster.InkJson.text);
        dialogueIsPlaying = true;

        if (activePanel != null) activePanel.SetActive(true);

        // 1. MÚSICA
        if (musicCoroutine != null) StopCoroutine(musicCoroutine);
        musicCoroutine = StartCoroutine(PlayMusicSequence(monster.MusicIntro, monster.MusicLoop));

        // 2. INICIAR
        if (!currentStory.canContinue && currentStory.currentChoices.Count == 0)
        {
            StartCoroutine(ExitDialogueMode(monster.MusicOutro));
            return;
        }

        StartCoroutine(InputCooldownRoutine());
        ContinueStory(monster);
    }

    private IEnumerator PlayMusicSequence(AudioClip intro, AudioClip loop)
    {
        if (audioSource == null) yield break;
        audioSource.Stop();

        if (intro != null)
        {
            audioSource.clip = intro;
            audioSource.loop = false;
            audioSource.Play();
            yield return new WaitForSeconds(intro.length);
        }

        if (loop != null)
        {
            audioSource.clip = loop;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private IEnumerator InputCooldownRoutine()
    {
        inputCooldown = true;
        yield return new WaitForSeconds(0.2f);
        inputCooldown = false;
    }

    private void ContinueStory(Monster monster = null)
    {
        if (currentStory.canContinue)
        {
            if (activeText != null) activeText.text = currentStory.Continue();
            HideChoices();

            if (currentStory.currentTags.Contains("auto"))
            {
                waitingForClick = false;
                StartCoroutine(WaitAndAutoAdvance(monster));
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
            if (currentStory.currentChoices.Count > 0)
            {
                waitingForClick = false;
                DisplayChoices();
            }
            else
            {
                AudioClip outClip = monster != null ? monster.MusicOutro : null;
                StartCoroutine(ExitDialogueMode(outClip));
            }
        }
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;
        int index = 0;
        foreach (Choice choice in currentChoices)
        {
            if (index < activeChoicesObj.Length && activeChoicesObj[index] != null)
            {
                activeChoicesObj[index].SetActive(true);
                if (activeChoicesText[index] != null) activeChoicesText[index].text = choice.text;
            }
            index++;
        }
        for (int i = index; i < activeChoicesObj.Length; i++)
        {
            if (activeChoicesObj[i] != null) activeChoicesObj[i].SetActive(false);
        }
    }

    private void HideChoices()
    {
        if (activeChoicesObj == null) return;
        foreach (GameObject choice in activeChoicesObj)
        {
            if (choice != null) choice.SetActive(false);
        }
    }

    public void MakeChoice(int choiceIndex)
    {
        if (inputCooldown) return;
        currentStory.ChooseChoiceIndex(choiceIndex);
        ContinueStory();
    }

    private IEnumerator ExitDialogueMode(AudioClip outroClip)
    {
        if (musicCoroutine != null) StopCoroutine(musicCoroutine);

        if (audioSource != null)
        {
            audioSource.Stop();
            if (outroClip != null)
            {
                audioSource.loop = false;
                audioSource.PlayOneShot(outroClip);
            }
        }

        yield return new WaitForSeconds(0.2f);

        dialogueIsPlaying = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (activePanel != null) activePanel.SetActive(false);
        if (activeText != null) activeText.text = "";
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);

        // Limpeza
        if (currentCustomUI != null) Destroy(currentCustomUI);
        ResetToDefaultUI();
        if (activePanel != null) activePanel.SetActive(false);

        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null)
        {
            pc.enabled = true;
            pc.SyncCameraRotation();
        }
    }

    private IEnumerator WaitAndAutoAdvance(Monster monster)
    {
        yield return new WaitForSeconds(1f);
        ContinueStory(monster);
    }

    private void OnEnable() { if (submitAction != null) submitAction.action.Enable(); }
    private void OnDisable() { if (submitAction != null) submitAction.action.Disable(); }
}