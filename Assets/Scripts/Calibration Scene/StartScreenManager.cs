using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StartScreenManager : MonoBehaviour
{
    [Header("UI References")]
    public Button startButton;
    public TextMeshProUGUI statusText;
    
    [Header("Limb Visuals")]
    public LimbVisualController[] limbVisuals;
    
    [Header("Skin Selection")]
    public BodySelectionPhase bodySelectionPhase;
    
    [Header("Settings")]
    public string gameSceneName = "SampleScene";
    public bool requireAllControllers = true;
    
    [Header("Instructions")]
    public TextMeshProUGUI instructionText;
    public string defaultInstruction = "Move sticks to identify your limb!\nPress Lock to select skin.\nPress START (Y) when ready!";

    [Header("Ready To Start UI")]
    public Image[] readyIndicators;
    public float readyIndicatorDisabledAlpha = 0.3f;
    public float readyIndicatorEnabledAlpha = 1.0f;
    
    private InputManager inputManager;
    private int connectedCount = 0;
    private bool[] playerReadyStates = new bool[5]; // Track ready state for each player
    private float[] lastReadyToggleTime = new float[5]; // Cooldown for toggle
    private const float READY_TOGGLE_COOLDOWN = 0.3f;

    void Start()
    {
        inputManager = InputManager.Instance;
        
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        
        if (inputManager != null)
        {
            inputManager.SetInputMode(InputManager.InputMode.Game);
        }
        
        if (instructionText != null)
        {
            instructionText.text = defaultInstruction;
        }

        // Initialize all ready states to false
        for (int i = 0; i < playerReadyStates.Length; i++)
        {
            playerReadyStates[i] = false;
            lastReadyToggleTime[i] = 0f;
        }

        // Initialize ready indicators
        foreach (var indicator in readyIndicators)
        {
            if (indicator != null)
            {
                SetColorAlpha(indicator, readyIndicatorDisabledAlpha);
            }
        }
        
        InitializeLimbVisuals();
    }

    void Update()
    {
        UpdateControllerStatus();
        HandleReadyUpInput();
        UpdateStartButton();
        UpdateReadyIndicators();
    }

    void SetColorAlpha(Image img, float alpha)
    {
        if (img != null)
        {
            Color color = img.color;
            color.a = alpha;
            img.color = color;
        }
    }

    void InitializeLimbVisuals()
    {
        if (limbVisuals == null || limbVisuals.Length == 0)
        {
            Debug.LogWarning("No limbVisuals assigned to StartScreenManager!");
            return;
        }
        
        for (int i = 0; i < limbVisuals.Length && i < 5; i++)
        {
            if (limbVisuals[i] != null)
            {
                limbVisuals[i].gameObject.SetActive(true);
            }
        }
    }

    void UpdateControllerStatus()
    {
        if (inputManager == null) return;
        
        connectedCount = 0;
        
        for (int i = 0; i < 5; i++)
        {
            bool isConnected = inputManager.IsControllerConnected(i);
            
            if (isConnected)
            {
                connectedCount++;
            }
            
            if (limbVisuals != null && i < limbVisuals.Length && limbVisuals[i] != null)
            {
                limbVisuals[i].gameObject.SetActive(isConnected);
            }
        }
        
        UpdateStatusText();
    }

    void HandleReadyUpInput()
    {
        if (inputManager == null) return;

        for (int i = 0; i < 5; i++)
        {
            if (!inputManager.IsControllerConnected(i)) continue;

            // Check for START button (Y/North) press to toggle ready state
            InputManager.LimbPlayer limb = (InputManager.LimbPlayer)i;
            
            if (inputManager.GetLimbStartButtonDown(limb))
            {
                // Apply cooldown to prevent accidental double-toggles
                if (Time.time - lastReadyToggleTime[i] > READY_TOGGLE_COOLDOWN)
                {
                    playerReadyStates[i] = !playerReadyStates[i];
                    lastReadyToggleTime[i] = Time.time;
                    
                    Debug.Log($"Player {i} ({limb}) ready state toggled: {playerReadyStates[i]}");
                    
                    // Play sound or visual feedback here if you want
                }
            }
        }

        // Check if all connected players are ready and auto-start
        if (AreAllPlayersReady())
        {
            StartGame();
        }
    }

    bool AreAllPlayersReady()
    {
        int connectedReadyCount = 0;
        int totalConnected = 0;

        for (int i = 0; i < 5; i++)
        {
            if (inputManager.IsControllerConnected(i))
            {
                totalConnected++;
                if (playerReadyStates[i])
                {
                    connectedReadyCount++;
                }
            }
        }

        // Need at least one player connected, and all connected players must be ready
        return totalConnected > 0 && connectedReadyCount == totalConnected;
    }

    void UpdateReadyIndicators()
    {
        if (readyIndicators == null) return;

        for (int i = 0; i < readyIndicators.Length && i < 5; i++)
        {
            if (readyIndicators[i] != null)
            {
                bool isConnected = inputManager != null && inputManager.IsControllerConnected(i);
                
                if (!isConnected)
                {
                    // Not connected - very dim
                    SetColorAlpha(readyIndicators[i], readyIndicatorDisabledAlpha);
                }
                else if (playerReadyStates[i])
                {
                    // Connected and ready - full brightness
                    SetColorAlpha(readyIndicators[i], readyIndicatorEnabledAlpha);
                }
                else
                {
                    // Connected but not ready - dim
                    SetColorAlpha(readyIndicators[i], readyIndicatorDisabledAlpha);
                }
            }
        }
    }

    void UpdateStatusText()
    {
        if (statusText == null) return;

        statusText.text = $"Controllers Connected: {connectedCount}/5";
        
        if (requireAllControllers && connectedCount < 5)
        {
            statusText.text += "\nPlease connect all 5 controllers!";
        }
        else if (connectedCount > 0)
        {
            int readyCount = 0;
            for (int i = 0; i < 5; i++)
            {
                if (inputManager.IsControllerConnected(i) && playerReadyStates[i])
                {
                    readyCount++;
                }
            }
            
            statusText.text += $"\nReady: {readyCount}/{connectedCount}";
            
            if (readyCount < connectedCount)
            {
                statusText.text += "\nPress START (Y) when ready!";
            }
            else
            {
                statusText.text += "\nStarting game...";
            }
        }
        else
        {
            statusText.text += "\nPlease connect at least one controller";
        }
    }

    void UpdateStartButton()
    {
        if (startButton == null) return;
        
        // Button is only interactable when all players are ready
        startButton.interactable = AreAllPlayersReady();
    }

    void StartGame()
    {
        Debug.Log("All players ready! Starting game...");
        SceneManager.LoadScene(gameSceneName);
    }

    void OnStartButtonClicked()
    {
        // Manual start via UI button
        if (AreAllPlayersReady())
        {
            StartGame();
        }
    }

    public void RefreshLimbVisuals()
    {
        if (limbVisuals == null) return;
        
        foreach (var limbVisual in limbVisuals)
        {
            if (limbVisual != null)
            {
                limbVisual.UpdateSkin();
            }
        }
    }

    public void ResetAllReady()
    {
        for (int i = 0; i < playerReadyStates.Length; i++)
        {
            playerReadyStates[i] = false;
        }
        
        Debug.Log("Reset all player ready states");
    }

    // Optional: Add this if you want to manually check individual player ready states
    public bool IsPlayerReady(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < playerReadyStates.Length)
        {
            return playerReadyStates[playerIndex];
        }
        return false;
    }
}