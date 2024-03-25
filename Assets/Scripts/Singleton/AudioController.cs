using AudioManager;
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
    /// Pause all audio sources.
    /// </summary>
    public void Pause()
    {
        foreach (AudioSource src in FindObjectsOfType<AudioSource>())
            src.Pause();
    }

    /// <summary>
    /// Unpause all audio sources.
    /// </summary>
    public void Unpause()
    {
        foreach (AudioSource src in FindObjectsOfType<AudioSource>())
            src.UnPause();
    }

    /// <summary>
    /// Play the specificed track from the playlist. Loop by default.
    /// </summary>
    public void PlayTrack(SoundTrack track, bool loop = true)
    {
        musicSource.clip = playlist[(int)track];
        musicSource.loop = loop;
        musicSource.Stop();
        musicSource.Play();
    }

    /// <summary>
    /// Play the specified sfx from the list. Don't loop by default.
    /// </summary>
    public void PlayEffect(AudioSource audioSource, SoundEffect effect, bool loop = false)
    {
        PlayEffect(audioSource, sfxList[(int)effect], loop);
    }

    private void PlayEffect(AudioSource src, AudioClip clip, bool loop)
    {
        if (loop)
        {
            src.clip = clip;
            src.loop = loop;
            src.Stop();
            src.Play();
        }
        else
        {
            src.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Stop all currently playing sfx's.
    /// </summary>
    public void ClearEffects(AudioSource src = null)
    {
        if (src != null)
            src.Stop();
        else
            FindObjectsOfType<AudioSource>().ToList().ForEach(s => s.Stop());
    }
}

namespace AudioManager
{
    public enum SoundTrack
    {
        Default
    }

    public enum SoundEffect
    {
        GunShot,
        ShotgunShot,
        SilencedGunShot,
        EnergyGunShot,
        GrenadeLaunch,
        RocketLaunch,
        WeaponSwap,
        GrenadeBounce,
        MetalHit,
        LowDrone,
        HighDrone,
        GearShift,
        MetalImpact,
        SmallExplosion,
        BigExplosion,
        SynthSting,
        Gust,
        BrewingStorm,
        Gale,
        IceLaunch,
        IceBounce,
        IceShatter,
        MetalLid,
        StoneImpact,
        QuietDrone,
        MetalClick,
        QuietSplash,
        ModerateSplash,
        LoudSplash,
        BubblingLiquid
    }
}