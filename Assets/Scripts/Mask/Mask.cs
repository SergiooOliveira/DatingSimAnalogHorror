using UnityEngine;

public class Mask : MonoBehaviour
{
    [SerializeField] private MaskData maskData;

    public MaskData MaskData => maskData;
    public void Initialize()
    {
        maskData.Initialize();
    }
}