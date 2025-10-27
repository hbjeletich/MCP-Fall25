using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LimbVisualController : MonoBehaviour
{
    [Header("Settings")]
    public InputManager.LimbPlayer assignedLimb;
    
    [Header("Visual Settings")]
    public float rotationSpeed = 100f;
    
    [Header("UI References")]
    private Image limbImage;
    private RectTransform limbTransform;
    
    private InputManager inputManager;
    private SkinManager skinManager;
    private BodySelectionPhase bodySelectionPhase;
    private float currentRotation = 0f;

    void Start()
    {
        inputManager = InputManager.Instance;
        skinManager = SkinManager.Instance;
        bodySelectionPhase = FindObjectOfType<BodySelectionPhase>();
        
        limbImage = GetComponent<Image>();
        if (limbImage == null)
        {
            limbImage = gameObject.AddComponent<Image>();
        }
        
        limbTransform = GetComponent<RectTransform>();
        
        LoadAndDisplaySkin();
    }

    void Update()
    {
        if (inputManager == null) return;
        
        if (!inputManager.IsControllerConnected((int)assignedLimb))
        {
            return;
        }
        
        HandleRotationInput();
    }

    void LoadAndDisplaySkin()
    {
        if (skinManager == null)
        {
            Debug.LogWarning($"SkinManager not found! Cannot load skin for {assignedLimb}");
            return;
        }
        
        int savedSkinIndex = skinManager.LoadSkin(assignedLimb);
        
        Sprite skinSprite = skinManager.GetSkinSprite(assignedLimb, savedSkinIndex);
        
        if (skinSprite != null && limbImage != null)
        {
            limbImage.sprite = skinSprite;
            Debug.Log($"{assignedLimb} loaded skin {savedSkinIndex}");
        }
        else
        {
            Debug.LogWarning($"Could not load skin sprite for {assignedLimb}");
        }
    }

    void HandleRotationInput()
    {
        float input = inputManager.GetLimbHorizontalAxis(assignedLimb);
        
        if (Mathf.Abs(input) > 0.1f)
        {
            currentRotation += input * rotationSpeed * Time.deltaTime;
            
            if (limbTransform != null)
            {
                limbTransform.localRotation = Quaternion.Euler(0, 0, currentRotation);
            }
        }
    }

    public void UpdateSkin()
    {
        LoadAndDisplaySkin();
    }

    public void UpdateSkin(Sprite newSkin)
    {
        if (limbImage != null && newSkin != null)
        {
            limbImage.sprite = newSkin;
        }
    }

    public float GetCurrentRotation()
    {
        return currentRotation;
    }

    public void ResetRotation()
    {
        currentRotation = 0f;
        if (limbTransform != null)
        {
            limbTransform.localRotation = Quaternion.identity;
        }
    }
}