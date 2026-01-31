using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private CardMechanics _cardMechanics;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private EndGamePanel _endGamePanel;
    [SerializeField] private InputController _inputController;
    [SerializeField] private SoundManager _soundManager;
    //[SerializeField] private HowToPlay _howToPlay;
    [SerializeField] private EffectsManager _effectsManager;
    [SerializeField] private CardView _cardView;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<CardMechanics>(Lifetime.Singleton);
        builder.Register<CardSelectionService>(Lifetime.Singleton);
        builder.Register<LocalizationManager>(Lifetime.Singleton);

        builder.RegisterComponent(_gameManager);
        builder.RegisterComponent(_uiManager);
        builder.RegisterComponent(_endGamePanel);
        builder.RegisterComponent(_inputController);
        builder.RegisterComponent(_soundManager);
        builder.RegisterComponent(_cardView);
        builder.RegisterComponent(_effectsManager);

        //builder.RegisterComponent(_howToPlay);
    }
}
