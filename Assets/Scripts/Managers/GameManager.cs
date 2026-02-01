using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public LayerMask playerMask;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
}
