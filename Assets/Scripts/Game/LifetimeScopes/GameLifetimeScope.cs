using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private CardMechanics _cardMechanics;
    [SerializeField] private GameManager _gameManager;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<CardMechanics>(Lifetime.Singleton);
        builder.RegisterComponent(_gameManager);
    }
}
