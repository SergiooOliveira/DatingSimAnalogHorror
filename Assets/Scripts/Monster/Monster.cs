using UnityEngine;

public class Monster : MonoBehaviour
{
    #region Variables
    // Serialized Fields
    [SerializeField] private string monsterName;
    [SerializeField] private TextAsset inkJson;

    // Public Properties
    public string MonsterName => monsterName;
    public TextAsset InkJson => inkJson;
    #endregion
}
