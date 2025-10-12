using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    [Header("Phase Controllers")]
    public RunningPhaseController runningPhaseController;
    public HidingPhaseController hidingPhaseController;
    // and then QTE controller
    
    [Header("Game Settings")]
    public float runningPhaseDuration = 15f; 
    public float transitionDelay = 3f;
    public int totalWallsToWin = 10;
    
    [Header("Difficulty")]
    public bool increaseDifficulty = true;
    public float difficultyMultiplier = 1.1f;
    
    private GameState currentState = GameState.Idle;
    private int wallsCompleted = 0;
    private float currentDifficulty = 1f;
    
    public enum GameState
    {
        Idle,
        Running,
        Hiding,
        QTE,
        Transition,
        GameOver,
        Victory
    }
    
    public delegate void OnStateChangeEvent(GameState newState);
    public event OnStateChangeEvent OnStateChange;
    
    public delegate void OnGameOverEvent(bool won);
    public event OnGameOverEvent OnGameOver;

    void Start()
    {
        if (hidingPhaseController != null)
        {
            hidingPhaseController.OnHidingSuccess += OnHidingSuccess;
            hidingPhaseController.OnHidingFail += OnHidingFail;
        }
        
        // FOR NOW auto start
        StartGame();
    }

    void OnDestroy()
    {
        if (hidingPhaseController != null)
        {
            hidingPhaseController.OnHidingSuccess -= OnHidingSuccess;
            hidingPhaseController.OnHidingFail -= OnHidingFail;
        }
    }

    public void StartGame()
    {
        wallsCompleted = 0;
        currentDifficulty = 1f;
        ChangeState(GameState.Running);
    }

    void ChangeState(GameState newState)
    {
        ExitState(currentState);
        
        currentState = newState;
        Debug.Log($"State changed to: {newState}");
        
        OnStateChange?.Invoke(newState);
        
        EnterState(newState);
    }

    void ExitState(GameState state)
    {
        switch (state)
        {
            case GameState.Running:
                if (runningPhaseController != null)
                {
                    runningPhaseController.StopRunning();
                }
                break;
                
            case GameState.Hiding:
                if (hidingPhaseController != null)
                {
                    hidingPhaseController.StopHiding();
                }
                break;
        }
    }

    void EnterState(GameState state)
    {
        switch (state)
        {
            case GameState.Running:
                StartRunningPhase();
                break;
                
            case GameState.Hiding:
                StartHidingPhase();
                break;
                
            case GameState.QTE:
                StartQTEPhase();
                break;
                
            case GameState.Transition:
                StartCoroutine(TransitionCoroutine());
                break;
                
            case GameState.GameOver:
                HandleGameOver(false);
                break;
                
            case GameState.Victory:
                HandleGameOver(true);
                break;
        }
    }

    void StartRunningPhase()
    {
        if (runningPhaseController != null)
        {
            if (increaseDifficulty)
            {
                // could modify running controller's speed/timing based on difficulty
            }
            
            runningPhaseController.StartRunning();
            
            StartCoroutine(RunningPhaseTimer());
        }
    }

    IEnumerator RunningPhaseTimer()
    {
        yield return new WaitForSeconds(runningPhaseDuration);
        
        if (currentState == GameState.Running)
        {
            ChangeState(GameState.Transition);
        }
    }

    void StartHidingPhase()
    {
        if (hidingPhaseController != null)
        {
            hidingPhaseController.StartHiding();
            // hiding phase handles its own timer and completion
            // which is not good game design sorry!
        }
        else
        {
            Debug.LogError("HidingPhaseController not assigned!");
        }
    }

    void StartQTEPhase()
    {
        Debug.Log("QTE Phase - Not yet implemented!");
        StartCoroutine(QTEPlaceholder());
    }

    IEnumerator QTEPlaceholder()
    {
        yield return new WaitForSeconds(2f);
        
        // for now, just continue the cycle
        IncreaseDifficulty();
        ChangeState(GameState.Transition);
    }

    IEnumerator TransitionCoroutine()
    {
        yield return new WaitForSeconds(transitionDelay);
        
        if (currentState == GameState.Transition)
        {
            // If coming from running, go to hiding
            // If coming from QTE, go back to running
            // For now, simple logic:
            if (hidingPhaseController != null && hidingPhaseController.IsHiding())
            {
                // Was hiding, now go to QTE (or back to running for now)
                ChangeState(GameState.Running);
            }
            else
            {
                // Was running, now go to hiding
                ChangeState(GameState.Hiding);
            }
        }
    }

    void OnHidingSuccess()
    {
        Debug.Log("Hiding phase succeeded!");
        wallsCompleted++;
        
        // check win condition
        if (wallsCompleted >= totalWallsToWin)
        {
            ChangeState(GameState.Victory);
        }
        else
        {
            // continue to QTE phase
            IncreaseDifficulty();
            ChangeState(GameState.QTE);
        }
    }

    void OnHidingFail()
    {
        Debug.Log("Hiding phase failed! Retrying...");
        
        // could implement lives/game over here
    }

    void IncreaseDifficulty()
    {
        if (increaseDifficulty)
        {
            currentDifficulty *= difficultyMultiplier;
            Debug.Log($"Difficulty increased to: {currentDifficulty:F2}");
        }
    }

    void HandleGameOver(bool won)
    {
        Debug.Log(won ? "VICTORY!" : "GAME OVER!");
        OnGameOver?.Invoke(won);
    }

    // Public getters for UI
    public GameState GetCurrentState()
    {
        return currentState;
    }

    public int GetWallsCompleted()
    {
        return wallsCompleted;
    }

    public int GetTotalWalls()
    {
        return totalWallsToWin;
    }

    public float GetCurrentDifficulty()
    {
        return currentDifficulty;
    }

    // Public methods for external control
    public void RestartGame()
    {
        StartGame();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }
}