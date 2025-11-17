using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLimbController : MonoBehaviour
{
    [Header("Limb Settings")]
    public string limbName; // "LeftArm", "RightArm", "LeftLeg", "RightLeg", "Head"
    public InputManager.LimbPlayer limbPlayer;
    public float rotationSpeed = 50f;
    public float minAngle = -45f;
    public float maxAngle = 45f;
    public SpriteRenderer spriteRenderer;


    private bool isLocked = false;
    private bool hidingModeEnabled = false;
    private float currentAngle = 0f;

    private InputManager inputManager;

    void Start()
    {
        inputManager = InputManager.Instance;

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
        if(SkinManager.Instance == null)
        {
            Debug.LogWarning($"{limbName}: SkinManager not found! Using default sprite.");
            return;
        }

        if(spriteRenderer == null)
        {
            Debug.LogWarning($"{limbName}: No SpriteRenderer found!");
            return;
        }

        int savedSkinIndex = SkinManager.Instance.LoadSkin(limbPlayer);
        Sprite skinSprite = SkinManager.Instance.GetSkinSprite(limbPlayer, savedSkinIndex);

        if(skinSprite != null)
        {
            spriteRenderer.sprite = skinSprite;
        }
        else
        {
            Debug.LogWarning($"{limbName}: Could not load skin sprite at index {savedSkinIndex}");
        }
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