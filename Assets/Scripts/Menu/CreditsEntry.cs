using UnityEngine;
using TMPro;
using UnityEngine.EventSystems; // Necessário para detetar o Rato (Hover)
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CreditsEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Staff Information")]
    [Tooltip("The name that is always visible")]
    public string staffName;

    [Tooltip("The role/text that gets 'written' when hovering")]
    [TextArea]
    public string hiddenRole;

    [Header("Writing Settings")]
    public float writingSpeed = 0.05f; 
    public string separator = " - "; 

    [Header("Visual Effects")]
    public Color nameColor = Color.black;
    public Color roleColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    public bool useHandwritingWiggle = true;

    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip scribblingSound;

    private TextMeshProUGUI _textComponent;
    private Coroutine _writingCoroutine;
    private string _currentDisplayedText;

    void Awake()
    {
        _textComponent = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        ResetText();
    }

    void Update()
    {
        if (useHandwritingWiggle)
        {
            float wiggle = Mathf.Sin(Time.time * 2f + transform.GetSiblingIndex()) * 0.5f;
            transform.localRotation = Quaternion.Euler(0, 0, wiggle);
        }
    }

    // --- MOUSE ENTER (HOVER START) ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_writingCoroutine != null) StopCoroutine(_writingCoroutine);
        _writingCoroutine = StartCoroutine(WriteRoleRoutine());
    }

    // --- MOUSE EXIT (HOVER END) ---
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_writingCoroutine != null) StopCoroutine(_writingCoroutine);
        ResetText();
    }

    // --- TYPEWRITER LOGIC ---
    IEnumerator WriteRoleRoutine()
    {
        string baseText = $"<color=#{ColorUtility.ToHtmlStringRGBA(nameColor)}>{staffName}</color>";
        string roleColorHex = ColorUtility.ToHtmlStringRGBA(roleColor);

        _textComponent.text = baseText + $"<color=#{roleColorHex}>{separator}</color>";

        if (audioSource != null && scribblingSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.Play(); 
        }

        string currentRoleText = "";
        foreach (char letter in hiddenRole)
        {
            currentRoleText += letter;

            _textComponent.text = baseText +
                                  $"<color=#{roleColorHex}>{separator}{currentRoleText}</color>";

            yield return new WaitForSeconds(writingSpeed + Random.Range(-0.01f, 0.01f));
        }

        if (audioSource != null) audioSource.Stop();
    }

    void ResetText()
    {
        _textComponent.text = $"<color=#{ColorUtility.ToHtmlStringRGBA(nameColor)}>{staffName}</color>";

        if (audioSource != null) audioSource.Stop();
    }
}