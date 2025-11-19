using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAudioController : MonoBehaviour
{
    [Header("Monster Audio Sources")]
    public AudioSource footstepsSource;
    public AudioSource groansSource;
    
    [Header("Monster Audio Clips")]
    public AudioClip footstepsClip;
    public AudioClip groansClip;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float footstepsVolume = 0.5f;
    [Range(0f, 1f)] public float groansVolumeRunning = 0.4f;
    [Range(0f, 1f)] public float groansVolumeHiding = 0.6f;
    
    private bool isRunningPhase = false;
    
    private void Awake()
    {
        SetupAudioSource(footstepsSource, footstepsClip, footstepsVolume);
        SetupAudioSource(groansSource, groansClip, groansVolumeRunning);
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
    
    public void StartMonsterAudio()
    {
        if (groansSource != null && !groansSource.isPlaying)
        {
            groansSource.Play();
        }
    }
    
    public void StopMonsterAudio()
    {
        if (footstepsSource != null && footstepsSource.isPlaying)
        {
            footstepsSource.Stop();
        }
        
        if (groansSource != null && groansSource.isPlaying)
        {
            groansSource.Stop();
        }
        
        isRunningPhase = false;
    }
    
    public void StartRunningPhase()
    {
        isRunningPhase = true;
        
        if (footstepsSource != null && !footstepsSource.isPlaying)
        {
            footstepsSource.Play();
        }
        
        if (groansSource != null)
        {
            groansSource.volume = groansVolumeRunning;
        }
    }
    
    public void StartHidingPhase()
    {
        isRunningPhase = false;
        
        if (footstepsSource != null && footstepsSource.isPlaying)
        {
            footstepsSource.Stop();
        }
        
        if (groansSource != null)
        {
            groansSource.volume = groansVolumeHiding;
        }
    }
}