using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WallSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public Image[] objectPreviewImages;
    public GameObject[] selectionFrames;

    [Header("Colors")]
    public Color unselectedColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
    public Color selectedColor = Color.white;

    [Header("Settings")]
    public bool showInstructions = true;
    public bool useColorTint = true;
    public bool useScaling = true;
    public float selectedScale = 1.2f;
    public float unselectedScale = 1.0f;

    [Header("Border Settings")]
    public bool useBorders = true;
    public Image[] borderImages;

    private RunningPhaseController runningController;
    private HidingObjectManager hidingObjectManager;
    private InputManager inputManager;
    private int currentSelection = 0;

    void Awake()
    {
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

        UpdateSelection(currentSelection);
    }

    void OnDestroy()
    {
        if (runningController != null)
        {
            runningController.OnWallSelectionChange -= OnWallSelectionChanged;
        }
    }

    void OnWallSelectionChanged(int objectIndex)
    {
        UpdateSelection(objectIndex);
    }

    void LoadPreviewsFromManager()
    {
        if (hidingObjectManager == null || hidingObjectManager.objectPrefabs == null) return;
        if (objectPreviewImages == null) return;

        int previewCount = Mathf.Min(objectPreviewImages.Length, hidingObjectManager.objectPrefabs.Length, hidingObjectManager.numberOfObjects);

        for (int i = 0; i < previewCount; i++)
        {
            if (objectPreviewImages[i] == null) continue;
            if (hidingObjectManager.objectPrefabs[i] == null) continue;

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

    public void LoadObjectPreviews(HidingObject[] hidingObjects)
    {
        if (hidingObjects == null || objectPreviewImages == null) return;

        for (int i = 0; i < objectPreviewImages.Length && i < hidingObjects.Length; i++)
        {
            if (objectPreviewImages[i] == null || hidingObjects[i] == null) continue;

            SpriteRenderer hidingSprite = hidingObjects[i].GetComponent<SpriteRenderer>();
            if (hidingSprite != null && hidingSprite.sprite != null)
            {
                objectPreviewImages[i].sprite = hidingSprite.sprite;
                objectPreviewImages[i].enabled = true;
            }
            else
            {
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

    void UpdateSelection(int selectedIndex)
    {
        currentSelection = selectedIndex;

        for (int i = 0; i < objectPreviewImages.Length && i < 3; i++)
        {
            if (objectPreviewImages[i] == null) continue;

            bool isSelected = (i == selectedIndex);

            if (useColorTint)
            {
                objectPreviewImages[i].color = isSelected ? selectedColor : unselectedColor;
            }

            if (useScaling)
            {
                float scale = isSelected ? selectedScale : unselectedScale;
                objectPreviewImages[i].transform.localScale = Vector3.one * scale;
            }

            if (selectionFrames != null && i < selectionFrames.Length && selectionFrames[i] != null)
            {
                selectionFrames[i].SetActive(isSelected);
            }
        }
    }

    public void SetVisible(bool visible)
    {
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

        if (selectionFrames != null)
        {
            foreach (GameObject frame in selectionFrames)
            {
                if (frame != null)
                {
                    frame.SetActive(false);
                }
            }
        }
    }

    public int GetSelectedIndex()
    {
        return currentSelection;
    }
}