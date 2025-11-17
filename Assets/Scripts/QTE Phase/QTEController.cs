using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QTEController : MonoBehaviour
{
    [Header("Timing")]
    public float countdownDuration = 3f;
    public float qteWindow = 2f;
    public float syncWindow = 0.3f; // how close presses need to be

    [Header("Success Criteria")]
    public int requiredPlayers = 5;
    public bool requireAllConnected = true;

    private InputManager inputManager;
    public GameObject scientistLaugh;
    public GameObject canvas;
    private bool isQTEActive = false;
    private bool hasStarted = false;
    private float countdownTimer = 0f;
    private float qteStartTime = 0f;
    private float[] buttonPressTimes = new float[5];
    private bool[] hasPressed = new bool[5];
    private List<int> missedPlayers = new List<int>();
    private int pressCount = 0;

    // for UI
    public delegate void OnCountdownEvent(int count);
    public event OnCountdownEvent OnCountdownTick;

    public delegate void OnQTEStartEvent();
    public event OnQTEStartEvent OnQTEStart;

    public delegate void OnPlayerPressEvent(int playerIndex);
    public event OnPlayerPressEvent OnPlayerPress;

    public delegate void OnPlayerMissEvent(int playerIndex);
    public event OnPlayerMissEvent OnPlayerMiss;

    public delegate void OnQTESuccessEvent();
    public event OnQTESuccessEvent OnQTESuccess;

    public delegate void OnQTEFailEvent(List<int> missedPlayerIndices);
    public event OnQTEFailEvent OnQTEFail;

    public QTEDoors DoorScript;

    public Animator frankensteinAnimator;

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

            // check if time has run out
            if (Time.time - qteStartTime >= qteWindow)
            {
                HandleQTETimeout();
            }
        }
    }

    public void BeginQTE()
    {
        isQTEActive = true;
        hasStarted = false;
        countdownTimer = 0f;
        pressCount = 0;
        missedPlayers.Clear();

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
        qteStartTime = Time.time;
        Debug.Log("QTE: PRESS NOW!");
        OnQTEStart?.Invoke();
        DoorScript.ThrowObject(1);
    }

    // CHECKS FOR ALL PLAYERS' BUTTON PRESSES
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

    // QTE TIMEOUT (IF NOT ALL PLAYERS PRESSED THEIR BUTTONS)
    void HandleQTETimeout()
    {
        // GET TOTAL PLAYER COUNT
        int targetCount = GetTargetPlayerCount();

        // IF TOTAL PRESSES IS LESS THAN TOTAL PLAYERS
        if (pressCount < targetCount)
        {
            // ITERATES THROUGH PLAYERS
            for (int i = 0; i < 5; i++)
            {
                if (requireAllConnected && !inputManager.IsControllerConnected(i))
                {
                    continue;
                }
                // ADDS EACH PLAYER TO LIST WHO FAILED TO PRESS BUTTON
                if (!hasPressed[i])
                {
                    missedPlayers.Add(i);
                    OnPlayerMiss?.Invoke(i);
                    Debug.Log($"QTE: Player {i} ({(InputManager.LimbPlayer)i}) MISSED!");
                }
            }
            // TIMES OUT OF QUICK TIME EVENT (FAIL STATE) (?)
            Debug.Log($"QTE: TIMEOUT! {missedPlayers.Count} player(s) missed.");
            EndQTE(false);
        }
    }

    // CHECKS RESULT OF QTE
    void CheckQTEResult()
    {
        // TOTAL # OF PLAYERS CONNECTED
        int targetCount = GetTargetPlayerCount();
        // RETURNS IF NOT ALL PLAYERS PRESSED BUTTON
        if (pressCount < targetCount) return;
        // EARLIEST AND LATEST PRESS TIME
        float firstPressTime = float.MaxValue;
        float lastPressTime = float.MinValue;

        // SETS FIRST AND LAST PRESS TIMES
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
        // CALCULATE TIME BETWEEN FIRST AND LAST PRESS
        float timeDifference = lastPressTime - firstPressTime;
        Debug.Log($"QTE: Time difference = {timeDifference:F3}s (Window: {syncWindow}s)");

        // IF ALL PLAYERS PRESSED WITHIN THE DESIGNATED SYNC WINDOW
        if (timeDifference <= syncWindow)
        {
            // SUCCESS!
            Debug.Log("QTE: SUCCESS! All pressed in sync!");
            EndQTE(true);
        }
        // ELSE IF NOT
        else
        {
            // FAIL - ALL PLAYERS PRESSED, BUT NOT SYNCHRONIZED ENOUGH
            Debug.Log("QTE: FAILED! Presses not synchronized.");

            // ADDS WHOEVER WAS OUT OF SYNC TO MISSED PLAYERS LIST
            for (int i = 0; i < 5; i++)
            {
                if (hasPressed[i])
                {
                    float deviation = Mathf.Abs(buttonPressTimes[i] - firstPressTime);
                    if (deviation > syncWindow)
                    {
                        missedPlayers.Add(i);
                        OnPlayerMiss?.Invoke(i);
                    }
                }
            }
            // ENDS IN FAIL STATE (?)
            EndQTE(false);
        }
    }

    // GETS TARGET NUMBER OF PLAYERS (# OF PLAYERS CONNECTED WITH CONTROLLERS)
    int GetTargetPlayerCount()
    {
        if (requireAllConnected)
        {
            int count = 0;
            for (int i = 0; i < 5; i++)
            {
                if (inputManager.IsControllerConnected(i))
                {
                    count++;
                }
            }
            return count;
        }
        return requiredPlayers;
    }

    // ENDS QUICK TIME EVENT
    void EndQTE(bool success)
    {
        isQTEActive = false;
        hasStarted = false;

        StartCoroutine(EndAfterDelay(success, 1.0f));
    }

    // ENDS QTE AFTER DELAY
    IEnumerator EndAfterDelay(bool success, float delay)
    {
        // DELAY TIME
        yield return new WaitForSeconds(delay);

        // IF QTE IS SUCCESSFULLY COMPLETED
        if (success)
        {
            OnQTESuccess?.Invoke();
        }
        // IF QTE IS NOT SUCCESSFULLY COMPLETED
        else
        {
            //OnQTEFail?.Invoke(missedPlayers);

            scientistLaugh.SetActive(true);
            frankensteinAnimator.enabled = false;
            DoorScript.StopScroll();
            canvas.SetActive(false);
        }
    }

    // RETURNS IF QTE IS CURRENTLY ACTIVE
    public bool IsQTEActive()
    {
        return isQTEActive;
    }

    // RETURNS IF QTE HAS STARTED
    public bool HasStarted()
    {
        return hasStarted;
    }

    // RETURNS COUNTDOWN TIMER
    public float GetCountdownTimer()
    {
        return countdownTimer;
    }

    // RETURNS REMAINING TIME LEFT IN QTE
    public float GetRemainingQTETime()
    {
        if (!hasStarted) return qteWindow;
        return Mathf.Max(0, qteWindow - (Time.time - qteStartTime));
    }

    // RETURNS LIST OF PLAYERS WHO MISSED THE QTE WINDOW
    public List<int> GetMissedPlayers()
    {
        return new List<int>(missedPlayers);
    }
}