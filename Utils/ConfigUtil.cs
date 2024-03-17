using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using MonitorEnhance.Utils;
using UnityEngine;

namespace MonitorEnhance;

public static class ConfigUtil
{
    public static Sprite HOVER_ICON
    {
        get; private set;
    }
    public static ConfigEntry<string> CONFIG_PRIMARY
    {
        get; private set;
    }
    public static ConfigEntry<string> CONFIG_SECONDARY
    {
        get; private set;
    }
    public static ConfigEntry<string> CONFIG_QUICK_SWITCH
    {
        get; private set;
    }
    public static ConfigEntry<string> CONFIG_DOOR_SWITCH
    {
        get; private set;
    }
    public static ConfigEntry<string> CONFIG_SELECT_TARGET
    {
        get; private set;
    }
    public static ConfigEntry<string> CONFIG_ALT_QUICK_SWITCH
    {
        get; private set;
    }
    //public static ConfigEntry<bool> CONFIG_ALT_REVERSE
    //{
    //    get; private set;
    //}
    //public static ConfigEntry<bool> CONFIG_SHOW_POINTER
    //{
    //    get; private set;
    //}
    public static ConfigEntry<bool> CONFIG_SHOW_TOOLTIP
    {
        get; private set;
    }
    public static ConfigEntry<string> CONFIG_LANGUAGE
    {
        get; private set;
    }

    public static bool IGNORE_OVERRIDE { get; private set; } = false;

