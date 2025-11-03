using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class PlayerLimbController : MonoBehaviour
{
    [Header("Limb Settings")]
    public string limbName; // "LeftArm", "RightArm", "LeftLeg", "RightLeg", "Head"
    public InputManager.LimbPlayer limbPlayer;
    public float rotationSpeed = 50f;
    public float minAngle = -45f;
    public float maxAngle = 45f;

    
    private bool isLocked = false;
    private bool hidingModeEnabled = false;
    private float currentAngle = 0f;

    private InputManager inputManager;
    private SpriteResolver spriteResolver;

    void Start()
    {
        inputManager = InputManager.Instance;
        
        spriteResolver = GetComponent<SpriteResolver>();
        
        LoadSavedSkin();
    }

    void Update()
    {
        if (inputManager == null) return;
        
        if (!hidingModeEnabled) return;
        
        if (!isLocked)
        {
            float input = inputManager.GetLimbHorizontalAxis(limbPlayer);
            
            if (Mathf.Abs(input) > 0.1f) // Deadzone
            {
                currentAngle += input * rotationSpeed * Time.deltaTime;
                currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
                transform.localRotation = Quaternion.Euler(0, 0, currentAngle);
            }
        }

        if (inputManager.GetLimbLockButtonDown(limbPlayer) && !isLocked)
        {
            LockLimb();
        }
    }

    void LoadSavedSkin()
    {
        if (SkinManager.Instance == null)
        {
            Debug.LogWarning($"{limbName}: SkinManager not found! Using default skin.");
            return;
        }

        if (spriteResolver == null)
        {
            Debug.LogWarning($"{limbName}: No SpriteResolver found! Cannot apply skin.");
            return;
        }

        string category = SkinManager.Instance.GetCategoryForLimb(limbPlayer);
        
        string savedLabel = SkinManager.Instance.LoadSkin(limbPlayer);

        if (!string.IsNullOrEmpty(savedLabel))
        {
            spriteResolver.SetCategoryAndLabel(category, savedLabel);
            Debug.Log($"{limbName} loaded skin: {category}/{savedLabel}");
        }
        else
        {
            Debug.LogWarning($"{limbName}: Could not load skin label");
        }
    }

    public void ChangeSkin(string labelName)
    {
        if (spriteResolver == null || SkinManager.Instance == null) return;

        string category = SkinManager.Instance.GetCategoryForLimb(limbPlayer);
        spriteResolver.SetCategoryAndLabel(category, labelName);
        
        // Save the change
        SkinManager.Instance.SaveSkin(limbPlayer, labelName);
        
        Debug.Log($"{limbName} changed to skin: {labelName}");
    }

    public void ChangeSkinByIndex(int index)
    {
        if (SkinManager.Instance == null) return;
        
        string labelName = SkinManager.Instance.GetLabelByIndex(limbPlayer, index);
        ChangeSkin(labelName);
    }

    public void LockLimb()
    {
        isLocked = true;
        Debug.Log($"{limbName} locked at angle: {currentAngle}");
    }

    public void UnlockLimb()
    {
        isLocked = false;
    }

    public bool IsLocked()
    {
        return isLocked;
    }

    public float GetCurrentAngle()
    {
        return currentAngle;
    }

    public void EnableHidingMode()
    {
        hidingModeEnabled = true;
        isLocked = false;
    }

    public void DisableHidingMode()
    {
        hidingModeEnabled = false;
    }
}