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

    [Header("Debug")]
    public bool debugThisUI = true;

    private RunningPhaseController runningController;
    private InputManager inputManager;
    private bool isActive = false;
    private float promptStartTime;
    private float promptEndTime;

    private Sprite idleButtonSprite;
    private Sprite waitingButtonSprite;
    private Sprite successButtonSprite;
    private Sprite missButtonSprite;
    

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

        // Debug: Log configuration
        if (debugThisUI)
        {
            Debug.Log($"[RunningPromptUI] Initialized: limbName='{limbName}', limbPlayer={limbPlayer} ({(int)limbPlayer})");
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
        if (isActive && inputManager != null)
        {
            bool buttonDown = inputManager.GetLimbLockButtonDown(limbPlayer);

            if (debugThisUI && buttonDown)
            {
                Debug.Log($"[RunningPromptUI-{limbName}] Button detected from controller! limbPlayer={limbPlayer}");
            }

            if (buttonDown)
            {
                OnInputPressed();
            }
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
        if (debugThisUI)
        {
            Debug.Log($"[RunningPromptUI-{limbName}] OnPromptShown called: targetLimb='{targetLimb}', myLimbName='{limbName}', match={targetLimb == limbName}");
        }

        if (targetLimb != limbName) return;

        isActive = true;
        promptStartTime = Time.time;
        promptEndTime = windowEndTime;

        if (debugThisUI)
        {
            Debug.Log($"[RunningPromptUI-{limbName}] NOW ACTIVE - Waiting for input from {limbPlayer}");
        }

        SetActive();
    }

    void OnPromptExpired(string targetLimb)
    {
        if (targetLimb != limbName) return;

        if (debugThisUI)
        {
            Debug.Log($"[RunningPromptUI-{limbName}] Prompt expired!");
        }

        isActive = false;
        ShowMiss();
        Invoke(nameof(SetInactive), 0.5f);
    }

    void OnInputPressed()
    {
        if (debugThisUI)
        {
            Debug.Log($"[RunningPromptUI-{limbName}] OnInputPressed - Sending to controller");
        }

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
        if (buttonImage != null) buttonImage.sprite = idleButtonSprite;
        if (timerSlider != null) timerSlider.value = 0f;
    }

    void SetInactive()
    {
        if (promptPanel != null) promptPanel.SetActive(true);
        if (buttonImage != null) buttonImage.sprite = idleButtonSprite;
        if (timerSlider != null) timerSlider.value = 0f;
    }

    void ShowSuccess()
    {
        if (buttonImage != null) buttonImage.sprite = successButtonSprite;
    }

    void ShowMiss()
    {
        if (buttonImage != null) buttonImage.sprite = missButtonSprite;
    }

    public void AssignButtonSprite(Sprite newSprite, string spriteType)
    {
        switch(spriteType)
        {
            case ("miss"):
                missButtonSprite = newSprite;
                break;
            case("success"):
                successButtonSprite = newSprite;
                break;
            case("wait"):
                waitingButtonSprite = newSprite;
                break;
            case("idle"):
                idleButtonSprite = newSprite;
                break;
        }

        AssignAllSprites();
    }

    void AssignAllSprites()
    {
        if (buttonImage != null)
        {
            buttonImage.sprite = idleButtonSprite;
        }
    }
}