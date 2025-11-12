using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidingPhaseController : MonoBehaviour
{
    [Header("References")]
    public PlayerLimbController[] limbs;
    public HidingObjectManager objectManager;
    public RunningPhaseController runningPhaseController;

    [Header("Timing")]
    public float hidingDuration = 10f;

    private HidingObject[] objects;
    private HidingObject currentObject;
    private int currentObjectIndex = 0;
    private float hidingTimer = 0f;
    private bool isHiding = false;
    private SpriteRenderer[] limbRenderers;

    public delegate void OnHidingStartEvent(HidingObject hidingObject);
    public event OnHidingStartEvent OnHidingStart;

    public delegate void OnHidingSuccessEvent();
    public event OnHidingSuccessEvent OnHidingSuccess;

    public delegate void OnHidingFailEvent();
    public event OnHidingFailEvent OnHidingFail;

    public QTEDoors DoorScript;

    void Start()
    {
        SetupLimbRenderers();
    }

    void SetupLimbRenderers()
    {
        limbRenderers = new SpriteRenderer[limbs.Length];
        for (int i = 0; i < limbs.Length; i++)
        {
            if (limbs[i] != null)
            {
                limbRenderers[i] = limbs[i].GetComponent<SpriteRenderer>();
                if (limbRenderers[i] == null)
                {
                    Debug.LogWarning($"Limb {limbs[i].limbName} does not have a SpriteRenderer component!");
                }
            }
        }
    }

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
        DoorScript.StopScroll();
        
        if (objects == null || objects.Length == 0)
        {
            if (objectManager != null)
            {
                objects = objectManager.GenerateObjects();

                foreach (var obj in objects)
                {
                    obj.SetAlpha(0f);
                }
            }
            else
            {
                Debug.LogError("No object manager assigned!");
                return;
            }
        }

        isHiding = true;
        hidingTimer = 0f;

        if (runningPhaseController != null)
        {
            int selectedIndex = runningPhaseController.GetSelectedWallIndex();
            currentObject = objects[selectedIndex];
        }

        foreach (var obj in objects)
        {
            obj.SetAlpha(0f);
        }

        currentObject.SetAlpha(1f);

        foreach (var limb in limbs)
        {
            limb.EnableHidingMode();
        }

        OnHidingStart?.Invoke(currentObject);
        Debug.Log($"Hiding phase started! Round {currentObjectIndex + 1}");
    }

    public void StopHiding()
    {
        isHiding = false;
        DoorScript.StartScroll();

        foreach (var limb in limbs)
        {
            limb.DisableHidingMode();
        }

        Debug.Log("Hiding phase stopped!");
    }

    void CheckHidingResult()
    {
        isHiding = false;

        bool success = currentObject.CheckMatch(limbRenderers);

        if (success)
        {
            Debug.Log("Successfully hid!");
            OnHidingSuccess?.Invoke();
            currentObjectIndex++;

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

        if (objects != null)
        {
            foreach (var obj in objects)
            {
                obj.SetAlpha(0f);
            }
        }
    }

    public float GetRemainingTime()
    {
        return hidingDuration - hidingTimer;
    }

    public int GetCurrentObjectIndex()
    {
        return currentObjectIndex;
    }

    public int GetTotalObjects()
    {
        return objects != null ? objects.Length : 0;
    }

    public bool IsHiding()
    {
        return isHiding;
    }
}