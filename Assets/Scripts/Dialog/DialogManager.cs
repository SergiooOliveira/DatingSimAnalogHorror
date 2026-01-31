using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
        Swapper(false);
        Cursor.lockState = CursorLockMode.None;
        choicesText = new TextMeshProUGUI[choices.Length];
        int index = 0;
        foreach (GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }

    private void Update()
    {
        if (waitingForClick && Input.GetMouseButtonDown(0))
        {
            ContinueStory();
        }
    }
    #endregion

    #region Methods
    public void EnterDialogueMode (TextAsset inkJson)
    {
        
        Cursor.visible = true;

        currentStory = new Story(inkJson.text);
        Swapper(true);

        ContinueStory();
    }


    private void Swapper(bool state)
    {
        dialogueIsPlaying = state;
        dialoguePanel.SetActive(state);
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
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
            StartCoroutine(ExitDialogueMode());
        }
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        if (currentChoices.Count > choices.Length)
        {
            Debug.LogError("More choices were given than the UI can support. Number of choices given: " + currentChoices.Count);
        }

        int index = 0;

        foreach (Choice choice in currentChoices)
        {
            choices[index].SetActive(true);
            choicesText[index].text = choice.text;
            index++;
        }

        for (int i = index; i < choices.Length; i++)
        {
            choices[i].gameObject.SetActive(false);
        }
    }

    private void HideChoices()
    {
        foreach (GameObject choice in choices)
        {
            choice.SetActive(false);
        }
    }

    public void MakeChoice(int choiceIndex)
    {
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
        Swapper(false);
        dialogueText.text = "";
    }

    private IEnumerator WaitAndAutoAdvance()
    {
        yield return new WaitForSeconds(1f);
        ContinueStory();
    }
    #endregion
}
