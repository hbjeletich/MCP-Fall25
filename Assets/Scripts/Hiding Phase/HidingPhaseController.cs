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
    public float scientistSearchCooldown;
    private bool isHiding = false;
    private SpriteRenderer[] limbRenderers;

    public delegate void OnHidingStartEvent(HidingObject hidingObject);
    public event OnHidingStartEvent OnHidingStart;

    public delegate void OnHidingSuccessEvent();
    public event OnHidingSuccessEvent OnHidingSuccess;

    public delegate void OnHidingFailEvent();
    public event OnHidingFailEvent OnHidingFail;

    public QTEDoors DoorScript;

    // START FUNCTION
    void Start()
    {
        scientistSearchCooldown = 0.5f;
        SetupLimbRenderers();
    }

    // SETS UP SPRITE RENDERERS FOR EACH LIMB
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

    // UPDATE FUNCTION
    void Update()
    {
        if (!isHiding) return; // do nothing if not currently hiding

        hidingTimer += Time.deltaTime; // increment phase timer

        if (hidingTimer >= hidingDuration)
        {
            CheckHidingResult(); // check hiding result once timer runs out
        }
    }

    void FixedUpdate()
    {
        if (!isHiding) return;

        if (scientistSearchCooldown > 0)
        {
            scientistSearchCooldown -= Time.deltaTime;
        }
        else
        {
            scientistSearchCooldown = 3.0f;
            if(hidingTimer < 1.5f)
            {
                Debug.Log("SEARCHIN");
                DoorScript.ScientistSearch(3, false);
            }
            else if (hidingTimer < 5.5f)
            {
                DoorScript.ScientistSearch(2, false);
            }
            else if (hidingTimer < 8.0f)
            {
                DoorScript.ScientistSearch(1, true);
            }
        }
    }

    // START HIDING PHASE
    public void StartHiding()
    {
        runningPhaseController.isRunning = false;
        DoorScript.StopScroll(); // stop background scroll animation
        
        if (objects == null || objects.Length == 0) // if no objects generated
        {
            if (objectManager != null)
            {
                objects = objectManager.GenerateObjects(); // generates objects

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

        isHiding = true; // sets hiding phase to true
        hidingTimer = 0f; // begins phase timer

        if (runningPhaseController != null)
        {
            // SETS WALL TO WHATEVER WAS SELECTED DURING RUN PHASE
            int selectedIndex = runningPhaseController.GetSelectedWallIndex();
            currentObject = objects[selectedIndex];
        }

        // SETS OTHER OBJECTS INVISIBLE
        foreach (var obj in objects)
        {
            obj.SetAlpha(0f);
        }

        // SETS CURRENT OBJECT VISIBLE
        currentObject.SetAlpha(1f);

        // ALLOWS FOR LIMB ROTATION
        foreach (var limb in limbs)
        {
            limb.EnableHidingMode();
        }

        OnHidingStart?.Invoke(currentObject);
        Debug.Log($"Hiding phase started! Round {currentObjectIndex + 1}");
    }

    // STOP HIDING PHASE
    public void StopHiding()
    {
        isHiding = false; // hiding mode set to false
        DoorScript.StartScroll(); // starts background scroll effect

        // DISABLES LIMB ROTATION
        foreach (var limb in limbs)
        {
            limb.DisableHidingMode();
        }

        Debug.Log("Hiding phase stopped!");
    }

    // CHECKS RESULT OF HIDING PHASE
    void CheckHidingResult()
    {
        isHiding = false; // hiding mode set to false
        DoorScript.ScientistSearch(0, true);

            // CHECKS IF ALL LIMBS SUCCESSFULLY HIDDEN
        bool success = currentObject.CheckMatch(limbRenderers);

        if (success) // SUCCESS 
        {
            Debug.Log("Successfully hid!");
            OnHidingSuccess?.Invoke();
            currentObjectIndex++;

            ResetLimbs();
        }
        else // FAILURE
        {
            Debug.Log("Failed to hide! Retrying...");
            OnHidingFail?.Invoke();

            ResetLimbs();
        }
    }

    // RESETS LIMB POSITIONS
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

    // GET TIME LEFT IN HIDING PHASE
    public float GetRemainingTime()
    {
        return hidingDuration - hidingTimer;
    }

    // GET INDEX OF CURRENT HIDING OBJECT
    public int GetCurrentObjectIndex()
    {
        return currentObjectIndex;
    }

    // GET TOTAL NUMBER OF POSSIBLE HIDING OBJECTS
    public int GetTotalObjects()
    {
        return objects != null ? objects.Length : 0;
    }

    // CHECK IF HIDING PHASE IS ACTIVE
    public bool IsHiding()
    {
        return isHiding;
    }
}