using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "AudioSettings", menuName = "AudioSettings")]
public class AudioSettings : ScriptableObject
{
    public enum Channel
    {
        Master,
        Music,
        SFX,
        Dialogue
    }
    public float defaultMasterVolume;
    public float defaultMusicVolume;
    public float defaultSFXVolume;
    public float defaultDialogueVolume;


    public void Initialize()
    {
        SetVolume(Channel.Master, GetVolume(Channel.Master));
        SetVolume(Channel.Music, GetVolume(Channel.Music));
        SetVolume(Channel.SFX, GetVolume(Channel.SFX));
        SetVolume(Channel.Dialogue, GetVolume(Channel.Dialogue));
    }

    public void ChangeVolume(Channel mixerChannel, float value)
    {
        SetVolume(mixerChannel, value);
        PlayerPrefs.SetFloat(mixerChannel.ToString() + "Volume", value);
    }

    public float GetVolume(Channel mixerChannel)
    {
        var defaultVolume = mixerChannel switch
        {
            Channel.Master => defaultMasterVolume,
            Channel.Music => defaultMusicVolume,
            Channel.SFX => defaultSFXVolume,
            Channel.Dialogue => defaultDialogueVolume,
            _ => throw new ArgumentOutOfRangeException(nameof(mixerChannel), mixerChannel, null)
        };
        return PlayerPrefs.GetFloat(mixerChannel.ToString() + "Volume", defaultVolume);
    }

    private void SetVolume(Channel mixerChannel, float value)
    {
        string mixerChannelName = mixerChannel.ToString() + "Volume";
        // TODO: Wwise - change mixer bus volume
        //AkSoundEngine.SetRTPCValue(mixerChannelName, value);
    }
}