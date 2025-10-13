using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QTEController : MonoBehaviour
{
    [Header("Timing")]
    public float countdownDuration = 3f;
    public float syncWindow = 0.3f; // how close
    
    [Header("Success Criteria")]
    public int requiredPlayers = 5;
    public bool requireAllConnected = true;
    
    private InputManager inputManager;
    private bool isQTEActive = false;
    private bool hasStarted = false;
    private float countdownTimer = 0f;
    private float[] buttonPressTimes = new float[5];
    private bool[] hasPressed = new bool[5];
    private int pressCount = 0;
    
    // for UI
    public delegate void OnCountdownEvent(int count);
    public event OnCountdownEvent OnCountdownTick;
    
    public delegate void OnQTEStartEvent();
    public event OnQTEStartEvent OnQTEStart;
    
    public delegate void OnPlayerPressEvent(int playerIndex);
    public event OnPlayerPressEvent OnPlayerPress;

    public delegate void OnQTESuccessEvent();
    public event OnQTESuccessEvent OnQTESuccess;
    public delegate void OnQTEFailEvent();
    public event OnQTEFailEvent OnQTEFail;

    void Start()
    {
        inputManager = InputManager.Instance;
    }

    void Update()
    {
        if (!isQTEActive) return;
        
        // countdown
        if (!hasStarted)
        {
            countdownTimer += Time.deltaTime;
            
            int remainingCount = Mathf.CeilToInt(countdownDuration - countdownTimer);
            
            if (countdownTimer >= countdownDuration)
            {
                StartQTE();
            }
        }
        else
        {
            CheckPlayerInputs();
        }
    }

    public void BeginQTE()
    {
        isQTEActive = true;
        hasStarted = false;
        countdownTimer = 0f;
        pressCount = 0;
        
        for (int i = 0; i < 5; i++)
        {
            buttonPressTimes[i] = -999f;
            hasPressed[i] = false;
        }
        
        Debug.Log("QTE: Countdown started!");
        
        OnCountdownTick?.Invoke(3);
    }

    void StartQTE()
    {
        hasStarted = true;
        Debug.Log("QTE: PRESS NOW!");
        OnQTEStart?.Invoke();
    }

    void CheckPlayerInputs()
    {
        if (inputManager == null) return;
        
        for (int i = 0; i < 5; i++)
        {
            if (hasPressed[i]) continue;
            
            if (requireAllConnected && !inputManager.IsControllerConnected(i))
            {
                continue;
            }
            
            InputManager.LimbPlayer limb = (InputManager.LimbPlayer)i;
            if (inputManager.GetLimbLockButtonDown(limb))
            {
                hasPressed[i] = true;
                buttonPressTimes[i] = Time.time;
                pressCount++;
                
                Debug.Log($"QTE: Player {i} ({limb}) pressed! Total: {pressCount}");
                OnPlayerPress?.Invoke(i);
                
                CheckQTEResult();
            }
        }
    }

    void CheckQTEResult()
    {
        int targetCount = requiredPlayers;
        if (requireAllConnected)
        {
            targetCount = 0;
            for (int i = 0; i < 5; i++)
            {
                if (inputManager.IsControllerConnected(i))
                {
                    targetCount++;
                }
            }
        }
        
        if (pressCount < targetCount) return;
        
        float firstPressTime = float.MaxValue;
        float lastPressTime = float.MinValue;
        
        for (int i = 0; i < 5; i++)
        {
            if (hasPressed[i])
            {
                if (buttonPressTimes[i] < firstPressTime)
                    firstPressTime = buttonPressTimes[i];
                if (buttonPressTimes[i] > lastPressTime)
                    lastPressTime = buttonPressTimes[i];
            }
        }
        
        float timeDifference = lastPressTime - firstPressTime;
        
        Debug.Log($"QTE: Time difference = {timeDifference:F3}s (Window: {syncWindow}s)");
        
        if (timeDifference <= syncWindow)
        {
            // SUCCESS!
            Debug.Log("QTE: SUCCESS! All pressed in sync!");
            EndQTE(true);
        }
        else
        {
            // FAIL
            Debug.Log("QTE: FAILED! Presses not synchronized.");
            EndQTE(false);
        }
    }

    void EndQTE(bool success)
    {
        isQTEActive = false;
        hasStarted = false;

        StartCoroutine(EndAfterDelay(success, 1.0f));
    }

    IEnumerator EndAfterDelay(bool success, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (success)
        {
            OnQTESuccess?.Invoke();
        }
        else
        {
            OnQTEFail?.Invoke();
        }
    }

    public bool IsQTEActive()
    {
        return isQTEActive;
    }

    public bool HasStarted()
    {
        return hasStarted;
    }

    public float GetCountdownTimer()
    {
        return countdownTimer;
    }
}