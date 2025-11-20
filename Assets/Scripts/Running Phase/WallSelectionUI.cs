using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WallSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public Image[] objectPreviewImages; // Preview images for the 3 hiding objects
    public GameObject[] selectionFrames; // Optional: frames/borders around each preview
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI selectionText;

    [Header("Colors")]
    public Color unselectedColor = new Color(0.5f, 0.5f, 0.5f, 0.6f); // Dimmed/grayed out
    public Color selectedColor = Color.white; // Full brightness
    public Color selectedBorderColor = Color.cyan;
    public Color unselectedBorderColor = Color.gray;

    [Header("Settings")]
    public bool showInstructions = true;
    public bool useColorTint = true; // Tint preview images
    public bool useScaling = true; // Scale up selected preview
    public float selectedScale = 1.2f;
    public float unselectedScale = 1.0f;

    [Header("Border Settings")]
    public bool useBorders = true;
    public Image[] borderImages; // Optional border images for each preview

    private RunningPhaseController runningController;
    private HidingObjectManager hidingObjectManager;
    private InputManager inputManager;
    private int currentSelection = 0;

    void Awake()
    {
        // Auto-populate preview images from HidingObjectManager prefabs
        hidingObjectManager = FindObjectOfType<HidingObjectManager>();
        
        if (hidingObjectManager != null && hidingObjectManager.objectPrefabs != null)
        {
            LoadPreviewsFromManager();
        }
        else
        {
            Debug.LogWarning("WallSelectionUI: HidingObjectManager or objectPrefabs not found during Awake");
        }
    }

    void Start()
    {
        runningController = FindObjectOfType<RunningPhaseController>();
        inputManager = InputManager.Instance;

        if (runningController != null)
        {
            runningController.OnWallSelectionChange += OnWallSelectionChanged;
            currentSelection = runningController.GetSelectedWallIndex();
        }

        SetupInstructions();
        UpdateSelection(currentSelection);
    }

    void OnDestroy()
    {
        if (runningController != null)
        {
            runningController.OnWallSelectionChange -= OnWallSelectionChanged;
        }
    }

    void SetupInstructions()
    {
        if (!showInstructions || instructionText == null) return;

        if (inputManager != null && inputManager.inputMode == InputManager.InputMode.Debug)
        {
            instructionText.text = "HEAD: Use [ and ] to select hiding spot";
        }
        else
        {
            instructionText.text = "HEAD: Move stick left/right to select hiding spot";
        }
    }

    void OnWallSelectionChanged(int objectIndex)
    {
        UpdateSelection(objectIndex);
    }

    /// <summary>
    /// Automatically load preview images from HidingObjectManager's prefabs.
    /// Called during Awake to populate the UI with the available objects.
    /// </summary>
    void LoadPreviewsFromManager()
    {
        if (hidingObjectManager == null || hidingObjectManager.objectPrefabs == null) return;
        if (objectPreviewImages == null) return;

        int previewCount = Mathf.Min(objectPreviewImages.Length, hidingObjectManager.objectPrefabs.Length, hidingObjectManager.numberOfObjects);

        for (int i = 0; i < previewCount; i++)
        {
            if (objectPreviewImages[i] == null) continue;
            if (hidingObjectManager.objectPrefabs[i] == null) continue;

            // Get sprite from prefab
            GameObject prefab = hidingObjectManager.objectPrefabs[i];
            SpriteRenderer prefabSprite = prefab.GetComponent<SpriteRenderer>();
            
            if (prefabSprite == null)
            {
                prefabSprite = prefab.GetComponentInChildren<SpriteRenderer>();
            }

            if (prefabSprite != null && prefabSprite.sprite != null)
            {
                objectPreviewImages[i].sprite = prefabSprite.sprite;
                objectPreviewImages[i].enabled = true;
                Debug.Log($"WallSelectionUI: Loaded preview {i} from prefab {prefab.name}");
            }
            else
            {
                Debug.LogWarning($"WallSelectionUI: No sprite found on prefab {prefab.name}");
            }
        }

        UpdateSelection(currentSelection);
    }

    /// <summary>
    /// OPTIONAL: Call this to update preview images from instantiated hiding objects.
    /// The previews are already loaded from prefabs in Awake(), so this is only needed
    /// if you want to refresh/update them during gameplay.
    /// </summary>
    public void LoadObjectPreviews(HidingObject[] hidingObjects)
    {
        if (hidingObjects == null || objectPreviewImages == null) return;

        for (int i = 0; i < objectPreviewImages.Length && i < hidingObjects.Length; i++)
        {
            if (objectPreviewImages[i] == null || hidingObjects[i] == null) continue;

            // Get the sprite from the hiding object
            SpriteRenderer hidingSprite = hidingObjects[i].GetComponent<SpriteRenderer>();
            if (hidingSprite != null && hidingSprite.sprite != null)
            {
                objectPreviewImages[i].sprite = hidingSprite.sprite;
                objectPreviewImages[i].enabled = true;
            }
            else
            {
                // Try to find sprite in children
                SpriteRenderer childSprite = hidingObjects[i].GetComponentInChildren<SpriteRenderer>();
                if (childSprite != null && childSprite.sprite != null)
                {
                    objectPreviewImages[i].sprite = childSprite.sprite;
                    objectPreviewImages[i].enabled = true;
                }
            }
        }

        UpdateSelection(currentSelection);
    }

    /// <summary>
    /// OPTIONAL: Alternative method to load from prefabs with custom selection indices.
    /// The previews are already loaded from prefabs in Awake(), so this is only needed
    /// if you want to show a different set of objects than the manager's default prefabs.
    /// </summary>
    public void LoadObjectPreviewsFromPrefabs(GameObject[] objectPrefabs, int[] selectedIndices)
    {
        if (objectPrefabs == null || objectPreviewImages == null || selectedIndices == null) return;

        for (int i = 0; i < objectPreviewImages.Length && i < selectedIndices.Length; i++)
        {
            if (objectPreviewImages[i] == null) continue;
            if (selectedIndices[i] >= objectPrefabs.Length) continue;

            GameObject prefab = objectPrefabs[selectedIndices[i]];
            if (prefab == null) continue;

            // Get sprite from prefab
            SpriteRenderer prefabSprite = prefab.GetComponent<SpriteRenderer>();
            if (prefabSprite == null)
            {
                prefabSprite = prefab.GetComponentInChildren<SpriteRenderer>();
            }

            if (prefabSprite != null && prefabSprite.sprite != null)
            {
                objectPreviewImages[i].sprite = prefabSprite.sprite;
                objectPreviewImages[i].enabled = true;
            }
        }

        UpdateSelection(currentSelection);
    }

    void UpdateSelection(int selectedIndex)
    {
        currentSelection = selectedIndex;

        if (selectionText != null)
        {
            selectionText.text = $"Hiding Spot: {selectedIndex + 1}/3";
        }

        // Update each preview
        for (int i = 0; i < objectPreviewImages.Length && i < 3; i++)
        {
            if (objectPreviewImages[i] == null) continue;

            bool isSelected = (i == selectedIndex);

            // Apply color tint
            if (useColorTint)
            {
                objectPreviewImages[i].color = isSelected ? selectedColor : unselectedColor;
            }

            // Apply scaling
            if (useScaling)
            {
                float scale = isSelected ? selectedScale : unselectedScale;
                objectPreviewImages[i].transform.localScale = Vector3.one * scale;
            }

            // Update borders if using them
            if (useBorders && borderImages != null && i < borderImages.Length && borderImages[i] != null)
            {
                borderImages[i].color = isSelected ? selectedBorderColor : unselectedBorderColor;
            }

            // Update selection frames if present
            if (selectionFrames != null && i < selectionFrames.Length && selectionFrames[i] != null)
            {
                selectionFrames[i].SetActive(isSelected);
            }
        }
    }

    public void SetVisible(bool visible)
    {
        // Hide/show preview images
        if (objectPreviewImages != null)
        {
            foreach (Image previewImage in objectPreviewImages)
            {
                if (previewImage != null)
                {
                    previewImage.gameObject.SetActive(visible);
                }
            }
        }

        // Hide/show borders
        if (borderImages != null)
        {
            foreach (Image border in borderImages)
            {
                if (border != null)
                {
                    border.gameObject.SetActive(visible);
                }
            }
        }

        // Hide/show selection frames
        if (selectionFrames != null)
        {
            foreach (GameObject frame in selectionFrames)
            {
                if (frame != null)
                {
                    frame.SetActive(false); // Frames only show on selected
                }
            }
        }

        // Hide/show text
        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(visible);
        }

        if (selectionText != null)
        {
            selectionText.gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// Get the currently selected object index
    /// </summary>
    public int GetSelectedIndex()
    {
        return currentSelection;
    }
}