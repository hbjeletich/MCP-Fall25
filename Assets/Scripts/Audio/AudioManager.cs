using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Controllers")]
    public AmbienceController ambienceController;
    public MonsterAudioController monsterAudioController;
    public PhaseAudioController phaseAudioController;
    
    [Header("Game State Manager")]
    public GameStateManager gameStateManager;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        if (gameStateManager != null)
        {
            gameStateManager.OnStateChange += OnGameStateChanged;
        }
        else
        {
            Debug.LogError("GameStateManager not assigned to AudioManager!");
        }

        StartGameplayAudio();

        if (monsterAudioController != null)
        {
            monsterAudioController.StartRunningPhase();
        }
    }
    
    private void OnDestroy()
    {
        if (gameStateManager != null)
        {
            gameStateManager.OnStateChange -= OnGameStateChanged;
        }
    }
    
    private void OnGameStateChanged(GameStateManager.GameState newState)
    {
        switch (newState)
        {
            case GameStateManager.GameState.Idle:
                StopAllGameplayAudio();
                break;
                
            case GameStateManager.GameState.Running:
                break;
                
            case GameStateManager.GameState.Hiding:
                if (monsterAudioController != null)
                {
                    monsterAudioController.StartHidingPhase();
                }
                break;
                
            case GameStateManager.GameState.QTE:
                break;
                
            case GameStateManager.GameState.Transition:
                break;
                
            case GameStateManager.GameState.GameOver:
            case GameStateManager.GameState.Victory:
                StopAllGameplayAudio();
                break;
        }
    }
    
    private void StartGameplayAudio()
    {
        if (ambienceController != null)
        {
            ambienceController.StartAmbience();
        }
        
        if (monsterAudioController != null)
        {
            monsterAudioController.StartMonsterAudio();
        }
    }
    
    private void StopAllGameplayAudio()
    {
        if (ambienceController != null)
        {
            ambienceController.StopAmbience();
        }
        
        if (monsterAudioController != null)
        {
            monsterAudioController.StopMonsterAudio();
        }
        
        if (phaseAudioController != null)
        {
            phaseAudioController.StopAllQTEAudio();
        }
    }
    
    public void PlayQTECountdown()
    {
        if (phaseAudioController != null)
        {
            phaseAudioController.PlayCountdown();
        }
    }
    
    public void StopQTECountdown()
    {
        if (phaseAudioController != null)
        {
            phaseAudioController.StopCountdown();
        }
    }
    
    public void PlayQTESuccess()
    {
        if (phaseAudioController != null)
        {
            phaseAudioController.PlaySuccess();
        }
    }
    
    public void PlayQTEFail()
    {
        if (phaseAudioController != null)
        {
            phaseAudioController.PlayFail();
        }
    }
    
    public void ToggleBurner(bool enable)
    {
        if (ambienceController != null)
        {
            ambienceController.ToggleBurner(enable);
        }
    }
    
    public void ToggleOldHouseAmbience(bool enable)
    {
        if (ambienceController != null)
        {
            ambienceController.ToggleOldHouseAmbience(enable);
        }
    }
    
    public void ToggleVialsBubbling(bool enable)
    {
        if (ambienceController != null)
        {
            ambienceController.ToggleVialsBubbling(enable);
        }
    }
    
    public void ToggleChainsRattling(bool enable)
    {
        if (ambienceController != null)
        {
            ambienceController.ToggleChainsRattling(enable);
        }
    }
}