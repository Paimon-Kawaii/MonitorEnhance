using System;
using System.Collections.Generic;
using System.Resources;
using MonitorEnhance.Lang;

namespace MonitorEnhance.Utils;
public class LocalizationManager
{
    private static ResourceManager resourceManager;

    public static Dictionary<string, Type> Languages { get; set; } = new()
    {
        { "en_us", typeof(en_US) },
        { "ko_kr", typeof(ko_KR) },
        { "zh_cn", typeof(zh_CN) }
    };

    public static void SetLanguage(string language)
    {
        if (Languages.ContainsKey(language.ToLower()))
        {
            resourceManager = new ResourceManager(Languages[language.ToLower()]);
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
