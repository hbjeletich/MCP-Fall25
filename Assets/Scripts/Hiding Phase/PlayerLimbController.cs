using UnityEngine;

public class PlayerLimbController : MonoBehaviour
{
    [Header("Limb Settings")]
    public string limbName; // "LeftArm", "RightArm", "LeftLeg", "RightLeg", "Head"
    public KeyCode lockKey;
    public float rotationSpeed = 50f;
    public float minAngle = -45f;
    public float maxAngle = 45f;
    
    [Header("Input Axes")]
    public string horizontalAxis = "Horizontal"; // will be changed per limb
    
    private bool isLocked = false;
    private bool hidingModeEnabled = false;
    private float currentAngle = 0f;

    void Update()
    {
        if (!hidingModeEnabled) return;
        
        if (!isLocked)
        {
            float input = Input.GetAxis(horizontalAxis);
            
            if (Mathf.Abs(input) > 0.1f) // Deadzone
            {
                currentAngle += input * rotationSpeed * Time.deltaTime;
                currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
                transform.localRotation = Quaternion.Euler(0, 0, currentAngle);
            }
        }

        if (Input.GetKeyDown(lockKey) && !isLocked)
        {
            LockLimb();
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