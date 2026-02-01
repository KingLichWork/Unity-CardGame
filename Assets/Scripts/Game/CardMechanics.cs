using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class CardMechanics
{
    private IObjectResolver _objectResolver;

    private UIManager _uiManager;
    private EffectsManager _effectsManager;
    private SoundManager _soundManager;

    [Inject]
    private void Construct(IObjectResolver resolver, UIManager uiManager, EffectsManager effectsManager, SoundManager soundManager)
    {
        _objectResolver = resolver;
        _uiManager = uiManager;
        _effectsManager = effectsManager;
        _soundManager = soundManager;
    }

    public void Deployment(CardInfoScript targetCard, CardInfoScript startCard, int distanceNearCard = 0)
    {
        if (startCard.SelfCard.BoostOrDamage.Boost != 0)
        {
            ChangeCardPoints(startCard, targetCard, startCard.SelfCard.BoostOrDamage.Boost + distanceNearCard * startCard.SelfCard.BoostOrDamage.ChangeNearBoost);

            _effectsManager.StartParticleEffects(startCard.transform, targetCard.transform, startCard.SelfCard.BoostOrDamage.Boost);
        }

        if (startCard.SelfCard.BoostOrDamage.Damage != 0)
        {
            ChangeCardPoints(startCard, targetCard, -startCard.SelfCard.BoostOrDamage.Damage - distanceNearCard * startCard.SelfCard.BoostOrDamage.ChangeNearDamage);

            _effectsManager.StartParticleEffects(startCard.transform, targetCard.transform, -startCard.SelfCard.BoostOrDamage.Damage);
        }

        CheckUICards(targetCard, startCard);
    }

    public void Self(CardInfoScript startCard, CardInfoScript targetCard)
    {
        if (startCard.SelfCard.BoostOrDamage.SelfBoost != 0)
        {
            ChangeCardPoints(startCard, startCard, startCard.SelfCard.BoostOrDamage.SelfBoost);

            _effectsManager.StartParticleEffects(startCard.transform, targetCard.transform, startCard.SelfCard.BoostOrDamage.SelfBoost);
        }

        if (startCard.SelfCard.BoostOrDamage.SelfDamage != 0)
        {
            ChangeCardPoints(startCard, startCard, -startCard.SelfCard.BoostOrDamage.SelfDamage);

            _effectsManager.StartParticleEffects(startCard.transform, targetCard.transform, -startCard.SelfCard.BoostOrDamage.SelfDamage);
        }

        CheckUICards(targetCard, startCard);
    }

    public void EndTurn(CardInfoScript startCard, CardInfoScript targetCard = null, bool isSelfOrNear = false)
    {
        void ApplyEffect(CardInfoScript from, CardInfoScript to, int value)
        {
            if (value == 0 || to == null) return;

            ChangeCardPoints(from, to, value, true);
            _effectsManager.StartParticleEffects(from.transform, to.transform, value);
        }

        if (isSelfOrNear)
        {
            ApplyEffect(startCard, startCard, startCard.SelfCard.EndTurnActions.EndTurnSelfBoost);
            ApplyEffect(startCard, startCard, -startCard.SelfCard.EndTurnActions.EndTurnSelfDamage);

            startCard.CheckSiblingIndex();

            var nearLeft = ReturnNearCard(startCard, 1, false)?.FirstOrDefault();
            var nearRight = ReturnNearCard(startCard, 1, true)?.FirstOrDefault();

            ApplyEffect(startCard, nearLeft, startCard.SelfCard.EndTurnActions.EndTurnNearBoost);
            ApplyEffect(startCard, nearRight, startCard.SelfCard.EndTurnActions.EndTurnNearBoost);

            ApplyEffect(startCard, nearLeft, -startCard.SelfCard.EndTurnActions.EndTurnNearDamage);
            ApplyEffect(startCard, nearRight, -startCard.SelfCard.EndTurnActions.EndTurnNearDamage);

            if (startCard.SelfCard.EndTurnActions.EndTurnSelfBoost != 0 || startCard.SelfCard.EndTurnActions.EndTurnSelfDamage != 0 || 
                startCard.SelfCard.EndTurnActions.EndTurnNearBoost != 0 || startCard.SelfCard.EndTurnActions.EndTurnNearDamage != 0)
            {
                _soundManager.EndTurnSound(startCard);
            }
        }
        else if (targetCard != null)
        {
            ApplyEffect(startCard, targetCard, startCard.SelfCard.EndTurnActions.EndTurnRandomBoost);
            ApplyEffect(startCard, targetCard, -startCard.SelfCard.EndTurnActions.EndTurnRandomDamage);

            if (startCard.SelfCard.EndTurnActions.EndTurnRandomBoost != 0
                || startCard.SelfCard.EndTurnActions.EndTurnRandomDamage != 0)
            {
                _soundManager.EndTurnSound(startCard);
            }
        }
    }

    public void BleedingOrEndurance(CardInfoScript startCard, CardInfoScript targetCard)
    {
        targetCard.SelfCard.StatusEffects.SelfEnduranceOrBleeding += startCard.SelfCard.StatusEffects.EnduranceOrBleedingOther;
    }

    private void CheckUICards(CardInfoScript targetCard, CardInfoScript startCard, bool isEndTurn = false)
    {
        if (targetCard != null)
        {
            _uiManager.CheckColorPointsCard(targetCard);
            CheckStatusEffects(targetCard);
            IsDestroyCard(targetCard, isEndTurn);
        }

        if (startCard != null)
        {
            _uiManager.CheckColorPointsCard(startCard);
            IsDestroyCard(startCard, isEndTurn);
        }
    }

    public void ChangeCardPoints(CardInfoScript startCardInfo, CardInfoScript targetCardInfo, int value, bool isEndTurn = false, bool isPiercingDamage = false)
    {
        ref Card targetCard = ref targetCardInfo.SelfCard;
        ref Card startCard = ref startCardInfo.SelfCard;

        if (targetCard.StatusEffects.IsIllusion && value < 0)
            value *= 2;

        if (targetCard.StatusEffects.IsSelfShielded && value < 0)
        {
            value = 0;
            targetCard.StatusEffects.IsSelfShielded = false;
        }

        if (targetCard.UniqueMechanics.HealDamageValue != 0 && value < 0)
        {
            value = targetCard.UniqueMechanics.HealDamageValue == -1 ? -value : -targetCard.UniqueMechanics.HealDamageValue;
            _soundManager.EndTurnSound(targetCardInfo);
        }

        if (targetCard.UniqueMechanics.ReturnDamageValue != 0 && value < 0 && targetCardInfo != startCardInfo)
        {
            int reflectedValue = targetCard.UniqueMechanics.ReturnDamageValue == -1 ? -value : -1;
            ChangeCardPoints(targetCardInfo, startCardInfo, reflectedValue, isEndTurn);
            _soundManager.EndTurnSound(targetCardInfo);
        }

        if (!isPiercingDamage && value < 0 && targetCard.BaseCard.ArmorPoints > 0)
        {
            int absorbed = Mathf.Min(targetCard.BaseCard.ArmorPoints, -value);
            value += absorbed;
            targetCard.BaseCard.ArmorPoints -= absorbed;
            targetCard.BaseCard.ArmorPoints = Mathf.Max(0, targetCard.BaseCard.ArmorPoints);

            _uiManager.CheckArmor(targetCardInfo);
            Debug.Log($"{startCard.BaseCard.Name} изменила броню {targetCard.BaseCard.Name} на {-absorbed} ({targetCard.BaseCard.ArmorPoints + absorbed} => {targetCard.BaseCard.ArmorPoints})");
        }

        targetCard.BaseCard.Points += value;

        Color effectColor = value > 0 ? Color.green : (value < 0 ? Color.red : Color.grey);
        _effectsManager.StartShaderEffect(targetCardInfo, effectColor, value);

        if (value != 0)
            Debug.Log($"{startCard.BaseCard.Name} изменила силу {targetCard.BaseCard.Name} на {value} ({targetCard.BaseCard.Points - value} => {targetCard.BaseCard.Points})");

        CheckUICards(targetCardInfo, startCardInfo, isEndTurn);
        ShowPointsUI(targetCardInfo);
    }

    public void ShowPointsUI(CardInfoScript cardInfo)
    {
        cardInfo.Point.text = cardInfo.SelfCard.BaseCard.Points.ToString();
    }

    public void IsDestroyCard(CardInfoScript card, bool isEndTurn)
    {
        if (card.SelfCard.BaseCard.Points > 0)
            return;

        card.SelfCard.BaseCard.Points = 0;

        var field = GetCardField(card);

        if (!isEndTurn)
        {
            DestroyCard(card);
            return;
        }

        card.SelfCard.BaseCard.isDestroyed = true;
        card.gameObject.transform.SetParent(card.transform.parent.parent);
        card.gameObject.SetActive(false);

        field?.DestroyedInEndTurnCards.Add(card);
    }

    public void DestroyCard(CardInfoScript card, CardInfoScript startCard = null)
    {
        if (startCard != null)
        {
            Debug.Log($"{startCard.SelfCard.BaseCard.Name} уничтожила {card.SelfCard.BaseCard.Name}");
            _effectsManager.StartParticleEffects(startCard.transform, card.transform, -1);
        }

        var field = GetCardField(card);
        field?.Remove(card);
        field?.InvulnerabilityCards.Remove(card);

        _effectsManager.StartDestroyCoroutine(card);

        Object.Destroy(card.DescriptionObject);
        Object.Destroy(card.gameObject, 1.5f);
    }

    private FieldZone GetCardField(CardInfoScript card)
    {
        if (GameManager.Instance.PlayerState.Field.Cards.Contains(card) ||
            GameManager.Instance.PlayerState.Field.InvulnerabilityCards.Contains(card))
            return GameManager.Instance.PlayerState.Field;

        if (GameManager.Instance.EnemyState.Field.Cards.Contains(card) ||
            GameManager.Instance.EnemyState.Field.InvulnerabilityCards.Contains(card))
            return GameManager.Instance.EnemyState.Field;

        return null;
    }

    public void SwapPoints(CardInfoScript firstCard, CardInfoScript secondCard)
    {
        int pointsDifference = firstCard.SelfCard.BaseCard.Points - secondCard.SelfCard.BaseCard.Points;

        (firstCard.SelfCard.BaseCard.Points, secondCard.SelfCard.BaseCard.Points) =
            (secondCard.SelfCard.BaseCard.Points, firstCard.SelfCard.BaseCard.Points);

        CheckUICards(firstCard, secondCard);
        ShowPointsUI(firstCard);
        ShowPointsUI(secondCard);

        _effectsManager.StartParticleEffects(firstCard.transform, secondCard.transform, pointsDifference);
    }

    public IEnumerator EndTurnActions()
    {
        EffectOwner owner = GameManager.Instance.IsPlayerTurn ? EffectOwner.Player : EffectOwner.Enemy;
        PlayerState currentState = owner == EffectOwner.Player ? GameManager.Instance.PlayerState : GameManager.Instance.EnemyState;
        PlayerState opponentState = owner == EffectOwner.Player ? GameManager.Instance.EnemyState : GameManager.Instance.PlayerState;

        currentState.Field.SetCards(GameManager.Instance.EndTurnOrderCard(currentState.Field.Cards, owner == EffectOwner.Player));

        foreach (CardInfoScript card in currentState.Field.Cards)
        {
            if (card.SelfCard.EndTurnActions.Timer > 0)
            {
                card.SelfCard.EndTurnActions.Timer--;
                _soundManager.TimerSound(card);
                _uiManager.CheckTimer(card);
            }

            if (!card.SelfCard.BaseCard.isDestroyed && card.SelfCard.EndTurnActions.Timer == 0)
            {
                if (card.SelfCard.EndTurnActions.EndTurnActionCount > 0 && !card.SelfCard.StatusEffects.IsSelfStunned)
                {
                    for (int i = 0; i < card.SelfCard.EndTurnActions.EndTurnActionCount; i++)
                    {
                        if (card.SelfCard.BaseCard.isDestroyed) break;

                        EndTurn(card, isSelfOrNear: true);

                        if (card.SelfCard.EndTurnActions.EndTurnRandomDamage != 0 && opponentState.Field.Cards.Count - opponentState.Field.DestroyedInEndTurnCards.Count > 0)
                        {
                            EndTurn(card, GetRandomExistingCard(opponentState.Field), false);
                        }

                        if (card.SelfCard.EndTurnActions.EndTurnRandomBoost != 0 && currentState.Field.Cards.Count - currentState.Field.DestroyedInEndTurnCards.Count > 0)
                        {
                            EndTurn(card, GetRandomExistingCard(currentState.Field), false);
                        }

                        if (card.SelfCard.EndTurnActions.TimerNoMoreActions)
                            card.SelfCard.EndTurnActions.Timer = -1;

                        yield return new WaitForSeconds(0.25f);
                    }

                    yield return new WaitForSeconds(0.5f);
                }

                CheckBleedingOrEndurance(card);
            }

            if (card.SelfCard.StatusEffects.IsSelfStunned)
            {
                card.SelfCard.StatusEffects.IsSelfStunned = false;
                CheckStatusEffects(card);
            }
        }
    }

    private CardInfoScript GetRandomExistingCard(FieldZone field)
    {
        List<CardInfoScript> existingCards = new List<CardInfoScript>(field.Cards);
        foreach (var destroyedCard in field.DestroyedInEndTurnCards)
            existingCards.Remove(destroyedCard);

        if (existingCards.Count == 0) return null;

        return existingCards[Random.Range(0, existingCards.Count)];
    }

    private void CheckBleedingOrEndurance(CardInfoScript card)
    {
        if (card.SelfCard.StatusEffects.SelfEnduranceOrBleeding != 0)
        {
            if (card.SelfCard.StatusEffects.SelfEnduranceOrBleeding < 0)
            {
                ChangeCardPoints(card, card, -1, isPiercingDamage: true);
                _effectsManager.StartParticleEffects(card.transform, card.transform, -1);
                card.SelfCard.StatusEffects.SelfEnduranceOrBleeding++;
            }
            else
            {
                ChangeCardPoints(card, card, 1, isPiercingDamage: true);
                _effectsManager.StartParticleEffects(card.transform, card.transform, 1);
                card.SelfCard.StatusEffects.SelfEnduranceOrBleeding--;
            }

            _uiManager.CheckBleeding(card);
        }
    }

    public void SpawnCard(CardInfoScript card, bool player)
    {
        var field = player ? GameManager.Instance.PlayerState.Field : GameManager.Instance.EnemyState.Field;
        int maxField = GameManager.MaxNumberCardInField;

        Card spawnTemplate = card.SelfCard.Spawns.SpawnCardName == "self" ? card.SelfCard : CardManagerList.FindCard(card.SelfCard.Spawns.SpawnCardName);

        for (int i = 0; i < card.SelfCard.Spawns.SpawnCardCount; i++)
        {
            if (field.Cards.Count >= maxField)
                return;

            GameObject spawnCard = _objectResolver.Instantiate(GameManager.Instance.CardPref, card.transform.parent, false);
            CardInfoScript summonCardInfo = spawnCard.GetComponent<CardInfoScript>();

            card.CheckSiblingIndex();
            spawnCard.transform.SetSiblingIndex(card.SiblingIndex + (i > 0 ? 1 : 0));

            field.Add(summonCardInfo);

            summonCardInfo.ShowCardInfo(spawnTemplate);

            if (card.SelfCard.Spawns.SpawnCardName == "self")
            {
                summonCardInfo.SelfCard.StatusEffects.IsIllusion = true;
                CheckStatusEffects(summonCardInfo);
            }

            ChoseCard choseCard = spawnCard.AddComponent<ChoseCard>();
            _objectResolver.Inject(choseCard);
            choseCard.enabled = false;
        }
    }

    public void Transformation(CardInfoScript card)
    {
        card.SelfCard = CardManagerList.FindCard(card.SelfCard.UniqueMechanics.TransformationCardName);
        card.ShowCardInfo(card.SelfCard);

        _effectsManager.StartParticleEffects(card.transform, card.transform, 1);
    }

    public List<CardInfoScript> ReturnNearCard(CardInfoScript card, int range, bool IsRight)
    {
        List<CardInfoScript> RightNearCard = new List<CardInfoScript>();
        List<CardInfoScript> LeftNearCard = new List<CardInfoScript>();

        for (int i = 1; i <= range; i++)
        {
            if ((IsRight) && (card.SiblingIndex + i < card.transform.parent.childCount))
            {
                RightNearCard.Add(card.transform.parent.GetChild(card.SiblingIndex + i).GetComponent<CardInfoScript>());
            }

            else if ((!IsRight) && (card.SiblingIndex - i >= 0))
            {
                LeftNearCard.Add(card.transform.parent.GetChild(card.SiblingIndex - i).GetComponent<CardInfoScript>());
            }
        }

        if ((IsRight) && (RightNearCard.Count != 0))
            return RightNearCard;
        else if ((!IsRight) && (LeftNearCard.Count != 0))
            return LeftNearCard;
        else return null;
    }

    public void CheckStatusEffects(CardInfoScript card)
    {
        void HandleStatusEffect(bool condition, ref GameObject effectObj, Material material, StatusEffectsType type)
        {
            if (condition && effectObj == null)
            {
                if (material != null)
                    card.CardStatusEffectImage.material = new Material(material);

                effectObj = _objectResolver.Instantiate(card.StatusEffectPrefab, card.CardStatusEffectImage.transform);
                effectObj.GetComponent<StatusEffect>().InitializeStatusEffect(type);
            }
            else if (!condition && effectObj != null)
            {
                Object.Destroy(effectObj);
                effectObj = null;
            }
        }

        HandleStatusEffect(card.SelfCard.StatusEffects.IsSelfShielded, ref card.StatusEffectShield, _effectsManager.ShieldMaterial, StatusEffectsType.shield);
        HandleStatusEffect(card.SelfCard.StatusEffects.IsIllusion, ref card.StatusEffectIllusion, _effectsManager.IllusionMaterial, StatusEffectsType.illusion);
        HandleStatusEffect(card.SelfCard.StatusEffects.IsSelfStunned, ref card.StatusEffectStunned, null, StatusEffectsType.stun);
        HandleStatusEffect(card.SelfCard.StatusEffects.IsInvulnerability, ref card.StatusEffectInvulnerability, _effectsManager.InvulnerabilityMaterial, StatusEffectsType.invulnerability);
        HandleStatusEffect(card.SelfCard.StatusEffects.IsInvisibility, ref card.StatusEffectInvisibility, _effectsManager.InvisibilityMaterial, StatusEffectsType.invisibility);

        _uiManager.CheckBleeding(card);
    }
}
