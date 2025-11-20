using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbienceController : MonoBehaviour
{
    [Header("Ambience Audio Sources")]
    public AudioSource roomToneSource;
    public AudioSource burnerSource;
    public AudioSource oldHouseAmbienceSource;
    public AudioSource vialsBobblingSource;
    public AudioSource chainsRattlingSource;
    
    [Header("Ambience Audio Clips")]
    public AudioClip roomToneClip;
    public AudioClip burnerClip;
    public AudioClip oldHouseAmbienceClip;
    public AudioClip vialsBobblingClip;
    public AudioClip chainsRattlingClip;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float roomToneVolume = 0.5f;
    [Range(0f, 1f)] public float burnerVolume = 0.3f;
    [Range(0f, 1f)] public float oldHouseAmbienceVolume = 0.3f;
    [Range(0f, 1f)] public float vialsBobblingVolume = 0.3f;
    [Range(0f, 1f)] public float chainsRattlingVolume = 0.3f;
    
    private void Awake()
    {
        SetupAudioSource(roomToneSource, roomToneClip, roomToneVolume);
        SetupAudioSource(burnerSource, burnerClip, burnerVolume);
        SetupAudioSource(oldHouseAmbienceSource, oldHouseAmbienceClip, oldHouseAmbienceVolume);
        SetupAudioSource(vialsBobblingSource, vialsBobblingClip, vialsBobblingVolume);
        SetupAudioSource(chainsRattlingSource, chainsRattlingClip, chainsRattlingVolume);
    }
    
    private void SetupAudioSource(AudioSource source, AudioClip clip, float volume)
    {
        if (source != null && clip != null)
        {
            source.clip = clip;
            source.loop = true;
            source.volume = volume;
            source.playOnAwake = false;
        }
    }
    
    public void StartAmbience()
    {
        if (roomToneSource != null && !roomToneSource.isPlaying)
        {
            roomToneSource.Play();
        }
    }
    
    public void StopAmbience()
    {
        if (roomToneSource != null && roomToneSource.isPlaying)
        {
            roomToneSource.Stop();
        }
        
        if (burnerSource != null && burnerSource.isPlaying)
        {
            burnerSource.Stop();
        }
        
        if (oldHouseAmbienceSource != null && oldHouseAmbienceSource.isPlaying)
        {
            oldHouseAmbienceSource.Stop();
        }
        
        if (vialsBobblingSource != null && vialsBobblingSource.isPlaying)
        {
            vialsBobblingSource.Stop();
        }
        
        if (chainsRattlingSource != null && chainsRattlingSource.isPlaying)
        {
            chainsRattlingSource.Stop();
        }
    }
    
    public void ToggleBurner(bool enable)
    {
        if (burnerSource != null)
        {
            if (enable && !burnerSource.isPlaying)
            {
                burnerSource.Play();
            }
            else if (!enable && burnerSource.isPlaying)
            {
                burnerSource.Stop();
            }
        }
    }
    
    public void ToggleOldHouseAmbience(bool enable)
    {
        if (oldHouseAmbienceSource != null)
        {
            if (enable && !oldHouseAmbienceSource.isPlaying)
            {
                oldHouseAmbienceSource.Play();
            }
            else if (!enable && oldHouseAmbienceSource.isPlaying)
            {
                oldHouseAmbienceSource.Stop();
            }
        }
    }
    
    public void ToggleVialsBubbling(bool enable)
    {
        if (vialsBobblingSource != null)
        {
            if (enable && !vialsBobblingSource.isPlaying)
            {
                vialsBobblingSource.Play();
            }
            else if (!enable && vialsBobblingSource.isPlaying)
            {
                vialsBobblingSource.Stop();
            }
        }
    }
    
    public void ToggleChainsRattling(bool enable)
    {
        if (chainsRattlingSource != null)
        {
            if (enable && !chainsRattlingSource.isPlaying)
            {
                chainsRattlingSource.Play();
            }
            else if (!enable && chainsRattlingSource.isPlaying)
            {
                chainsRattlingSource.Stop();
            }
        }
    }
}