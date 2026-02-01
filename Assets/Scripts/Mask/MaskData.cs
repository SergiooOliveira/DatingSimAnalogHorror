using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Mask", menuName = "Mask/Mask Data")]
public class MaskData : ScriptableObject, IMask
{
    #region Serialized Fields
    [Header("Identity")]
    [SerializeField] private int maskID;
    [SerializeField] private string maskName;
    [SerializeField] private GameObject maskPrefab;
    [SerializeField] private Sprite maskIcon;
    [SerializeField] private MaskCorruption maskCorruptionLevel;
    [SerializeField] private List<MaskEffect> maskEffects;
    #endregion

    #region Properties
    public int MaskID => maskID;
    public string MaskName => maskName;
    public GameObject MaskPrefab => maskPrefab;
    public Sprite MaskIcon => maskIcon;
    public MaskCorruption MaskCorruptionLevel => maskCorruptionLevel;
    public List<MaskEffect> MaskEffects => maskEffects;

    // Retorna a taxa de corrupção por segundo baseada no nível.
    // Níveis positivos aumentam a corrupção, níveis negativos (como Humana) recuperam.
    public float CorruptionRate => MaskCorruptionLevel switch
    {
        MaskCorruption.Humana => -1.0f, // Recupera corrupção
        MaskCorruption.Baixa => 1.0f,  // 100 segundos para corrupção total
        MaskCorruption.Media => 3.0f,  // ~33 segundos
        MaskCorruption.Alta => 7.0f,   // ~14 segundos
        _ => 0f
    };
    #endregion

    public void Initialize() { }
    public void Corruption() { /* Lógica extra se necessário */ }
}