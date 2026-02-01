using UnityEngine;

public class Monster : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string monsterName;
    [SerializeField] private TextAsset inkJson;

    [Header("Visuals - UI")]
    [Tooltip("O Prefab do HUD específico. Certifica-te que a imagem lá dentro se chama 'Portrait'.")]
    [SerializeField] private GameObject hudPrefab;

    [Header("Audio")]
    [SerializeField] private AudioClip musicIntro;
    [SerializeField] private AudioClip musicLoop;
    [SerializeField] private AudioClip musicOutro;

    public string MonsterName => monsterName;
    public TextAsset InkJson => inkJson;
    public GameObject HudPrefab => hudPrefab;
    public AudioClip MusicIntro => musicIntro;
    public AudioClip MusicLoop => musicLoop;
    public AudioClip MusicOutro => musicOutro;
}