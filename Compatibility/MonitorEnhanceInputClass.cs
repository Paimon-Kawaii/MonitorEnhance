using LethalCompanyInputUtils.Api;
using UnityEngine.InputSystem;

namespace MonitorEnhance;

public class MonitorEnhanceInputClass : LcInputActions
{
    public static readonly MonitorEnhanceInputClass Instance = new();

    [InputAction("<Mouse>/leftButton", ActionId = "MonitorEnhance:Primary", Name = "Primary")]
    public InputAction PRIMARY
    {
        get => InputUtil.INPUT_PRIMARY;
        set => InputUtil.INPUT_PRIMARY = value;
    }

    [InputAction("<Mouse>/rightButton", ActionId = "MonitorEnhance:Secondary", Name = "Secondary")]
    public InputAction SECONDARY
    {
        get => InputUtil.INPUT_SECONDARY;
        set => InputUtil.INPUT_SECONDARY = value;
    }

    [InputAction("<Keyboard>/e", ActionId = "MonitorEnhance:QuickSwitch", Name = "QuickSwitch")]
    public InputAction QUICK_SWITCH
    {
        get => InputUtil.INPUT_QUICKSWITCH;
        set => InputUtil.INPUT_QUICKSWITCH = value;
    }

    [InputAction("<Keyboard>/q", ActionId = "MonitorEnhance:AltQuickSwitch", Name = "Alt QuickSwitch")]
    public InputAction ALT_QUICK_SWITCH
    {
        get => InputUtil.INPUT_ALT_QUICKSWITCH;
        set => InputUtil.INPUT_ALT_QUICKSWITCH = value;
    }

    [InputAction("<Keyboard>/leftShift", ActionId = "MonitorEnhance:DoorSwitch", Name = "DoorSwitch")]
    public InputAction DOOR_SWITCH
    {
        get => InputUtil.INPUT_DOOR_SWITCH;
        set => InputUtil.INPUT_DOOR_SWITCH = value;
    }

    [InputAction("<Keyboard>/leftAlt", ActionId = "MonitorEnhance:TrapTrigger", Name = "TrapTrigger")]
    public InputAction TRAP_TRIGGER
    {
        get => InputUtil.INPUT_TRAP_TRIGGER;
        set => InputUtil.INPUT_TRAP_TRIGGER = value;
    }

    [InputAction("<Keyboard>/1", ActionId = "MonitorEnhance:QuickSwitch1", Name = "QuickSwitch Player1")]
    public InputAction QUICK_SWITCH_1
    {
        get => InputUtil.INPUT_QUICKSWITCH_1;
        set => InputUtil.INPUT_QUICKSWITCH_1 = value;
    }

    [InputAction("<Keyboard>/2", ActionId = "MonitorEnhance:QuickSwitch2", Name = "QuickSwitch Player2")]
    public InputAction QUICK_SWITCH_2
    {
        get => InputUtil.INPUT_QUICKSWITCH_2;
        set => InputUtil.INPUT_QUICKSWITCH_2 = value;
    }

    [InputAction("<Keyboard>/3", ActionId = "MonitorEnhance:QuickSwitch3", Name = "QuickSwitch Player3")]
    public InputAction QUICK_SWITCH_3
    {
        get => InputUtil.INPUT_QUICKSWITCH_3;
        set => InputUtil.INPUT_QUICKSWITCH_3 = value;
    }

    [InputAction("<Keyboard>/4", ActionId = "MonitorEnhance:QuickSwitch4", Name = "QuickSwitch Player4")]
    public InputAction QUICK_SWITCH_4
    {
        get => InputUtil.INPUT_QUICKSWITCH_4;
        set => InputUtil.INPUT_QUICKSWITCH_4 = value;
    }

    [InputAction("<Keyboard>/5", ActionId = "MonitorEnhance:QuickSwitch5", Name = "QuickSwitch Player5")]
    public InputAction QUICK_SWITCH_5
    {
        get => InputUtil.INPUT_QUICKSWITCH_5;
        set => InputUtil.INPUT_QUICKSWITCH_5 = value;
    }

    [InputAction("<Keyboard>/6", ActionId = "MonitorEnhance:QuickSwitch6", Name = "QuickSwitch Player6")]
    public InputAction QUICK_SWITCH_6
    {
        get => InputUtil.INPUT_QUICKSWITCH_6;
        set => InputUtil.INPUT_QUICKSWITCH_6 = value;
    }

    [InputAction("<Keyboard>/7", ActionId = "MonitorEnhance:QuickSwitch7", Name = "QuickSwitch Player7")]
    public InputAction QUICK_SWITCH_7
    {
        get => InputUtil.INPUT_QUICKSWITCH_7;
        set => InputUtil.INPUT_QUICKSWITCH_7 = value;
    }

    [InputAction("<Keyboard>/8", ActionId = "MonitorEnhance:QuickSwitch8", Name = "QuickSwitch Player8")]
    public InputAction QUICK_SWITCH_8
    {
        get => InputUtil.INPUT_QUICKSWITCH_8;
        set => InputUtil.INPUT_QUICKSWITCH_8 = value;
    }

    [InputAction("<Keyboard>/9", ActionId = "MonitorEnhance:QuickSwitch9", Name = "QuickSwitch Player9")]
    public InputAction QUICK_SWITCH_9
    {
        get => InputUtil.INPUT_QUICKSWITCH_9;
        set => InputUtil.INPUT_QUICKSWITCH_9 = value;
    }

    [InputAction("<Keyboard>/0", ActionId = "MonitorEnhance:QuickSwitch0", Name = "QuickSwitch Player0")]
    public InputAction QUICK_SWITCH_0
    {
        get => InputUtil.INPUT_QUICKSWITCH_0;
        set => InputUtil.INPUT_QUICKSWITCH_0= value;
    }
}