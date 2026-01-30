using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocalizationManager
{
    public static Languages Language = Languages.En;

    public static void SetLanguage(int numberLocate)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[numberLocate];
        Language = (Languages)numberLocate;

        PlayerPrefs.SetInt("Language", numberLocate);
        PlayerPrefs.Save();
    }

    public static string GetLocalizedString(TableNames table, string key, params object[] data)
    {
        var localizedString = LocalizationSettings.StringDatabase.GetLocalizedString(table.ToString(), key, data);

        return localizedString;
    }
}

public enum Languages
{
    En,
    Ru
}

public enum TableNames
{
    EnRu
}

