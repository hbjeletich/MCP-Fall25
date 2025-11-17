using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
//using static System.Net.Mime.MediaTypeNames;

public class QTEUI : MonoBehaviour
{
    [Header("References")]
    public QTEController qteController;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI instructionText;
    public string countdownInstruction = "Get Ready!";
    public string pressInstruction = "PRESS NOW!";
    //sloane adding new code below this point
    public Image countdownImage;
    public Image instructionImage;

    [Header("Countdown Sprites")]
    public Sprite countdown3Sprite;
    public Sprite countdown2Sprite;
    public Sprite countdown1Sprite;
    public Sprite goSprite;

    [Header("Instruction Sprites")]
    public Sprite getReadySprite;
    public Sprite pressNowSprite;
    //sloane code finished here

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
        float timer = qteController.GetCountdownTimer();
        float countdownDuration = 3f;

        int remainingCount = Mathf.CeilToInt(countdownDuration - timer);
        remainingCount = Mathf.Max(0, remainingCount);

        if (countdownImage != null)
        {
            switch (remainingCount)
            {
                case 3:
                    countdownImage.sprite = countdown3Sprite;
                    break;
                case 2:
                    countdownImage.sprite = countdown2Sprite;
                    break;
                case 1:
                    countdownImage.sprite = countdown1Sprite;
                    break;
                default:
                    countdownImage.sprite = null;
                    break;
            }
        }

        if (instructionImage != null)
        {
            instructionImage.sprite = getReadySprite;
        }
    }

    void OnQTEStart()
    {
      
        //Debug.Log("QTEUI: QTE Started!");

        if (countdownImage != null)
        {
            countdownImage.sprite = goSprite;
        }

        if (instructionImage != null)
        {
            instructionImage.sprite = pressNowSprite;
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
        if (countdownImage != null)
        {
            // Optionally show a success image if you have one
            countdownImage.sprite = null; // Or a successSprite
        }
    }

    void OnQTEFail()
    {
        if (countdownImage != null)
        {
            // Optionally show a fail image if you have one
            countdownImage.sprite = null; // Or a failSprite
        }
    }
}