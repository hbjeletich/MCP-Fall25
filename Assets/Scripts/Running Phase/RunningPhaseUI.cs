using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunningPhaseUI : MonoBehaviour
{
    [Header("UI References")]
    public RunningPromptUI[] promptUIs;
    public Sprite[] idleButtonSprites;
    public Sprite[] waitingButtonSprites;
    public Sprite[] successButtonSprites;
    public Sprite[] missButtonSprites; 

    [Header("Randomization (Implement Later LOL)")]
    public bool allAtOnce = false;
    private int currentIndex;
    private string[] buttonTypes = new string[] { "idle", "wait", "success", "miss" };   

    void Start()
    {
        //promptUIs = FindObjectsOfType<RunningPromptUI>();
        Debug.Log($"[RunningPhaseUI] Found {promptUIs.Length} RunningPromptUI instances.");

        currentIndex = Random.Range(0, idleButtonSprites.Length);

        AssignAllSprites();
    }

    void AssignAllSprites()
    {
        foreach (RunningPromptUI promptUI in promptUIs)
        {
            foreach (string buttonType in buttonTypes)
            {
                switch (buttonType)
                {
                    case("idle"):
                        promptUI.AssignButtonSprite(idleButtonSprites[currentIndex], buttonType);
                        break;
                    case("wait"):
                        promptUI.AssignButtonSprite(waitingButtonSprites[currentIndex], buttonType);
                        break;
                    case("success"):
                        promptUI.AssignButtonSprite(successButtonSprites[currentIndex], buttonType);
                        break;
                    case("miss"):
                        promptUI.AssignButtonSprite(missButtonSprites[currentIndex], buttonType);
                        break;
                }
            }
        }
    }
}
