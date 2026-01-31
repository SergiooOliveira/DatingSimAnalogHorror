using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using UnityEngine.SceneManagement;
using System.Collections;

public class GlitchMenu : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI startButtonText;
    public Image backgroundPanel;

    [Header("Definições do Terror")]
    [Tooltip("Frase original fofinha do título")]
    public string normalTitle = "Love Date: Forever <3";
    [Tooltip("Frase original do botão")]
    public string normalButton = "Start Date";

    [Tooltip("Frases assustadoras que aparecem aleatoriamente")]
    public string[] creepyPhrases = {
        "RUN AWAY",
        "HE SEES YOU",
        "DON'T CLICK",
        "BEHIND YOU",
        "NO ESCAPE",
        "HELP ME"
    };

    [Header("Configuração de Glitch")]
    public float minGlitchTime = 2.0f;
    public float maxGlitchTime = 5.0f;
    public Color normalColor = Color.white;
    public Color glitchColor = Color.red;

    [Header("Audio (Opcional)")]
    public AudioSource musicSource;
    public AudioClip glitchSound;

    private void Start()
    {
        titleText.text = normalTitle;
        startButtonText.text = normalButton;
        titleText.color = normalColor;

        StartCoroutine(GlitchRoutine());
    }

    IEnumerator GlitchRoutine()
    {
        while (true)
        {
            // Espera um tempo aleatório antes do próximo glitch
            float waitTime = Random.Range(minGlitchTime, maxGlitchTime);
            yield return new WaitForSeconds(waitTime);

            TriggerGlitch();

            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));

            ResetGlitch();
        }
    }

    void TriggerGlitch()
    {
        string scaryText = creepyPhrases[Random.Range(0, creepyPhrases.Length)];

        if (Random.value > 0.5f)
        {
            titleText.text = scaryText;
            titleText.color = glitchColor;
        }
        else
        {
            startButtonText.text = scaryText;
            startButtonText.color = glitchColor;
        }

        if (musicSource != null && glitchSound != null)
        {
            musicSource.PlayOneShot(glitchSound);
        }

        if (backgroundPanel != null)
        {
            backgroundPanel.color = Color.black;
        }
    }

    void ResetGlitch()
    {
        titleText.text = normalTitle;
        startButtonText.text = normalButton;

        titleText.color = normalColor;
        startButtonText.color = normalColor;

        if (backgroundPanel != null)
        {
            backgroundPanel.color = Color.white;
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
}