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

    [Header("Manual Device Assignment")]
    public bool useManualDeviceAssignment = false;
    public InputDevice[] manualDeviceAssignment = new InputDevice[5];

    [Header("Debug Key Mappings")]
    public KeyCode leftArmLockKey = KeyCode.Q;
    public KeyCode rightArmLockKey = KeyCode.W;
    public KeyCode leftLegLockKey = KeyCode.A;
    public KeyCode rightLegLockKey = KeyCode.S;
    public KeyCode headLockKey = KeyCode.Space;

    [Header("Debug Info")]
    public bool showDetailedDebug = true;
    public bool logAllButtonPresses = true;

    private static InputManager instance;
    private Dictionary<int, InputDevice> deviceMap = new Dictionary<int, InputDevice>();

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
            InitializeDeviceMap();

            // Subscribe to device change events
            InputSystem.onDeviceChange += OnDeviceChange;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from device events
        if (instance == this)
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
        }
    }

    void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Added:
                Debug.Log($"Device connected: {device.displayName}");
                RefreshDevices();
                break;

            case InputDeviceChange.Removed:
                Debug.Log($"Device disconnected: {device.displayName}");

                // Remove from device map if it's assigned
                foreach (var kvp in new Dictionary<int, InputDevice>(deviceMap))
                {
                    if (kvp.Value == device)
                    {
                        deviceMap.Remove(kvp.Key);
                        Debug.Log($"Removed {device.displayName} from Player {kvp.Key}");
                    }
                }

                RefreshDevices();
                break;

            case InputDeviceChange.Reconnected:
                Debug.Log($"Device reconnected: {device.displayName}");
                RefreshDevices();
                break;
        }
    }

    public static InputManager Instance
    {
        get { return instance; }
    }

    void InitializeDeviceMap()
    {
        if (useManualDeviceAssignment)
        {
            for (int i = 0; i < manualDeviceAssignment.Length && i < 5; i++)
            {
                if (manualDeviceAssignment[i] != null)
                {
                    deviceMap[i] = manualDeviceAssignment[i];
                }
            }
        }
        else
        {
            AutoAssignDevices();
        }

        LogDeviceMap();
    }

    void AutoAssignDevices()
    {
        // Clean up invalid devices first
        CleanupInvalidDevices();

        deviceMap.Clear();

        // Get all unique input devices (excluding keyboard/mouse)
        List<InputDevice> allDevices = new List<InputDevice>();

        // Add all gamepads
        try
        {
            foreach (var gamepad in Gamepad.all)
            {
                if (gamepad != null && gamepad.added && !allDevices.Contains(gamepad))
                {
                    allDevices.Add(gamepad);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error reading gamepads: {e.Message}");
        }

        // Add joysticks that aren't already in the list as gamepads
        try
        {
            foreach (var joystick in Joystick.all)
            {
                if (joystick == null || !joystick.added) continue;

                // Check if this joystick is already registered as a gamepad
                bool isDuplicate = false;
                foreach (var device in allDevices)
                {
                    if (device.deviceId == joystick.deviceId)
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                if (!isDuplicate)
                {
                    allDevices.Add(joystick);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error reading joysticks: {e.Message}");
        }

        // Assign first 5 unique devices to players
        for (int i = 0; i < Mathf.Min(allDevices.Count, 5); i++)
        {
            deviceMap[i] = allDevices[i];
        }
    }

    void CleanupInvalidDevices()
    {
        // Remove any devices that are no longer connected
        List<int> keysToRemove = new List<int>();

        foreach (var kvp in deviceMap)
        {
            if (kvp.Value == null || !kvp.Value.added)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (int key in keysToRemove)
        {
            deviceMap.Remove(key);
        }
    }

    void LogDeviceMap()
    {
        Debug.Log("=== DEVICE MAPPING ===");

        // Check for duplicate device assignments
        Dictionary<int, List<int>> deviceIdToPlayers = new Dictionary<int, List<int>>();

        for (int i = 0; i < 5; i++)
        {
            if (deviceMap.ContainsKey(i) && deviceMap[i] != null)
            {
                int deviceId = deviceMap[i].deviceId;

                if (!deviceIdToPlayers.ContainsKey(deviceId))
                {
                    deviceIdToPlayers[deviceId] = new List<int>();
                }
                deviceIdToPlayers[deviceId].Add(i);

                Debug.Log($"Player {i} ({(LimbPlayer)i}): {deviceMap[i].displayName} (ID: {deviceMap[i].deviceId})");
            }
            else
            {
                Debug.Log($"Player {i} ({(LimbPlayer)i}): No device assigned");
            }
        }

        // Check for duplicates
        foreach (var kvp in deviceIdToPlayers)
        {
            if (kvp.Value.Count > 1)
            {
                string playerList = string.Join(", ", kvp.Value);
                Debug.LogError($"WARNING: Device ID {kvp.Key} is assigned to multiple players: {playerList}");
            }
        }

        Debug.Log("======================");
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
        bool result = false;

        if (inputMode == InputMode.Debug)
        {
            result = GetDebugLockButton(limb);
        }
        else
        {
            result = GetControllerLockButton(limb);
        }

        if (result && logAllButtonPresses)
        {
            Debug.Log($"[InputManager] Button pressed by: {limb} (Index {(int)limb})");
        }

        return result;
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

        // Try PlayerInput first
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
            catch { }
        }

        // Try device map
        if (deviceMap.ContainsKey(playerIndex) && deviceMap[playerIndex] != null)
        {
            return ReadAxisFromDevice(deviceMap[playerIndex]);
        }

        return 0f;
    }

    private bool GetControllerLockButton(LimbPlayer limb)
    {
        int playerIndex = (int)limb;

        // Try PlayerInput first
        if (playerIndex < playerInputs.Length && playerInputs[playerIndex] != null)
        {
            try
            {
                var lockAction = playerInputs[playerIndex].actions["Lock"];
                if (lockAction != null && lockAction.WasPressedThisFrame())
                {
                    if (logAllButtonPresses)
                    {
                        Debug.Log($"[InputManager] PlayerInput detected for {limb}");
                    }
                    return true;
                }
            }
            catch { }
        }

        // Try device map
        if (deviceMap.ContainsKey(playerIndex) && deviceMap[playerIndex] != null)
        {
            bool pressed = ReadButtonFromDevice(deviceMap[playerIndex]);
            if (pressed && logAllButtonPresses)
            {
                Debug.Log($"[InputManager] Device {deviceMap[playerIndex].displayName} pressed for {limb}");
            }
            return pressed;
        }

        return false;
    }

    private float ReadAxisFromDevice(InputDevice device)
    {
        try
        {
            // Check if device is still valid
            if (device == null || !device.added)
            {
                return 0f;
            }

            // Try gamepad
            if (device is Gamepad gamepad)
            {
                return gamepad.leftStick.x.ReadValue();
            }

            // Try joystick
            if (device is Joystick joystick)
            {
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
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error reading axis from device: {e.Message}");
        }

        return 0f;
    }

    private bool ReadButtonFromDevice(InputDevice device)
    {
        try
        {
            // Check if device is still valid
            if (device == null || !device.added)
            {
                return false;
            }

            // Try gamepad
            if (device is Gamepad gamepad)
            {
                return gamepad.buttonSouth.wasPressedThisFrame;
            }

            // Try joystick
            if (device is Joystick joystick)
            {
                // Try common button names
                string[] buttonNames = { "trigger", "button0", "button1", "button2", "button3" };

                foreach (string buttonName in buttonNames)
                {
                    if (joystick.TryGetChildControl<ButtonControl>(buttonName) is ButtonControl button)
                    {
                        if (button.wasPressedThisFrame)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error reading button from device: {e.Message}");
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

        if (deviceMap.ContainsKey(playerIndex) &&
            deviceMap[playerIndex] != null &&
            deviceMap[playerIndex].added)
        {
            return true;
        }

        return false;
    }

    public void RefreshDevices()
    {
        InitializeDeviceMap();
    }

    public void ManuallyAssignDevice(int playerIndex, InputDevice device)
    {
        if (playerIndex >= 0 && playerIndex < 5)
        {
            deviceMap[playerIndex] = device;
            Debug.Log($"Manually assigned {device.displayName} to Player {playerIndex} ({(LimbPlayer)playerIndex})");
        }
    }

    public float GetLimbVerticalAxis(LimbPlayer limb)
    {
        if (!IsControllerConnected((int)limb))
            return 0f;
        
        int controllerIndex = (int)limb;
        string axisName = $"Joy{controllerIndex + 1}_Vertical";
        
        try
        {
            return Input.GetAxis(axisName);
        }
        catch
        {
            return 0f;
        }
    }
}
