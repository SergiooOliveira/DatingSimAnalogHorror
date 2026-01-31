using UnityEngine;

public class Player : MonoBehaviour
{
    public void InteractWithMonster(Monster monster)
    {
        DialogManager.Instance.EnterDialogueMode(monster.InkJson);
    }
}
