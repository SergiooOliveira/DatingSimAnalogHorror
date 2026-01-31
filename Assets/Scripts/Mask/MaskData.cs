using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Mask", menuName = "Masks/Mask Data")]
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
    #endregion

    #region Runtime Fields

    #endregion

    #region Methods
    public void Initialize()
    {

    }
    public void Corruption()
    {
        
    }
    #endregion
}