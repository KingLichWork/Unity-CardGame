using System.Linq;
using VContainer;

public class CardEffectResolver
{
    private CardMechanics _cardMechanics;
    private UIManager _uiManager;
    private SoundManager _soundManager;

    private PlayerState _playerState;
    private PlayerState _enemyState;

    [Inject]
    public void Construct(CardMechanics cardMechanics, UIManager uiManager, SoundManager soundManager)
    {
        _cardMechanics = cardMechanics;
        _uiManager = uiManager;
        _soundManager = soundManager;
    }

    public void Initialize(PlayerState playerState, PlayerState enemyState)
    {
        _playerState = playerState;
        _enemyState = enemyState;
    }

    public void HandleBoost(CardInfoScript card)
    {
        var boost = card.SelfCard.BoostOrDamage;

        if (boost.NearBoost == -1)
        {
            foreach (var ally in _playerState.Field.Cards)
            {
                _cardMechanics.Deployment(ally, card);

                if (card.SelfCard.StatusEffects.EnduranceOrBleedingOther != 0 &&
                    !card.SelfCard.StatusEffects.IsEnemyTargetEnduranceOrBleeding)
                {
                    _cardMechanics.BleedingOrEndurance(card, ally);
                    _uiManager.CheckBleeding(ally);
                }

                if (card.SelfCard.EndTurnActions.ArmorOther > 0)
                {
                    ally.SelfCard.BaseCard.ArmorPoints += card.SelfCard.EndTurnActions.ArmorOther;
                    _uiManager.CheckArmor(ally);
                }
            }

            _soundManager.PlayerStartEffectSound(card);
        }
    }

    public bool NeedBoostChoice(CardInfoScript card)
    {
        var boost = card.SelfCard.BoostOrDamage;

        if (boost.Boost == 0)
            return false;

        if (boost.NearBoost == -1)
            return false;

        int validTargets =
            _playerState.Field.Cards.Count -
            _playerState.Field.InvulnerabilityCards.Count;

        return validTargets > 1;
    }


    public void HandleDamage(CardInfoScript card)
    {
        var dmg = card.SelfCard.BoostOrDamage;

        if (dmg.NearDamage == -1)
        {
            foreach (var enemy in _enemyState.Field.Cards)
            {
                _cardMechanics.Deployment(enemy, card);

                if (card.SelfCard.StatusEffects.IsStunOther)
                {
                    enemy.SelfCard.StatusEffects.IsSelfStunned = true;
                    _cardMechanics.CheckStatusEffects(enemy);
                }

                if (card.SelfCard.StatusEffects.EnduranceOrBleedingOther != 0 &&
                    card.SelfCard.StatusEffects.IsEnemyTargetEnduranceOrBleeding)
                {
                    _cardMechanics.BleedingOrEndurance(card, enemy);
                }
            }

            _soundManager.PlayerStartEffectSound(card);
        }
    }

    public bool NeedDamageChoice(CardInfoScript card)
    {
        var dmg = card.SelfCard.BoostOrDamage;

        if (dmg.Damage == 0)
            return false;

        if (dmg.NearDamage == -1)
            return false;

        int validTargets =
            _enemyState.Field.Cards.Count -
            _enemyState.Field.InvulnerabilityCards.Count;

        return validTargets > 0;
    }

    public void HandleSelf(CardInfoScript card)
    {
        var boost = card.SelfCard.BoostOrDamage;

        if ((boost.SelfBoost != 0 || boost.SelfDamage != 0) &&
            !boost.AddictionWithAlliedField &&
            !boost.AddictionWithEnemyField)
        {
            _cardMechanics.Self(card, card);
            _soundManager.PlayerStartEffectSound(card);
        }
    }

    public void HandleSpawn(CardInfoScript card)
    {
        if (card.SelfCard.Spawns.SpawnCardCount == 0)
            return;

        if (card.SelfCard.BoostOrDamage.AddictionWithEnemyField)
            return;

        _soundManager.PlayerStartEffectSound(card);
        _cardMechanics.SpawnCard(card, true);
    }

    public bool HandleDestroy(CardInfoScript card)
    {
        int limit = card.SelfCard.UniqueMechanics.DestroyCardPoints;

        if (limit == 0)
            return false;

        if (limit == -1)
            return true; // нужен выбор цели

        return _enemyState.Field.Cards.Any(c =>
            c.SelfCard.BaseCard.Points <= limit &&
            !c.SelfCard.StatusEffects.IsInvulnerability);
    }

    public bool HandleSwap(CardInfoScript card)
    {
        if (!card.SelfCard.UniqueMechanics.SwapPoints)
            return false;

        bool enemyHas = (_enemyState.Field.Cards.Count - _enemyState.Field.InvulnerabilityCards.Count) > 0;
        bool playerHas = (_playerState.Field.Cards.Count - _playerState.Field.InvulnerabilityCards.Count) > 1;

        return enemyHas || playerHas;
    }

    public bool HandleBleedingChoice(CardInfoScript card)
    {
        var st = card.SelfCard.StatusEffects;

        if (st.EnduranceOrBleedingOther == 0)
            return false;

        if (st.IsEnemyTargetEnduranceOrBleeding)
            return (_enemyState.Field.Cards.Count - _enemyState.Field.InvulnerabilityCards.Count) > 0;

        return (_playerState.Field.Cards.Count - _playerState.Field.InvulnerabilityCards.Count) > 1;
    }
}