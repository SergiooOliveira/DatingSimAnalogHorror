using UnityEngine;

public class Monster : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string monsterName;
    [SerializeField] private TextAsset inkJson;

    [Header("Visuals - UI")]
    [Tooltip("A imagem (Portrait) que aparecerá no painel de diálogo.")]
    [SerializeField] private Sprite monsterPortrait;

    [Header("Audio")]
    [SerializeField] private AudioClip musicIntro;
    [SerializeField] private AudioClip musicLoop;
    [SerializeField] private AudioClip musicOutro;

    // Getters
    public string MonsterName => monsterName;
    public TextAsset InkJson => inkJson;
    public Sprite MonsterPortrait => monsterPortrait;
    public AudioClip MusicIntro => musicIntro;
    public AudioClip MusicLoop => musicLoop;
    public AudioClip MusicOutro => musicOutro;
}