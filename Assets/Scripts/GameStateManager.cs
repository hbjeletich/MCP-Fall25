using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class GameStateManager : MonoBehaviour
{
    [Header("Phase Controllers")]
    public RunningPhaseController runningPhaseController;
    public HidingPhaseController hidingPhaseController;
    public HidingObjectManager hidingObjectManager;
    public WallSelectionUI wallSelectionUI;
    public QTEController qteController;
    public QTEDoors DoorScript;
    
    [Header("Game Settings")]
    public float runningPhaseDuration = 15f; 
    public float transitionDelay = 5f;
    public int totalWallsToWin = 10;
    
    [Header("Difficulty")]
    public bool increaseDifficulty = true;
    public float difficultyMultiplier = 1.1f;

    [Header("Lives")]
    public GameObject hearts;
    public SpriteResolver heartSprite;
    public int remainingHearts;

    [Header("Animation")]
    public GameObject scientistLaugh;
    public GameObject canvas;
    public Animator frankensteinAnimator;
    
    private GameState currentState = GameState.Idle;
    private GameState previousState = GameState.Idle;
    private int wallsCompleted = 0;
    private float currentDifficulty = 1f;
    private HidingObject[] currentHidingObjects;
    
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

    void Update()
    {
        frankensteinAnimator.SetInteger("remainingHearts", remainingHearts);
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
                
                // destroy all objects when leaving hiding phase - fresh objects next round
                if (hidingObjectManager != null)
                {
                    hidingObjectManager.DestroyAllObjects();
                }
                
                if(frankensteinAnimator != null)
                {
                    frankensteinAnimator.enabled = true;
                }
                break;
            // case GameState.QTE:
            //     // nothing special
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
                if(frankensteinAnimator != null)
                {
                    frankensteinAnimator.enabled = false;
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
            //  3 fresh random objects for this round (destroys old ones)
            if (hidingObjectManager != null)
            {
                currentHidingObjects = hidingObjectManager.GenerateObjects();
                
                // update preview UI with the new random objects
                if (wallSelectionUI != null && currentHidingObjects != null)
                {
                    wallSelectionUI.LoadObjectPreviews(currentHidingObjects);
                    Debug.Log("Updated wall selection UI with new random objects");
                }
                
                Debug.Log("=== New round: Generated fresh hiding objects ===");
            }
            else
            {
                Debug.LogWarning("HidingObjectManager not assigned!");
            }
            
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
            // enable only the selected hiding object
            if (runningPhaseController != null && hidingObjectManager != null)
            {
                int selectedIndex = runningPhaseController.GetSelectedWallIndex();
                hidingObjectManager.ShowSelectedObject(selectedIndex);
                
                Debug.Log($"Player selected hiding object {selectedIndex}");
            }
            
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
        
        // Destroy objects for this round - fresh objects will be generated next round
        if (hidingObjectManager != null)
        {
            hidingObjectManager.DestroyAllObjects();
        }
        
        IncreaseDifficulty();
        ChangeState(GameState.Transition);
    }

    void OnHidingFail()
    {
        LoseHeart();
        Debug.Log("Hiding phase failed! Retrying...");
        
        // destroy objects for this round - fresh objects will be generated next round
        if (hidingObjectManager != null)
        {
            hidingObjectManager.DestroyAllObjects();
        }
        
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
        Debug.Log("LOSE HEART called!");
        remainingHearts -= 1;

        // wait for animation and then update heart sprite!
        StartCoroutine(WaitAndLoseHeart());
    }

    void HandleGameOver(bool won)
    {
        // currently no game over it is infinite whoops
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

    private IEnumerator WaitAndLoseHeart()
    {
        yield return new WaitForSeconds(2.2f); // wait for animation duration

        frankensteinAnimator.enabled = false;

        if (heartSprite.GetLabel() == "Three Hearts")
        {
            Debug.Log("Changing heart sprite to Two Hearts");
            heartSprite.SetCategoryAndLabel("Hearts", "Two Hearts");
            RefreshSpriteResolver();
        }
        else if (heartSprite.GetLabel() == "Two Hearts")
        {
            Debug.Log("Changing heart sprite to One Heart");
            heartSprite.SetCategoryAndLabel("Hearts", "One Heart");
            RefreshSpriteResolver();
        }
        else if (heartSprite.GetLabel() == "One Heart")
        {
            Debug.Log("Changing heart sprite to No Hearts");
            hearts.SetActive(false);
            HandleGameOver(false);
        }

        frankensteinAnimator.enabled = true;
    }

    private void RefreshSpriteResolver()
    {
        heartSprite.enabled = false;
        heartSprite.enabled = true;
    }

    public void ThrowAtPlayer(string obj)
    {
        bool dodgingRight = true;
        switch(obj)
        {
            case "bottle":
                dodgingRight = false;
                break;
            case "garlic":
                dodgingRight = false;
                break;
            case "hammer":
                break;
            case "orange":
                break;
        }

        if(dodgingRight) frankensteinAnimator.SetBool("DodgeRight", true);
        else frankensteinAnimator.SetBool("DodgeRight", false);
    }

    // animation events
    public void AnimationWalkFail()
    {
        frankensteinAnimator.SetTrigger("walkFail");
    }

    public void AnimationWalkSuccess()
    {
        frankensteinAnimator.SetTrigger("walkSuccess");
    }

    public void AnimationDodgeFail()
    {
        frankensteinAnimator.SetTrigger("dodgeFail");
    }

    public void AnimationDodgeSuccess()
    {
        frankensteinAnimator.SetTrigger("dodgeSuccess");
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

    public float GetLives()
    {
        return remainingHearts;
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