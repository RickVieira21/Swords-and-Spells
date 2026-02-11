using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour {

    public  AudioMixer audioMixer;

	//Volume
	public void SetVolume(float volume)
	{
		audioMixer.SetFloat("volume", volume);
	}

	//Qualidade
	public void SetQuality (int qualityIndex)
	{
		QualitySettings.SetQualityLevel(qualityIndex);
	}

	//Fullscreen
	public void SetFullscreen (bool isFullscreen)
	{
		Screen.fullScreen = isFullscreen;
	}

}
