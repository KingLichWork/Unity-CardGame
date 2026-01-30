using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MenuLifetimeScope : LifetimeScope
{
    [SerializeField] private SettingsPanel _settingsPanel;
    [SerializeField] private MenuPanel _menuPanel;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(_settingsPanel);
        builder.RegisterComponent(_menuPanel);

        builder.Register<LocalizationManager>(Lifetime.Singleton).AsSelf();

        builder.RegisterEntryPoint<Bootstrap>();
    }
}