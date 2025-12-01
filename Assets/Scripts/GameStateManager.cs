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
    
    [Header("Instruction Panels")]
    public GameObject runningInstructionPanel;
    public GameObject hidingInstructionPanel;
    public GameObject qteInstructionPanel;
    public float instructionDisplayTime = 3f;
    public float preInstructionDelay = 2f;
    public float postInstructionDelay = 2f;
    
    private GameState currentState = GameState.Idle;
    private GameState previousState = GameState.Idle;
    private int wallsCompleted = 0;
    private float currentDifficulty = 1f;
    private HidingObject[] currentHidingObjects;
    private int sprintNumber = 0;
    
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

    public delegate void OnLoseHeartEvent();
    public event OnLoseHeartEvent OnLoseHeart;

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

        HideAllInstructionPanels();

        // FOR NOW auto start
        StartGame();
        remainingHearts = 3;
        sprintNumber = 0;
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
        previousState = GameState.Idle; // set previous state so transition knows where we came from
        ChangeState(GameState.Transition);
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
                StartGameOver();
                HandleGameOver(false);
                break;
                
            case GameState.Victory:
                HandleGameOver(true);
                Invoke("GoToStartMenu", 4f);
                break;
        }
    }

    public void StartGameOver()
    {
        // scientistLaugh.SetActive(true);
        frankensteinAnimator.enabled = false;
        DoorScript.StopScroll();
        
        Instantiate(scientistLaugh, new Vector3(0, 0, 0), Quaternion.identity);
        canvas.SetActive(false);

        // add sprint number to final score display?
        Debug.Log($"Final sprint number: {sprintNumber}");

        Invoke("GoToStartMenu", 4f);
    }

    void HandleGameOver(bool won)
    {
        Debug.Log(won ? "VICTORY!" : "GAME OVER!");
        OnGameOver?.Invoke(won);

        if (wallSelectionUI != null)
        {
            wallSelectionUI.SetVisible(false);
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

        // increase sprint number here
        IncreaseSprintNumber();

        IncreaseDifficulty();
        ChangeState(GameState.Transition);
    }

    void OnQTEFail(List<int> missedPlayers)
    {
        Debug.Log($"QTE Failed! {missedPlayers.Count} player(s) missed.");
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
        // Determine which phase is coming next
        GameState nextState = GameState.Running;
        
        if (previousState == GameState.Running)
        {
            nextState = GameState.Hiding;
        }
        else if (previousState == GameState.QTE)
        {
            nextState = GameState.Running;
        }
        else if (previousState == GameState.Hiding)
        {
            nextState = GameState.QTE;
        }
        else if (previousState == GameState.Idle)
        {
            nextState = GameState.Running; // Game start goes to Running
        }
        
        yield return new WaitForSeconds(preInstructionDelay);
        
        // show instruction panel based on next state
        ShowInstructionPanel(nextState);
        
        yield return new WaitForSeconds(instructionDisplayTime);
        
        HideAllInstructionPanels();
        
        yield return new WaitForSeconds(postInstructionDelay);
        
        // transition to next state
        if (currentState == GameState.Transition)
        {
            ChangeState(nextState);
        }
    }
    
    void ShowInstructionPanel(GameState nextState)
    {
        HideAllInstructionPanels();
        
        switch (nextState)
        {
            case GameState.Running:
                if (runningInstructionPanel != null)
                {
                    runningInstructionPanel.SetActive(true);
                    Debug.Log("Showing Running instruction panel");
                }
                break;
                
            case GameState.Hiding:
                if (hidingInstructionPanel != null)
                {
                    hidingInstructionPanel.SetActive(true);
                    Debug.Log("Showing Hiding instruction panel");
                }
                break;
                
            case GameState.QTE:
                if (qteInstructionPanel != null)
                {
                    qteInstructionPanel.SetActive(true);
                    Debug.Log("Showing QTE instruction panel");
                }
                break;
        }
    }
    
    void HideAllInstructionPanels()
    {
        if (runningInstructionPanel != null)
            runningInstructionPanel.SetActive(false);
        if (hidingInstructionPanel != null)
            hidingInstructionPanel.SetActive(false);
        if (qteInstructionPanel != null)
            qteInstructionPanel.SetActive(false);
    }

    void OnHidingSuccess()
    {
        Debug.Log("Hiding phase succeeded!");
        wallsCompleted++;
        
        // destroy objects for this round - fresh objects will be generated next round
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
        OnLoseHeart?.Invoke();

        // wait for animation and then update heart sprite!
        StartCoroutine(WaitAndLoseHeart());
    }

    private IEnumerator WaitAndLoseHeart()
    {
        string currentLabel = heartSprite.GetLabel();
        string newLabel = "";

        // determine which heart sprite to change to
        if (currentLabel == "Three Hearts")
        {
            newLabel = "Two Hearts";
        }
        else if (currentLabel == "Two Hearts")
        {
            newLabel = "One Heart";
        }
        else if (currentLabel == "One Heart")
        {
            for(int i = 0; i < 3; i++)
            {
                hearts.SetActive(false);
                yield return new WaitForSeconds(0.2f);
                hearts.SetActive(true);
                yield return new WaitForSeconds(0.2f);
            }
            hearts.SetActive(false);
            
            ChangeState(GameState.GameOver);
            yield break;
        }

        // blink between current and new sprite 3 times
        for (int i = 0; i < 3; i++)
        {
            heartSprite.SetCategoryAndLabel("Hearts", newLabel);
            yield return new WaitForSeconds(0.2f);
            heartSprite.SetCategoryAndLabel("Hearts", currentLabel);
            yield return new WaitForSeconds(0.2f);
        }

        // set it to the new value permanently
        Debug.Log($"Changing heart sprite to {newLabel}");
        heartSprite.SetCategoryAndLabel("Hearts", newLabel);
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

    void GoToStartMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartingScene");
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

    public void IncreaseSprintNumber()
    {
        sprintNumber++;
    }

    public int GetSprintNumber()
    {
        return sprintNumber;
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