using System;
using System.Collections.Generic;
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

    private readonly Dictionary<AudioChannel, string> _soundMixerParams =
    new()
    {
        { AudioChannel.Master, "MasterVolume" },
        { AudioChannel.Effects, "EffectsVolume" },
        { AudioChannel.Voice, "VoiceVolume" }
    };

    private void OnEnable()
    {
        SetStart();

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
        LoadVolume(AudioChannel.Master);
        LoadVolume(AudioChannel.Effects);
        LoadVolume(AudioChannel.Voice);

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

    private void SetStart()
    {
        _masterSoundVolume.value = PlayerPrefs.GetFloat(_soundMixerParams[AudioChannel.Master], 0.2f);
        _effectsSoundVolume.value = PlayerPrefs.GetFloat(_soundMixerParams[AudioChannel.Effects], 0.2f);
        _voiceSoundVolume.value = PlayerPrefs.GetFloat(_soundMixerParams[AudioChannel.Voice], 0.2f);
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

    private void LoadVolume(AudioChannel channel)
    {
        float value = PlayerPrefs.GetFloat(
            channel.ToString(),
            0.2f
        );

        _mainAudioMixer.SetFloat(
            _soundMixerParams[channel],
            LinearToDb(value)
        );
    }

    private float LinearToDb(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);
        return Mathf.Log10(value) * 20f;
    }

    private void MasterVolumeChanged(float value)
    {
        _mainAudioMixer.SetFloat("MasterVolume", LinearToDb(value));

        SaveVolume(_soundMixerParams[AudioChannel.Master], value);
        PlayRandomSound(_audioSourceVoice, true);
    }

    private void EffectsVolumeChanged(float value)
    {
        _mainAudioMixer.SetFloat("EffectsVolume", LinearToDb(value));

        SaveVolume(_soundMixerParams[AudioChannel.Effects], value);
        PlayRandomSound(_audioSourceVolume, false);
    }

    private void VoiceVolumeChanged(float value)
    {
        _mainAudioMixer.SetFloat("VoiceVolume", LinearToDb(value));

        SaveVolume(_soundMixerParams[AudioChannel.Voice], value);
        PlayRandomSound(_audioSourceVoice, true);
    }

    private void SaveVolume(string typeVolume, float value)
    {
        PlayerPrefs.SetFloat(typeVolume, value);
        PlayerPrefs.Save();
    }

    private void PlayRandomSound(AudioSource audioSource, bool isVoice)
    {
        if (isVoice)
            audioSource.clip = _voiceClips[UnityEngine.Random.Range(0, _voiceClips.Length)];

        else
            audioSource.clip = _effectsClips[UnityEngine.Random.Range(0, _effectsClips.Length)];

        audioSource.Play();
    }

    private void SetHowToPlay(bool isTrue)
    {
        HowToPlay.Instance.SetIsHowToPlay(isTrue);
    }
}

public enum AudioChannel
{
    Master,
    Effects,
    Voice
}
