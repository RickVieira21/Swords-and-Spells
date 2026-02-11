using UnityEngine;
using System.Collections;

public class LightingController : MonoBehaviour
{
    [Header("Lighting Settings")]
    public Light directionalLight;
    public float targetIntensity = 0.7f;
    public float transitionDuration = 2f;

    private float originalIntensity;

    void Start()
    {
        originalIntensity = directionalLight.intensity;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("SinglePlayer"))
        {
            StartCoroutine(DimLighting());
        }
    }

    IEnumerator DimLighting()
    {
        float elapsed = 0f;
        float currentIntensity = directionalLight.intensity;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            // Calcula intensidade da luz com lerp entre a atual e a target
            directionalLight.intensity = Mathf.Lerp(currentIntensity, targetIntensity, t);

            yield return null;
        }
        directionalLight.intensity = targetIntensity;
    }
}
