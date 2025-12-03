using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SprintStatsScreen : MonoBehaviour
{
    [Header("Stats Display")]
    public GameObject statsSection;
    public TextMeshProUGUI yourScoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI instructionText;
    
    [Header("Accuracy Display")]
    public Transform accuracyListParent; 
    public GameObject accuracyLinePrefab;
    public float revealDelay = 0.45f;
    public float scalePopAmount = 1.25f;
    public float scalePopTime = 0.15f;

    [Header("Ready To Continue UI")]
    public Image[] readyIndicators;
    public float readyIndicatorDisabledAlpha = 0.3f;
    public float readyIndicatorEnabledAlpha = 1.0f;
    
    private InputManager inputManager;
    private bool[] playerReadyStates = new bool[5];
    private float[] lastReadyToggleTime = new float[5];
    private const float READY_TOGGLE_COOLDOWN = 0.3f;
    private bool isActive = false;
    
    void Start()
    {
        statsSection.SetActive(false);
        
        inputManager = InputManager.Instance;
    }
    
    void Update()
    {
        if (!isActive) return;
        
        HandleReadyUpInput();
        UpdateReadyIndicators();
        UpdateInstructionText();
    }
    
    public void ShowStats(int currentSprint, int highestSprint)
    {
        statsSection.SetActive(true);
        isActive = true;
        
        for (int i = 0; i < playerReadyStates.Length; i++)
        {
            playerReadyStates[i] = false;
            lastReadyToggleTime[i] = 0f;
        }
        
        if (readyIndicators != null)
        {
            foreach (var indicator in readyIndicators)
            {
                if (indicator != null)
                {
                    SetColorAlpha(indicator, readyIndicatorDisabledAlpha);
                }
            }
        }
        
        if (yourScoreText != null)
        {
            yourScoreText.text = $"YOUR SCORE: SPRINT {currentSprint}";
        }
        
        if (highScoreText != null)
        {
            if (currentSprint >= highestSprint)
            {
                highScoreText.text = $"NEW HIGH SCORE!";
            }
            else
            {
                highScoreText.text = $"HIGH SCORE: SPRINT {highestSprint}";
            }
        }

        UpdatePlayerAccuracyDisplay();
        
        Debug.Log($"Sprint Stats Screen shown - Current: {currentSprint}, High: {highestSprint}");
    }
    
    void HandleReadyUpInput()
    {
        if (inputManager == null) return;

        for (int i = 0; i < 5; i++)
        {
            if (!inputManager.IsControllerConnected(i)) continue;

            InputManager.LimbPlayer limb = (InputManager.LimbPlayer)i;
            
            if (inputManager.GetLimbStartButtonDown(limb))
            {
                if (Time.time - lastReadyToggleTime[i] > READY_TOGGLE_COOLDOWN)
                {
                    playerReadyStates[i] = !playerReadyStates[i];
                    lastReadyToggleTime[i] = Time.time;
                    
                    Debug.Log($"Player {i} ({limb}) ready state toggled: {playerReadyStates[i]}");
                }
            }
        }

        if (AreAllPlayersReady())
        {
            ContinueToMenu();
        }
    }
    
    bool AreAllPlayersReady()
    {
        int connectedReadyCount = 0;
        int totalConnected = 0;

        for (int i = 0; i < 5; i++)
        {
            if (inputManager.IsControllerConnected(i))
            {
                totalConnected++;
                if (playerReadyStates[i])
                {
                    connectedReadyCount++;
                }
            }
        }

        return totalConnected > 0 && connectedReadyCount == totalConnected;
    }
    
    void UpdateReadyIndicators()
    {
        if (readyIndicators == null) return;

        for (int i = 0; i < readyIndicators.Length && i < 5; i++)
        {
            if (readyIndicators[i] != null)
            {
                bool isConnected = inputManager != null && inputManager.IsControllerConnected(i);
                
                if (!isConnected)
                {
                    SetColorAlpha(readyIndicators[i], readyIndicatorDisabledAlpha);
                }
                else if (playerReadyStates[i])
                {
                    SetColorAlpha(readyIndicators[i], readyIndicatorEnabledAlpha);
                }
                else
                {
                    SetColorAlpha(readyIndicators[i], readyIndicatorDisabledAlpha);
                }
            }
        }
    }
    
    void UpdateInstructionText()
    {
        if (instructionText == null) return;
        
        int readyCount = 0;
        int totalConnected = 0;
        
        for (int i = 0; i < 5; i++)
        {
            if (inputManager.IsControllerConnected(i))
            {
                totalConnected++;
                if (playerReadyStates[i])
                {
                    readyCount++;
                }
            }
        }
        
        if (readyCount < totalConnected)
        {
            instructionText.text = $"Press START (Y) to continue\nReady: {readyCount}/{totalConnected}";
        }
        else
        {
            instructionText.text = "Returning to menu...";
        }
    }
    
    void SetColorAlpha(Image img, float alpha)
    {
        if (img != null)
        {
            Color color = img.color;
            color.a = alpha;
            img.color = color;
        }
    }
    
    void ContinueToMenu()
    {
        Debug.Log("All players ready! Returning to menu...");
        
        isActive = false;
        statsSection.SetActive(false);
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartingScene");
    }

    void UpdatePlayerAccuracyDisplay()
    {
        if (accuracyListParent == null || accuracyLinePrefab == null) return;

        foreach (Transform child in accuracyListParent)
            Destroy(child.gameObject);

        string[] limbNames = new string[]
        {
            "Left Arm",
            "Right Arm",
            "Left Leg",
            "Right Leg",
            "Head"
        };

        List<(string name, float value)> accuracyData = new List<(string, float)>();

        for (int i = 0; i < 5; i++)
        {
            float accuracy = PlayerStatsManager.Instance.GetFinalPlayerAccuracy(i);
            accuracyData.Add((limbNames[i], accuracy));
        }

        accuracyData.Sort((a, b) => a.value.CompareTo(b.value));

        StartCoroutine(RevealAccuracyLines(accuracyData));
    }

    IEnumerator RevealAccuracyLines(List<(string name, float value)> data)
    {
        foreach (var entry in data)
        {
            GameObject line = Instantiate(accuracyLinePrefab, accuracyListParent);
            line.transform.localScale = Vector3.zero;

            TextMeshProUGUI tmp = line.GetComponent<TextMeshProUGUI>();
            int percent = Mathf.RoundToInt(entry.value * 100f);
            tmp.text = $"{entry.name}: {percent}%";

            Vector3 targetScale = Vector3.one;
            Vector3 popScale = Vector3.one * scalePopAmount;

            float t = 0f;
            while (t < scalePopTime)
            {
                t += Time.deltaTime;
                float s = t / scalePopTime;
                line.transform.localScale = Vector3.Lerp(Vector3.zero, popScale, s);
                yield return null;
            }

            t = 0f;
            while (t < scalePopTime)
            {
                t += Time.deltaTime;
                float s = t / scalePopTime;
                line.transform.localScale = Vector3.Lerp(popScale, targetScale, s);
                yield return null;
            }

            yield return new WaitForSeconds(revealDelay);
        }
    }

}