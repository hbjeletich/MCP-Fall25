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
    public ScientistAudioController scientistAudioController;
    
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
                if (scientistAudioController != null)
                {
                    scientistAudioController.StartRunningPhase();
                }
                break;
                
            case GameStateManager.GameState.Hiding:
                if (monsterAudioController != null)
                {
                    monsterAudioController.StartHidingPhase();
                }
                if (scientistAudioController != null)
                {
                    scientistAudioController.PlayCreepyLaugh();
                }
                if (scientistAudioController != null)
                {
                    scientistAudioController.StartHallwayRun();
                }
                break;
                
            case GameStateManager.GameState.QTE:
                if (scientistAudioController != null)
                {
                    scientistAudioController.StartHallwayRun();
                }
                break;
                
            case GameStateManager.GameState.Transition:
                if (scientistAudioController != null)
                {
                    scientistAudioController.StopRunningPhase();
                }
                if (scientistAudioController != null)
                {
                    scientistAudioController.StopHallwayRun();
                }
                break;
                
            case GameStateManager.GameState.GameOver:
                if (scientistAudioController != null)
                {
                    scientistAudioController.PlayCreepyLaugh();
                }
                break;
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
        
        if (scientistAudioController != null)
        {
            scientistAudioController.StopAllScientistAudio();
        }
    }
    
    public void StartCountdownAudio()
    {
        if (phaseAudioController != null)
        {
            phaseAudioController.PlayCountdown();
        }
    }
    
    public void StopCountdownAudio()
    {
        if (phaseAudioController != null)
        {
            phaseAudioController.StopCountdown();
        }
    }
    
    public void PlaySuccess()
    {
        if (phaseAudioController != null)
        {
            phaseAudioController.PlaySuccess();
        }
    }
    
    public void PlayFail()
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
    
    public void PlayScientistThrowing()
    {
        if (scientistAudioController != null)
        {
            scientistAudioController.PlayThrowing();
        }
    }
}