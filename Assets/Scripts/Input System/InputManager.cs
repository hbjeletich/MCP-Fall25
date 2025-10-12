using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("Mode Settings")]
    public InputMode inputMode = InputMode.Debug;
    
    [Header("Player Assignments")]
    public PlayerInput[] playerInputs = new PlayerInput[5];
    
    [Header("Debug Key Mappings")]
    public KeyCode leftArmLockKey;
    public KeyCode rightArmLockKey;
    public KeyCode leftLegLockKey;
    public KeyCode rightLegLockKey;
    public KeyCode headLockKey;
    
    private static InputManager instance;
    
    public enum InputMode
    {
        Debug,      // Keyboard controls
        Game        // 5 controllers
    }
    
    public enum LimbPlayer
    {
        LeftArm = 0,
        RightArm = 1,
        LeftLeg = 2,
        RightLeg = 3,
        Head = 4
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static InputManager Instance
    {
        get { return instance; }
    }

    public float GetLimbHorizontalAxis(LimbPlayer limb)
    {
        if (inputMode == InputMode.Debug)
        {
            return GetDebugHorizontalAxis(limb);
        }
        else
        {
            return GetControllerHorizontalAxis(limb);
        }
    }

    public bool GetLimbLockButtonDown(LimbPlayer limb)
    {
        if (inputMode == InputMode.Debug)
        {
            return GetDebugLockButton(limb);
        }
        else
        {
            return GetControllerLockButton(limb);
        }
    }

    private float GetDebugHorizontalAxis(LimbPlayer limb)
    {
        switch (limb)
        {
            case LimbPlayer.LeftArm:
                return Input.GetAxis("Horizontal"); // A/D or Left/Right arrows
            case LimbPlayer.RightArm:
                return Input.GetKey(KeyCode.J) ? -1f : Input.GetKey(KeyCode.L) ? 1f : 0f;
            case LimbPlayer.LeftLeg:
                return Input.GetKey(KeyCode.Z) ? -1f : Input.GetKey(KeyCode.C) ? 1f : 0f;
            case LimbPlayer.RightLeg:
                return Input.GetKey(KeyCode.N) ? -1f : Input.GetKey(KeyCode.M) ? 1f : 0f;
            case LimbPlayer.Head:
                return Input.GetKey(KeyCode.LeftBracket) ? -1f : Input.GetKey(KeyCode.RightBracket) ? 1f : 0f;
            default:
                return 0f;
        }
    }

    private bool GetDebugLockButton(LimbPlayer limb)
    {
        switch (limb)
        {
            case LimbPlayer.LeftArm:
                return Input.GetKeyDown(leftArmLockKey);
            case LimbPlayer.RightArm:
                return Input.GetKeyDown(rightArmLockKey);
            case LimbPlayer.LeftLeg:
                return Input.GetKeyDown(leftLegLockKey);
            case LimbPlayer.RightLeg:
                return Input.GetKeyDown(rightLegLockKey);
            case LimbPlayer.Head:
                return Input.GetKeyDown(headLockKey);
            default:
                return false;
        }
    }

    // Game mode - controller inputs
    private float GetControllerHorizontalAxis(LimbPlayer limb)
    {
        int playerIndex = (int)limb;
        
        if (playerIndex < playerInputs.Length && playerInputs[playerIndex] != null)
        {
            // read from the controller's left stick X axis
            return playerInputs[playerIndex].actions["Move"].ReadValue<Vector2>().x;
        }
        
        return 0f;
    }

    private bool GetControllerLockButton(LimbPlayer limb)
    {
        int playerIndex = (int)limb;
        
        if (playerIndex < playerInputs.Length && playerInputs[playerIndex] != null)
        {
            // read the "Lock" action (mapped to a button like South/A button)
            return playerInputs[playerIndex].actions["Lock"].WasPressedThisFrame();
        }
        
        return false;
    }

    public void SetInputMode(InputMode mode)
    {
        inputMode = mode;
        Debug.Log($"Input mode changed to: {mode}");
    }

    // Check if a specific controller is connected
    public bool IsControllerConnected(int playerIndex)
    {
        if (inputMode == InputMode.Debug) return true; // Always true in debug mode
        
        return playerIndex < playerInputs.Length && 
               playerInputs[playerIndex] != null && 
               playerInputs[playerIndex].devices.Count > 0;
    }

    void OnGUI()
    {
        if (inputMode == InputMode.Game)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Controller Status:");
            
            for (int i = 0; i < 5; i++)
            {
                bool connected = IsControllerConnected(i);
                string status = connected ? "✓ Connected" : "✗ Not Connected";
                string limbName = ((LimbPlayer)i).ToString();
                GUILayout.Label($"{limbName}: {status}");
            }
            
            GUILayout.EndArea();
        }
    }
}
