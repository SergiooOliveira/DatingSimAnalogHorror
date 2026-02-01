using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;

    [Header("UI References")]
    [Tooltip("O GameObject vazio que contém o Nome, Imagem e Painel.")]
    [SerializeField] private GameObject dialogueContainer;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private GameObject[] choiceButtons;

    [Header("Settings")]
    [SerializeField] private float bobSpeed = 2.0f;
    [SerializeField] private float bobDistance = 10.0f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private InputActionReference submitAction;

    private TextMeshProUGUI[] choiceTexts;
    private Story currentStory;
    private bool waitingForClick = false;
    private bool inputCooldown = false;
    private Vector3 portraitOriginalPos;
    private Coroutine musicCoroutine;

    public bool dialogueIsPlaying { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        dialogueIsPlaying = false;

        // Garante que tudo começa desativado
        if (dialogueContainer != null) dialogueContainer.SetActive(false);
        else if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // Inicializa referências de texto dos botões
        InitializeChoicesText();

        // Guarda a posição original para a animação de levitação
        if (portraitImage != null)
            portraitOriginalPos = portraitImage.rectTransform.anchoredPosition;
    }

    private void InitializeChoicesText()
    {
        choiceTexts = new TextMeshProUGUI[choiceButtons.Length];
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] != null)
            {
                choiceTexts[i] = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();

                // Setup dos cliques uma única vez
                int index = i;
                Button btn = choiceButtons[i].GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => { Debug.Log("Cliquei no botão: " + choiceButtons[index].name); MakeChoice(index); });
                }
            }
        }
    }

    private void Update()
    {
        if (!dialogueIsPlaying) return;

        // Animação de "levitação" (Bobbing) da imagem do monstro
        if (portraitImage != null && portraitImage.sprite != null)
        {
            float newY = portraitOriginalPos.y + Mathf.Sin(Time.time * bobSpeed) * bobDistance;
            portraitImage.rectTransform.anchoredPosition = new Vector2(portraitOriginalPos.x, newY);
        }

        if (inputCooldown) return;

        bool submitPressed = (submitAction != null && submitAction.action.WasPressedThisFrame()) || Input.GetMouseButtonDown(0);

        if (waitingForClick && submitPressed)
        {
            ContinueStory();
        }
    }

    public void EnterDialogueMode(Monster monster)
    {
        if (monster == null || monster.InkJson == null)
        {
            Debug.LogWarning("DialogManager: Monstro ou InkJson não encontrados!");
            return;
        }

        dialogueIsPlaying = true;

        // --- 1. CONFIGURAR VISUAIS ---
        // Atribui o nome do monstro
        if (nameText != null) nameText.text = monster.MonsterName;

        // Atribui o sprite do monstro à imagem na cena
        if (portraitImage != null)
        {
            portraitImage.sprite = monster.MonsterPortrait;
            // Garante que a imagem está visível (Alpha = 1)
            var color = portraitImage.color;
            color.a = monster.MonsterPortrait != null ? 1f : 0f;
            portraitImage.color = color;
        }

        // Ativa o contentor principal que engloba tudo
        if (dialogueContainer != null) dialogueContainer.SetActive(true);
        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        // --- 2. CONFIGURAR INK ---
        currentStory = new Story(monster.InkJson.text);

        // --- 3. CONFIGURAR MÚSICA ---
        if (musicCoroutine != null) StopCoroutine(musicCoroutine);
        musicCoroutine = StartCoroutine(PlayMusicSequence(monster.MusicIntro, monster.MusicLoop));

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        StartCoroutine(InputCooldownRoutine());
        ContinueStory(monster);
    }

    private void ContinueStory(Monster monster = null)
    {
        if (currentStory.canContinue)
        {
            dialogueText.text = currentStory.Continue();
            DisplayChoices();

            if (currentStory.currentTags.Contains("auto"))
            {
                waitingForClick = false;
                StartCoroutine(WaitAndAutoAdvance(monster));
            }
            else
            {
                // Se houver escolhas, espera pela interação nos botões, senão espera clique geral
                waitingForClick = currentStory.currentChoices.Count == 0;
            }
        }
        else if (currentStory.currentChoices.Count > 0)
        {
            DisplayChoices();
            waitingForClick = false;
        }
        else
        {
            StartCoroutine(ExitDialogueMode(monster != null ? monster.MusicOutro : null));
        }
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < currentChoices.Count)
            {
                choiceButtons[i].SetActive(true);
                choiceTexts[i].text = currentChoices[i].text;
            }
            else
            {
                choiceButtons[i].SetActive(false);
            }
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
            if (outroClip != null) audioSource.PlayOneShot(outroClip);
        }

        yield return new WaitForSeconds(0.2f);

        dialogueIsPlaying = false;

        // Desativa o contentor principal
        if (dialogueContainer != null) dialogueContainer.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null) { pc.enabled = true; pc.SyncCameraRotation(); }
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

    private IEnumerator WaitAndAutoAdvance(Monster monster)
    {
        yield return new WaitForSeconds(1.5f);
        ContinueStory(monster);
    }
}