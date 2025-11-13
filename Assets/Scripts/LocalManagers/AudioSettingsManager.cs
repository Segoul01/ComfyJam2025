using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioSettingsManager : MonoBehaviour
{
    [SerializeField] private AudioMixer mainAudioMixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider bgmSlider;


    private void Awake()
    {
        masterSlider.onValueChanged.AddListener(OnMasterChangeValue);
        musicSlider.onValueChanged.AddListener(OnMusicChangeValue);
        sfxSlider.onValueChanged.AddListener(OnSFXChangeValue);
        bgmSlider.onValueChanged.AddListener(OnBGMChangeValue);
    }


    private void OnDisable()
    {
        masterSlider.onValueChanged.RemoveListener(OnMasterChangeValue);
        musicSlider.onValueChanged.RemoveListener(OnMusicChangeValue);
        sfxSlider.onValueChanged.RemoveListener(OnSFXChangeValue);
        bgmSlider.onValueChanged.RemoveListener(OnBGMChangeValue);    
    }


    private void OnMasterChangeValue(float value)
    {
        mainAudioMixer.SetFloat("master", Mathf.Log10(value) * 20);
    }


    private void OnMusicChangeValue(float value)
    {
        mainAudioMixer.SetFloat("music", Mathf.Log10(value) * 20);   
    }


    private void OnSFXChangeValue(float value)
    {
        mainAudioMixer.SetFloat("sfx", Mathf.Log10(value) * 20);    
    }


    private void OnBGMChangeValue(float value)
    {
        mainAudioMixer.SetFloat("bgm", Mathf.Log10(value) * 20); 
    }
}
