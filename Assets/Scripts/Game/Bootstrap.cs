using VContainer.Unity;
using UnityEngine;
using VContainer;

public class Bootstrap : IInitializable
{
    private SettingsPanel _settingsPanel;

    [Inject]
    private void Construct(SettingsPanel settingsPanel)
    {
        _settingsPanel = settingsPanel;
    }

    public void Initialize()
    {
        if (PlayerPrefs.HasKey("Language"))
            LocalizationManager.SetLanguage(PlayerPrefs.GetInt("Language"));
        else
            LocalizationManager.SetLanguage((int)Languages.En);

        _settingsPanel.Init();
    }
}
