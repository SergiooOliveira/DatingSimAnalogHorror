using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    // Player Data, controls all the information about the player
    public string playerName = "Hero";
    public List<MaskData> playerMasks = new List<MaskData>();

    [Header("Diary Progression")]
    public bool hasDiaryCover = false;

    public bool AddMask(MaskData newMask)
    {
        if (!playerMasks.Contains(newMask))
        {
            playerMasks.Add(newMask);
            return true;
        }
        return false;
    }
}