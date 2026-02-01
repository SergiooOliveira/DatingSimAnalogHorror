using UnityEngine;

public abstract class MaskEffect : ScriptableObject
{
    [TextArea] public string description;

    public abstract void ActivateEffect(GameObject target);

    public abstract void DeactivateEffect(GameObject target);
}