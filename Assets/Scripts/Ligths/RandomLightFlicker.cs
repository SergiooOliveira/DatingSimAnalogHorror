using UnityEngine;
using System.Collections;

public class RandomLightFlicker : MonoBehaviour
{
    public Light spotLight;

    [Header("Tempo entre falhas")]
    public float minTimeBetweenFlickers = 5f;
    public float maxTimeBetweenFlickers = 15f;

    [Header("Piscar")]
    public float flickerDuration = 0.3f;
    public float flickerSpeed = 0.05f;

    void Start()
    {
        if (spotLight == null)
            spotLight = GetComponent<Light>();

        StartCoroutine(FlickerRoutine());
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // Espera um tempo aleatório antes de piscar
            float waitTime = Random.Range(minTimeBetweenFlickers, maxTimeBetweenFlickers);
            yield return new WaitForSeconds(waitTime);

            // Pisca por um curto período
            float timer = 0f;
            while (timer < flickerDuration)
            {
                spotLight.enabled = !spotLight.enabled;
                timer += flickerSpeed;
                yield return new WaitForSeconds(flickerSpeed);
            }

            // Garante que a luz fique ligada no final
            spotLight.enabled = true;
        }
    }
}
