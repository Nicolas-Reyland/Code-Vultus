using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionSettings : MonoBehaviour
{
    public AudioMixer audio_mixer;
    public Dropdown resolution_dropdown;

    Resolution[] resolutions;

    void Start()
    {
        resolutions = Screen.resolutions;

        resolution_dropdown.ClearOptions();

        List<string> options = new List<string>();

        int current_resolution_index = 0;
        for(int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                current_resolution_index = i;
            }
        }
        resolution_dropdown.AddOptions(options);
        resolution_dropdown.value = current_resolution_index;
        resolution_dropdown.RefreshShownValue();
    }

    public void SetResolution(int resolution_index)
    {
        Resolution resolution = resolutions[resolution_index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetVolume(float volume)
    {
        audio_mixer.SetFloat("volume", volume);
    }

    public void SetQuality (int quality_index)
    {
        QualitySettings.SetQualityLevel(quality_index);
    }

    public void SetFullScreen(bool is_fullscreen)
    {
        Screen.fullScreen = is_fullscreen;
    }

}
