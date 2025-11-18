using UnityEngine;
using System.Collections.Generic;

public class RunningPhaseController : MonoBehaviour
{
    [Header("Timing Settings")]
    public float basePromptInterval = 1.5f;
    public float promptWindow = 0.5f;
    public float difficultyIncreaseRate = 0.05f;
    public float scientistRunCooldown = 2.5f;

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

    [Header("Debug")]
    public bool showInputDebug = true;

    private InputManager inputManager;
    public QTEDoors DoorScript;
    public GameStateManager gameManager;
    private float nextPromptTime;
    private float currentPromptInterval;
    public bool isRunning = false;
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

        if (inputManager == null)
        {
            Debug.LogError("InputManager not found! Make sure InputManager exists in the scene.");
            return;
        }

        currentPromptInterval = basePromptInterval;
        currentSpeed = baseSpeed;

        StartRunning();
    }

    void Update()
    {
        if (!isRunning) return;

        currentPromptInterval = Mathf.Max(0.5f, currentPromptInterval - (difficultyIncreaseRate * Time.deltaTime));
        CheckHeadInput();

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

    void FixedUpdate()
    {
        if (!isRunning) return;

        if (scientistRunCooldown > 0)
        {
            scientistRunCooldown -= Time.deltaTime;
        }
        else
        {
            scientistRunCooldown = 2.5f;
            if(currentSpeed > 3)
            {
                DoorScript.ScientistRun(2);
            }
            else if (currentSpeed > 1.5f)
            {
                DoorScript.ScientistRun(1);
            }
            else
            {
                DoorScript.ScientistRun(0);
            }
        }
    }

    void CheckHeadInput()
    {
        if (inputManager == null) return;
        if (Time.time - lastWallSelectionTime < wallSelectionCooldown) return;

        float headInput = inputManager.GetLimbHorizontalAxis(InputManager.LimbPlayer.Head);

        if (showInputDebug && headInput != 0)
        {
            Debug.Log($"Head Input: {headInput:F2}");
        }

        // Left input - previous wall
        if (headInput < -0.5f)
        {
            int oldIndex = selectedWallIndex;
            selectedWallIndex--;
            if (selectedWallIndex < 0)
            {
                selectedWallIndex = numberOfWalls - 1; // wrap around
            }
            lastWallSelectionTime = Time.time;
            OnWallSelectionChange?.Invoke(selectedWallIndex);
            Debug.Log($">>> Head selected LEFT: {oldIndex} → {selectedWallIndex}");
        }
        // Right input - next wall
        else if (headInput > 0.5f)
        {
            int oldIndex = selectedWallIndex;
            selectedWallIndex++;
            if (selectedWallIndex >= numberOfWalls)
            {
                selectedWallIndex = 0; // wrap around
            }
            lastWallSelectionTime = Time.time;
            OnWallSelectionChange?.Invoke(selectedWallIndex);
            Debug.Log($">>> Head selected RIGHT: {oldIndex} → {selectedWallIndex}");
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
                if (showInputDebug)
                {
                    Debug.Log($"Input detected from {limbName} (expected: {currentPromptLimb})");
                }

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

        Debug.Log($"Prompt for {currentPromptLimb}! Press now! (Pattern index: {currentPatternIndex})");
    }

    public void OnPlayerInput(string limbName)
    {
        if (currentPromptLimb != limbName)
        {
            if (showInputDebug)
            {
                Debug.Log($"Wrong limb! {limbName} pressed but {currentPromptLimb} was expected.");
            }
            return;
        }

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
        Debug.Log(">>> RunningPhase: selectedWallIndex reset to 0 (player can now select during running phase)");
        nextPromptTime = Time.time + 1f;
        Debug.Log("Running phase started!");
    }

    public void StopRunning()
    {
        isRunning = false;
        currentPromptLimb = "";
        Debug.Log($">>> RunningPhase STOPPED. Final selection: {selectedWallIndex}");
        if (currentSpeed < 1.0f)
        {
            gameManager.LoseHeart();
            Debug.Log("Running phase failed! Speed must stay over 1.0!");
        }
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

    public string GetCurrentPromptLimb()
    {
        return currentPromptLimb;
    }
}