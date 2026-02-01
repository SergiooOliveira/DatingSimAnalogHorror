using UnityEngine;
using System.Collections.Generic;

public enum MaskCorruption { Humana, Baixa, Media, Alta }

public interface IMask
{
    #region Properties
    // *----- Identity -----*
    string MaskName { get; }
    GameObject MaskPrefab { get; }
    MaskCorruption MaskCorruptionLevel { get; }
    List<MaskEffect> MaskEffects { get; }
    #endregion

    void Corruption();
}