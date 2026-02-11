using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MusicZone : MonoBehaviour
{
    public AudioSource newAudioSource; 
    public float fadeDuration = 1.0f; 

    private static List<AudioSource> playedAudioSources = new List<AudioSource>(); // Lista de músicas já tocadas

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Player") || other.CompareTag("SinglePlayer"))
        {

            CleanUpPlayedAudioSources();

            // Verifica se a música da zona já tocou antes
            if (!playedAudioSources.Contains(newAudioSource))
            {
                // Faz fade in da nova música e fade out da música anterior
                AudioSource currentAudioSource = FindCurrentAudioSource();
                if (currentAudioSource != null)
                {
                    StartCoroutine(FadeOut(currentAudioSource, fadeDuration));
                }
                StartCoroutine(FadeIn(newAudioSource, fadeDuration));

                // Atualiza a lista de músicas tocadas
                playedAudioSources.Add(newAudioSource);
            }
        }
    }


    private void CleanUpPlayedAudioSources()
    {
        playedAudioSources.RemoveAll(audioSource => audioSource == null);
    }


    private AudioSource FindCurrentAudioSource()
    {
        foreach (AudioSource audioSource in playedAudioSources)
        {
            if (audioSource.isPlaying)
            {
                return audioSource;
            }
        }
        return null;
    }

    private IEnumerator FadeIn(AudioSource audioSource, float duration)
    {
        audioSource.volume = 0f;
        audioSource.Play();

        while (audioSource.volume < 1.0f)
        {
            audioSource.volume += Time.deltaTime / duration;
            yield return null;
        }

        audioSource.volume = 1.0f;
    }

    private IEnumerator FadeOut(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0f)
        {
            audioSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    //Usado no boss final 
    public void StopAllAudioSources()
    {
        foreach (AudioSource audioSource in playedAudioSources)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                //audioSource.Stop();
                StartCoroutine(FadeOut(audioSource, fadeDuration));
            }
        }
    }


    //Usado no boss final 
    public void StopAllAudioAndPlayNew(AudioSource gameEndedAudio)
    {
        StopAllAudioSources();
        StartCoroutine(FadeIn(gameEndedAudio, fadeDuration));

        //playedAudioSources.Add(gameEndedAudio);
    }
}
