using UnityEngine;

public class Monster : MonoBehaviour
{
    #region Variables
    // Serialized Fields
    [SerializeField] private string monsterName;
    [SerializeField] private TextAsset inkJson;

    [Header("Visuals")]
    [SerializeField] private GameObject hudPrefab;

    // Public Properties
    public string MonsterName => monsterName;
    public TextAsset InkJson => inkJson;
    public GameObject HudPrefab => hudPrefab;
    #endregion
}