using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GlitchMenu : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject startButtonObject;
    public GameObject creditsButtonObject;
    public GameObject creditsPanel;
    public TextMeshProUGUI titleText;
    public Image backgroundPanel;
    public RectTransform spawnArea;

    [Header("Corruption Mechanic")]
    public GameObject wordPrefab;
    public int wordsToClear = 5;
    public float spawnInterval = 1.5f;
    public float mouseInteractionRadius = 50f;

    [Header("Visual Settings")]
    [Tooltip("The main background color set in the inspector")]
    public Color backgroundColor = Color.black;
    public Color glitchColor = Color.red;

    [Header("Animation Settings")]
    [Tooltip("Duration of the expand animation for Title and Start Button")]
    public float expandDuration = 0.5f;
    [Tooltip("Dramatic pause (and unlock sound duration) after clearing words")]
    public float silencePauseDuration = 1.0f;

    [Header("Word Glitch Effect")]
    [Tooltip("How much the words shake (Position Offset)")]
    public float shakeIntensity = 3.0f;
    [Tooltip("How fast the words shake (Time in seconds)")]
    public float shakeSpeed = 0.05f;

    [Header("Mask Whispers")]
    [Tooltip("Phrases related to the asylum and mask possession (English Only)")]
    public string[] obsessionPhrases = {
        "WEAR THE MASK", "WE ARE NOT CRAZY", "THE FACE HURTS",
        "THEY LIE", "JOIN US", "CORRUPTION",
        "SMILE FOREVER", "ACCEPT THE VOID",
        "REMOVE THE FACE", "NO ESCAPE"
    };

    [Header("Audio Settings")]
    public AudioSource musicSource;
    public AudioClip glitchSound;
    public AudioClip whisperSound;
    public AudioClip unlockSound;
    [Tooltip("Music to play after the menu is unlocked (Loops)")]
    public AudioClip menuThemeLoop;

    private List<TextMeshProUGUI> activeWords = new List<TextMeshProUGUI>();
    private bool gameUnlocked = false;
    private int wordsClearedCount = 0;

    private void Start()
    {
        if (backgroundPanel != null)
            backgroundPanel.color = backgroundColor;

        if (startButtonObject != null) startButtonObject.SetActive(false);
        if (creditsButtonObject != null) creditsButtonObject.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        if (titleText != null)
        {
            titleText.transform.localScale = Vector3.zero;
        }

        StartCoroutine(SpawnWordsRoutine());
    }

    private void Update()
    {
        if (gameUnlocked) return;

        CheckMouseInteraction();
    }

    IEnumerator SpawnWordsRoutine()
    {
        while (!gameUnlocked)
        {
            SpawnFloatingWord();
            yield return new WaitForSeconds(spawnInterval);

            if (spawnInterval > 0.5f) spawnInterval -= 0.1f;
        }
    }

    void SpawnFloatingWord()
    {
        if (wordPrefab == null || spawnArea == null) return;
        if (obsessionPhrases.Length == 0) return;

        GameObject newWordObj = Instantiate(wordPrefab, spawnArea);
        TextMeshProUGUI textComp = newWordObj.GetComponent<TextMeshProUGUI>();
        RectTransform rectComp = newWordObj.GetComponent<RectTransform>();

        float width = spawnArea.rect.width;
        float height = spawnArea.rect.height;
        Vector2 randomPos = new Vector2(
            Random.Range(-width / 2, width / 2),
            Random.Range(-height / 2, height / 2)
        );
        rectComp.anchoredPosition = randomPos;

        if (textComp != null)
        {
            textComp.text = obsessionPhrases[Random.Range(0, obsessionPhrases.Length)];
            textComp.color = new Color(1, 1, 1, 0);

            StartCoroutine(FadeInWord(textComp));
            StartCoroutine(WordGlitchEffect(rectComp));

            activeWords.Add(textComp);
        }

        if (musicSource && whisperSound) musicSource.PlayOneShot(whisperSound, 0.5f);
    }

    IEnumerator FadeInWord(TextMeshProUGUI word)
    {
        float alpha = 0;
        while (word != null && alpha < 1)
        {
            alpha += Time.deltaTime;
            word.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
    }

    IEnumerator WordGlitchEffect(RectTransform rect)
    {
        if (rect == null) yield break;

        Vector2 originalPos = rect.anchoredPosition;

        while (rect != null)
        {
            float x = Random.Range(-shakeIntensity, shakeIntensity);
            float y = Random.Range(-shakeIntensity, shakeIntensity);

            rect.anchoredPosition = originalPos + new Vector2(x, y);

            yield return new WaitForSeconds(shakeSpeed);
        }
    }

    void CheckMouseInteraction()
    {
        for (int i = activeWords.Count - 1; i >= 0; i--)
        {
            if (gameUnlocked) return;

            if (activeWords[i] == null)
            {
                activeWords.RemoveAt(i);
                continue;
            }

            Vector3 wordScreenPos = activeWords[i].transform.position;
            float dist = Vector3.Distance(Input.mousePosition, wordScreenPos);

            if (dist < mouseInteractionRadius)
            {
                DestroyWord(activeWords[i]);

                if (!gameUnlocked)
                {
                    activeWords.RemoveAt(i);
                }
                else
                {
                    return;
                }
            }
        }
    }

    void DestroyWord(TextMeshProUGUI word)
    {
        wordsClearedCount++;

        if (musicSource && glitchSound) musicSource.PlayOneShot(glitchSound);

        if (word != null) Destroy(word.gameObject);

        if (wordsClearedCount >= wordsToClear && !gameUnlocked)
        {
            StartCoroutine(UnlockSequence());
        }
    }

    IEnumerator UnlockSequence()
    {
        gameUnlocked = true;

        foreach (var w in activeWords)
        {
            if (w != null) Destroy(w.gameObject);
        }
        activeWords.Clear();

        if (musicSource != null) musicSource.Stop();

        if (musicSource != null && unlockSound != null)
        {
            musicSource.clip = unlockSound;
            musicSource.loop = false;
            musicSource.Play();
        }

        yield return new WaitForSeconds(silencePauseDuration);

        if (musicSource != null) musicSource.Stop();

        UnlockGame();
    }

    void UnlockGame()
    {
        if (titleText != null)
        {
            StartCoroutine(AnimateExpand(titleText.transform));
        }

        if (startButtonObject != null)
        {
            startButtonObject.SetActive(true);
            StartCoroutine(AnimateExpand(startButtonObject.transform));
        }

        if (creditsButtonObject != null)
        {
            creditsButtonObject.SetActive(true);
            StartCoroutine(AnimateExpand(creditsButtonObject.transform));
        }

        if (musicSource != null && menuThemeLoop != null)
        {
            musicSource.clip = menuThemeLoop;
            musicSource.loop = true;
            musicSource.Play();
        }

        StartCoroutine(FlashBackground());
    }

    IEnumerator AnimateExpand(Transform target)
    {
        float timer = 0f;
        Vector3 initialScale = Vector3.zero;
        Vector3 finalScale = Vector3.one;

        target.localScale = initialScale;

        while (timer < expandDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / expandDuration;
            float curve = Mathf.Sin(progress * Mathf.PI * 0.5f);

            target.localScale = Vector3.Lerp(initialScale, finalScale, curve);
            yield return null;
        }

        target.localScale = finalScale;
    }

    IEnumerator FlashBackground()
    {
        if (backgroundPanel != null)
        {
            backgroundPanel.color = Color.black;
            yield return new WaitForSeconds(0.1f);

            backgroundPanel.color = Color.white;
            yield return new WaitForSeconds(0.1f);

            backgroundPanel.color = backgroundColor;
        }
    }

    public void StartGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OpenCredits()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
            creditsPanel.transform.SetAsLastSibling();
        }
    }

    public void CloseCredits()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
    }
}