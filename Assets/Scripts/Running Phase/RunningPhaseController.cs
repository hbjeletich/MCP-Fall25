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

    [Header("Wall Selection")]
    public int numberOfWalls = 3;
    public int selectedWallIndex = 0;
    public float wallSelectionCooldown = 0.2f;

    private InputManager inputManager;
    private float nextPromptTime;
    private float currentPromptInterval;
    private bool isRunning = false;
    private string currentPromptLimb = "";
    private float currentPromptStartTime;
    private float lastWallSelectionTime = 0f;

    public delegate void OnPromptEvent(string limbName, float windowEndTime);
    public event OnPromptEvent OnPromptShown;

    public delegate void OnPromptEndEvent(string limbName);
    public event OnPromptEndEvent OnPromptExpired;

    public delegate void OnWallSelectionChangeEvent(int wallIndex);
    public event OnWallSelectionChangeEvent OnWallSelectionChange;

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

        // Check head player input for wall selection
        CheckHeadInput();

        // Check limb inputs for rhythm game
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

    void CheckHeadInput()
    {
        if (inputManager == null) return;
        if (Time.time - lastWallSelectionTime < wallSelectionCooldown) return;

        float headInput = inputManager.GetLimbHorizontalAxis(InputManager.LimbPlayer.Head);

        // Left input - previous wall
        if (headInput < -0.5f)
        {
            selectedWallIndex--;
            if (selectedWallIndex < 0)
            {
                selectedWallIndex = numberOfWalls - 1; // wrap around
            }
            lastWallSelectionTime = Time.time;
            OnWallSelectionChange?.Invoke(selectedWallIndex);
            Debug.Log($"Head selected wall: {selectedWallIndex}");
        }
        // Right input - next wall
        else if (headInput > 0.5f)
        {
            selectedWallIndex++;
            if (selectedWallIndex >= numberOfWalls)
            {
                selectedWallIndex = 0; // wrap around
            }
            lastWallSelectionTime = Time.time;
            OnWallSelectionChange?.Invoke(selectedWallIndex);
            Debug.Log($"Head selected wall: {selectedWallIndex}");
        }
    }

    void CheckPlayerInputs()
    {
        if (inputManager == null) return;

        // Check each limb to see if it pressed (0-3, skip head)
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
        selectedWallIndex = 0;
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

    public int GetSelectedWallIndex()
    {
        return selectedWallIndex;
    }
}