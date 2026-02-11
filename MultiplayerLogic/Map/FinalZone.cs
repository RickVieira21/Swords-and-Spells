using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;

public class FinalZone : NetworkBehaviour
{
    public string playerTag = "Player";
    public Transform targetPosition; 
    public float fadeDuration = 1f; 
    public AudioSource audioDoors;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            BaseCharacter baseCharacter = other.GetComponent<BaseCharacter>();
            if (baseCharacter != null)
            {
                StartCoroutine(TransportPlayer(baseCharacter));
            }
        }
    }

    //Transporta o jogador para a zona final (com fade e som)
    private IEnumerator TransportPlayer(BaseCharacter baseCharacter)
    {
        PlayAudioDoors();

        Mage mage = baseCharacter.GetComponent<Mage>();
        Thief thief = baseCharacter.GetComponent<Thief>();
        Warrior warrior = baseCharacter.GetComponent<Warrior>();

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
