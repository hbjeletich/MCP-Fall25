using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WallSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public Image[] wallImages;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI selectionText;

    [Header("Colors")]
    public Color unselectedColor = Color.gray;
    public Color selectedColor = Color.cyan;

    [Header("Settings")]
    public bool showInstructions = true;

    private RunningPhaseController runningController;
    private InputManager inputManager;
    private int currentSelection = 0;

    void Start()
    {
        runningController = FindObjectOfType<RunningPhaseController>();
        inputManager = InputManager.Instance;

        if (runningController != null)
        {
            runningController.OnWallSelectionChange += OnWallSelectionChanged;
            currentSelection = runningController.GetSelectedWallIndex();
        }

        SetupInstructions();
        UpdateSelection(currentSelection);
    }

    void OnDestroy()
    {
        if (runningController != null)
        {
            runningController.OnWallSelectionChange -= OnWallSelectionChanged;
        }
    }

    void SetupInstructions()
    {
        if (!showInstructions || instructionText == null) return;

        if (inputManager != null && inputManager.inputMode == InputManager.InputMode.Debug)
        {
            instructionText.text = "HEAD: Use [ and ] to select wall";
        }
        else
        {
            instructionText.text = "HEAD: Move stick left/right to select wall";
        }
    }

    void OnWallSelectionChanged(int wallIndex)
    {
        UpdateSelection(wallIndex);
    }

    void UpdateSelection(int selectedIndex)
    {
        currentSelection = selectedIndex;

        if (selectionText != null)
        {
            selectionText.text = $"Selected: Wall {selectedIndex + 1}";
        }

        for (int i = 0; i < wallImages.Length && i < 3; i++)
        {
            if (i == selectedIndex)
            {
                wallImages[i].transform.localScale = Vector3.one * 1.2f;
                if (wallImages != null && i < wallImages.Length && wallImages[i] != null)
                {
                    wallImages[i].color = selectedColor;
                }
            }
            else
            {
                wallImages[i].transform.localScale = Vector3.one;
                if (wallImages != null && i < wallImages.Length && wallImages[i] != null)
                {
                    wallImages[i].color = unselectedColor;
                }

            }
        }
    }

    public void SetVisible(bool visible)
    {
        foreach(Image wallImage in wallImages)
        {
            wallImage.gameObject.SetActive(visible);
        }

        instructionText.gameObject.SetActive(visible);
        selectionText.gameObject.SetActive(visible);
    }
}