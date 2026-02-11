using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;

public class FinalZoneSingleplayer : MonoBehaviour
{
    public string playerTag = "SinglePlayer";
    public Transform targetPosition;
    public float fadeDuration = 1f;
    public AudioSource audioDoors;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            BaseCharacterSingleplayer baseCharacter = other.GetComponent<BaseCharacterSingleplayer>();
            if (baseCharacter != null)
            {
                StartCoroutine(TransportPlayer(baseCharacter));
            }
        }
    }

    private IEnumerator TransportPlayer(BaseCharacterSingleplayer baseCharacter)
    {
        PlayAudioDoors();

        MageSingleplayer mage = baseCharacter.GetComponent<MageSingleplayer>();
        ThiefSingleplayer thief = baseCharacter.GetComponent<ThiefSingleplayer>();
        WarriorSingleplayer warrior = baseCharacter.GetComponent<WarriorSingleplayer>();

        Image fadeImage = null;
        if (mage != null)
        {
            fadeImage = mage.FadeImage;
        }
        else if (thief != null)
        {
            fadeImage = thief.FadeImage;
        }
        else if (warrior != null)
        {
            fadeImage = warrior.FadeImage;
        }

        if (fadeImage != null)
        {
            yield return StartCoroutine(FadeToBlack(fadeImage));
            baseCharacter.transform.position = targetPosition.position;
            yield return StartCoroutine(FadeFromBlack(fadeImage));
        }
        else
        {
            Debug.LogError("player não tem imagem de fade");
        }
    }

    private IEnumerator FadeToBlack(Image fadeImage)
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
    }

    private IEnumerator FadeFromBlack(Image fadeImage)
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        StopAudioDoors();
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
            fadeImage.color = color;
            yield return null;
        }
    }

    public void PlayAudioDoors()
    {
        audioDoors.Play();
    }

    public void StopAudioDoors()
    {
        audioDoors.Stop();
    }
}
