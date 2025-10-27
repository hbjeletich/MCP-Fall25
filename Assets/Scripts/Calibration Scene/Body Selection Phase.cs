/*
Code to handle selecting body parts

used chatgpt to help with specific syntax 

*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BodySelectionPhase : MonoBehaviour
{
    [System.Serializable]
    public class BodyPart
    {
        [Header("Player Assignment")]
        public InputManager.LimbPlayer limbPlayer;

        [Header("UI References")]
        public GameObject selectionPopup;
        public Image previewImage;
        public TextMeshProUGUI skinNameText;
        public Image[] navigationArrows;

        [HideInInspector] public int currentIndex = 0;
        [HideInInspector] public bool isSelecting = false;
        [HideInInspector] public bool isConfirmed = false;

        private float lastInputTime = 0f;
        private const float INPUT_COOLDOWN = 0.2f;

        private SkinManager skinManager;
        private InputManager inputManager;

        public void Initialize()
        {
            skinManager = SkinManager.Instance;
            inputManager = InputManager.Instance;
            
            if (skinManager == null)
            {
                Debug.LogError("SkinManager not found! Make sure it exists in the scene.");
                return;
            }
            
            currentIndex = skinManager.LoadSkin(limbPlayer);
            
            if (selectionPopup != null)
            {
                selectionPopup.SetActive(false);
            }
            
            ShowCurrentOption();
        }

        public void HandleInput()
        {
            if (inputManager == null || skinManager == null) return;

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

            if (isSelecting) //returns if every player has selected a body part
                return;

            //string horizontalAxis = $"P{playerIndex + 1}_Horizontal"; // e.g., P1_Horizontal
            // changed to work with input manager
            float horizontal = inputManager.GetLimbHorizontalAxis(limbPlayer);;

            if (Time.time - lastInputTime > INPUT_COOLDOWN)
            {
                if (horizontal > 0.5f)
                {
                    NextOption(); //if joystick is moved right, option to the right
                    lastInputTime = Time.time;
                }
                else if (horizontal < -0.5f)
                {
                    PreviousOption(); //if joystick is moved left, option to the left
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
                skinManager.SaveSkin(limbPlayer, currentIndex);
            }
            
            Debug.Log($"{limbPlayer} confirmed skin {currentIndex}");
        }

        private void NextOption()
        {
            if (skinManager == null) return;
            
            int skinCount = skinManager.GetSkinCount(limbPlayer);
            if (skinCount == 0) return;
            
            currentIndex = (currentIndex + 1) % skinCount; //sets the index to be one to the right
            ShowCurrentOption();
            
            Debug.Log($"{limbPlayer} cycled to skin {currentIndex}");
        }

        private void PreviousOption()
        {
            if (skinManager == null) return;
            
            int skinCount = skinManager.GetSkinCount(limbPlayer);
            if (skinCount == 0) return;
            
            currentIndex = (currentIndex - 1 + skinCount) % skinCount; //sets the index to be one to the left
            ShowCurrentOption();
            
            Debug.Log($"{limbPlayer} cycled to skin {currentIndex}");
        }

        public void ShowCurrentOption()
        {
            if (skinManager == null || previewImage == null) return;
            
            Sprite currentSkin = skinManager.GetSkinSprite(limbPlayer, currentIndex);
            
            if (currentSkin != null)
            {
                previewImage.sprite = currentSkin;
                previewImage.enabled = true;
            }
            else
            {
                Debug.LogWarning($"No skin sprite found for {limbPlayer} at index {currentIndex}");
            }
            
            if (skinNameText != null)
            {
                skinNameText.text = $"Skin {currentIndex + 1}";
            }
        }

        public Sprite GetCurrentSkin()
        {
            if (skinManager == null) return null;
            return skinManager.GetSkinSprite(limbPlayer, currentIndex);
        }
    }

    [Header("Body Parts")]
    public List<BodyPart> bodyParts = new List<BodyPart>();

    void Start()
    {
        foreach (var part in bodyParts)
        {
            part.Initialize(); //shows active selection
        }
    }

    void Update()
    {
        foreach (var part in bodyParts)
        {
            part.HandleInput();
        }

        if (AllPartsSelected())
        {
            Debug.Log("All players have selected their limbs!");
            // Proceed to next phase
            enabled = false;
        }
    }

    private bool AllPartsSelected() //returns true when every player has selected a body part
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

    public Sprite GetSelectedSkin(InputManager.LimbPlayer limb)
    {
        foreach (var part in bodyParts)
        {
            if (part.limbPlayer == limb)
            {
                return part.GetCurrentSkin();
            }
        }
        return null;
    }
}

