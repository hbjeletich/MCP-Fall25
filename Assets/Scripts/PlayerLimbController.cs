using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class PlayerLimbController : MonoBehaviour
{
    [Header("Limb Settings")]
    public string limbName;
    public InputManager.LimbPlayer limbPlayer;
    
    [Header("IK Settings")]
    public Transform ikTarget;
    public Transform pivotPoint;
    public float rotationRange = 45f;
    public float rotationSpeed = 100f;
    
    private bool isLocked = false;
    private bool hidingModeEnabled = false;
    private float currentAngle = 0f;
    private float startAngle = 0f;
    private float reachRadius = 0f;

    private InputManager inputManager;
    private SpriteResolver spriteResolver;

    [HideInInspector] public float minAngle = -180f;
    [HideInInspector] public float maxAngle = 180f;

    void Start()
    {
        inputManager = InputManager.Instance;
        spriteResolver = GetComponent<SpriteResolver>();
        LoadSavedSkin();
        
        if (ikTarget == null || pivotPoint == null)
        {
            Debug.LogWarning($"{limbName}: IK Target or Pivot Point not assigned!");
        }
    }

    void Update()
    {
        if (inputManager == null) return;
        if (!hidingModeEnabled) return;
        
        if (!isLocked)
        {
            HandleRotation();
        }

        if (inputManager.GetLimbLockButtonDown(limbPlayer) && !isLocked)
        {
            LockLimb();
        }
    }

    void HandleRotation()
    {
        if (ikTarget == null || pivotPoint == null) return;
        
        float input = inputManager.GetLimbHorizontalAxis(limbPlayer);
        
        if (Mathf.Abs(input) > 0.1f)
        {
            currentAngle += input * rotationSpeed * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, startAngle - rotationRange, startAngle + rotationRange);
            
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
        if (SkinManager.Instance == null || spriteResolver == null) return;

        string category = SkinManager.Instance.GetCategoryForLimb(limbPlayer);
        string savedLabel = SkinManager.Instance.LoadSkin(limbPlayer);

        if (!string.IsNullOrEmpty(savedLabel))
        {
            spriteResolver.SetCategoryAndLabel(category, savedLabel);
        }
    }

    public void ChangeSkin(string labelName)
    {
        if (spriteResolver == null || SkinManager.Instance == null) return;

        string category = SkinManager.Instance.GetCategoryForLimb(limbPlayer);
        spriteResolver.SetCategoryAndLabel(category, labelName);
        SkinManager.Instance.SaveSkin(limbPlayer, labelName);
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
        Debug.Log($"{limbName} locked at angle: {currentAngle:F1}°");
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
        
        if (ikTarget != null && pivotPoint != null)
        {
            Vector3 offset = ikTarget.position - pivotPoint.position;
            startAngle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            currentAngle = startAngle;
            reachRadius = offset.magnitude;
            
            minAngle = startAngle - rotationRange;
            maxAngle = startAngle + rotationRange;
            
            Debug.Log($"{limbName}: Start angle = {startAngle:F1}°, range = {minAngle:F1}° to {maxAngle:F1}°");
        }
    }

    public void DisableHidingMode()
    {
        hidingModeEnabled = false;
    }
}