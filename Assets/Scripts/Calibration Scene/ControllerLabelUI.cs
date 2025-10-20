using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ControllerLabelUI : MonoBehaviour
{
    [Header("Settings")]
    public InputManager.LimbPlayer assignedController;
    private TextMeshProUGUI labelText;
    
    [Header("Display")]
    public bool showControllerName = true;
    public bool showInputValues = true;
    
    private InputManager inputManager;

    void Start()
    {
        labelText = GetComponent<TextMeshProUGUI>();
        inputManager = InputManager.Instance;
        UpdateLabel();
    }

    void Update()
    {
        UpdateLabel();
    }

    void UpdateLabel()
    {
        if (labelText == null || inputManager == null) return;
        
        string label = "";
        
        if (showControllerName)
        {
            label = assignedController.ToString();
        }
        
        bool isConnected = inputManager.IsControllerConnected((int)assignedController);
        label += isConnected ? "\nConnected" : "\nDisconnected";
        
        if (showInputValues && isConnected)
        {
            float axis = inputManager.GetLimbHorizontalAxis(assignedController);
            label += $"\nAxis: {axis:F2}";
        }
        
        labelText.text = label;
    }
}
