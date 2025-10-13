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
    public GameObject[] controllerSquares;
    
    [Header("Settings")]
    public string gameSceneName = "SampleScene";
    public bool requireAllControllers = true;
    
    private InputManager inputManager;
    private int connectedCount = 0;

    void Start()
    {
        inputManager = InputManager.Instance;
        
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        
        // make sure we're in game mode to detect controllers
        if (inputManager != null)
        {
            inputManager.SetInputMode(InputManager.InputMode.Game);
        }
    }

    void Update()
    {
        UpdateControllerStatus();
        UpdateStartButton();
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
            
            // show/hide squares based on connection
            if (i < controllerSquares.Length && controllerSquares[i] != null)
            {
                controllerSquares[i].SetActive(isConnected);
            }
        }
        
        // status text
        if (statusText != null)
        {
            statusText.text = $"Controllers Connected: {connectedCount}/5";
            
            if (requireAllControllers && connectedCount < 5)
            {
                statusText.text += "\nPlease connect all 5 controllers!";
            }
            else if (connectedCount > 0)
            {
                statusText.text += "\nMove sticks and press buttons to test!";
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
        
        // enable button only if requirements are met
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
}
