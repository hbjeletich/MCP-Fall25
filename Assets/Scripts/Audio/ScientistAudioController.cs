using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScientistAudioController : MonoBehaviour
{
    [Header("Scientist Audio Sources")]
    public AudioSource hallwayRunSource;
    public AudioSource giggleSource;
    public AudioSource creepyLaughSource;
    public AudioSource throwingSource;
    
    [Header("Scientist Audio Clips")]
    public AudioClip hallwayRunClip;
    public AudioClip[] giggleClips;
    public AudioClip creepyLaughClip;
    public AudioClip throwingClip;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float hallwayRunVolume = 0.5f;
    [Range(0f, 1f)] public float giggleVolume = 0.5f;
    [Range(0f, 1f)] public float creepyLaughVolume = 0.5f;
    [Range(0f, 1f)] public float throwingVolume = 0.6f;
    
    [Header("Giggle Settings")]
    [Range(1f, 10f)] public float minGiggleInterval = 3f;
    [Range(1f, 10f)] public float maxGiggleInterval = 8f;
    
    private bool isRunningPhase = false;
    private Coroutine giggleCoroutine;
    
    private void Awake()
    {
        SetupAudioSource(hallwayRunSource, hallwayRunClip, hallwayRunVolume, true);
        SetupAudioSource(giggleSource, null, giggleVolume, false);
        SetupAudioSource(creepyLaughSource, creepyLaughClip, creepyLaughVolume, false);
        SetupAudioSource(throwingSource, throwingClip, throwingVolume, false);
    }
    
    private void SetupAudioSource(AudioSource source, AudioClip clip, float volume, bool loop)
    {
        if (source != null)
        {
            if (clip != null)
            {
                source.clip = clip;
            }
            source.loop = loop;
            source.volume = volume;
            source.playOnAwake = false;
        }
    }
    
    public void StartRunningPhase()
    {
        isRunningPhase = true;
        
        if (hallwayRunSource != null && !hallwayRunSource.isPlaying)
        {
            hallwayRunSource.Play();
        }
        
        if (giggleCoroutine != null)
        {
            StopCoroutine(giggleCoroutine);
        }
        giggleCoroutine = StartCoroutine(RandomGiggleLoop());
    }
    
    public void StopRunningPhase()
    {
        isRunningPhase = false;
        
        if (hallwayRunSource != null && hallwayRunSource.isPlaying)
        {
            hallwayRunSource.Stop();
        }
        
        if (giggleCoroutine != null)
        {
            StopCoroutine(giggleCoroutine);
            giggleCoroutine = null;
        }
    }
    
    public void PlayCreepyLaugh()
    {
        if (creepyLaughSource != null)
        {
            creepyLaughSource.Play();
        }
    }
    
    public void PlayThrowing()
    {
        if (throwingSource != null)
        {
            throwingSource.Play();
        }
    }
    
    public void StopAllScientistAudio()
    {
        if (hallwayRunSource != null && hallwayRunSource.isPlaying)
        {
            hallwayRunSource.Stop();
        }
        
        if (giggleSource != null && giggleSource.isPlaying)
        {
            giggleSource.Stop();
        }
        
        if (creepyLaughSource != null && creepyLaughSource.isPlaying)
        {
            creepyLaughSource.Stop();
        }
        
        if (throwingSource != null && throwingSource.isPlaying)
        {
            throwingSource.Stop();
        }
        
        if (giggleCoroutine != null)
        {
            StopCoroutine(giggleCoroutine);
            giggleCoroutine = null;
        }
        
        isRunningPhase = false;
    }
    
    private IEnumerator RandomGiggleLoop()
    {
        while (isRunningPhase)
        {
            float waitTime = Random.Range(minGiggleInterval, maxGiggleInterval);
            yield return new WaitForSeconds(waitTime);
            
            if (isRunningPhase && giggleClips != null && giggleClips.Length > 0)
            {
                PlayRandomGiggle();
            }
        }
    }
    
    public void PlayRandomGiggle()
    {
        if (giggleSource != null && giggleClips.Length > 0)
        {
            AudioClip randomClip = giggleClips[Random.Range(0, giggleClips.Length)];
            giggleSource.clip = randomClip;
            giggleSource.Play();
        }
    }

    public void StartHallwayRun()
    {
        if (hallwayRunSource != null && !hallwayRunSource.isPlaying)
        {
            hallwayRunSource.Play();
        }
    }

    public void StopHallwayRun()
    {
        if (hallwayRunSource != null && hallwayRunSource.isPlaying)
        {
            hallwayRunSource.Stop();
        }
    }
}