using UnityEngine;
using TMPro;

public class RunningSpeedDisplay : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI speedText;
    public RunningPhaseController runningController;
    
    void Update()
    {
        if (runningController != null && speedText != null)
        {
            float speed = runningController.GetCurrentSpeed();
            speedText.text = $"Speed: {speed:F1}";
        }
    }
}
