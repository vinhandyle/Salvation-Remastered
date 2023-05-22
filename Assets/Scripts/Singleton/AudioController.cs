using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;


/// <summary>
/// Defines all audio-related actions.
/// </summary>
public class AudioController : Singleton<AudioController>
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private List<AudioClip> playlist;
    [SerializeField] private List<AudioClip> sfxList;

    /// <summary>
    /// Change the volume of a specific audio channel.
    /// </summary>
    public void ChangeVolume(string channel, float volume)
    {
        mixer.SetFloat(channel, Mathf.Log10(volume + 0.0001f) * 20);
    }

    /// <summary>
    /// Get the volume of a specific audio channel.
    /// </summary>
    public float GetVolume(string channel)
    {
        float volume;
        mixer.GetFloat(channel, out volume);
        return Mathf.Pow(10, volume / 20);
    }

    /// <summary>
    /// Play the specificed track from the playlist. Loop by default.
    /// </summary>
    public void PlayTrack(int trackNum, bool loop = true)
    {
        musicSource.clip = playlist[trackNum];
        musicSource.loop = loop;
        musicSource.Stop();
        musicSource.Play();
    }

    /// <summary>
    /// Play the specified sfx from the list. Don't loop by default.
    /// </summary>
    public void PlayEffect(AudioSource audioSource, int effectNum, bool loop = false)
    {
        PlayEffect(audioSource, sfxList[effectNum], loop);
    }

    private void PlayEffect(AudioSource src, AudioClip clip, bool loop)
    {
        src.clip = clip;
        src.loop = loop;
        src.Stop();
        src.Play();
    }

    /// <summary>
    /// Stop all currently playing sfx's.
    /// </summary>
    public void ClearEffects()
    {
        FindObjectsOfType<AudioSource>().ToList().ForEach(src => src.Stop());
    }
}