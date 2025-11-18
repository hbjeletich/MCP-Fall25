using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class GameStateManager : MonoBehaviour
{
    [Header("Phase Controllers")]
    public RunningPhaseController runningPhaseController;
    public HidingPhaseController hidingPhaseController;
    public WallSelectionUI wallSelectionUI;
    public QTEController qteController;
    public QTEDoors DoorScript;
    
    [Header("Object Management")]
    public HidingObjectManager hidingObjectManager;
    
    [Header("Game Settings")]
    public float runningPhaseDuration = 15f; 
    public float transitionDelay = 3f;
    public int totalWallsToWin = 10;
    
    [Header("Difficulty")]
    public bool increaseDifficulty = true;
    public float difficultyMultiplier = 1.1f;

    [Header("Player Animator")]
    public Animator playerAnimator;
    
    private GameState currentState = GameState.Idle;
    private GameState previousState = GameState.Idle;
    private int wallsCompleted = 0;
    private float currentDifficulty = 1f;

    private HidingObject[] currentHidingObjects;

    public GameObject hearts;
    public SpriteResolver heartSprite;
    public int remainingHearts;

    public GameObject scientistLaugh;
    public GameObject canvas;
    public Animator frankensteinAnimator;
    
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

        StartGame();
        remainingHearts = 3;
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
                if(playerAnimator != null)
                {
                    playerAnimator.enabled = true;
                }
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
                if(playerAnimator != null)
                {
                    playerAnimator.enabled = false;
                }
                
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
            if (hidingObjectManager != null)
            {
                currentHidingObjects = hidingObjectManager.GenerateObjects();
                
                if (wallSelectionUI != null && currentHidingObjects != null)
                {
                    wallSelectionUI.LoadObjectPreviews(currentHidingObjects);
                    Debug.Log("Loaded object previews into WallSelectionUI");
                }
            }
            else
            {
                Debug.LogError("HidingObjectManager not assigned!");
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

            if (runningPhaseController != null && currentHidingObjects != null)
            {
                int selectedIndex = runningPhaseController.GetSelectedWallIndex();
                HidingObject selectedObject = currentHidingObjects[selectedIndex];
                
                Debug.Log($"Player selected hiding object {selectedIndex}");
            }
            
            hidingPhaseController.StartHiding();
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
        LoseHeart();

        foreach (int playerIndex in missedPlayers)
        {
            Debug.Log($"  - Player {playerIndex} ({(InputManager.LimbPlayer)playerIndex}) missed!");
        }

        IncreaseDifficulty();
        ChangeState(GameState.Transition);
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
        LoseHeart();
        Debug.Log("Hiding phase failed! Retrying...");
        IncreaseDifficulty();
        ChangeState(GameState.Transition);
    }

    void IncreaseDifficulty()
    {
        if (increaseDifficulty)
        {
            currentDifficulty *= difficultyMultiplier;
            Debug.Log($"Difficulty increased to: {currentDifficulty:F2}");
        }
    }

    public void LoseHeart()
    {
        remainingHearts -= 1;
        if (heartSprite.GetLabel() == "Three Hearts")
        {
            heartSprite.SetCategoryAndLabel("Hearts", "Two Hearts");
        }
        else if (heartSprite.GetLabel() == "Two Hearts")
        {
            heartSprite.SetCategoryAndLabel("Hearts", "One Heart");
        }
        else if (heartSprite.GetLabel() == "One Heart")
        {
            hearts.SetActive(false);
            HandleGameOver(false);
        }
    }

    void HandleGameOver(bool won)
    {
        Debug.Log(won ? "VICTORY!" : "GAME OVER!");
        OnGameOver?.Invoke(won);

        if (!won)
        {
            scientistLaugh.SetActive(true);
            frankensteinAnimator.enabled = false;
            DoorScript.StopScroll();
            canvas.SetActive(false);
        }

        if (wallSelectionUI != null)
        {
            wallSelectionUI.SetVisible(false);
        }
    }

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
    
    public HidingObject[] GetCurrentHidingObjects()
    {
        return currentHidingObjects;
    }

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