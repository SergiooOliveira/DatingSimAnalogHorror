using UnityEngine;
using System.Collections;

public class TriggerLightIntensity : MonoBehaviour
{
    public Light spotLight;

    [Header("Intensidade")]
    public float normalIntensity = 1.2f;
    public float intenseIntensity = 2.5f;

    [Header("Transição")]
    public float transitionSpeed = 2f;

    Coroutine currentRoutine;

    void Start()
    {
        if (spotLight == null)
            spotLight = GetComponent<Light>();

        spotLight.intensity = normalIntensity;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartTransition(intenseIntensity);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartTransition(normalIntensity);
        }
    }

    void StartTransition(float targetIntensity)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ChangeIntensity(targetIntensity));
    }

    IEnumerator ChangeIntensity(float target)
    {
        while (!Mathf.Approximately(spotLight.intensity, target))
        {
            spotLight.intensity = Mathf.Lerp(
                spotLight.intensity,
                target,
                Time.deltaTime * transitionSpeed
            );

            yield return null;
        }
    }
}
