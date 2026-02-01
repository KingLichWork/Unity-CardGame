using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using static UnityEngine.EventSystems.EventTrigger;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
            }

            return _instance;
        }
    }

    public static Game CurrentGame;

    public GameObject CardPref;

    public CardInfoScript StartChoseCard;
    public GameObject[] HowToPlayList;
    public GameObject HowToPlayFon;

    //ChangeGameCharacteristics
    public static int MaxNumberCardInField = 10;
    public int ValueHandCards = 10;
    public int TurnDuration = 30;
    public int ValueDeckCards = 20;

    public float TimeDrawCardStart = 0.15f;
    public float TimeDrawCard = 0.3f;

    [HideInInspector] public static bool IsDrag;
    [HideInInspector] public static bool IsStartGiveCards = false;
    [HideInInspector] public static bool IsChoosing;
    [HideInInspector] public static bool IsHandCardPlaying;

    [HideInInspector] public bool IsChooseCard;

    [HideInInspector] public List<CardInfoScript> CardsCanChooseOnWickEnd = new List<CardInfoScript>();

    [HideInInspector] public List<Coroutine> AllCoroutine = new List<Coroutine>();

    [SerializeField] private Transform _enemyHand;
    [SerializeField] private Transform _playerHand;
    [SerializeField] private Transform _enemyField;
    [SerializeField] private Transform _playerField;
    [SerializeField] private GameObject _enemyHandPass;
    [SerializeField] private GameObject _playerHandPass;

    private int _turn;
    private int _turnTime;
    private int _playerPoints;
    private int _enemyPoints;

    private bool _isPlayerPassed;
    private bool _isEnemyPassed;

    private CardInfoScript _choosenCard;
    private Camera _mainCamera;

    private IObjectResolver _objectResolver;

    private CardMechanics _cardMechanics;
    private UIManager _uiManager;
    private EffectsManager _effectsManager;
    private SoundManager _soundManager;
    private CardEffectResolver _cardEffectResolver;

    public PlayerState PlayerState { get; private set; }
    public PlayerState EnemyState { get; private set; }

    public static event Action HideEndGamePanel;
    public static event Action<int, int> ChangePoints;

    public bool IsPlayerTurn
    {
        get { return _turn % 2 == 0; }
    }

    [Inject]
    private void Construct(IObjectResolver objectResolver, CardMechanics cardMechanics, UIManager uiManager, 
        EffectsManager effectsManager, SoundManager soundManager, CardEffectResolver cardEffectResolver)
    {
        _objectResolver = objectResolver;
        _cardMechanics = cardMechanics;
        _uiManager = uiManager;
        _effectsManager = effectsManager;
        _soundManager = soundManager;
        _cardEffectResolver = cardEffectResolver;
    }

    private void OnEnable()
    {
        DropField.DropCardAction += PlayerDropCardStartCoroutine;
        DropField.ThrowCardAction += ThrowCard;
    }

    private void OnDisable()
    {
        DropField.DropCardAction -= PlayerDropCardStartCoroutine;
        DropField.ThrowCardAction -= ThrowCard;
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        _mainCamera = Camera.main;
    }

    private void Start()
    {
        StartCoroutine(StartGame());
    }

    private void DebugGame()
    {
        int i = 0;

        while (i++ < MaxNumberCardInField)
        {
            GameObject cardDebug = Instantiate(CardPref, _enemyField, false);
            cardDebug.GetComponent<CardInfoScript>().ShowCardInfo(CardManagerList.AllCards[0]);

            ChoseCard choseCard = cardDebug.AddComponent<ChoseCard>();
            _objectResolver.Inject(choseCard);
            choseCard.enabled = false;

            cardDebug.transform.SetParent(_enemyField);
            EnemyState.Field.Cards.Add(cardDebug.GetComponent<CardInfoScript>());
        }
    }

    private IEnumerator StartGame()
    {
        _turn = 0;
        _playerPoints = 0;
        _enemyPoints = 0;

        PlayerState = new PlayerState(ValueHandCards, MaxNumberCardInField);
        EnemyState = new PlayerState(ValueHandCards, MaxNumberCardInField);

        _cardEffectResolver.Initialize(PlayerState, EnemyState);

        CurrentGame = new Game(this);

        _uiManager.ChangeEndTurnButtonInteractable(false);

        //DebugGame();
        Deck.Instance.CreateDeck(CurrentGame.PlayerDeck);

        IsStartGiveCards = true;

        StartCoroutine(GiveHandCards(CurrentGame.EnemyDeck, _enemyHand, false, true));
        yield return StartCoroutine(GiveHandCards(CurrentGame.PlayerDeck, _playerHand, true, true));
        _effectsManager.HideDrawCardEffect();

        if (Object.FindObjectOfType<HowToPlay>() != null && HowToPlay.Instance.IsHowToPlay)
        {
            HowToPlay.Instance.HowToPlayGame(HowToPlayList, HowToPlayFon);
        }

        else
            AllCoroutine.Add(StartCoroutine(TurnFunk()));

        IsStartGiveCards = false;
        _uiManager.ChangeEndTurnButtonInteractable(true);
    }

    private IEnumerator GiveHandCards(List<Card> deck, Transform hand, bool isPlayer, bool isStart = false)
    {
        int i = 0;
        while (i++ < ValueHandCards)
        {
            yield return StartCoroutine(GiveCardtoHand(deck, hand, TimeDrawCardStart, isPlayer, isStart));
        }
    }

    private IEnumerator GiveCardtoHand(List<Card> deck, Transform hand, float time, bool isPlayer, bool isStart = false)
    {
        if (deck.Count == 0)
            yield break;

        Card card = deck[0];

        _effectsManager.DrawCardEffect(time, hand, isPlayer);

        yield return new WaitForSeconds(time);

        if (!isStart)
            _effectsManager.HideDrawCardEffect();

        GameObject cardHand = Instantiate(CardPref, hand, false);

        ChoseCard choseCard = cardHand.AddComponent<ChoseCard>();
        _objectResolver.Inject(choseCard);
        choseCard.enabled = false;

        if (hand == _enemyHand)
        {
            cardHand.GetComponent<CardInfoScript>().HideCardInfo(card);
            EnemyState.Hand.Add(cardHand.GetComponent<CardInfoScript>());
            _uiManager.CheckColorPointsCard(cardHand.GetComponent<CardInfoScript>());
        }

        else
        {
            cardHand.GetComponent<CardInfoScript>().ShowCardInfo(card);
            PlayerState.Hand.Add(cardHand.GetComponent<CardInfoScript>());
            _uiManager.CheckColorPointsCard(cardHand.GetComponent<CardInfoScript>());

            Deck.Instance.DeleteFirstCardFromDeck();
        }

        deck.RemoveAt(0);

        _uiManager.ChangeDeckCount(CurrentGame);
    }

    public void StartTurnCoroutine()
    {
        AllCoroutine.Add(StartCoroutine(TurnFunk()));
    }

    private IEnumerator TurnFunk()
    {
        _turnTime = TurnDuration;

        _uiManager.ChangeWick(_turnTime);

        if (IsPlayerTurn)
        {
            if (!_isPlayerPassed)
            {
                while (_turnTime-- > 0)
                {
                    _uiManager.ChangeWick(_turnTime);

                    yield return new WaitForSeconds(1);

                    if (_turnTime == 0 && !IsHandCardPlaying && PlayerState.Hand.Cards.Count != 0)
                    {
                        ThrowCard(PlayerState.Hand.Cards[Random.Range(0, PlayerState.Hand.Cards.Count)], true);
                    }
                }

                if (IsChooseCard == true)
                {
                    _choosenCard = CardsCanChooseOnWickEnd[Random.Range(0, CardsCanChooseOnWickEnd.Count)];
                    yield return null;
                }

            }

            StartCoroutine(ChangeTurn());
        }

        else
        {
            AllCoroutine.Add(StartCoroutine(EnemyTurn(EnemyState.Hand.Cards)));
        }
    }

    public void StartChangeTurn()
    {
        StartCoroutine(ChangeTurn());
    }

    private IEnumerator ChangeTurn()
    {
        _uiManager.ChangeEndTurnButtonInteractable(false);

        if (IsPlayerTurn && !IsHandCardPlaying && PlayerState.Hand.Cards.Count != 0)
            ThrowCard(PlayerState.Hand.Cards[Random.Range(0, PlayerState.Hand.Cards.Count)], true);

        yield return StartCoroutine(_cardMechanics.EndTurnActions());

        ClearDestroyedInEndTurnCards();

        foreach (Coroutine coroutine in AllCoroutine)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        AllCoroutine.Clear();

        ChangeEnemyPoints();
        ChangePlayerPoints();

        if (PlayerState.Hand.Cards.Count == 0)
        {
            _playerHandPass.SetActive(true);
            _isPlayerPassed = true;
        }

        if (EnemyState.Hand.Cards.Count == 0)
        {
            _enemyHandPass.SetActive(true);
            _isEnemyPassed = true;
        }

        if (_isPlayerPassed && _isEnemyPassed)
        {
            EndGame();
            _uiManager.EndGame(_playerPoints, _enemyPoints);
        }

        _turn++;
        IsHandCardPlaying = false;
        _uiManager.ChangeEndTurnButtonInteractable(IsPlayerTurn);

        if (!(_isPlayerPassed && _isEnemyPassed))
            AllCoroutine.Add(StartCoroutine(TurnFunk()));
    }

    private IEnumerator EnemyTurn(IReadOnlyList<CardInfoScript> enemyHandCards)
    {
        yield return new WaitForSeconds(1.0f);

        int enemyPlayedCard = Random.Range(0, enemyHandCards.Count);

        if ((EnemyState.Field.Cards.Count >= MaxNumberCardInField) || (EnemyState.Hand.Cards.Count == 0))
        {
            if (EnemyState.Field.Cards.Count >= MaxNumberCardInField)
            {
                ThrowCard(enemyHandCards[enemyPlayedCard], false);
            }

            StartCoroutine(ChangeTurn());
            yield break;
        }

        if (!enemyHandCards[enemyPlayedCard].SelfCard.StatusEffects.IsInvisibility)
            enemyHandCards[enemyPlayedCard].GetComponent<CardMove>().EnemyMoveToField(_enemyField.transform);

        else
            enemyHandCards[enemyPlayedCard].GetComponent<CardMove>().EnemyMoveToField(_playerField.transform);

        yield return new WaitForSeconds(0.6f);

        enemyHandCards[enemyPlayedCard].ShowCardInfo(enemyHandCards[enemyPlayedCard].SelfCard);

        if (!enemyHandCards[enemyPlayedCard].SelfCard.StatusEffects.IsInvisibility)
            enemyHandCards[enemyPlayedCard].transform.SetParent(_enemyField);
        else
            enemyHandCards[enemyPlayedCard].transform.SetParent(_playerField);

        yield return StartCoroutine(EnemyDropCard(enemyHandCards[enemyPlayedCard]));

        StartCoroutine(ChangeTurn());
    }

    private IEnumerator EnemyDropCard(CardInfoScript card)
    {
        yield return new WaitForSeconds(0.3f);

        EnemyState.Hand.Remove(card);

        if (!card.SelfCard.StatusEffects.IsInvisibility)
        {
            EnemyState.Field.Add(card);
            ChangeEnemyPoints();

            if (card.SelfCard.StatusEffects.IsInvulnerability)
                EnemyState.Field.InvulnerabilityCards.Add(card);
        }

        else if (card.SelfCard.StatusEffects.IsInvisibility)
        {
            PlayerState.Field.Add(card);
            ChangePlayerPoints();

            if (card.SelfCard.StatusEffects.IsInvulnerability)
                PlayerState.Field.InvulnerabilityCards.Add(card);
        }

        _cardMechanics.CheckStatusEffects(card);
        _soundManager.DeploymentSound(card);

        if (_cardEffectResolver.NeedBoostChoice(card))
            _cardEffectResolver.ResolveBoostWithAutoTarget(card);
        else
            _cardEffectResolver.HandleBoost(card, EffectOwner.Enemy);

        if (_cardEffectResolver.NeedDamageChoice(card))
            _cardEffectResolver.ResolveDamageWithAutoTarget(card);
        else
            _cardEffectResolver.HandleDamage(card, EffectOwner.Enemy);

        _cardEffectResolver.HandleSelf(card);
        _cardEffectResolver.HandleSpawn(card);

        yield return HandleEnemyDraw(card);

        _cardEffectResolver.ResolveEnemyUnique(card);

        FinalPointsUpdate();
    }

    private void PlayerDropCardStartCoroutine(CardInfoScript card)
    {
        AllCoroutine.Add(StartCoroutine(PlayerDropCard(card)));
    }

    private IEnumerator PlayerDropCard(CardInfoScript card)
    {
        IsHandCardPlaying = true;

        yield return PlayDropAnimation(card);

        MoveCardToField(card);

        OnCardPlaced(card);

        yield return HandleDraw(card);

        HandleUniqueMechanics(card);

        FinalPointsUpdate();
    }

    private IEnumerator PlayDropAnimation(CardInfoScript card)
    {
        var move = card.GetComponent<CardMove>();
        move.MoveTopHierarchy();

        var targetField = card.SelfCard.StatusEffects.IsInvisibility
            ? _enemyField
            : _playerField;

        move.PlayerMoveToField(
            targetField.GetComponent<DropField>(),
            _playerHand.GetComponent<DropField>().EmptyHandCard,
            card.SelfCard.StatusEffects.IsInvisibility
        );

        yield return new WaitForSeconds(0.6f);

        card.IsAnimationCard = false;
        move.MoveBackHierarchy();
    }

    private void MoveCardToField(CardInfoScript card)
    {
        PlayerState.Hand.Remove(card);

        bool toEnemy = card.SelfCard.StatusEffects.IsInvisibility;
        var field = toEnemy ? EnemyState.Field : PlayerState.Field;

        field.Add(card);

        if (card.SelfCard.StatusEffects.IsInvulnerability)
            field.InvulnerabilityCards.Add(card);

        if (toEnemy)
            ChangeEnemyPoints();
        else
            ChangePlayerPoints();
    }

    private void OnCardPlaced(CardInfoScript card)
    {
        _cardMechanics.CheckStatusEffects(card);
        _soundManager.DeploymentSound(card);

        if (_cardEffectResolver.NeedBoostChoice(card))
        {
            PrepareToChoseCard(card, false);
            AllCoroutine.Add(StartCoroutine(ChoseCardCoroutine(card, isBoost: true)));
        }
        else
            _cardEffectResolver.HandleBoost(card, EffectOwner.Player);

        if (_cardEffectResolver.NeedDamageChoice(card))
        {
            PrepareToChoseCard(card, true);
            AllCoroutine.Add(StartCoroutine(ChoseCardCoroutine(card, isDamage: true)));
        }
        else
            _cardEffectResolver.HandleDamage(card, EffectOwner.Player);

        _cardEffectResolver.HandleSelf(card);
        _cardEffectResolver.HandleSpawn(card);
    }

    private void HandleUniqueMechanics(CardInfoScript card)
    {
        if (_cardEffectResolver.HandleDestroy(card))
        {
            PrepareToChoseCard(card, true);
            AllCoroutine.Add(StartCoroutine(ChoseCardCoroutine(card, isDestroy: true)));
        }

        if (_cardEffectResolver.HandleSwap(card))
        {
            PrepareToChoseCard(card, true);
            PrepareToChoseCard(card, false);
            AllCoroutine.Add(StartCoroutine(ChoseCardCoroutine(card, isSwapPoints: true)));
        }

        if (_cardEffectResolver.HandleBleedingChoice(card))
        {
            bool toEnemy = card.SelfCard.StatusEffects.IsEnemyTargetEnduranceOrBleeding;
            PrepareToChoseCard(card, toEnemy);
            AllCoroutine.Add(StartCoroutine(ChoseCardCoroutine(card, isEnduranceOrBleeding: true, isEnduranceOrBleedingEnemy: toEnemy)));
        }

        if (!string.IsNullOrEmpty(card.SelfCard.UniqueMechanics.TransformationCardName))
            _cardMechanics.Transformation(card);
    }

    private IEnumerator HandleDraw(CardInfoScript card)
    {
        if (card.SelfCard.DrawCard.DrawCardCount == 0)
            yield break;

        _uiManager.ChangeEndTurnButtonInteractable(false);

        for (int i = 0; i < card.SelfCard.DrawCard.DrawCardCount; i++)
            yield return GiveCardtoHand(CurrentGame.PlayerDeck, _playerHand, TimeDrawCard, true);

        _uiManager.ChangeEndTurnButtonInteractable(true);
    }

    private IEnumerator HandleEnemyDraw(CardInfoScript card)
    {
        if (card.SelfCard.DrawCard.DrawCardCount == 0)
            yield break;

        if (card.SelfCard.DrawCard.DrawCardCount != 0)
        {
            for (int i = 0; i < card.SelfCard.DrawCard.DrawCardCount; i++)
            {
                yield return GiveCardtoHand(CurrentGame.EnemyDeck, _enemyHand, TimeDrawCard, false);
            }
        }
    }

    private void FinalPointsUpdate()
    {
        ChangeEnemyPoints();
        ChangePlayerPoints();
    }

    private void PrepareToChoseCard(CardInfoScript playedCard, bool isEnemyField)
    {
        List<CardInfoScript> cards = isEnemyField ? EnemyState.Field.Cards : PlayerState.Field.Cards;
        Color lineColor = isEnemyField ? Color.red : Color.green;

        foreach (var card in cards)
        {
            card.IsOrderCard = true;

            if (!card.SelfCard.StatusEffects.IsInvulnerability)
            {
                if (card.TryGetComponent<ChoseCard>(out var chose))
                    chose.enabled = true;

                CardsCanChooseOnWickEnd.Add(card);
            }
        }

        if (playedCard.TryGetComponent<ChoseCard>(out var playedChose))
            playedChose.enabled = false;

        CardsCanChooseOnWickEnd.Remove(playedCard);

        _uiManager.ChangeLineColor(Color.white, lineColor);
    }

    private void RemovePrepareToChoseCard(bool isEnemyField)
    {
        List<CardInfoScript> cards = isEnemyField ? EnemyState.Field.Cards : PlayerState.Field.Cards;

        foreach (var card in cards)
        {
            card.GetComponent<ChoseCard>().enabled = false;
            CardsCanChooseOnWickEnd.Remove(card);
            card.ImageEdge1.color = Color.white;
            card.IsOrderCard = false;
        }
    }

    private void ChangeEnemyPoints()
    {
        _enemyPoints = 0;

        foreach (CardInfoScript card in EnemyState.Field.Cards)
            _enemyPoints += card.ShowPoints(card.SelfCard);

        ChangePoints.Invoke(_playerPoints, _enemyPoints);
    }

    private void ChangePlayerPoints()
    {
        _playerPoints = 0;

        foreach (CardInfoScript card in PlayerState.Field.Cards)
            _playerPoints += card.ShowPoints(card.SelfCard);

        ChangePoints.Invoke(_playerPoints, _enemyPoints);
    }

    private CardInfoScript ChooseCard(bool isPlayerChoose, bool isFriendlyCard = true)
    {
        if (isPlayerChoose)
            return _choosenCard;

        FieldZone field = isFriendlyCard ? EnemyState.Field : PlayerState.Field;

        List<CardInfoScript> availableCards = new(field.Cards);

        foreach (var invulCard in field.InvulnerabilityCards)
            availableCards.Remove(invulCard);

        if (availableCards.Count == 0)
            return null;

        return availableCards[Random.Range(0, availableCards.Count)];
    }

    private IEnumerator ChoseCardCoroutine(CardInfoScript playedCard, bool isBoost = false, bool isDamage = false, bool isDestroy = false, 
        bool isSwapPoints = false, bool isEnduranceOrBleeding = false, bool isEnduranceOrBleedingEnemy = false)
    {
        StartChoseCard = playedCard;
        playedCard.ImageEdge1.color = Color.green;
        _uiManager.ChangeEndTurnButtonInteractable(false);

        yield return StartCoroutine(WaitForChoseCard(playedCard));
        IsChooseCard = false;

        var targetAlly = ChooseCard(true);
        var targetEnemy = ChooseCard(true, false);

        if (isBoost)
        {
            _cardMechanics.Deployment(targetAlly, playedCard);

            if (playedCard.SelfCard.StatusEffects.IsShieldOther)
                targetAlly.SelfCard.StatusEffects.IsSelfShielded = true;

            RemovePrepareToChoseCard(false);

            int near = playedCard.SelfCard.BoostOrDamage.NearBoost;
            if (near > 0)
                ApplyNearEffect(targetAlly, playedCard, near, false);
        }

        if (isDamage)
        {
            _cardMechanics.Deployment(targetEnemy, playedCard);

            if (playedCard.SelfCard.StatusEffects.IsStunOther)
            {
                targetEnemy.SelfCard.StatusEffects.IsSelfStunned = true;
                _cardMechanics.CheckStatusEffects(targetEnemy);
            }

            RemovePrepareToChoseCard(true);

            int near = playedCard.SelfCard.BoostOrDamage.NearDamage;
            if (near > 0)
                ApplyNearEffect(targetEnemy, playedCard, near, true);
        }

        if (isDestroy)
        {
            _cardMechanics.DestroyCard(targetEnemy, playedCard);
            RemovePrepareToChoseCard(true);
        }

        if (isSwapPoints)
        {
            _cardMechanics.SwapPoints(playedCard, targetAlly);
            RemovePrepareToChoseCard(true);
            RemovePrepareToChoseCard(false);
        }

        if (isEnduranceOrBleeding)
        {
            _cardMechanics.BleedingOrEndurance(playedCard, targetAlly);
            _uiManager.CheckBleeding(targetAlly);

            RemovePrepareToChoseCard(isEnduranceOrBleedingEnemy);
        }

        if (playedCard.SelfCard.Spawns.SpawnCardCount > 0 &&
            playedCard.SelfCard.BoostOrDamage.AddictionWithEnemyField &&
            EnemyState.Field.Cards.Count > 0)
        {
            _cardMechanics.SpawnCard(playedCard, true);
        }

        FinishChoose(playedCard);
    }


    private void ApplyNearEffect(CardInfoScript center, CardInfoScript playedCard, int range, bool isDamage)
    {
        center.CheckSiblingIndex();

        ApplyNearSide(center, playedCard, range, true, isDamage);
        ApplyNearSide(center, playedCard, range, false, isDamage);
    }

    private void ApplyNearSide(CardInfoScript center, CardInfoScript playedCard, int range, bool right, bool isDamage)
    {
        var nearCards = _cardMechanics.ReturnNearCard(center, range, right);
        if (nearCards == null) return;

        for (int i = 0; i < nearCards.Count; i++)
        {
            var near = nearCards[i];
            _cardMechanics.Deployment(near, playedCard, i + 1);

            if (isDamage && playedCard.SelfCard.StatusEffects.IsStunOther)
            {
                near.SelfCard.StatusEffects.IsSelfStunned = true;
                _cardMechanics.CheckStatusEffects(near);
            }
        }
    }

    private void FinishChoose(CardInfoScript playedCard)
    {
        _uiManager.ChangeLinePosition(0, Vector3.zero);
        _uiManager.ChangeLinePosition(1, Vector3.zero);

        playedCard.ImageEdge1.color = Color.white;
        _uiManager.ChangeEndTurnButtonInteractable(true);

        ChangeEnemyPoints();
        ChangePlayerPoints();

        _soundManager.StartEffectSound(playedCard);
    }

    private IEnumerator WaitForChoseCard(CardInfoScript card)
    {
        IsChooseCard = true;
        _choosenCard = null;

        while (_choosenCard == null)
        {
            _uiManager.ChangeLinePosition(0, new Vector3(card.transform.position.x, card.transform.position.y, 1));
            _uiManager.ChangeLinePosition(1, _mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1)));

            yield return null;
        }
    }

    public void ChoseCard(CardInfoScript card)
    {
        //_сardSelectionService.SetChosen(card);
        _choosenCard = card;
    }

    private void ClearDestroyedInEndTurnCards()
    {
        foreach (CardInfoScript destroyedCard in PlayerState.Field.DestroyedInEndTurnCards)
            _cardMechanics.DestroyCard(destroyedCard);

        foreach (CardInfoScript destroyedCard in EnemyState.Field.DestroyedInEndTurnCards)
            _cardMechanics.DestroyCard(destroyedCard);

        PlayerState.Field.DestroyedInEndTurnCards.Clear();
        EnemyState.Field.DestroyedInEndTurnCards.Clear();
    }

    public List<CardInfoScript> EndTurnOrderCard(List<CardInfoScript> cardsInField, bool isPlayerField)
    {
        List<CardInfoScript> temporyList = new List<CardInfoScript>(cardsInField);

        if (isPlayerField)
            for (int i = 0; i < _playerField.childCount; i++)
                temporyList[i] = _playerField.GetChild(i).GetComponent<CardInfoScript>();
        else
            for (int i = 0; i < _enemyField.childCount; i++)
                temporyList[i] = _enemyField.GetChild(i).GetComponent<CardInfoScript>();

        return temporyList;
    }

    public void NewGame()
    {
        HideEndGamePanel.Invoke();

        StopAllCoroutines();

        ResetCoreState();

        ClearZone(EnemyState.Hand);
        ClearZone(PlayerState.Hand);
        ClearZone(EnemyState.Field);
        ClearZone(PlayerState.Field);

        PlayerState.Field.InvulnerabilityCards.Clear();
        EnemyState.Field.InvulnerabilityCards.Clear();

        ResetPassState();

        CurrentGame = new Game(this);

        Deck.Instance.DeleteDeck();
        Deck.Instance.CreateDeck(CurrentGame.PlayerDeck);

        StartCoroutine(GiveHandCards(CurrentGame.EnemyDeck, _enemyHand, false));
        StartCoroutine(GiveHandCards(CurrentGame.PlayerDeck, _playerHand, true));

        _uiManager.ChangeEndTurnButtonInteractable(true);

        AllCoroutine.Add(StartCoroutine(TurnFunk()));
    }

    private void ClearZone(ICardZone zone)
    {
        for (int i = zone.Cards.Count - 1; i >= 0; i--)
        {
            Destroy(zone.Cards[i].gameObject);
            zone.Remove(zone.Cards[i]);
        }
    }

    private void ResetPassState()
    {
        _enemyHandPass.SetActive(false);
        _playerHandPass.SetActive(false);
        _isPlayerPassed = false;
        _isEnemyPassed = false;
    }

    private void ResetCoreState()
    {
        IsHandCardPlaying = false;

        _turn = 0;
        _playerPoints = 0;
        _enemyPoints = 0;

        ChangePoints?.Invoke(_playerPoints, _enemyPoints);
    }

    public void ToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void DeckBuild()
    {
        SceneManager.LoadScene("DeckBuild");
    }

    public void ThrowCard(CardInfoScript card, bool isPlayer)
    {
        if (isPlayer)
        {
            PlayerState.Hand.Remove(card);
            IsHandCardPlaying = true;
        }
        else
        {
            EnemyState.Hand.Remove(card);
        }

        Destroy(card.transform.gameObject);
    }

    private void EndGame()
    {
        StopAllCoroutines();
    }
}
