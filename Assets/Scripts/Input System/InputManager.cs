using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

public class InputManager : MonoBehaviour
{
    [Header("Mode Settings")]
    public InputMode inputMode = InputMode.Debug;
    
    [Header("Player Assignments")]
    public PlayerInput[] playerInputs = new PlayerInput[5];
    
    [Header("Device Detection")]
    public bool useJoystickFallback = true;
    
    [Header("Debug Key Mappings")]
    public KeyCode leftArmLockKey = KeyCode.Q;
    public KeyCode rightArmLockKey = KeyCode.W;
    public KeyCode leftLegLockKey = KeyCode.A;
    public KeyCode rightLegLockKey = KeyCode.S;
    public KeyCode headLockKey = KeyCode.Space;
    
    [Header("Debug Info")]
    public bool showDetailedDebug = true;
    
    private static InputManager instance;
    
    public enum InputMode
    {
        Debug,
        Game
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
                return Input.GetAxis("Horizontal");
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

    private float GetControllerHorizontalAxis(LimbPlayer limb)
    {
        int playerIndex = (int)limb;

        if (playerIndex < playerInputs.Length && playerInputs[playerIndex] != null)
        {
            try
            {
                var moveAction = playerInputs[playerIndex].actions["Move"];
                if (moveAction != null)
                {
                    return moveAction.ReadValue<Vector2>().x;
                }
            }
            catch
            {
                // Fall through
            }
        }

        // Try gamepad first
        if (playerIndex < Gamepad.all.Count)
        {
            return Gamepad.all[playerIndex].leftStick.x.ReadValue();
        }

        // Fall back to joystick with offset
        if (useJoystickFallback)
        {
            int joystickIndex = playerIndex - Gamepad.all.Count;
            if (joystickIndex >= 0 && joystickIndex < Joystick.all.Count)
            {
                var joystick = Joystick.all[joystickIndex];

                if (joystick.stick != null)
                {
                    return joystick.stick.x.ReadValue();
                }

                if (joystick.TryGetChildControl<AxisControl>("x") is AxisControl xAxis)
                {
                    return xAxis.ReadValue();
                }


            }
        }

        return 0f;
    }

    private bool GetControllerLockButton(LimbPlayer limb)
    {
        int playerIndex = (int)limb;

        if (playerIndex < playerInputs.Length && playerInputs[playerIndex] != null)
        {
            try
            {
                var lockAction = playerInputs[playerIndex].actions["Lock"];
                if (lockAction != null)
                {
                    return lockAction.WasPressedThisFrame();
                }
            }
            catch
            {
                // Fall through
            }
        }

        // Try gamepad first
        if (playerIndex < Gamepad.all.Count)
        {
            return Gamepad.all[playerIndex].buttonSouth.wasPressedThisFrame;
        }

        // Fall back to joystick with offset
        if (useJoystickFallback)
        {
            int joystickIndex = playerIndex - Gamepad.all.Count;
            if (joystickIndex >= 0 && joystickIndex < Joystick.all.Count)
            {
                var joystick = Joystick.all[joystickIndex];

                if (joystick.TryGetChildControl<ButtonControl>("trigger") is ButtonControl trigger)
                {
                    if (trigger.wasPressedThisFrame) return true;
                }

                if (joystick.TryGetChildControl<ButtonControl>("button0") is ButtonControl button0)
                {
                    if (button0.wasPressedThisFrame) return true;
                }

                if (joystick.TryGetChildControl<ButtonControl>("button1") is ButtonControl button1)
                {
                    if (button1.wasPressedThisFrame) return true;
                }

                if (joystick.TryGetChildControl<ButtonControl>("button2") is ButtonControl button2)
                {
                    if (button2.wasPressedThisFrame) return true;
                }

                if (joystick.TryGetChildControl<ButtonControl>("button3") is ButtonControl button3)
                {
                    if (button3.wasPressedThisFrame) return true;
                }
            }
        }

        return false;
    }

    public void SetInputMode(InputMode mode)
    {
        inputMode = mode;
        Debug.Log($"Input mode changed to: {mode}");
    }

    public bool IsControllerConnected(int playerIndex)
    {
        if (inputMode == InputMode.Debug) return true;

        if (playerIndex < playerInputs.Length &&
            playerInputs[playerIndex] != null &&
            playerInputs[playerIndex].devices.Count > 0)
        {
            return true;
        }

        if (playerIndex < Gamepad.all.Count && Gamepad.all[playerIndex] != null)
        {
            return true;
        }

        if (useJoystickFallback)
        {
            int joystickIndex = playerIndex - Gamepad.all.Count;
            if (joystickIndex >= 0 && joystickIndex < Joystick.all.Count && Joystick.all[joystickIndex] != null)
            {
                return true;
            }
        }

        return false;
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 500, 600));
        
        GUILayout.Label("=== INPUT MANAGER DEBUG ===");
        GUILayout.Label($"Current Mode: {inputMode}");
        GUILayout.Label($"Total Gamepads: {Gamepad.all.Count}");
        GUILayout.Label($"Total Joysticks: {Joystick.all.Count}");
        
        GUILayout.Space(10);
        
        if (inputMode == InputMode.Game)
        {
            GUILayout.Label("=== CONTROLLER STATUS ===");
            
            for (int i = 0; i < 5; i++)
            {
                bool connected = IsControllerConnected(i);
                string status = connected ? "Connected" : "Not Connected";
                string limbName = ((LimbPlayer)i).ToString();
                GUILayout.Label($"{limbName} (Index {i}): {status}");
                
                if (connected || Gamepad.all.Count > i || Joystick.all.Count > i)
                {
                    float axis = GetControllerHorizontalAxis((LimbPlayer)i);
                    bool button = GetControllerLockButton((LimbPlayer)i);
                    GUILayout.Label($"  Axis: {axis:F2} | Button: {button}");
                }
            }
        }
        else
        {
            GUILayout.Label("=== DEBUG MODE ===");
            GUILayout.Label("Using Keyboard Controls");
        }
        
        GUILayout.EndArea();
    }
}