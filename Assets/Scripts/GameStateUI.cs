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
    
    void Start()
    {
        if (gameStateManager != null)
        {
            gameStateManager.OnStateChange += OnStateChanged;
            gameStateManager.OnGameOver += OnGameOver;
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (gameStateManager != null)
        {
            gameStateManager.OnStateChange -= OnStateChanged;
            gameStateManager.OnGameOver -= OnGameOver;
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
}
