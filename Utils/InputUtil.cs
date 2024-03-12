using System;
using BepInEx;
using BepInEx.Bootstrap;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

namespace MonitorEnhance;

public static class InputUtil
{
    private static ScreenScript _screen_script = null;
    public static ScreenScript SCREEN_SCRIPT
    {
        get => _screen_script;
        internal set
        {
            // Check if ScreenScript is null
            if (value)
            { 
                // Script is not null / was enabled
                    setupActions(value);
            }
            else
            {
                // Script is null / was disabled
                clearActions();
            }

            _screen_script = value;
        }
    }

    // Inputs
    internal static InputAction INPUT_PRIMARY;
    internal static InputAction INPUT_SECONDARY;
    //internal static InputAction INPUT_TRAP_TRIGGER;
    internal static InputAction INPUT_QUICKSWITCH;
    internal static InputAction INPUT_DOOR_SWITCH;
    internal static InputAction INPUT_ALT_QUICKSWITCH;

    internal static InputAction INPUT_QUICKSWITCH_1;
    internal static InputAction INPUT_QUICKSWITCH_2;
    internal static InputAction INPUT_QUICKSWITCH_3;
    internal static InputAction INPUT_QUICKSWITCH_4;
    internal static InputAction INPUT_QUICKSWITCH_5;
    internal static InputAction INPUT_QUICKSWITCH_6;
    internal static InputAction INPUT_QUICKSWITCH_7;
    internal static InputAction INPUT_QUICKSWITCH_8;
    internal static InputAction INPUT_QUICKSWITCH_9;
    internal static InputAction INPUT_QUICKSWITCH_0; // 0 always self.

    // Input Actions
    private delegate void Execute();
    private static Execute _primaryExecute = () => { };
    private static Execute _secondaryExecute = () => { };
    private static Execute _doorSwitchExecute = () => { };
    //private static Execute _trapTriggerExecute = () => { };
    private static Execute _playerSwitchExecute = () => { };
    private static Execute _raderSwitchExecute = () => { };
    private static Execute _playerQuickSwitchExecute = () => { };

    /*
        Helper functions    
    */

    public static string GetButtonDescription(InputAction action)
    {
        bool isController = StartOfRound.Instance ? StartOfRound.Instance.localPlayerUsingController : false;
        bool isPS = isController && (Gamepad.current is DualShockGamepad || Gamepad.current is DualShock3GamepadHID || Gamepad.current is DualShock4GamepadHID);
        InputBinding? binding = null;
        foreach (InputBinding x in action.bindings)
        {
            if (isController && x.effectivePath.StartsWith("<Gamepad>"))
            {
                binding = x;
                break;
            }
            else if (!isController && (x.effectivePath.StartsWith("<Keyboard>") || x.effectivePath.StartsWith("<Mouse>")))
            {
                binding = x;
                break;
            }
        }

        string path = binding != null ? binding.Value.effectivePath : "";
        string[] splits = path.Split('/');
        return (splits.Length > 1 ? path : "") switch
        {
            // Mouse
            "<Mouse>/leftButton" => "鼠标左键",  // Uses 'Greek Capital Letter Mu' for M
            "<Mouse>/rightButton" => "鼠标右键", // Uses 'Greek Capital Letter Mu' for M
            // Keyboard
            "<Keyboard>/escape" => "ESC",
            "<Keyboard>/leftShift" => "左Shift",
            "<Keyboard>/leftAlt" => "左Alt",
            // Controller
            // Right buttons
            "<Gamepad>/buttonNorth" => isPS ? "△" : "Y",
            "<Gamepad>/buttonEast" => isPS ? "◯" : "B",
            "<Gamepad>/buttonSouth" => isPS ? "X" : "A",
            "<Gamepad>/buttonWest" => isPS ? "□" : "X",
            // Sticks
            "<Gamepad>/leftStickPress" => "L-Stick",
            "<Gamepad>/rightStickPress" => "R-Stick",
            // Shoulder, Trigger buttons
            "<Gamepad>/leftShoulder" => isPS ? "L1" : "L-Shoulder",
            "<Gamepad>/leftTrigger" => isPS ? "L2" : "L-Trigger",
            "<Gamepad>/rightShoulder" => isPS ? "R1" : "R-Shoulder",
            "<Gamepad>/rightTrigger" => isPS ? "R2" : "R-Trigger",
            _ => splits.Length > 1 ? splits[1].ToUpper() : "?"
        };
    }

    private static InputAction CreateKeybind(string key, string binding, Action<InputAction.CallbackContext> action)
    {
        InputAction inputAction = new InputAction(
            name: key,
            type: InputActionType.Button,
            binding: binding
        );
        inputAction.performed += action;
        inputAction.Enable();
        Plugin.LOGGER.LogInfo($"Set {key} button to: {GetButtonDescription(inputAction)}");
        return inputAction;
    }

