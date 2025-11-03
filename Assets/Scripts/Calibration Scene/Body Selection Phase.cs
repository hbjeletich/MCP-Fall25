/*
Code to handle selecting body parts

used chatgpt to help with specific syntax 

*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D.Animation;

public class BodySelectionPhase : MonoBehaviour
{
    [System.Serializable]
    public class BodyPart
    {
        [Header("Player Assignment")]
        public InputManager.LimbPlayer limbPlayer;

        [Header("UI References")]
        public GameObject selectionPopup;
        
        [HideInInspector] public int currentIndex = 0;
        [HideInInspector] public string currentLabel = "";
        [HideInInspector] public bool isSelecting = false;
        [HideInInspector] public bool isConfirmed = false;

        private float lastInputTime = 0f;
        private const float INPUT_COOLDOWN = 0.2f;

        private SkinManager skinManager;
        private InputManager inputManager;
        private List<string> availableLabels;
        private Image previewImage;

        public delegate void OnSkinChangedEvent(string newLabel);
        public event OnSkinChangedEvent OnSkinChanged;

        public void Initialize()
        {
            skinManager = SkinManager.Instance;
            inputManager = InputManager.Instance;

            if (skinManager == null)
            {
                Debug.LogError("SkinManager not found! Make sure it exists in the scene.");
                return;
            }

            availableLabels = skinManager.GetLabelsForLimb(limbPlayer);
            
            if (availableLabels.Count == 0)
            {
                Debug.LogError($"No labels found for {limbPlayer}!");
                return;
            }

            currentIndex = skinManager.LoadSkinAsIndex(limbPlayer);
            currentLabel = availableLabels[currentIndex];

            if (selectionPopup != null)
            {
                selectionPopup.SetActive(false);
                
                previewImage = selectionPopup.GetComponentInChildren<Image>();
                
                if (previewImage == null)
                {
                    Debug.LogWarning($"{limbPlayer}: No Image component found in selection popup children!");
                }
                else
                {
                    Debug.Log($"{limbPlayer}: Found preview image - {previewImage.name}");
                }
            }

            ShowCurrentOption();
        }

        public void HandleInput()
        {
            if (inputManager == null || skinManager == null) return;

            if (selectionPopup != null && !selectionPopup.activeSelf && isSelecting)
            {
                Debug.LogWarning($"[{limbPlayer}] isSelecting=true but popup is closed! Fixing...");
                isSelecting = false;
            }

            if (inputManager.GetLimbLockButtonDown(limbPlayer))
            {
                if (!isSelecting)
                {
                    OpenSelectionMenu();
                }
                else
                {
                    ConfirmSelection();
                }
                return;
            }

            if (!isSelecting) return;

            float horizontal = inputManager.GetLimbHorizontalAxis(limbPlayer);

            if (Mathf.Abs(horizontal) > 0.1f)
            {
                Debug.Log($"[{limbPlayer}] Menu OPEN - Horizontal input: {horizontal:F2}");
            }

            if (Time.time - lastInputTime > INPUT_COOLDOWN)
            {
                if (horizontal > 0.5f)
                {
                    NextOption();
                    lastInputTime = Time.time;
                }
                else if (horizontal < -0.5f)
                {
                    PreviousOption();
                    lastInputTime = Time.time;
                }
            }
        }

        private void OpenSelectionMenu()
        {
            isSelecting = true;

            if (selectionPopup != null)
            {
                selectionPopup.SetActive(true);
            }

            Debug.Log($"{limbPlayer} opened skin selection menu");
        }

        private void ConfirmSelection()
        {
            isSelecting = false;
            isConfirmed = true;

            if (selectionPopup != null)
            {
                selectionPopup.SetActive(false);
            }

            if (skinManager != null)
            {
                skinManager.SaveSkin(limbPlayer, currentLabel);
            }

            Debug.Log($"{limbPlayer} confirmed skin: {currentLabel}");
        }

        private void NextOption()
        {
            if (skinManager == null || availableLabels == null || availableLabels.Count == 0) return;

            int oldIndex = currentIndex;
            currentIndex = (currentIndex + 1) % availableLabels.Count;
            currentLabel = availableLabels[currentIndex];

            Debug.Log($"{limbPlayer} cycling NEXT: {oldIndex} → {currentIndex} ({currentLabel})");

            ShowCurrentOption();
        }

        private void PreviousOption()
        {
            if (skinManager == null || availableLabels == null || availableLabels.Count == 0) return;

            int oldIndex = currentIndex;
            currentIndex = (currentIndex - 1 + availableLabels.Count) % availableLabels.Count;
            currentLabel = availableLabels[currentIndex];

            Debug.Log($"{limbPlayer} cycling PREV: {oldIndex} → {currentIndex} ({currentLabel})");

            ShowCurrentOption();
        }

        public void ShowCurrentOption()
        {
            if (skinManager == null)
            {
                Debug.LogWarning($"{limbPlayer} ShowCurrentOption: skinManager is null!");
                return;
            }

            if (previewImage != null && skinManager.spriteLibraryAsset != null)
            {
                string category = skinManager.GetCategoryForLimb(limbPlayer);
                Sprite sprite = skinManager.spriteLibraryAsset.GetSprite(category, currentLabel);
                
                if (sprite != null)
                {
                    previewImage.sprite = sprite;
                    previewImage.enabled = true;
                }
                else
                {
                    Debug.LogWarning($"Could not find sprite for {category}/{currentLabel}");
                }
            }

            OnSkinChanged?.Invoke(currentLabel);
            
            Debug.Log($"{limbPlayer} ShowCurrentOption: {currentLabel}");
        }

        public string GetCurrentLabel()
        {
            return currentLabel;
        }

        public int GetCurrentIndex()
        {
            return currentIndex;
        }
    }

    [Header("Body Parts")]
    public List<BodyPart> bodyParts = new List<BodyPart>();

    void Start()
    {
        foreach (var part in bodyParts)
        {
            part.Initialize();
        }
    }

    void Update()
    {
        foreach (var part in bodyParts)
        {
            part.HandleInput();
        }
    }

    private bool AllPartsConfirmed()
    {
        foreach (var part in bodyParts)
        {
            if (!part.isConfirmed)
                return false;
        }
        return true;
    }

    public void ResetAllConfirmations()
    {
        foreach (var part in bodyParts)
        {
            part.isConfirmed = false;
            part.isSelecting = false;

            if (part.selectionPopup != null)
            {
                part.selectionPopup.SetActive(false);
            }
        }

        Debug.Log("Reset all skin confirmations");
    }

    public string GetSelectedLabel(InputManager.LimbPlayer limb)
    {
        foreach (var part in bodyParts)
        {
            if (part.limbPlayer == limb)
            {
                return part.GetCurrentLabel();
            }
        }
        return "";
    }
}