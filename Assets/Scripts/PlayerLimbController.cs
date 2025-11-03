using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class PlayerLimbController : MonoBehaviour
{
    [Header("Limb Settings")]
    public string limbName; // "LeftArm", "RightArm", "LeftLeg", "RightLeg", "Head"
    public InputManager.LimbPlayer limbPlayer;
    
    [Header("IK Settings")]
    public Transform ikTarget;
    public Transform pivotPoint;
    public float reachRadius = 1.5f;
    public float moveSpeed = 100f;
    
    [Header("Constraints")]
    public float minAngle = -180f;
    public float maxAngle = 180f;
    
    private bool isLocked = false;
    private bool hidingModeEnabled = false;
    private float currentAngle = 0f;

    private InputManager inputManager;
    private SpriteResolver spriteResolver;
    private Vector3 initialOffset;

    void Start()
    {
        inputManager = InputManager.Instance;
        spriteResolver = GetComponent<SpriteResolver>();
        
        LoadSavedSkin();
        
        if (ikTarget != null && pivotPoint != null)
        {
            initialOffset = ikTarget.position - pivotPoint.position;
            currentAngle = Mathf.Atan2(initialOffset.y, initialOffset.x) * Mathf.Rad2Deg;
            
            Debug.Log($"{limbName}: Initial IK angle = {currentAngle:F1}°, radius = {initialOffset.magnitude:F2}");
        }
        else
        {
            Debug.LogWarning($"{limbName}: IK Target or Pivot Point not assigned!");
        }
    }

    void LateUpdate()
    {
        //Debug.Log($"{limbName} Update() is running!");

        if (Input.GetKeyDown(KeyCode.T))
        {
            ikTarget.position += Vector3.right * 5f;
            Debug.Log($"Forced IK move: {ikTarget.position}");
        }

        if (inputManager == null) 
        {
            //Debug.LogWarning($"{limbName}: InputManager not found!");
            return;
        }
        
        if (!hidingModeEnabled)
        {
            //Debug.Log($"{limbName}: Hiding mode not enabled, skipping IK handling.");
            return;
        }

        
        if (!isLocked)
        {
            HandleIKTargetMovement();
        }

        if (inputManager.GetLimbLockButtonDown(limbPlayer) && !isLocked)
        {
            LockLimb();
        }
    }

    void HandleIKTargetMovement()
    {
        if (ikTarget == null || pivotPoint == null) return;
        
        float input = inputManager.GetLimbHorizontalAxis(limbPlayer);
        
        if (Mathf.Abs(input) > 0.1f)
        {
            currentAngle += input * moveSpeed * Time.deltaTime;
            
            currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
            
            float angleRad = currentAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(angleRad) * reachRadius,
                Mathf.Sin(angleRad) * reachRadius,
                0f
            );
            
            ikTarget.position = pivotPoint.position + offset;
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
        Debug.Log($"{limbName} locked at angle: {currentAngle:F1}° (IK mode)");
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
    
    public Vector3 GetIKTargetPosition()
    {
        return ikTarget != null ? ikTarget.position : Vector3.zero;
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