    private static void setupActions(ScreenScript script)
    {
        Plugin.LOGGER.LogInfo(" > Setup actions");

        _doorSwitchExecute += script.ShipDoor;
        //_trapTriggerExecute += script.TriggerTrap;
        _primaryExecute += () => script.OnPlayerInteraction(false);
        _secondaryExecute += () => script.OnPlayerInteraction(true);
        _playerSwitchExecute += () => script.OnMonitorQuickSwitch(false);
        _raderSwitchExecute += () => script.OnMonitorQuickSwitch(true);
        _playerQuickSwitchExecute += script.OnPlayerQuickSwitchByNum;
    }

    private static void clearActions()
    {
        Plugin.LOGGER.LogInfo(" > Clear actions");
        _primaryExecute = () => { };
        _secondaryExecute = () => { };
        _doorSwitchExecute = () => { };
        //_trapTriggerExecute = () => { };
        _playerSwitchExecute = () => { };
        _raderSwitchExecute = () => { };
        _playerQuickSwitchExecute = () => { };
    }

    /*
        Setup    
    */
    internal static void Setup()
    {
        // Create keybinds
        Plugin.Supplier<bool> _iu_create = () => MonitorEnhanceInputClass.Instance != null;
        if (Chainloader.PluginInfos.TryGetValue("com.rune580.LethalCompanyInputUtils", out PluginInfo iu) && _iu_create.Invoke())
        {
            INPUT_PRIMARY.performed += _ => _primaryExecute();
            INPUT_SECONDARY.performed += _ => _secondaryExecute();
            //INPUT_TRAP_TRIGGER.performed += _ => _trapTriggerExecute();
            INPUT_DOOR_SWITCH.performed += _ => _doorSwitchExecute();
            INPUT_QUICKSWITCH.performed += _ => _raderSwitchExecute();
            INPUT_ALT_QUICKSWITCH.performed += _ => _playerSwitchExecute();

            INPUT_QUICKSWITCH_1.performed += _ => _playerQuickSwitchExecute();
            INPUT_QUICKSWITCH_2.performed += _ => _playerQuickSwitchExecute();
            INPUT_QUICKSWITCH_3.performed += _ => _playerQuickSwitchExecute();
            INPUT_QUICKSWITCH_4.performed += _ => _playerQuickSwitchExecute();
            INPUT_QUICKSWITCH_5.performed += _ => _playerQuickSwitchExecute();
            INPUT_QUICKSWITCH_6.performed += _ => _playerQuickSwitchExecute();
            INPUT_QUICKSWITCH_7.performed += _ => _playerQuickSwitchExecute();
            INPUT_QUICKSWITCH_8.performed += _ => _playerQuickSwitchExecute();
            INPUT_QUICKSWITCH_9.performed += _ => _playerQuickSwitchExecute();
            INPUT_QUICKSWITCH_0.performed += _ => _playerQuickSwitchExecute();

            Plugin.LOGGER.LogInfo($" > Hooked into InputUtils {iu.Metadata.Version}");
        }
        else
        {
            INPUT_PRIMARY = CreateKeybind(
                "MonitorEnhance:Primary",
                ConfigUtil.CONFIG_PRIMARY.Value,
                _ => _primaryExecute()
            );
            INPUT_SECONDARY = CreateKeybind(
                "MonitorEnhance:Secondary",
                ConfigUtil.CONFIG_SECONDARY.Value,
                _ => _secondaryExecute()
            );
            INPUT_QUICKSWITCH = CreateKeybind(
                "MonitorEnhance:QuickSwitch",
                ConfigUtil.CONFIG_QUICK_SWITCH.Value,
                _ => _playerSwitchExecute()
            );
            INPUT_DOOR_SWITCH = CreateKeybind(
                "MonitorEnhance:DoorSwitch",
                ConfigUtil.CONFIG_DOOR_SWITCH.Value,
                _ => _doorSwitchExecute()
            );
            //INPUT_TRAP_TRIGGER = CreateKeybind(
            //    "MonitorEnhance:TrapTrigger",
            //    ConfigUtil.CONFIG_TRAP_TRIGGER.Value,
            //    _ => _trapTriggerExecute()
            //);
            INPUT_ALT_QUICKSWITCH = CreateKeybind(
                "MonitorEnhance:AltQuickSwitch",
                ConfigUtil.CONFIG_ALT_QUICK_SWITCH.Value,
                _ => _raderSwitchExecute()
            );
        }
    }
}