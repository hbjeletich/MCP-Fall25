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
    public string defaultInstruction = "Move sticks to identify your limb!\nPress Lock button to select skin.";
    
    private InputManager inputManager;
    private int connectedCount = 0;

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
        
        InitializeLimbVisuals();
    }

    void Update()
    {
        UpdateControllerStatus();
        UpdateStartButton();
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
        
        if (statusText != null)
        {
            statusText.text = $"Controllers Connected: {connectedCount}/5";
            
            if (requireAllControllers && connectedCount < 5)
            {
                statusText.text += "\nPlease connect all 5 controllers!";
            }
            else if (connectedCount > 0)
            {
                statusText.text += "\nReady to play!";
            }
            else
            {
                statusText.text += "\nPlease connect at least one controller";
            }
        }
    }

    void UpdateStartButton()
    {
        if (startButton == null) return;
        
        if (requireAllControllers)
        {
            startButton.interactable = (connectedCount >= 5);
        }
        else
        {
            startButton.interactable = (connectedCount > 0);
        }
    }

    void OnStartButtonClicked()
    {
        Debug.Log("Starting game...");
          
        SceneManager.LoadScene(gameSceneName);
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
}