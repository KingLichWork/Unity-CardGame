using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyTargetSelector : IEnemyTargetSelector
{
    private readonly PlayerState _player;
    private readonly PlayerState _enemy;

    public EnemyTargetSelector(PlayerState player, PlayerState enemy)
    {
        _player = player;
        _enemy = enemy;
    }

    public CardInfoScript SelectRandomEnemy()
    {
        return SelectRandom(
            _player.Field.Cards,
            c => !c.SelfCard.StatusEffects.IsInvulnerability
        );
    }

    public CardInfoScript SelectEnemyByPoints(int maxPoints)
    {
        return SelectRandom(
            _player.Field.Cards,
            c => !c.SelfCard.StatusEffects.IsInvulnerability &&
                 c.SelfCard.BaseCard.Points <= maxPoints
        );
    }

    public CardInfoScript SelectRandomAlly(CardInfoScript exclude = null)
    {
        return SelectRandom(
            _enemy.Field.Cards,
            c => !c.SelfCard.StatusEffects.IsInvulnerability &&
                 c != exclude
        );
    }

    private CardInfoScript SelectRandom(IEnumerable<CardInfoScript> source, System.Func<CardInfoScript, bool> predicate)
    {
        var list = source.Where(predicate).ToList();
        if (list.Count == 0)
            return null;

        return list[Random.Range(0, list.Count)];
    }
}
