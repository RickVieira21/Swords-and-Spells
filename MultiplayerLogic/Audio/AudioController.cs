using UnityEngine;

//Script usado para controlar só o audio do menu principal
public class AudioController : MonoBehaviour
{
    
    public AudioSource audioSource;
    public AudioListener audioListener;

    
    public void StopAudio()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

  
    public void PlayAudio()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }


    public void DisableAudioListener()
    {
        if (audioListener != null)
        {
            audioListener.enabled = false;
        }
    }
}
