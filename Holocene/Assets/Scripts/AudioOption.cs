using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioOptionsController : MonoBehaviour
{
    public AudioMixer mainMixer;
    public Slider musicSlider;
    public Slider soundSlider;

    private void Start()
    {
        if (PlayerPrefs.HasKey("MusicVol"))
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVol");
        }
        else
        {
            musicSlider.value = 0.5f;
        }

        if (PlayerPrefs.HasKey("SoundVol"))
        {
            soundSlider.value = PlayerPrefs.GetFloat("SoundVol");
        }
        else
        {
            soundSlider.value = 0.5f;
        }

        SetMusicVolume(musicSlider.value);
        SetSoundVolume(soundSlider.value);

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        soundSlider.onValueChanged.AddListener(SetSoundVolume);
    }

    public void SetMusicVolume(float volume)
    {
        mainMixer.SetFloat("MusicVol", Mathf.Log10(volume) * 20f);
        PlayerPrefs.SetFloat("MusicVol", volume);
    }

    public void SetSoundVolume(float volume)
    {
        mainMixer.SetFloat("SoundVol", Mathf.Log10(volume) * 20f);
        PlayerPrefs.SetFloat("SoundVol", volume);
    }
}