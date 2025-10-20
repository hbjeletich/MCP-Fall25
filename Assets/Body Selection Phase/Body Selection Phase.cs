/*
Code to handle selecting body parts

used chatgpt to help with specific syntax 

*/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodySelectionPhase : MonoBehaviour
{
    [System.Serializable]
    public class BodyPart
    {
        public string name;
        public int playerIndex; // 0 = Player1, 1 = Player2, etc.
        public GameObject[] options;
        [HideInInspector] public int currentIndex = 0;
        [HideInInspector] public bool isSelected = false;
        private float lastInputTime = 0f;

        public void HandleInput()
        {
            if (isSelected) //returns if every player has selected a body part
                return;

            string horizontalAxis = $"P{playerIndex + 1}_Horizontal"; // e.g., P1_Horizontal
            float horizontal = Input.GetAxis(horizontalAxis);

            if (Time.time - lastInputTime > 0.3f)
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

            string confirmButton = $"P{playerIndex + 1}_Submit"; // e.g., P1_Submit
            if (Input.GetButtonDown(confirmButton))
            {
                isSelected = true;
                Debug.Log($"Player {playerIndex + 1} selected {name}: {options[currentIndex].name}");
            }
        }

        private void NextOption()
        {
            currentIndex = (currentIndex + 1) % options.Length; //sets the index to be one to the right
            ShowCurrentOption();
        }

        private void PreviousOption()
        {
            currentIndex = (currentIndex - 1 + options.Length) % options.Length; //sets the index to be one to the left
            ShowCurrentOption();
        }

        public void ShowCurrentOption()
        {
            for (int i = 0; i < options.Length; i++)
            {
                options[i].SetActive(i == currentIndex); //displays option at the ith index
            }

            Debug.Log($"{name} showing option {currentIndex + 1}");
        }
    }

    public List<BodyPart> bodyParts = new List<BodyPart>();

    void Start()
    {
        foreach (var part in bodyParts)
        {
            part.ShowCurrentOption(); //shows active selection
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
            if (!part.isSelected)
                return false;
        }
        return true;
    }
}

