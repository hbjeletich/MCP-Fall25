using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerTestSquare : MonoBehaviour
{
    [Header("Settings")]
    public InputManager.LimbPlayer assignedController;
    private Image squareImage;
    private RectTransform squareTransform;
    
    [Header("Visual Settings")]
    public Color defaultColor = Color.white;
    public Color pressedColor = Color.green;
    public float rotationSpeed = 100f;
    
    private InputManager inputManager;
    private float currentRotation = 0f;
    private Color currentColor;

    void Start()
    {
        inputManager = InputManager.Instance;

        squareImage = GetComponent<Image>();
        
        if (squareImage != null)
        {
            currentColor = defaultColor;
            squareImage.color = currentColor;
        }
        
        if (squareTransform == null)
        {
            squareTransform = GetComponent<RectTransform>();
        }
    }

    void Update()
    {
        if (inputManager == null) return;
        
        if (!inputManager.IsControllerConnected((int)assignedController))
        {
            return;
        }
        
        float input = inputManager.GetLimbHorizontalAxis(assignedController);
        if (Mathf.Abs(input) > 0.1f)
        {
            currentRotation += input * rotationSpeed * Time.deltaTime;
            if (squareTransform != null)
            {
                squareTransform.localRotation = Quaternion.Euler(0, 0, currentRotation);
            }
        }
        
        if (inputManager.GetLimbLockButtonDown(assignedController))
        {
            currentColor = (currentColor == defaultColor) ? pressedColor : defaultColor;
            
            if (squareImage != null)
            {
                squareImage.color = currentColor;
            }
        }
    }
}
