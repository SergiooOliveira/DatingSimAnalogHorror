using UnityEngine;

[CreateAssetMenu(menuName = "Mask/Effects/Disguise Effect")]
public class DisguiseEffect : MaskEffect
{
    public override void ActivateEffect(GameObject target)
    {
        Player.Instance.SetDisguise(true);
    }

    public override void DeactivateEffect(GameObject target)
    {
        Player.Instance.SetDisguise(false);
    }
}