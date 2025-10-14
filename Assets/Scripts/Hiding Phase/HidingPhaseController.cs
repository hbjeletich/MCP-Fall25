using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidingPhaseController : MonoBehaviour
{
    [Header("References")]
    public PlayerLimbController[] limbs;
    public WallGenerator wallGenerator;
    public RunningPhaseController runningPhaseController;

    [Header("Timing")]
    public float hidingDuration = 10f;

    private WallHole[] walls;
    private WallHole currentWall;
    private int currentWallIndex = 0;
    private float hidingTimer = 0f;
    private bool isHiding = false;

    public delegate void OnHidingStartEvent(WallHole wall);
    public event OnHidingStartEvent OnHidingStart;

    public delegate void OnHidingSuccessEvent();
    public event OnHidingSuccessEvent OnHidingSuccess;

    public delegate void OnHidingFailEvent();
    public event OnHidingFailEvent OnHidingFail;

    void Update()
    {
        if (!isHiding) return;

        hidingTimer += Time.deltaTime;

        if (hidingTimer >= hidingDuration)
        {
            CheckHidingResult();
        }
    }

    public void StartHiding()
    {
        if (walls == null || walls.Length == 0)
        {
            if (wallGenerator != null)
            {
                walls = wallGenerator.GenerateWalls();

                foreach (var wall in walls)
                {
                    wall.SetAlpha(0f);
                }
            }
            else
            {
                Debug.LogError("No wall generator assigned!");
                return;
            }
        }

        isHiding = true;
        hidingTimer = 0f;

        if (runningPhaseController != null)
        {
            int selectedIndex = runningPhaseController.GetSelectedWallIndex();
            currentWall = walls[selectedIndex];
        }

        foreach (var wall in walls)
        {
            wall.SetAlpha(0f);
        }

        currentWall.SetAlpha(1f);

        foreach (var limb in limbs)
        {
            limb.EnableHidingMode();
        }

        OnHidingStart?.Invoke(currentWall);
        Debug.Log($"Hiding phase started! Round {currentWallIndex + 1}");
    }

    public void StopHiding()
    {
        isHiding = false;

        foreach (var limb in limbs)
        {
            limb.DisableHidingMode();
        }

        Debug.Log("Hiding phase stopped!");
    }

    void CheckHidingResult()
    {
        isHiding = false;

        bool success = currentWall.CheckMatch(limbs);

        if (success)
        {
            Debug.Log("Successfully hid!");
            OnHidingSuccess?.Invoke();
            currentWallIndex++;

            ResetLimbs();
        }
        else
        {
            Debug.Log("Failed to hide! Retrying...");
            OnHidingFail?.Invoke();

            ResetLimbs();
        }
    }

    void ResetLimbs()
    {
        foreach (var limb in limbs)
        {
            limb.UnlockLimb();
        }

        if (walls != null)
        {
            foreach (var wall in walls)
            {
                wall.SetAlpha(0f);
            }
        }
    }

    public float GetRemainingTime()
    {
        return hidingDuration - hidingTimer;
    }

    public int GetCurrentWallIndex()
    {
        return currentWallIndex;
    }

    public int GetTotalWalls()
    {
        return walls != null ? walls.Length : 0;
    }

    public bool IsHiding()
    {
        return isHiding;
    }
}