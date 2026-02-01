using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public class CardEffectResolver
{
    private CardMechanics _cardMechanics;
    private UIManager _uiManager;
    private SoundManager _soundManager;

    private PlayerState _playerState;
    private PlayerState _enemyState;

    private EnemyTargetSelector _targetSelector;

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

        _targetSelector = new EnemyTargetSelector(_playerState, _enemyState);
    }

    public void HandleBoost(CardInfoScript card, EffectOwner owner)
    {
        var boost = card.SelfCard.BoostOrDamage;

        if (boost.NearBoost != -1)
            return;

        var ally = GetOwnerState(owner);

        foreach (var target in ally.Field.Cards)
        {
            _cardMechanics.Deployment(target, card);

            if (card.SelfCard.StatusEffects.EnduranceOrBleedingOther != 0 &&
                !card.SelfCard.StatusEffects.IsEnemyTargetEnduranceOrBleeding)
            {
                _cardMechanics.BleedingOrEndurance(card, target);
                _uiManager.CheckBleeding(target);
            }

            if (card.SelfCard.EndTurnActions.ArmorOther > 0)
            {
                target.SelfCard.BaseCard.ArmorPoints += card.SelfCard.EndTurnActions.ArmorOther;
                _uiManager.CheckArmor(target);
            }
        }

        _soundManager.StartEffectSound(card);      
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


    public void HandleDamage(CardInfoScript card, EffectOwner owner)
    {
        var dmg = card.SelfCard.BoostOrDamage;

        if (dmg.NearDamage != -1)
            return;

        var opponent = GetOpponentState(owner);

        foreach (var target in opponent.Field.Cards)
        {
            _cardMechanics.Deployment(target, card);

            if (card.SelfCard.StatusEffects.IsStunOther)
            {
                target.SelfCard.StatusEffects.IsSelfStunned = true;
                _cardMechanics.CheckStatusEffects(target);
            }

            if (card.SelfCard.StatusEffects.EnduranceOrBleedingOther != 0 &&
                card.SelfCard.StatusEffects.IsEnemyTargetEnduranceOrBleeding)
            {
                _cardMechanics.BleedingOrEndurance(card, target);
            }
        }

        _soundManager.StartEffectSound(card);
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
            _soundManager.StartEffectSound(card);
        }
    }

    public void HandleSpawn(CardInfoScript card)
    {
        if (card.SelfCard.Spawns.SpawnCardCount == 0)
            return;

        if (card.SelfCard.BoostOrDamage.AddictionWithEnemyField)
            return;

        _soundManager.StartEffectSound(card);
        _cardMechanics.SpawnCard(card, true);
    }

    public bool HandleDestroy(CardInfoScript card)
    {
        int limit = card.SelfCard.UniqueMechanics.DestroyCardPoints;

        if (limit == 0)
            return false;

        if (limit == -1)
            return true;

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

    public void ResolveDamageWithAutoTarget(CardInfoScript card)
    {
        var dmg = card.SelfCard.BoostOrDamage;

        var target = _targetSelector.SelectRandomEnemy();
        if (target == null)
            return;

        _cardMechanics.Deployment(target, card);

        if (card.SelfCard.StatusEffects.IsStunOther)
        {
            target.SelfCard.StatusEffects.IsSelfStunned = true;
            _cardMechanics.CheckStatusEffects(target);
        }

        ApplyBleedingIfNeeded(card, target, enemyTarget: true);

        if (dmg.NearDamage > 0)
        {
            target.CheckSiblingIndex();

            ApplyNearEffect(target, dmg.NearDamage, card, isAlly: false);
        }

        if (!card.SelfCard.BoostOrDamage.AddictionWithEnemyField)
            _soundManager.StartEffectSound(card);
    }

    public void ResolveBoostWithAutoTarget(CardInfoScript card)
    {
        var boost = card.SelfCard.BoostOrDamage;

        var target = _targetSelector.SelectRandomAlly(card);
        if (target == null)
            return;

        _cardMechanics.Deployment(target, card);

        if (card.SelfCard.StatusEffects.IsShieldOther)
        {
            target.SelfCard.StatusEffects.IsSelfShielded = true;
            _cardMechanics.CheckStatusEffects(target);
        }

        if (boost.NearBoost > 0)
        {
            target.CheckSiblingIndex();

            ApplyNearEffect(target, boost.NearBoost, card, isAlly: true);
        }

        ApplyBleedingIfNeeded(card, target, enemyTarget: false);

        _soundManager.StartEffectSound(card);
    }

    private void ApplyBleedingIfNeeded(CardInfoScript source, CardInfoScript target, bool enemyTarget)
    {
        var st = source.SelfCard.StatusEffects;

        if (st.EnduranceOrBleedingOther == 0)
            return;

        if (st.IsEnemyTargetEnduranceOrBleeding != enemyTarget)
            return;

        _cardMechanics.BleedingOrEndurance(source, target);
        _uiManager.CheckBleeding(target);
    }

    public void ResolveEnemyUnique(CardInfoScript card)
    {
        ResolveEnemyDestroy(card);
        ResolveEnemySwap(card);
        ResolveEnemyBleeding(card);

        if (!string.IsNullOrEmpty(card.SelfCard.UniqueMechanics.TransformationCardName))
        {
            _cardMechanics.Transformation(card);
        }
    }

    private void ResolveEnemyDestroy(CardInfoScript card)
    {
        int limit = card.SelfCard.UniqueMechanics.DestroyCardPoints;

        if (limit == 0)
            return;

        CardInfoScript target = limit == -1 ? _targetSelector.SelectRandomEnemy() : _targetSelector.SelectEnemyByPoints(limit);

        if (target == null)
            return;

        _cardMechanics.DestroyCard(target, card);
    }

    private void ResolveEnemySwap(CardInfoScript card)
    {
        if (!card.SelfCard.UniqueMechanics.SwapPoints)
            return;

        var enemyTarget = _targetSelector.SelectRandomEnemy();
        if (enemyTarget != null)
        {
            _cardMechanics.SwapPoints(card, enemyTarget);
            return;
        }

        var allyTarget = _targetSelector.SelectRandomAlly(card);
        if (allyTarget != null)
            _cardMechanics.SwapPoints(card, allyTarget);
    }

    private void ResolveEnemyBleeding(CardInfoScript card)
    {
        var st = card.SelfCard.StatusEffects;
        if (st.EnduranceOrBleedingOther == 0)
            return;

        CardInfoScript target = null;

        if (st.IsEnemyTargetEnduranceOrBleeding &&
            card.SelfCard.BoostOrDamage.NearBoost != -1)
        {
            target = _targetSelector.SelectRandomEnemy();
        }
        else if (card.SelfCard.BoostOrDamage.NearDamage != -1)
        {
            target = _targetSelector.SelectRandomAlly(card);
        }

        if (target == null)
            return;

        _cardMechanics.BleedingOrEndurance(card, target);
        _uiManager.CheckBleeding(target);
    }

    private void ApplyNearEffect(CardInfoScript center, int radius, CardInfoScript source, bool isAlly)
    {
        var left = _cardMechanics.ReturnNearCard(center, radius, true);
        var right = _cardMechanics.ReturnNearCard(center, radius, false);

        ApplyNearList(left, source);
        ApplyNearList(right, source);
    }

    private void ApplyNearList(List<CardInfoScript> cards, CardInfoScript source)
    {
        if (cards == null)
            return;

        for (int i = 0; i < cards.Count; i++)
        {
            _cardMechanics.Deployment(cards[i], source, i + 1);

            if (source.SelfCard.StatusEffects.IsStunOther)
            {
                cards[i].SelfCard.StatusEffects.IsSelfStunned = true;
                _cardMechanics.CheckStatusEffects(cards[i]);
            }
        }
    }

    private PlayerState GetOwnerState(EffectOwner owner)
    {
        return owner == EffectOwner.Player
            ? _playerState
            : _enemyState;
    }

    private PlayerState GetOpponentState(EffectOwner owner)
    {
        return owner == EffectOwner.Player
            ? _enemyState
            : _playerState;
    }
}

public enum EffectOwner
{
    Player,
    Enemy
}