    internal static void Setup(ConfigFile config, string pluginFolder)
    {
        // Language
        CONFIG_LANGUAGE = config.Bind("Config", "Languge", "en_US", "Mod languge");
        // Keybinds
        CONFIG_PRIMARY = config.Bind(
            "Layout", "Primary",
            "<Mouse>/leftButton",
            """
            Name of the key mapping for the primary (switch, ping, trigger) actions
            Allowed value format: "<Keyboard>/KEY", "<Mouse>/BUTTON", "<Gamepad>/BUTTON"
            Examples: "<Keyboard>/f" "<Mouse>/leftButton" "<Gamepad>/buttonNorth"
            For in depth instructions see: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.InputControlPath.html
            """
        );
        CONFIG_SECONDARY = config.Bind(
            "Layout", "Secondary",
            "<Mouse>/rightButton",
            """
            Name of the key mapping for the secondary (Flash) action
            Allowed value format: "<Keyboard>/KEY", "<Mouse>/BUTTON", "<Gamepad>/BUTTON"
            Examples: "<Keyboard>/g" "<Mouse>/rightButton" "<Gamepad>/buttonWest"
            For in depth instructions see: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.InputControlPath.html
            """
        );
        CONFIG_QUICK_SWITCH = config.Bind(
            "Layout", "Switch",
            "<Keyboard>/e",
            """
            Name of the key mapping for the quick switch action
            Allowed value format: "<Keyboard>/KEY", "<Mouse>/BUTTON", "<Gamepad>/BUTTON"
            Examples: "<Keyboard>/g" "<Mouse>/rightButton" "<Gamepad>/buttonWest"
            For in depth instructions see: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.InputControlPath.html
            """
        );
        CONFIG_DOOR_SWITCH = config.Bind(
            "Layout", "DoorSwitch",
            "<Keyboard>/leftShift",
            """
            Name of the key mapping for the quick door switch action
            Allowed value format: "<Keyboard>/KEY", "<Mouse>/BUTTON", "<Gamepad>/BUTTON"
            Examples: "<Keyboard>/g" "<Mouse>/rightButton" "<Gamepad>/buttonWest"
            For in depth instructions see: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.InputControlPath.html
            """
        );
        CONFIG_SELECT_TARGET = config.Bind(
            "Layout", "SelectTarget",
            "<Keyboard>/r",
            """
            Name of the key mapping for the trap trigger action
            Allowed value format: "<Keyboard>/KEY", "<Mouse>/BUTTON", "<Gamepad>/BUTTON"
            Examples: "<Keyboard>/g" "<Mouse>/rightButton" "<Gamepad>/buttonWest"
            For in depth instructions see: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.InputControlPath.html
            """
        );
        //CONFIG_ALT_REVERSE = config.Bind(
        //    "Layout", "ReverseSwitch",
        //    true,
        //    """
        //    Decides what the "SwitchAlternative" does when pressed
        //    true: When the alternative key is pressed, the quick switch will go through the reverse order
        //    false: When the alternative key is pressed the previous radar target will be selected
        //    """
        //);
        CONFIG_ALT_QUICK_SWITCH = config.Bind(
            "Layout", "SwitchAlternative",
            "<Keyboard>/q",
            """
            Name of the key mapping for the alternative quick switch action
            The behaviour of the key is dependent on the "ReverseSwitch" option
            Allowed value format: "<Keyboard>/KEY", "<Mouse>/BUTTON", "<Gamepad>/BUTTON"
            Examples: "<Keyboard>/g" "<Mouse>/rightButton" "<Gamepad>/buttonWest"
            For in depth instructions see: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.InputControlPath.html
            """
        );

        // Image
        IGNORE_OVERRIDE = config.Bind(
            "Features", "IgnoreOverride",
            false,
            """
            Set if other plugins can disable / enable the MonitorEnhance feature.
             > "true": Other plugins can no longer toggle it
             > "false": Other plugins may disable / enable it
            """
        ).Value;
        //ConfigEntry<string> imagePath = config.Bind(
        //    "UI", "PointerIcon",
        //    "HoverIcon.png",
        //    string.Format("""
        //    Accepts a file name relative to the plugin name or a full system path
        //    You can either choose one of the three default icons "HoverIcon.png", "CrossIcon.png", "DotIcon.png" or
        //    create your own (Only .png and .jpg are supported) and place it in: {0}
        //    Examples: "HoverIcon.png" or "X:\Images\SomeImage.png"
        //    """, pluginFolder)
        //);
        //CONFIG_SHOW_POINTER = config.Bind(
        //    "UI", "ShowPointer",
        //    false,
        //    string.Format("""
        //    Enable / Disable the pointer when hovering over the monitor
        //    """, pluginFolder)
        //);
        CONFIG_SHOW_TOOLTIP = config.Bind(
            "UI", "ShowTooltip",
            true,
            string.Format("""
            Enable / Disable the keybind tooltip when hovering over the monitor
            """, pluginFolder)
        );

        bool flag = CONFIG_LANGUAGE.Value.IsNullOrWhiteSpace();
        Plugin.LOGGER.LogInfo($"> 语言: {CONFIG_LANGUAGE.Value}");
        LocalizationManager.SetLanguage(flag? System.Globalization.CultureInfo.InstalledUICulture.Name: CONFIG_LANGUAGE.Value);
        // Try to resolve imagePath to full path
        //string iconPath;
        //if (imagePath.Value.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || imagePath.Value.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
        //{
        //    // Check if provided imagePath is relative file name or full path
        //    iconPath = (Path.GetFileName(imagePath.Value) == imagePath.Value) ?
        //        Path.Combine(pluginFolder, imagePath.Value) :
        //        imagePath.Value;
        //}
        //else
        //{
        //    Plugin.LOGGER.LogWarning("The provided icon file extension is not supported. Please make sure it's either a .png or .jpg file. Trying to use default icon...");
        //    iconPath = Path.Combine(pluginFolder, imagePath.DefaultValue.ToString());
        //}

        Plugin.LOGGER.LogWarning(" > =======图片测试开始======== ");
        Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MonitorEnhance.Resources.HoverIcon.png");

        if (stream is not null)
        {
            Plugin.LOGGER.LogWarning($" > 长度 {stream.Length} ");
            Texture2D tex = new(0, 0);
            tex.LoadImage(StreamToBytes(stream));

            HOVER_ICON = Sprite.Create(
                tex,
                new Rect(0f, 0f, tex.width, tex.height),
                new Vector2(.5f, .5f),
                100f
            );

            stream.Dispose();
            stream.Close();
        }

        Plugin.LOGGER.LogWarning(" > =======图片结束开始======== ");
    }

    private static byte[] StreamToBytes(Stream stream)
    {
        byte[] bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);
        // 设置当前流的位置为流的开始 
        stream.Seek(0, SeekOrigin.Begin);

        return bytes;
    }
}