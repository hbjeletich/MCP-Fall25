using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseAudioController : MonoBehaviour
{
    [Header("QTE Audio Sources")]
    public AudioSource countdownSource;
    public AudioSource successSource;
    public AudioSource failSource;
    
    [Header("QTE Audio Clips")]
    public AudioClip countdownClip;
    public AudioClip successClip;
    public AudioClip failClip;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float countdownVolume = 0.6f;
    [Range(0f, 1f)] public float successVolume = 0.7f;
    [Range(0f, 1f)] public float failVolume = 0.7f;
    
    private void Awake()
    {
        SetupAudioSource(countdownSource, countdownClip, countdownVolume, true);
        SetupAudioSource(successSource, successClip, successVolume, false);
        SetupAudioSource(failSource, failClip, failVolume, false);
    }
    
    private void SetupAudioSource(AudioSource source, AudioClip clip, float volume, bool loop)
    {
        if (source != null && clip != null)
        {
            source.clip = clip;
            source.loop = loop;
            source.volume = volume;
            source.playOnAwake = false;
        }
    }
    
    public void PlayCountdown()
    {
        if (countdownSource != null && !countdownSource.isPlaying)
        {
            countdownSource.Play();
        }
    }
    
    public void StopCountdown()
    {
        if (countdownSource != null && countdownSource.isPlaying)
        {
            countdownSource.Stop();
        }
    }
    
    public void PlaySuccess()
    {
        if (successSource != null)
        {
            successSource.Play();
        }
    }
    
    public void PlayFail()
    {
        if (failSource != null)
        {
            failSource.Play();
        }
    }
    
    public void StopAllQTEAudio()
    {
        StopCountdown();
        
        if (successSource != null && successSource.isPlaying)
        {
            successSource.Stop();
        }
        
        if (failSource != null && failSource.isPlaying)
        {
            failSource.Stop();
        }
    }
}