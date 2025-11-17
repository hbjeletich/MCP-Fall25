using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HidingPhaseUI : MonoBehaviour
{
    [Header("References")]
    public HidingPhaseController hidingController;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI wallProgressText;
    
    [Header("Limb Lock Indicators")]
    public Image leftArmIndicator;
    public Image rightArmIndicator;
    public Image leftLegIndicator;
    public Image rightLegIndicator;
    public Image headIndicator;
    
    [Header("Colors")]
    public Color unlockedColor = Color.red;
    public Color lockedColor = Color.green;
    
    [Header("Feedback")]
    public GameObject successFeedback;
    public GameObject failFeedback;
    public float feedbackDuration = 1f;

    void Start()
    {
        if (hidingController != null)
        {
            hidingController.OnHidingSuccess += ShowSuccess;
            hidingController.OnHidingFail += ShowFail;
        }
        
        if (successFeedback != null) successFeedback.SetActive(false);
        if (failFeedback != null) failFeedback.SetActive(false);
    }

    void OnDestroy()
    {
        if (hidingController != null)
        {
            hidingController.OnHidingSuccess -= ShowSuccess;
            hidingController.OnHidingFail -= ShowFail;
        }
    }

    void Update()
    {
        if (hidingController == null) return;
        
        if (timerText != null && hidingController.IsHiding())
        {
            float timeRemaining = hidingController.GetRemainingTime();
            timerText.text = $"Time: {timeRemaining:F1}s";
        }
        
        if (wallProgressText != null)
        {
            int current = hidingController.GetCurrentWallIndex() + 1;
            int total = hidingController.GetTotalWalls();
            wallProgressText.text = $"Wall {current}/{total}";
        }
        
        UpdateLimbIndicators();
    }

    void UpdateLimbIndicators()
    {
        if (hidingController == null) return;
        
        foreach (var limb in hidingController.limbs)
        {
            Image indicator = GetIndicatorForLimb(limb.limbName);
            if (indicator != null)
            {
                indicator.color = limb.IsLocked() ? lockedColor : unlockedColor;
            }
        }
    }

    private Image GetIndicatorForLimb(string limbName)
    {
        switch (limbName)
        {
            case "LeftArm": return leftArmIndicator;
            case "RightArm": return rightArmIndicator;
            case "LeftLeg": return leftLegIndicator;
            case "RightLeg": return rightLegIndicator;
            case "Head": return headIndicator;
            default: return null;
        }
    }

    void ShowSuccess()
    {
        if (successFeedback != null)
        {
            successFeedback.SetActive(true);
            Invoke(nameof(HideSuccess), feedbackDuration);
        }
    }

    void ShowFail()
    {
        if (failFeedback != null)
        {
            failFeedback.SetActive(true);
            Invoke(nameof(HideFail), feedbackDuration);
        }
    }

    void HideSuccess()
    {
        if (successFeedback != null) successFeedback.SetActive(false);
    }

    void HideFail()
    {
        if (failFeedback != null) failFeedback.SetActive(false);
    }
}