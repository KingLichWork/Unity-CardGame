using System;
using TMPro;
using UnityEditor.Localization.Editor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using VContainer;

public class SettingsPanel : UIPanel
{
    [SerializeField] private Button _closeButton;

    [SerializeField] private AudioMixer _mainAudioMixer;
    [SerializeField] private AudioSource _audioSourceVoice;
    [SerializeField] private AudioSource _audioSourceVolume;

    [SerializeField] private Scrollbar _masterSoundVolume;
    [SerializeField] private Scrollbar _effectsSoundVolume;
    [SerializeField] private Scrollbar _voiceSoundVolume;

    [SerializeField] private Toggle _howToPlayToggle;

    [SerializeField] private TMP_Dropdown _languageDropdown;

    private AudioClip[] _voiceClips;
    private AudioClip[] _effectsClips;

    private int _baseVolume = 20;
    private int _minVolume = 20;

    private LocalizationManager _localizationManager;

    [Inject]
    private void Construct(LocalizationManager localizationManager)
    {
        _localizationManager = localizationManager;
    }

    private void OnEnable()
    {
        _closeButton.onClick.AddListener(Hide);

        _masterSoundVolume.onValueChanged.AddListener(MasterVolumeChanged);
        _effectsSoundVolume.onValueChanged.AddListener(EffectsVolumeChanged);
        _voiceSoundVolume.onValueChanged.AddListener(VoiceVolumeChanged);

        _howToPlayToggle.onValueChanged.AddListener(SetHowToPlay);

        _languageDropdown.onValueChanged.AddListener(SwitchLanguage);
    }

    private void OnDisable()
    {
        _closeButton.onClick.RemoveListener(Hide);

        _masterSoundVolume.onValueChanged.RemoveListener(MasterVolumeChanged);
        _effectsSoundVolume.onValueChanged.RemoveListener(EffectsVolumeChanged);
        _voiceSoundVolume.onValueChanged.RemoveListener(VoiceVolumeChanged);

        _howToPlayToggle.onValueChanged.RemoveListener(SetHowToPlay);

        _languageDropdown.onValueChanged.RemoveListener(SwitchLanguage);
    }

    public void Init()
    {
        _mainAudioMixer.SetFloat("MasterVolume", Mathf.Log10(_masterSoundVolume.value) * 20);
        _mainAudioMixer.SetFloat("EffectsVolume", Mathf.Log10(_effectsSoundVolume.value) * 20);
        _mainAudioMixer.SetFloat("VoiceVolume", Mathf.Log10(_voiceSoundVolume.value) * 20);

        _voiceClips = Resources.LoadAll<AudioClip>("Sounds/Cards/Deployment/");
        _effectsClips = Resources.LoadAll<AudioClip>("Sounds/Cards/StartOrder/");

        _howToPlayToggle.isOn = HowToPlay.Instance.IsHowToPlay;

        if (_languageDropdown != null)
        {
            switch (LocalizationManager.Language)
            {
                case Languages.En:
                    _languageDropdown.value = 0;
                    break;

                case Languages.Ru:
                    _languageDropdown.value = 1;
                    break;
            }
        }
    }

    public void SettingsShow()
    {
        _masterSoundVolume.value = GetFloatFromAudioMixer("MasterVolume") * _baseVolume - _minVolume;
        _effectsSoundVolume.value = GetFloatFromAudioMixer("EffectsVolume") * _baseVolume - _minVolume;
        _voiceSoundVolume.value = GetFloatFromAudioMixer("VoiceVolume") * _baseVolume - _minVolume;

        Show();
    }

    private void SwitchLanguage(int value)
    {
        switch (value)
        {
            case 0:
                LocalizationManager.SetLanguage((int)Languages.En);
                break;

            case 1:
                LocalizationManager.SetLanguage((int)Languages.Ru);
                break;
        }
    }

    private void MasterVolumeChanged(float value)
    {
        _mainAudioMixer.SetFloat("MasterVolume", value * _baseVolume - _minVolume);

        PlayRandomSound(_audioSourceVoice, true);
    }

    private void EffectsVolumeChanged(float value)
    {
        _mainAudioMixer.SetFloat("EffectsVolume", value * _baseVolume - _minVolume);

        PlayRandomSound(_audioSourceVolume, false);
    }

    private void VoiceVolumeChanged(float value)
    {
        _mainAudioMixer.SetFloat("VoiceVolume", value * _baseVolume - _minVolume);

        PlayRandomSound(_audioSourceVoice, true);
    }

    private void PlayRandomSound(AudioSource audioSource, bool isVoice)
    {
        if (isVoice)
            audioSource.clip = _voiceClips[UnityEngine.Random.Range(0, _voiceClips.Length)];

        else
            audioSource.clip = _effectsClips[UnityEngine.Random.Range(0, _effectsClips.Length)];

        audioSource.Play();
    }

    private float GetFloatFromAudioMixer(string nameFloat)
    {
        float value;
        _mainAudioMixer.GetFloat(nameFloat, out value);
        return value;
    }

    private void SetHowToPlay(bool isTrue)
    {
        HowToPlay.Instance.SetIsHowToPlay(isTrue);
    }
}
