using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QTEUI : MonoBehaviour
{
    [Header("References")]
    public QTEController qteController;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI instructionText;
    public string countdownInstruction = "Get Ready!";
    public string pressInstruction = "PRESS NOW!";
    
    [Header("Player Indicators")]
    public Image[] playerIndicators; // 5 indicators
    public Color notPressedColor = Color.red;
    public Color pressedColor = Color.green;

    void Start()
    {
        if (qteController != null)
        {
            qteController.OnQTEStart += OnQTEStart;
            qteController.OnPlayerPress += OnPlayerPressed;
        }
        
        ResetPlayerIndicators();
    }

    void OnDestroy()
    {
        if (qteController != null)
        {
            qteController.OnQTEStart -= OnQTEStart;
            qteController.OnPlayerPress -= OnPlayerPressed;
        }
    }

    void Update()
    {
        if (qteController == null) return;
        
        if (qteController.IsQTEActive() && !qteController.HasStarted())
        {
            UpdateCountdown();
        }
    }

    void UpdateCountdown()
    {
        if (countdownText == null) return;
        
        float timer = qteController.GetCountdownTimer();
        float countdownDuration = 3f;
        
        int remainingCount = Mathf.CeilToInt(countdownDuration - timer);
        remainingCount = Mathf.Max(0, remainingCount);
        
        if (remainingCount > 0)
        {
            countdownText.text = remainingCount.ToString();
        }
        else
        {
            countdownText.text = "";
        }
        
        if (instructionText != null)
        {
            instructionText.text = countdownInstruction;
        }
    }

    void OnQTEStart()
    {
        Debug.Log("QTEUI: QTE Started!");
        
        if (countdownText != null)
        {
            countdownText.text = "GO!";
        }
        
        if (instructionText != null)
        {
            instructionText.text = pressInstruction;
        }
        
        ResetPlayerIndicators();
    }

    void OnPlayerPressed(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerIndicators.Length) return;
        if (playerIndicators[playerIndex] == null) return;
        
        playerIndicators[playerIndex].color = pressedColor;
    }

    void ResetPlayerIndicators()
    {
        for (int i = 0; i < playerIndicators.Length; i++)
        {
            if (playerIndicators[i] != null)
            {
                playerIndicators[i].color = notPressedColor;
            }
        }
    }

    void OnQTESuccess()
    {
        if (countdownText != null)
        {
            countdownText.text = "SUCCESS";
        }
    }

    void OnQTEFail()
    {
        if (countdownText != null)
        {
            countdownText.text = "FAIL";
        }
    }
}