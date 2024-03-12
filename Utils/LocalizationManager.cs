using System;
using System.Collections.Generic;
using System.Resources;
using MonitorEnhance.Lang;

namespace MonitorEnhance.Utils;
public class LocalizationManager
{
    private static ResourceManager resourceManager;

    private static Dictionary<string, Type> languages = new Dictionary<string, Type>
    {
        { "en_us", typeof(en_US) },
        { "zh_cn", typeof(zh_CN) }
    };

    public static Dictionary<string, Type> Languages
    {
        get => languages;
        set => languages = value;
    }

    public static void SetLanguage(string language)
    {
        if (Languages.ContainsKey(language))
        {
            resourceManager = new ResourceManager(Languages[language]);
        }
        else
        {
            resourceManager = new ResourceManager(typeof(en_US));
        }
    }

    public static string GetString(string key)
    {
        try
        {
            return resourceManager.GetString(key);
        }
        catch (Exception)
        {
            return "Missing translation for key: " + key;
        }
    }
}
