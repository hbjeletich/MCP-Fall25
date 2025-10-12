using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RunningPromptUI : MonoBehaviour
{
    [Header("Player Settings")]
    public string limbName; // "LeftLeg", "RightLeg", "LeftArm", "RightArm"
    public InputManager.LimbPlayer limbPlayer;
    
    [Header("UI References")]
    public GameObject promptPanel;
    public Image buttonImage;
    public TextMeshProUGUI buttonText;
    public Slider timerSlider;
    public TextMeshProUGUI limbLabel;
    
    [Header("Colors")]
    public Color idleColor = Color.gray;
    public Color activeColor = Color.yellow;
    public Color successColor = Color.green;
    public Color missColor = Color.red;
    
    private RunningPhaseController runningController;
    private InputManager inputManager;
    private bool isActive = false;
    private float promptStartTime;
    private float promptEndTime;

    void Start()
    {
        runningController = FindObjectOfType<RunningPhaseController>();
        inputManager = InputManager.Instance;
        
        if (runningController != null)
        {
            runningController.OnPromptShown += OnPromptShown;
            runningController.OnPromptExpired += OnPromptExpired;
        }
        
        if (limbLabel != null)
        {
            limbLabel.text = limbName.ToUpper();
        }
        
        UpdateButtonText();
        SetInactive();
    }

    void OnDestroy()
    {
        if (runningController != null)
        {
            runningController.OnPromptShown -= OnPromptShown;
            runningController.OnPromptExpired -= OnPromptExpired;
        }
    }

    void Update()
    {
        // check for input from InputManager
        if (isActive && inputManager != null && inputManager.GetLimbLockButtonDown(limbPlayer))
        {
            OnInputPressed();
        }
        
        if (isActive && timerSlider != null)
        {
            float elapsed = Time.time - promptStartTime;
            float duration = promptEndTime - promptStartTime;
            timerSlider.value = elapsed / duration;
        }
    }

    void UpdateButtonText()
    {
        if (buttonText != null && inputManager != null)
        {
            if (inputManager.inputMode == InputManager.InputMode.Debug)
            {
                string keyLabel = GetDebugKeyLabel();
                buttonText.text = keyLabel;
            }
            else
            {
                // Show controller button (generic)
                buttonText.text = "A";
            }
        }
    }

    string GetDebugKeyLabel()
    {
        switch (limbPlayer)
        {
            case InputManager.LimbPlayer.LeftArm: return "Q";
            case InputManager.LimbPlayer.RightArm: return "W";
            case InputManager.LimbPlayer.LeftLeg: return "A";
            case InputManager.LimbPlayer.RightLeg: return "S";
            case InputManager.LimbPlayer.Head: return "SPACE";
            default: return "?";
        }
    }

    void OnPromptShown(string targetLimb, float windowEndTime)
    {
        if (targetLimb != limbName) return;
        
        isActive = true;
        promptStartTime = Time.time;
        promptEndTime = windowEndTime;
        
        SetActive();
    }

    void OnPromptExpired(string targetLimb)
    {
        if (targetLimb != limbName) return;
        
        isActive = false;
        ShowMiss();
        Invoke(nameof(SetInactive), 0.5f);
    }

    void OnInputPressed()
    {
        if (runningController != null)
        {
            runningController.OnPlayerInput(limbName);
        }
        
        isActive = false;
        ShowSuccess();
        Invoke(nameof(SetInactive), 0.3f);
    }

    void SetActive()
    {
        if (promptPanel != null) promptPanel.SetActive(true);
        if (buttonImage != null) buttonImage.color = activeColor;
        if (timerSlider != null) timerSlider.value = 0f;
    }

    void SetInactive()
    {
        if (promptPanel != null) promptPanel.SetActive(true);
        if (buttonImage != null) buttonImage.color = idleColor;
        if (timerSlider != null) timerSlider.value = 0f;
    }

    void ShowSuccess()
    {
        if (buttonImage != null) buttonImage.color = successColor;
    }

    void ShowMiss()
    {
        if (buttonImage != null) buttonImage.color = missColor;
    }
}