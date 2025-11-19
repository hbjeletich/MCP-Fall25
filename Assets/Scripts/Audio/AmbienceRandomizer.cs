using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbienceRandomizer : MonoBehaviour
{
    [Header("Randomization Settings")]
    [Range(1f, 10f)] public float minToggleInterval = 5f;
    [Range(1f, 10f)] public float maxToggleInterval = 8f;
    [Range(0.5f, 5f)] public float fadeDuration = 2f;
    
    [Header("Toggle Probabilities")]
    [Range(0f, 1f)] public float burnerToggleProbability = 0.5f;
    [Range(0f, 1f)] public float oldHouseAmbienceToggleProbability = 0.5f;
    [Range(0f, 1f)] public float vialsBobblingToggleProbability = 0.5f;
    [Range(0f, 1f)] public float chainsRattlingToggleProbability = 0.5f;
    
    private bool isRandomizing = false;
    
    private bool burnerEnabled = false;
    private bool oldHouseAmbienceEnabled = false;
    private bool vialsBobblingEnabled = false;
    private bool chainsRattlingEnabled = false;
    
    private Dictionary<string, Coroutine> activeFadeCoroutines = new Dictionary<string, Coroutine>();
    private Dictionary<string, float> soundTimers = new Dictionary<string, float>();

    void Start()
    {
        StartRandomizing();
    }
    
    public void StartRandomizing()
    {
        if (!isRandomizing)
        {
            isRandomizing = true;
            StartCoroutine(RandomizeAmbienceLoop());
        }
    }
    
    public void StopRandomizing()
    {
        isRandomizing = false;
        StopAllCoroutines();
    }
    
    private IEnumerator RandomizeAmbienceLoop()
    {
        while (isRandomizing)
        {
            float waitTime = Random.Range(minToggleInterval, maxToggleInterval);
            yield return new WaitForSeconds(waitTime);
            
            RandomizeAmbienceSounds();
        }
    }
    
    private void RandomizeAmbienceSounds()
    {
        if (Random.value < burnerToggleProbability)
        {
            ToggleAmbienceSound("burner", ref burnerEnabled);
        }
        
        if (Random.value < oldHouseAmbienceToggleProbability)
        {
            ToggleAmbienceSound("oldHouseAmbience", ref oldHouseAmbienceEnabled);
        }
        
        if (Random.value < vialsBobblingToggleProbability)
        {
            ToggleAmbienceSound("vialsBubbling", ref vialsBobblingEnabled);
        }
        
        if (Random.value < chainsRattlingToggleProbability)
        {
            ToggleAmbienceSound("chainsRattling", ref chainsRattlingEnabled);
        }
    }
    
    private void ToggleAmbienceSound(string soundName, ref bool isEnabled)
    {
        isEnabled = true;
        FadeInSound(soundName);
        
        float playDuration = Random.Range(minToggleInterval, maxToggleInterval);
        soundTimers[soundName] = Time.time + playDuration;
        
        if (activeFadeCoroutines.ContainsKey(soundName + "_timer"))
        {
            StopCoroutine(activeFadeCoroutines[soundName + "_timer"]);
        }
        activeFadeCoroutines[soundName + "_timer"] = StartCoroutine(ScheduleFadeOut(soundName, playDuration));
    }
    
    private IEnumerator ScheduleFadeOut(string soundName, float delay)
    {
        yield return new WaitForSeconds(delay);
        FadeOutSound(soundName);
        
        switch (soundName)
        {
            case "burner":
                burnerEnabled = false;
                break;
            case "oldHouseAmbience":
                oldHouseAmbienceEnabled = false;
                break;
            case "vialsBubbling":
                vialsBobblingEnabled = false;
                break;
            case "chainsRattling":
                chainsRattlingEnabled = false;
                break;
        }
        
        activeFadeCoroutines.Remove(soundName + "_timer");
    }
    
    private void FadeInSound(string soundName)
    {
        if (activeFadeCoroutines.ContainsKey(soundName))
        {
            StopCoroutine(activeFadeCoroutines[soundName]);
        }
        
        AudioSource source = GetAudioSourceForSound(soundName);
        if (source != null)
        {
            activeFadeCoroutines[soundName] = StartCoroutine(FadeAudioSource(source, 0f, GetTargetVolumeForSound(soundName), fadeDuration, soundName, true));
        }
    }
    
    private void FadeOutSound(string soundName)
    {
        if (activeFadeCoroutines.ContainsKey(soundName))
        {
            StopCoroutine(activeFadeCoroutines[soundName]);
        }
        
        AudioSource source = GetAudioSourceForSound(soundName);
        if (source != null)
        {
            activeFadeCoroutines[soundName] = StartCoroutine(FadeAudioSource(source, source.volume, 0f, fadeDuration, soundName, false));
        }
    }
    
    private IEnumerator FadeAudioSource(AudioSource source, float startVolume, float targetVolume, float duration, string soundName, bool fadeIn)
    {
        if (fadeIn)
        {
            EnableSound(soundName);
            source.volume = startVolume;
        }
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }
        
        source.volume = targetVolume;
        
        if (!fadeIn)
        {
            DisableSound(soundName);
        }
        
        activeFadeCoroutines.Remove(soundName);
    }
    
    private AudioSource GetAudioSourceForSound(string soundName)
    {
        if (AudioManager.Instance == null || AudioManager.Instance.ambienceController == null)
            return null;
            
        switch (soundName)
        {
            case "burner":
                return AudioManager.Instance.ambienceController.burnerSource;
            case "oldHouseAmbience":
                return AudioManager.Instance.ambienceController.oldHouseAmbienceSource;
            case "vialsBubbling":
                return AudioManager.Instance.ambienceController.vialsBobblingSource;
            case "chainsRattling":
                return AudioManager.Instance.ambienceController.chainsRattlingSource;
            default:
                return null;
        }
    }
    
    private float GetTargetVolumeForSound(string soundName)
    {
        if (AudioManager.Instance == null || AudioManager.Instance.ambienceController == null)
            return 0.3f;
            
        switch (soundName)
        {
            case "burner":
                return AudioManager.Instance.ambienceController.burnerVolume;
            case "oldHouseAmbience":
                return AudioManager.Instance.ambienceController.oldHouseAmbienceVolume;
            case "vialsBubbling":
                return AudioManager.Instance.ambienceController.vialsBobblingVolume;
            case "chainsRattling":
                return AudioManager.Instance.ambienceController.chainsRattlingVolume;
            default:
                return 0.3f;
        }
    }
    
    private void EnableSound(string soundName)
    {
        switch (soundName)
        {
            case "burner":
                AudioManager.Instance.ToggleBurner(true);
                break;
            case "oldHouseAmbience":
                AudioManager.Instance.ToggleOldHouseAmbience(true);
                break;
            case "vialsBubbling":
                AudioManager.Instance.ToggleVialsBubbling(true);
                break;
            case "chainsRattling":
                AudioManager.Instance.ToggleChainsRattling(true);
                break;
        }
    }
    
    private void DisableSound(string soundName)
    {
        switch (soundName)
        {
            case "burner":
                AudioManager.Instance.ToggleBurner(false);
                break;
            case "oldHouseAmbience":
                AudioManager.Instance.ToggleOldHouseAmbience(false);
                break;
            case "vialsBubbling":
                AudioManager.Instance.ToggleVialsBubbling(false);
                break;
            case "chainsRattling":
                AudioManager.Instance.ToggleChainsRattling(false);
                break;
        }
    }
}