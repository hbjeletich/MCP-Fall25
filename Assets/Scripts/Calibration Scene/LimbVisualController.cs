using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D.Animation;

public class LimbVisualController : MonoBehaviour
{
    [Header("Settings")]
    public InputManager.LimbPlayer assignedLimb;

    [Header("Visual Settings")]
    public float rotationSpeed = 100f;

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
        SubscribeToSkinChanges();
    }

    void OnDestroy()
    {
        UnsubscribeFromSkinChanges();
    }

    void SubscribeToSkinChanges()
    {
        if (bodySelectionPhase == null) return;

        foreach (var bodyPart in bodySelectionPhase.bodyParts)
        {
            if (bodyPart.limbPlayer == assignedLimb)
            {
                bodyPart.OnSkinChanged += OnSkinChangedHandler;
                Debug.Log($"{assignedLimb} subscribed to skin change events");
                break;
            }
        }
    }

    void UnsubscribeFromSkinChanges()
    {
        if (bodySelectionPhase == null) return;

        foreach (var bodyPart in bodySelectionPhase.bodyParts)
        {
            if (bodyPart.limbPlayer == assignedLimb)
            {
                bodyPart.OnSkinChanged -= OnSkinChangedHandler;
                break;
            }
        }
    }

    void OnSkinChangedHandler(string newLabel)
    {
        if (limbImage != null && skinManager != null && !string.IsNullOrEmpty(newLabel))
        {
            Sprite sprite = GetSpriteFromLibrary(newLabel);
            if (sprite != null)
            {
                limbImage.sprite = sprite;
                Debug.Log($"{assignedLimb} display updated to: {newLabel}");
            }
        }
    }

    void Update()
    {
        if (inputManager == null) return;

        if (!inputManager.IsControllerConnected((int)assignedLimb))
        {
            return;
        }

        if (bodySelectionPhase != null)
        {
            foreach (var bodyPart in bodySelectionPhase.bodyParts)
            {
                if (bodyPart.limbPlayer == assignedLimb && bodyPart.isSelecting)
                {
                    return;
                }
            }
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

        if (limbImage == null)
        {
            Debug.LogWarning($"No Image component! Cannot load skin for {assignedLimb}");
            return;
        }

        string savedLabel = skinManager.LoadSkin(assignedLimb);

        if (!string.IsNullOrEmpty(savedLabel))
        {
            Sprite sprite = GetSpriteFromLibrary(savedLabel);
            if (sprite != null)
            {
                limbImage.sprite = sprite;
                Debug.Log($"{assignedLimb} loaded skin: {savedLabel}");
            }
        }
        else
        {
            Debug.LogWarning($"Could not load skin for {assignedLimb}");
        }
    }

    Sprite GetSpriteFromLibrary(string labelName)
    {
        if (skinManager == null || skinManager.spriteLibraryAsset == null)
        {
            Debug.LogWarning("SkinManager or Sprite Library Asset not found!");
            return null;
        }

        string category = skinManager.GetCategoryForLimb(assignedLimb);
        
        Sprite sprite = skinManager.spriteLibraryAsset.GetSprite(category, labelName);
        
        if (sprite == null)
        {
            Debug.LogWarning($"Could not find sprite for category '{category}', label '{labelName}'");
        }

        return sprite;
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

    public void UpdateSkin(string labelName)
    {
        if (limbImage != null && !string.IsNullOrEmpty(labelName))
        {
            Sprite sprite = GetSpriteFromLibrary(labelName);
            if (sprite != null)
            {
                limbImage.sprite = sprite;
            }
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