using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameStateUI : MonoBehaviour
{
    [Header("References")]
    public GameStateManager gameStateManager;
    
    [Header("State Indicators")]
    public GameObject runningUI;
    public GameObject hidingUI;
    public GameObject qteUI;
    public GameObject transitionUI;
    
    [Header("Progress Display")]
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI stateText;
    public TextMeshProUGUI difficultyText;
    
    [Header("Game Over")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;

    [Header("Lose Heart")]
    public GameObject poofObject;
    public float poofTime = 2f;
    public GameObject armAnim;
    public float armAnimTime = 2f;
    public float moveAcrossAmount = -0.4f;
    
    void Start()
    {
        if (gameStateManager != null)
        {
            gameStateManager.OnStateChange += OnStateChanged;
            gameStateManager.OnGameOver += OnGameOver;
            gameStateManager.OnLoseHeart += OnLoseHeart;
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if(armAnim != null)
        {
            armAnim.SetActive(false);
        }

        if(poofObject != null)
        {
            poofObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (gameStateManager != null)
        {
            gameStateManager.OnStateChange -= OnStateChanged;
            gameStateManager.OnGameOver -= OnGameOver;
            gameStateManager.OnLoseHeart -= OnLoseHeart;
        }
    }

    void Update()
    {
        if (gameStateManager == null) return;
        
        // progress
        if (progressText != null)
        {
            int completed = gameStateManager.GetWallsCompleted();
            int total = gameStateManager.GetTotalWalls();
            progressText.text = $"Progress: {completed}/{total}";
        }
        
        // state display
        if (stateText != null)
        {
            stateText.text = $"State: {gameStateManager.GetCurrentState()}";
        }
        
        // difficulty
        if (difficultyText != null)
        {
            float difficulty = gameStateManager.GetCurrentDifficulty();
            difficultyText.text = $"Difficulty: {difficulty:F1}x";
        }
    }

    void OnStateChanged(GameStateManager.GameState newState)
    {
        // hide state-specific UIs
        if (runningUI != null) runningUI.SetActive(false);
        if (hidingUI != null) hidingUI.SetActive(false);
        if (qteUI != null) qteUI.SetActive(false);
        if (transitionUI != null) transitionUI.SetActive(false);
        
        // show correct UI for current state
        switch (newState)
        {
            case GameStateManager.GameState.Running:
                if (runningUI != null) runningUI.SetActive(true);
                break;
                
            case GameStateManager.GameState.Hiding:
                if (hidingUI != null) hidingUI.SetActive(true);
                break;
                
            case GameStateManager.GameState.QTE:
                if (qteUI != null) qteUI.SetActive(true);
                break;
                
            case GameStateManager.GameState.Transition:
                if (transitionUI != null) transitionUI.SetActive(true);
                break;
        }
    }

    void OnGameOver(bool won)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        if (gameOverText != null)
        {
            if (won)
            {
                gameOverText.text = "VICTORY!\nYou completed all walls!";
            }
            else
            {
                gameOverText.text = "GAME OVER\nTry again?";
            }
        }
    }

    void OnRestartClicked()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        if (gameStateManager != null)
        {
            gameStateManager.RestartGame();
        }
    }

    void OnLoseHeart()
    {
        if (poofObject == null || armAnim == null) return;
        AudioManager.Instance.PlayGiggle();;
        StartCoroutine(LoseHeartSequence());
    }

    private IEnumerator LoseHeartSequence()
    {
        // show poof immediately
        poofObject.SetActive(true);

        // move arm before it appears
        var pos = armAnim.transform.position;
        armAnim.transform.position = new Vector3(pos.x - moveAcrossAmount, pos.y, pos.z);

        // after 0.3s, show arm
        yield return new WaitForSeconds(0.3f);
        armAnim.SetActive(true);

        // at total 1.0s from start, hide poof (we've already waited 0.3s, so wait the remainder)
        float untilPoofOff = 1f - 0.3f; // 0.7f
        if (untilPoofOff > 0f)
            yield return new WaitForSeconds(untilPoofOff);
        poofObject.SetActive(false);

        // compute remaining time until arm should finish
        float timeUntilArmEnd = (0.3f + armAnimTime) - 1f; // arm end time minus current time (1s)
        if (timeUntilArmEnd <= 0f)
        {
            // arm duration already elapsed (or ends now)
            armAnim.SetActive(false);
            yield break;
        }

        // about 0.2s before arm ends, bring back the poof for a short disguise
        if (timeUntilArmEnd > 0.2f)
        {
            yield return new WaitForSeconds(timeUntilArmEnd - 0.2f);
            poofObject.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            poofObject.SetActive(false);
        }
        else
        {
            // not enough time to wait; show poof for the remaining time
            poofObject.SetActive(true);
            yield return new WaitForSeconds(timeUntilArmEnd);
            poofObject.SetActive(false);
        }

        // finally hide the arm
        armAnim.SetActive(false);
    }
}
