using UnityEngine;
using System.Collections.Generic;

public class RunningPhaseController : MonoBehaviour
{
    [Header("Timing Settings")]
    public float basePromptInterval = 1.5f;
    public float promptWindow = 0.5f;
    public float difficultyIncreaseRate = 0.05f;

    [Header("Pattern")]
    public List<string> limbPattern = new List<string> { "LeftLeg", "RightLeg", "LeftArm", "RightArm" };
    public int currentPatternIndex = 0;

    [Header("Speed")]
    public float baseSpeed = 5f;
    public float currentSpeed = 5f;
    public float speedBonusMultiplier = 1.2f;
    public float speedPenaltyMultiplier = 0.8f;

    private InputManager inputManager;
    private float nextPromptTime;
    private float currentPromptInterval;
    private bool isRunning = false;
    private string currentPromptLimb = "";
    private float currentPromptStartTime;

    public delegate void OnPromptEvent(string limbName, float windowEndTime);
    public event OnPromptEvent OnPromptShown;

    public delegate void OnPromptEndEvent(string limbName);
    public event OnPromptEndEvent OnPromptExpired;

    void Start()
    {
        inputManager = InputManager.Instance;
        currentPromptInterval = basePromptInterval;
        currentSpeed = baseSpeed;

        StartRunning();
    }

    void Update()
    {
        if (!isRunning) return;

        currentPromptInterval = Mathf.Max(0.5f, currentPromptInterval - (difficultyIncreaseRate * Time.deltaTime));
        if (currentPromptLimb != "")
        {
            CheckPlayerInputs();
        }

        if (currentPromptLimb != "" && Time.time >= currentPromptStartTime + promptWindow)
        {
            HandleMissedPrompt();
        }

        if (Time.time >= nextPromptTime && currentPromptLimb == "")
        {
            ShowNextPrompt();
        }
    }

    void CheckPlayerInputs()
    {
        if (inputManager == null) return;

        for (int i = 0; i < 4; i++)
        {
            InputManager.LimbPlayer limb = (InputManager.LimbPlayer)i;
            string limbName = limb.ToString();

            if (inputManager.GetLimbLockButtonDown(limb))
            {
                OnPlayerInput(limbName);
                return;
            }
        }
    }

    void ShowNextPrompt()
    {
        currentPromptLimb = limbPattern[currentPatternIndex];
        currentPromptStartTime = Time.time;

        OnPromptShown?.Invoke(currentPromptLimb, currentPromptStartTime + promptWindow);

        Debug.Log($"Prompt for {currentPromptLimb}! Press now!");
    }

    public void OnPlayerInput(string limbName)
    {
        if (currentPromptLimb != limbName) return;

        float timingDifference = Time.time - currentPromptStartTime;

        if (timingDifference <= promptWindow)
        {
            float accuracy = 1f - (timingDifference / promptWindow);
            HandleSuccessfulInput(accuracy);
        }
    }

    void HandleSuccessfulInput(float accuracy)
    {
        Debug.Log($"{currentPromptLimb} pressed! Accuracy: {accuracy:F2}");

        if (accuracy > 0.7f)
        {
            currentSpeed = Mathf.Min(baseSpeed * 2f, currentSpeed * speedBonusMultiplier);
            Debug.Log("Speed boost!");
        }

        AdvancePattern();
    }

    void HandleMissedPrompt()
    {
        Debug.Log($"{currentPromptLimb} MISSED!");

        OnPromptExpired?.Invoke(currentPromptLimb);

        currentSpeed *= speedPenaltyMultiplier;
        Debug.Log("Speed penalty!");

        AdvancePattern();
    }

    void AdvancePattern()
    {
        currentPatternIndex = (currentPatternIndex + 1) % limbPattern.Count;
        currentPromptLimb = "";
        nextPromptTime = Time.time + currentPromptInterval;
    }

    public void StartRunning()
    {
        isRunning = true;
        currentPromptInterval = basePromptInterval;
        currentSpeed = baseSpeed;
        currentPatternIndex = 0;
        currentPromptLimb = "";
        nextPromptTime = Time.time + 1f;
        Debug.Log("Running phase started!");
    }

    public void StopRunning()
    {
        isRunning = false;
        currentPromptLimb = "";
        Debug.Log("Running phase stopped!");
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public bool IsRunning()
    {
        return isRunning;
    }
}