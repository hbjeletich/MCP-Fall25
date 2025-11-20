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

    [Header("Player Indicators")]
    public Image[] playerIndicators; // 5 indicators
    public Sprite beforeGameSprite;
    public Sprite notPressedSprite;
    public Sprite pressedSprite;

    void Start()
    {
        if (qteController != null)
        {
            qteController.OnQTEStart += OnQTEStart;
            qteController.OnPlayerPress += OnPlayerPressed;
            qteController.OnPlayerMiss += OnPlayerMissed;
            qteController.OnQTESuccess += OnQTESuccess;
            qteController.OnQTEFail += OnQTEFail;
        }
        
        ResetUI();
    }

    void OnDestroy()
    {
        if (qteController != null)
        {
            qteController.OnQTEStart -= OnQTEStart;
            qteController.OnPlayerPress -= OnPlayerPressed;
            qteController.OnPlayerMiss -= OnPlayerMissed;
            qteController.OnQTESuccess -= OnQTESuccess;
            qteController.OnQTEFail -= OnQTEFail;
        }
    }

    void Update()
    {
        if (qteController == null) return;
        
        if (qteController.IsQTEActive() && !qteController.HasStarted())
        {
            ResetPlayerIndicators();
            UpdateCountdown();
        }

        // reset UI when QTE is not active
        else if (!qteController.IsQTEActive())
        {
            ClearSprites();
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
            countdownImage.enabled = true;
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
            instructionImage.enabled = true;
            instructionImage.sprite = getReadySprite;
        }
    }

    void OnQTEStart()
    {
        //Debug.Log("QTEUI: QTE Started!");

        if (countdownImage != null)
        {
            countdownImage.enabled = true;
            countdownImage.sprite = goSprite;
        }

        if (instructionImage != null)
        {
            instructionImage.enabled = true;
            instructionImage.sprite = pressNowSprite;
        }

        ResetPlayerIndicators();
    }

    void OnPlayerPressed(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerIndicators.Length) return;
        if (playerIndicators[playerIndex] == null) return;
        
        playerIndicators[playerIndex].sprite = pressedSprite;
    }

    void OnPlayerMissed(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerIndicators.Length) return;
        if (playerIndicators[playerIndex] == null) return;
        
        playerIndicators[playerIndex].sprite = notPressedSprite;
    }

    void ResetPlayerIndicators()
    {
        for (int i = 0; i < playerIndicators.Length; i++)
        {
            if (playerIndicators[i] != null)
            {
                playerIndicators[i].sprite = beforeGameSprite;
            }
        }
    }

    void OnQTESuccess()
    {
        // show success feedback briefly before clearing
        StartCoroutine(ClearUIAfterDelay(0.5f));
    }

    void OnQTEFail(List<int> missedPlayers)
    {
        // show fail feedback briefly before clearing
        StartCoroutine(ClearUIAfterDelay(0.5f));
    }

    IEnumerator ClearUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearSprites();
    }

    void ClearSprites()
    {
        if (countdownImage != null)
        {
            countdownImage.sprite = null;
            countdownImage.enabled = false;
        }
        
        if (instructionImage != null)
        {
            instructionImage.sprite = null;
            instructionImage.enabled = false;
        }
    }

    void ResetUI()
    {
        ClearSprites();
        ResetPlayerIndicators();
    }
}