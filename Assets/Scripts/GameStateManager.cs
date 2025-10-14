using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    [Header("Phase Controllers")]
    public RunningPhaseController runningPhaseController;
    public HidingPhaseController hidingPhaseController;
    public WallSelectionUI wallSelectionUI;
    public QTEController qteController;
    
    [Header("Game Settings")]
    public float runningPhaseDuration = 15f; 
    public float transitionDelay = 3f;
    public int totalWallsToWin = 10;
    
    [Header("Difficulty")]
    public bool increaseDifficulty = true;
    public float difficultyMultiplier = 1.1f;
    
    private GameState currentState = GameState.Idle;
    private GameState previousState = GameState.Idle;
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
        
        if (qteController != null)
        {
            qteController.OnQTESuccess += OnQTESuccess;
            qteController.OnQTEFail += OnQTEFail;
        }

        if (wallSelectionUI != null)
        {
            wallSelectionUI.SetVisible(false);
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
        
        if (qteController != null)
        {
            qteController.OnQTESuccess -= OnQTESuccess;
            qteController.OnQTEFail -= OnQTEFail;
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
        
        previousState = currentState;
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
                if (wallSelectionUI != null)
                {
                    wallSelectionUI.SetVisible(false);
                }
                break;
                
            case GameState.Hiding:
                if (hidingPhaseController != null)
                {
                    hidingPhaseController.StopHiding();
                }
                break;
            case GameState.QTE:
            // nothing special for QTE
                break;
        }
    }

    void EnterState(GameState state)
    {
        switch (state)
        {
            case GameState.Running:
                if (wallSelectionUI != null)
                {
                    wallSelectionUI.SetVisible(true);
                }
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
        else
        {
            Debug.LogError("RunningPhaseController not assigned!");
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
        }
        else
        {
            Debug.LogError("HidingPhaseController not assigned!");
        }
    }

    void StartQTEPhase()
    {
        if (qteController != null)
        {
            qteController.BeginQTE();
        }
        else
        {
            Debug.LogError("QTEController not assigned!");
        }
    }

    void OnQTESuccess()
    {
        Debug.Log("QTE Success!");
        IncreaseDifficulty();
        ChangeState(GameState.Transition);
    }

    void OnQTEFail(List<int> missedPlayers)
    {
        Debug.Log($"QTE Failed! {missedPlayers.Count} player(s) missed. Retrying...");

        foreach (int playerIndex in missedPlayers)
        {
            Debug.Log($"  - Player {playerIndex} ({(InputManager.LimbPlayer)playerIndex}) missed!");
        }

        // retry QTE
        ChangeState(GameState.QTE);
    }

    IEnumerator TransitionCoroutine()
    {
        yield return new WaitForSeconds(transitionDelay);
        
        if (currentState == GameState.Transition)
        {
            // Running → Hiding → QTE → Running
            if (previousState == GameState.Running)
            {
                ChangeState(GameState.Hiding);
            }
            else if (previousState == GameState.QTE)
            {
                ChangeState(GameState.Running);
            }
            else if (previousState == GameState.Hiding)
            {
                ChangeState(GameState.QTE);
            }
            else
            {
                ChangeState(GameState.Running);
            }
        }
    }

    void OnHidingSuccess()
    {
        Debug.Log("Hiding phase succeeded!");
        wallsCompleted++;
        
        IncreaseDifficulty();
        ChangeState(GameState.Transition);
    }

    void OnHidingFail()
    {
        Debug.Log("Hiding phase failed! Retrying...");
        IncreaseDifficulty();
        ChangeState(GameState.Transition);
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
        // currently no game over it is infinite whoops
        Debug.Log(won ? "VICTORY!" : "GAME OVER!");
        OnGameOver?.Invoke(won);

        if (wallSelectionUI != null)
        {
            wallSelectionUI.SetVisible(false);
        }
    }

    // getters for UI
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

    // external methods
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