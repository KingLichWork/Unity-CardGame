using System.Collections;
using System.Collections.Generic;
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
        if (startCard.SelfCard.EndTurnActions.EndTurnSelfBoost != 0 && isSelfOrNear)
        {
            ChangeCardPoints(startCard, startCard, startCard.SelfCard.EndTurnActions.EndTurnSelfBoost, true);
            _soundManager.EndTurnSound(startCard);
            _effectsManager.StartParticleEffects(startCard.transform, startCard.transform, startCard.SelfCard.EndTurnActions.EndTurnSelfBoost);
        }

        if (startCard.SelfCard.EndTurnActions.EndTurnSelfDamage != 0 && isSelfOrNear)
        {
            ChangeCardPoints(startCard, startCard, -startCard.SelfCard.EndTurnActions.EndTurnSelfDamage, true);
            _soundManager.EndTurnSound(startCard);
            _effectsManager.StartParticleEffects(startCard.transform, startCard.transform, -startCard.SelfCard.EndTurnActions.EndTurnSelfDamage);
        }

        if (startCard.SelfCard.EndTurnActions.EndTurnNearBoost != 0 && isSelfOrNear)
        {
            startCard.CheckSiblingIndex();

            if (ReturnNearCard(startCard, 1, false) != null)
            {
                ChangeCardPoints(startCard, ReturnNearCard(startCard, 1, false)[0], startCard.SelfCard.EndTurnActions.EndTurnNearBoost, true);
                _effectsManager.StartParticleEffects(startCard.transform, ReturnNearCard(startCard, 1, false)[0].transform, startCard.SelfCard.EndTurnActions.EndTurnNearBoost);
            }

            if (ReturnNearCard(startCard, 1, true) != null)
            {
                ChangeCardPoints(startCard, ReturnNearCard(startCard, 1, true)[0], startCard.SelfCard.EndTurnActions.EndTurnNearBoost, true);
                _effectsManager.StartParticleEffects(startCard.transform, ReturnNearCard(startCard, 1, true)[0].transform, startCard.SelfCard.EndTurnActions.EndTurnNearBoost);
            }

            _soundManager.EndTurnSound(startCard);
        }

        if (startCard.SelfCard.EndTurnActions.EndTurnNearDamage != 0 && isSelfOrNear)
        {
            startCard.CheckSiblingIndex();

            if (ReturnNearCard(startCard, 1, false) != null)
            {
                ChangeCardPoints(startCard, ReturnNearCard(startCard, 1, false)[0], -startCard.SelfCard.EndTurnActions.EndTurnNearDamage, true);
                _effectsManager.StartParticleEffects(startCard.transform, ReturnNearCard(startCard, 1, false)[0].transform, -startCard.SelfCard.EndTurnActions.EndTurnNearDamage);
            }
            if (ReturnNearCard(startCard, 1, true) != null)
            {
                ChangeCardPoints(startCard, ReturnNearCard(startCard, 1, true)[0], -startCard.SelfCard.EndTurnActions.EndTurnNearDamage, true);
                _effectsManager.StartParticleEffects(startCard.transform, ReturnNearCard(startCard, 1, true)[0].transform, -startCard.SelfCard.EndTurnActions.EndTurnNearDamage);
            }

            _soundManager.EndTurnSound(startCard);
        }

        if (startCard.SelfCard.EndTurnActions.EndTurnRandomBoost != 0 && !isSelfOrNear)
        {
            ChangeCardPoints(startCard, targetCard, startCard.SelfCard.EndTurnActions.EndTurnRandomBoost, true);
            _soundManager.EndTurnSound(startCard);
            _effectsManager.StartParticleEffects(startCard.transform, targetCard.transform, startCard.SelfCard.EndTurnActions.EndTurnRandomBoost);
        }

        if (startCard.SelfCard.EndTurnActions.EndTurnRandomDamage != 0 && !isSelfOrNear)
        {
            ChangeCardPoints(startCard, targetCard, -startCard.SelfCard.EndTurnActions.EndTurnRandomDamage, true);
            _soundManager.EndTurnSound(startCard);
            _effectsManager.StartParticleEffects(startCard.transform, targetCard.transform, -startCard.SelfCard.EndTurnActions.EndTurnRandomDamage);
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

        if ((targetCard.StatusEffects.IsIllusion) && (value < 0))
        {
            value += value;
        }

        if ((targetCard.StatusEffects.IsSelfShielded) && (value < 0))
        {
            value = 0;
            targetCard.StatusEffects.IsSelfShielded = false;
        }

        if (targetCard.UniqueMechanics.HealDamageValue != 0 && (value < 0))
        {
            if (targetCard.UniqueMechanics.HealDamageValue == -1)
                value = -value;
            else
            {
                value = -targetCard.UniqueMechanics.HealDamageValue;
            }

            _soundManager.EndTurnSound(targetCardInfo);
        }

        if (targetCard.UniqueMechanics.ReturnDamageValue != 0 && (value < 0) && targetCardInfo != startCardInfo)
        {
            if (targetCard.UniqueMechanics.ReturnDamageValue == -1)
            {
                ChangeCardPoints(targetCardInfo, startCardInfo, -value, isEndTurn);
            }
            else
            {
                ChangeCardPoints(targetCardInfo, startCardInfo, -1, isEndTurn);
            }

            _soundManager.EndTurnSound(targetCardInfo);
        }

        if ((!isPiercingDamage) && (value < 0 && targetCard.BaseCard.ArmorPoints > 0))
        {
            int temporaryArmor = Mathf.Min(targetCard.BaseCard.ArmorPoints, -value);

            value += temporaryArmor;
            targetCard.BaseCard.ArmorPoints += -temporaryArmor;

            if (targetCard.BaseCard.ArmorPoints < 0)
                targetCard.BaseCard.ArmorPoints = 0;

            _uiManager.CheckArmor(targetCardInfo);
            Debug.Log(startCard.BaseCard.Name + " изменила броню " + targetCard.BaseCard.Name + " в размере " + -temporaryArmor + "\n" + (targetCard.BaseCard.ArmorPoints + temporaryArmor) + " => " + targetCard.BaseCard.ArmorPoints);
        }

        targetCard.BaseCard.Points += value;

        if (value < 0)
            _effectsManager.StartShaderEffect(targetCardInfo, Color.red, value);
        else if (value > 0)
            _effectsManager.StartShaderEffect(targetCardInfo, Color.green, value);
        else
            _effectsManager.StartShaderEffect(targetCardInfo, Color.grey, value);

        if (value != 0)
            Debug.Log(startCard.BaseCard.Name + " изменила силу " + targetCard.BaseCard.Name + " в размере " + value + "\n" + (targetCard.BaseCard.Points - value) + " => " + targetCard.BaseCard.Points);

        CheckUICards(targetCardInfo, startCardInfo, isEndTurn);
        ShowPointsUI(targetCardInfo);
    }

    public void ShowPointsUI(CardInfoScript cardInfo)
    {
        cardInfo.Point.text = cardInfo.SelfCard.BaseCard.Points.ToString();
    }

    public void IsDestroyCard(CardInfoScript card, bool isEndTurn)
    {
        if (card.SelfCard.BaseCard.Points <= 0)
        {
            card.SelfCard.BaseCard.Points = 0;

            if (!isEndTurn)
            {
                DestroyCard(card);
            }

            else
            {
                card.SelfCard.BaseCard.isDestroyed = true;
                card.gameObject.transform.parent = card.transform.parent.parent;
                card.gameObject.SetActive(false);

                if (GameManager.Instance.PlayerState.Field.Cards.Contains(card))
                    GameManager.Instance.PlayerState.Field.DestroyedInEndTurnCards.Add(card);

                else if (GameManager.Instance.EnemyState.Field.Cards.Contains(card))
                    GameManager.Instance.EnemyState.Field.DestroyedInEndTurnCards.Add(card);
            }
        }
    }

    public void DestroyCard(CardInfoScript card, CardInfoScript startCard = null)
    {

        if (startCard != null)
        {
            Debug.Log(startCard.SelfCard.BaseCard.Name + " уничтожила " + card.SelfCard.BaseCard.Name);
            _effectsManager.StartParticleEffects(startCard.transform, card.transform, -1);
        }

        if (GameManager.Instance.PlayerState.Field.Cards.Contains(card))
            GameManager.Instance.PlayerState.Field.Remove(card);

        else if (GameManager.Instance.EnemyState.Field.Cards.Contains(card))
            GameManager.Instance.EnemyState.Field.Remove(card);

        if (GameManager.Instance.PlayerState.Field.InvulnerabilityCards.Contains(card))
            GameManager.Instance.PlayerState.Field.InvulnerabilityCards.Remove(card);

        else if (GameManager.Instance.EnemyState.Field.InvulnerabilityCards.Contains(card))
            GameManager.Instance.EnemyState.Field.InvulnerabilityCards.Remove(card);

        _effectsManager.StartDestroyCoroutine(card);

        Object.Destroy(card.DescriptionObject);
        Object.Destroy(card.gameObject, 1.5f);
    }

    public void SwapPoints(CardInfoScript firstCard, CardInfoScript secondCard)
    {
        int temporaryVariable;
        int color = firstCard.SelfCard.BaseCard.Points - secondCard.SelfCard.BaseCard.Points;

        temporaryVariable = firstCard.SelfCard.BaseCard.Points;
        firstCard.SelfCard.BaseCard.Points = secondCard.SelfCard.BaseCard.Points;
        secondCard.SelfCard.BaseCard.Points = temporaryVariable;

        CheckUICards(firstCard, secondCard);

        ShowPointsUI(firstCard);
        ShowPointsUI(secondCard);

        _effectsManager.StartParticleEffects(firstCard.transform, secondCard.transform, color);
    }

    public IEnumerator EndTurnActions()
    {
        if (GameManager.Instance.IsPlayerTurn)
        {
            GameManager.Instance.PlayerState.Field.SetCards(GameManager.Instance.EndTurnOrderCard(GameManager.Instance.PlayerState.Field.Cards, true));
            foreach (CardInfoScript card in GameManager.Instance.PlayerState.Field.Cards)
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
                            if (card.SelfCard.BaseCard.isDestroyed)
                                break;

                            EndTurn(card, isSelfOrNear: true);

                            if ((card.SelfCard.EndTurnActions.EndTurnRandomDamage != 0) && (GameManager.Instance.EnemyState.Field.Cards.Count - GameManager.Instance.EnemyState.Field.DestroyedInEndTurnCards.Count > 0))
                            {
                                List<CardInfoScript> existingEnemyFieldCards = new List<CardInfoScript>(GameManager.Instance.EnemyState.Field.Cards);

                                foreach (CardInfoScript nonExistentCard in GameManager.Instance.EnemyState.Field.DestroyedInEndTurnCards)
                                {
                                    existingEnemyFieldCards.Remove(nonExistentCard);
                                }

                                EndTurn(card, existingEnemyFieldCards[Random.Range(0, existingEnemyFieldCards.Count)], false);
                            }

                            if ((card.SelfCard.EndTurnActions.EndTurnRandomBoost != 0) && (GameManager.Instance.PlayerState.Field.Cards.Count - GameManager.Instance.PlayerState.Field.DestroyedInEndTurnCards.Count > 0))
                            {
                                List<CardInfoScript> existingPlayerFieldCards = new List<CardInfoScript>(GameManager.Instance.PlayerState.Field.Cards);

                                foreach (CardInfoScript nonExistentCard in GameManager.Instance.PlayerState.Field.DestroyedInEndTurnCards)
                                {
                                    existingPlayerFieldCards.Remove(nonExistentCard);
                                }

                                EndTurn(card, existingPlayerFieldCards[Random.Range(0, existingPlayerFieldCards.Count)], false);
                            }

                            if (card.SelfCard.EndTurnActions.TimerNoMoreActions)
                            {
                                card.SelfCard.EndTurnActions.Timer = -1;
                            }

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

        else
        {
            GameManager.Instance.EnemyState.Field.SetCards(GameManager.Instance.EndTurnOrderCard(GameManager.Instance.EnemyState.Field.Cards, false));
            foreach (CardInfoScript card in GameManager.Instance.EnemyState.Field.Cards)
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
                            EndTurn(card, isSelfOrNear: true);

                            if ((card.SelfCard.EndTurnActions.EndTurnRandomDamage != 0) && (GameManager.Instance.PlayerState.Field.Cards.Count - GameManager.Instance.PlayerState.Field.DestroyedInEndTurnCards.Count > 0))
                            {
                                List<CardInfoScript> existingPlayerFieldCards = new List<CardInfoScript>(GameManager.Instance.PlayerState.Field.Cards);

                                foreach (CardInfoScript nonExistentCard in GameManager.Instance.PlayerState.Field.DestroyedInEndTurnCards)
                                {
                                    existingPlayerFieldCards.Remove(nonExistentCard);
                                }

                                EndTurn(card, existingPlayerFieldCards[Random.Range(0, existingPlayerFieldCards.Count)], false);
                            }

                            if ((card.SelfCard.EndTurnActions.EndTurnRandomBoost != 0) && (GameManager.Instance.EnemyState.Field.Cards.Count - GameManager.Instance.EnemyState.Field.DestroyedInEndTurnCards.Count > 0))
                            {
                                List<CardInfoScript> existingEnemyFieldCards = new List<CardInfoScript>(GameManager.Instance.EnemyState.Field.Cards);

                                foreach (CardInfoScript nonExistentCard in GameManager.Instance.EnemyState.Field.DestroyedInEndTurnCards)
                                {
                                    existingEnemyFieldCards.Remove(nonExistentCard);
                                }

                                EndTurn(card, existingEnemyFieldCards[Random.Range(0, existingEnemyFieldCards.Count)], false);
                            }

                            if (card.SelfCard.EndTurnActions.TimerNoMoreActions)
                            {
                                card.SelfCard.EndTurnActions.Timer = -1;
                            }

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
        GameObject spawnCard;

        if (card.SelfCard.Spawns.SpawnCardName == "self")
        {
            for (int i = 0; i < card.SelfCard.Spawns.SpawnCardCount; i++)
            {
                if (!((player && GameManager.Instance.PlayerState.Field.Cards.Count < GameManager.MaxNumberCardInField) ||
                    (!player && GameManager.Instance.EnemyState.Field.Cards.Count < GameManager.MaxNumberCardInField)))
                    return;

                spawnCard = _objectResolver.Instantiate(GameManager.Instance.CardPref, card.transform.parent, false);
                CardInfoScript summonCardInfo = spawnCard.GetComponent<CardInfoScript>();

                card.CheckSiblingIndex();
                if ((i == 0) && (i % 2 == 0))
                    spawnCard.transform.SetSiblingIndex(card.SiblingIndex);
                else
                    spawnCard.transform.SetSiblingIndex(card.SiblingIndex + 1);

                if (player) GameManager.Instance.PlayerState.Field.Add(summonCardInfo);
                else GameManager.Instance.EnemyState.Field.Add(summonCardInfo);
                summonCardInfo.ShowCardInfo(card.SelfCard);
                summonCardInfo.SelfCard.StatusEffects.IsIllusion = true;
                CheckStatusEffects(summonCardInfo);

                ChoseCard choseCard  = spawnCard.AddComponent<ChoseCard>();
                _objectResolver.Inject(choseCard);
                choseCard.enabled = false;
            }
        }

        else
        {
            for (int i = 0; i < card.SelfCard.Spawns.SpawnCardCount; i++)
            {
                if (!((player && GameManager.Instance.PlayerState.Field.Cards.Count < GameManager.MaxNumberCardInField) ||
                (!player && GameManager.Instance.EnemyState.Field.Cards.Count < GameManager.MaxNumberCardInField)))
                    return;

                spawnCard = _objectResolver.Instantiate(GameManager.Instance.CardPref, card.transform.parent, false);
                CardInfoScript summonCardInfo = spawnCard.GetComponent<CardInfoScript>();

                card.CheckSiblingIndex();
                if ((i == 0) && (i % 2 == 0))
                    spawnCard.transform.SetSiblingIndex(card.SiblingIndex);
                else
                    spawnCard.transform.SetSiblingIndex(card.SiblingIndex + 1);

                if (player) GameManager.Instance.PlayerState.Field.Add(summonCardInfo);
                else GameManager.Instance.EnemyState.Field.Add(summonCardInfo);
                summonCardInfo.ShowCardInfo(CardManagerList.FindCard(card.SelfCard.Spawns.SpawnCardName));

                ChoseCard choseCard = spawnCard.AddComponent<ChoseCard>();
                _objectResolver.Inject(choseCard);
                choseCard.enabled = false;
            }
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
        if (card.SelfCard.StatusEffects.IsSelfShielded && card.StatusEffectShield == null)
        {
            card.CardStatusEffectImage.material = new Material(_effectsManager.ShieldMaterial);
            card.StatusEffectShield = _objectResolver.Instantiate(card.StatusEffectPrefab, card.CardStatusEffectImage.gameObject.transform);
            card.StatusEffectShield.GetComponent<StatusEffect>().InitializeStatusEffect(StatusEffectsType.shield);
        }

        else if (!card.SelfCard.StatusEffects.IsSelfShielded && card.StatusEffectShield != null)
        {
            card.CardStatusEffectImage.material = null;
            Object.Destroy(card.StatusEffectShield);
            card.StatusEffectShield = null;
        }

        if (card.SelfCard.StatusEffects.IsIllusion && card.StatusEffectIllusion == null)
        {
            card.CardStatusEffectImage.material = new Material(_effectsManager.IllusionMaterial);
            card.StatusEffectIllusion = _objectResolver.Instantiate(card.StatusEffectPrefab, card.CardStatusEffectImage.gameObject.transform);
            card.StatusEffectIllusion.GetComponent<StatusEffect>().InitializeStatusEffect(StatusEffectsType.illusion);
        }

        if (card.SelfCard.StatusEffects.IsSelfStunned && card.StatusEffectStunned == null)
        {
            card.StatusEffectStunned = _objectResolver.Instantiate(card.StatusEffectPrefab, card.CardStatusEffectImage.gameObject.transform);
            card.StatusEffectStunned.GetComponent<StatusEffect>().InitializeStatusEffect(StatusEffectsType.stun);
        }

        else if (!card.SelfCard.StatusEffects.IsSelfStunned && card.StatusEffectStunned != null)
        {
            Object.Destroy(card.StatusEffectStunned);
            card.StatusEffectStunned = null;
        }

        if (card.SelfCard.StatusEffects.IsInvulnerability && card.StatusEffectInvulnerability == null)
        {
            card.CardStatusEffectImage.material = new Material(_effectsManager.InvulnerabilityMaterial);
            card.StatusEffectInvulnerability = _objectResolver.Instantiate(card.StatusEffectPrefab, card.CardStatusEffectImage.gameObject.transform);
            card.StatusEffectInvulnerability.GetComponent<StatusEffect>().InitializeStatusEffect(StatusEffectsType.invulnerability);
        }

        if (card.SelfCard.StatusEffects.IsInvisibility && card.StatusEffectInvisibility == null)
        {
            card.CardStatusEffectImage.material = new Material(_effectsManager.InvisibilityMaterial);
            card.StatusEffectInvisibility = _objectResolver.Instantiate(card.StatusEffectPrefab, card.CardStatusEffectImage.gameObject.transform);
            card.StatusEffectInvisibility.GetComponent<StatusEffect>().InitializeStatusEffect(StatusEffectsType.invisibility);
        }

        _uiManager.CheckBleeding(card);
    }
}
