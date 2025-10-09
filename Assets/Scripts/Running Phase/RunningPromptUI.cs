using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RunningPromptUI : MonoBehaviour
{
    [Header("Player Settings")]
    public string limbName; // "LeftLeg", "RightLeg", "LeftArm", "RightArm"
    public KeyCode inputKey;
    
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
    private bool isActive = false;
    private float promptStartTime;
    private float promptEndTime;

    void Start()
    {
        runningController = FindObjectOfType<RunningPhaseController>();
        
        if (runningController != null)
        {
            runningController.OnPromptShown += OnPromptShown;
            runningController.OnPromptExpired += OnPromptExpired;
        }
        
        if (limbLabel != null)
        {
            limbLabel.text = limbName.ToUpper();
        }
        
        if (buttonText != null)
        {
            buttonText.text = inputKey.ToString();
        }
        
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
        if (isActive && Input.GetKeyDown(inputKey))
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
        Invoke(nameof(SetInactive), 0.5f); // miss feedback
    }

    void OnInputPressed()
    {
        if (runningController != null)
        {
            runningController.OnPlayerInput(limbName);
        }
        
        isActive = false;
        ShowSuccess();
        Invoke(nameof(SetInactive), 0.3f); // success feedback
    }

    void SetActive()
    {
        if (promptPanel != null) promptPanel.SetActive(true);
        if (buttonImage != null) buttonImage.color = activeColor;
        if (timerSlider != null) timerSlider.value = 0f;
    }

    void SetInactive()
    {
        if (promptPanel != null) promptPanel.SetActive(true); // dimmed
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
