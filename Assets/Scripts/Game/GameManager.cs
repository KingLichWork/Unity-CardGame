using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
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

    public PlayerState Player { get; private set; }
    public PlayerState Enemy { get; private set; }

    public static event Action HideEndGamePanel;

    public bool IsPlayerTurn
    {
        get { return _turn % 2 == 0; }
    }

    [Inject]
    private void Construct(IObjectResolver objectResolver, CardMechanics cardMechanics, UIManager uiManager, EffectsManager effectsManager, SoundManager soundManager)
    {
        _objectResolver = objectResolver;
        _cardMechanics = cardMechanics;
        _uiManager = uiManager;
        _effectsManager = effectsManager;
        _soundManager = soundManager;
    }

    private void OnEnable()
    {
        DropField.DropCardAction += PlayerDropCardStartCoroutine;
        DropField.DropCardAction += PlayerDropCardStartCoroutine;
        DropField.ThrowCardAction += ThrowCard;
    }

    private void OnDisable()
    {
        DropField.DropCardAction -= PlayerDropCardStartCoroutine;
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
            Enemy.Field.Cards.Add(cardDebug.GetComponent<CardInfoScript>());
        }
    }

    private IEnumerator StartGame()
    {
        _turn = 0;
        _playerPoints = 0;
        _enemyPoints = 0;

        Player = new PlayerState(ValueHandCards, MaxNumberCardInField);
        Enemy = new PlayerState(ValueHandCards, MaxNumberCardInField);

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
            Enemy.Hand.Add(cardHand.GetComponent<CardInfoScript>());
            _uiManager.CheckColorPointsCard(cardHand.GetComponent<CardInfoScript>());
        }

        else
        {
            cardHand.GetComponent<CardInfoScript>().ShowCardInfo(card);
            Player.Hand.Add(cardHand.GetComponent<CardInfoScript>());
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

                    if (_turnTime == 0 && !IsHandCardPlaying && Player.Hand.Cards.Count != 0)
                    {
                        ThrowCard(Player.Hand.Cards[Random.Range(0, Player.Hand.Cards.Count)], true);
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
            AllCoroutine.Add(StartCoroutine(EnemyTurn(Enemy.Hand.Cards)));
        }
    }

    public void StartChangeTurn()
    {
        StartCoroutine(ChangeTurn());
    }

    private IEnumerator ChangeTurn()
    {
        _uiManager.ChangeEndTurnButtonInteractable(false);

        if (IsPlayerTurn && !IsHandCardPlaying && Player.Hand.Cards.Count != 0)
            ThrowCard(Player.Hand.Cards[Random.Range(0, Player.Hand.Cards.Count)], true);

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

        if (Player.Hand.Cards.Count == 0)
        {
            _playerHandPass.SetActive(true);
            _isPlayerPassed = true;
        }

        if (Enemy.Hand.Cards.Count == 0)
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

        if ((Enemy.Field.Cards.Count >= MaxNumberCardInField) || (Enemy.Hand.Cards.Count == 0))
        {
            if (Enemy.Field.Cards.Count >= MaxNumberCardInField)
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
        yield return new WaitForSeconds(0.1f);

        CardInfoScript botChoosedCard;

        Enemy.Hand.Remove(card);

        if (!card.SelfCard.StatusEffects.IsInvisibility)
        {
            Enemy.Field.Add(card);
            ChangeEnemyPoints();

            if (card.SelfCard.StatusEffects.IsInvulnerability)
                Enemy.Field.InvulnerabilityCards.Add(card);
        }

        else if (card.SelfCard.StatusEffects.IsInvisibility)
        {
            Player.Field.Add(card);
            ChangePlayerPoints();

            if (card.SelfCard.StatusEffects.IsInvulnerability)
                Player.Field.InvulnerabilityCards.Add(card);
        }

        _cardMechanics.CheckStatusEffects(card);

        _soundManager.EnemyDeploymentSound(card);

        if (card.SelfCard.BoostOrDamage.NearBoost == -1)
        {
            for (int i = Enemy.Field.Cards.Count - 1; i >= 0; i--)
            {
                _cardMechanics.Deployment(Enemy.Field.Cards[i], card);

                if (card.SelfCard.StatusEffects.EnduranceOrBleedingOther != 0 && !card.SelfCard.StatusEffects.IsEnemyTargetEnduranceOrBleeding)
                {
                    _cardMechanics.BleedingOrEndurance(card, Enemy.Field.Cards[i]);
                    _uiManager.CheckBleeding(Enemy.Field.Cards[i]);
                }

                if (card.SelfCard.EndTurnActions.ArmorOther > 0)
                {
                    Enemy.Field.Cards[i].SelfCard.BaseCard.ArmorPoints += card.SelfCard.EndTurnActions.ArmorOther;
                    _uiManager.CheckArmor(Enemy.Field.Cards[i]);
                }
            }

            _soundManager.EnemyStartEffectSound(card);
        }


        else if ((card.SelfCard.BoostOrDamage.Boost != 0) && (Enemy.Field.Cards.Count != 1) && ((Enemy.Field.Cards.Count - Enemy.Field.InvulnerabilityCards.Count) > 0))
        {
            botChoosedCard = ChooseCard(false);
            _cardMechanics.Deployment(botChoosedCard, card);

            if (card.SelfCard.StatusEffects.IsShieldOther)
            {
                botChoosedCard.SelfCard.StatusEffects.IsSelfShielded = true;
                _cardMechanics.CheckStatusEffects(botChoosedCard);
            }

            if (card.SelfCard.BoostOrDamage.NearBoost > 0)
            {
                botChoosedCard.CheckSiblingIndex();

                if (_cardMechanics.ReturnNearCard(botChoosedCard, card.SelfCard.BoostOrDamage.NearBoost, true) != null)
                {
                    for (int i = 0; i < _cardMechanics.ReturnNearCard(botChoosedCard, card.SelfCard.BoostOrDamage.NearBoost, true).Count; i++)
                    {
                        _cardMechanics.Deployment(_cardMechanics.ReturnNearCard(botChoosedCard, card.SelfCard.BoostOrDamage.NearBoost, true)[i], card, i + 1);
                    }
                }

                if (_cardMechanics.ReturnNearCard(botChoosedCard, card.SelfCard.BoostOrDamage.NearBoost, false) != null)
                {
                    for (int i = 0; i < _cardMechanics.ReturnNearCard(botChoosedCard, card.SelfCard.BoostOrDamage.NearBoost, false).Count; i++)
                    {
                        _cardMechanics.Deployment(_cardMechanics.ReturnNearCard(botChoosedCard, card.SelfCard.BoostOrDamage.NearBoost, false)[i], card, i + 1);
                    }
                }
            }

            _soundManager.EnemyStartEffectSound(card);
        }

        if (card.SelfCard.BoostOrDamage.NearDamage == -1)
        {
            for (int i = Player.Field.Cards.Count - 1; i >= 0; i--)
            {
                _cardMechanics.Deployment(Player.Field.Cards[i], card);

                if (card.SelfCard.StatusEffects.IsStunOther)
                {
                    Player.Field.Cards[i].SelfCard.StatusEffects.IsSelfStunned = true;
                    _cardMechanics.CheckStatusEffects(Player.Field.Cards[i]);
                }

                if (card.SelfCard.StatusEffects.EnduranceOrBleedingOther != 0 && card.SelfCard.StatusEffects.IsEnemyTargetEnduranceOrBleeding)
                {
                    _cardMechanics.BleedingOrEndurance(card, Player.Field.Cards[i]);
                }
            }

            _soundManager.EnemyStartEffectSound(card);
        }

        else if ((card.SelfCard.BoostOrDamage.Damage != 0) && (Player.Field.Cards.Count != 0) && ((Player.Field.Cards.Count - Player.Field.InvulnerabilityCards.Count) > 0))
        {
            botChoosedCard = ChooseCard(false, false);
            _cardMechanics.Deployment(botChoosedCard, card);

            if (card.SelfCard.StatusEffects.IsStunOther)
            {
                botChoosedCard.SelfCard.StatusEffects.IsSelfStunned = true;
                _cardMechanics.CheckStatusEffects(botChoosedCard);
            }

            if (card.SelfCard.BoostOrDamage.NearDamage > 0)
            {
                botChoosedCard.CheckSiblingIndex();

                if (_cardMechanics.ReturnNearCard(botChoosedCard, card.SelfCard.BoostOrDamage.NearDamage, true) != null)
                {
                    for (int i = 0; i < _cardMechanics.ReturnNearCard(botChoosedCard, card.SelfCard.BoostOrDamage.NearDamage, true).Count; i++)
                    {
                        _cardMechanics.Deployment(_cardMechanics.ReturnNearCard(botChoosedCard, card.SelfCard.BoostOrDamage.NearDamage, true)[i], card, i + 1);

                        if (card.SelfCard.StatusEffects.IsStunOther)
                        {
                            _cardMechanics.ReturnNearCard(botChoosedCard, card.SelfCard.BoostOrDamage.NearDamage, true)[i].SelfCard.StatusEffects.IsSelfStunned = true;
                            _cardMechanics.CheckStatusEffects(_cardMechanics.ReturnNearCard(botChoosedCard, card.SelfCard.BoostOrDamage.NearDamage, true)[i]);
                        }
                    }
                }

                if (_cardMechanics.ReturnNearCard(botChoosedCard, card.SelfCard.BoostOrDamage.NearDamage, false) != null)
                {
                    for (int i = 0; i < _cardMechanics.ReturnNearCard(botChoosedCard, card.SelfCard.BoostOrDamage.NearDamage, false).Count; i++)
                    {
                        _cardMechanics.Deployment(_cardMechanics.ReturnNearCard(botChoosedCard, card.SelfCard.BoostOrDamage.NearDamage, false)[i], card, i + 1);

                        if (card.SelfCard.StatusEffects.IsStunOther)
                        {
                            _cardMechanics.ReturnNearCard(botChoosedCard, card.SelfCard.BoostOrDamage.NearDamage, false)[i].SelfCard.StatusEffects.IsSelfStunned = true;
                            _cardMechanics.CheckStatusEffects(_cardMechanics.ReturnNearCard(botChoosedCard, card.SelfCard.BoostOrDamage.NearDamage, false)[i]);
                        }
                    }
                }
            }

            if (!card.SelfCard.BoostOrDamage.AddictionWithEnemyField)
                _soundManager.EnemyStartEffectSound(card);
        }

        if ((card.SelfCard.BoostOrDamage.SelfBoost != 0 || card.SelfCard.BoostOrDamage.SelfDamage != 0) && ((!card.SelfCard.BoostOrDamage.AddictionWithAlliedField && !card.SelfCard.BoostOrDamage.AddictionWithEnemyField) ||
           (card.SelfCard.BoostOrDamage.AddictionWithAlliedField && (Enemy.Field.Cards.Count - Enemy.Field.InvulnerabilityCards.Count != 1) ||
           (card.SelfCard.BoostOrDamage.AddictionWithEnemyField && ((Player.Field.Cards.Count - Player.Field.InvulnerabilityCards.Count) > 0)))))
        {
            _cardMechanics.Self(card, card);
            _soundManager.EnemyStartEffectSound(card);
        }

        if (card.SelfCard.Spawns.SpawnCardCount != 0 && (!card.SelfCard.BoostOrDamage.AddictionWithEnemyField) || 
            (card.SelfCard.BoostOrDamage.AddictionWithEnemyField && Player.Field.Cards.Count > 0))
        {
            _soundManager.EnemyStartEffectSound(card);
            _cardMechanics.SpawnCard(card, false);
            ChangeEnemyPoints();
        }

        if (card.SelfCard.DrawCard.DrawCardCount != 0)
        {
            for (int i = 0; i < card.SelfCard.DrawCard.DrawCardCount; i++)
            {
                yield return StartCoroutine(GiveCardtoHand(CurrentGame.EnemyDeck, _enemyHand, TimeDrawCard, false));
            }
        }

        if ((card.SelfCard.UniqueMechanics.DestroyCardPoints != 0) && (Player.Field.Cards.Count != 0) && ((Player.Field.Cards.Count - Player.Field.InvulnerabilityCards.Count) > 0))
        {
            if (card.SelfCard.UniqueMechanics.DestroyCardPoints == -1)
            {
                botChoosedCard = ChooseCard(false, false);
                _cardMechanics.DestroyCard(botChoosedCard, card);
            }

            else
            {
                List<CardInfoScript> possibleCards = new List<CardInfoScript>();

                foreach (CardInfoScript playerFieldCard in Player.Field.Cards)
                {
                    if (playerFieldCard.SelfCard.BaseCard.Points <= card.SelfCard.UniqueMechanics.DestroyCardPoints && !playerFieldCard.SelfCard.StatusEffects.IsInvulnerability)
                    {
                        possibleCards.Add(playerFieldCard);
                    }
                }

                if (possibleCards.Count > 0)
                {
                    botChoosedCard = possibleCards[Random.Range(0, possibleCards.Count - 1)];
                    _cardMechanics.DestroyCard(botChoosedCard, card);
                }
            }
        }

        if (card.SelfCard.UniqueMechanics.SwapPoints)
        {
            if ((Player.Field.Cards.Count != 0) && ((Player.Field.Cards.Count - Player.Field.InvulnerabilityCards.Count) > 0))
            {
                botChoosedCard = ChooseCard(false, false);
                _cardMechanics.SwapPoints(card, botChoosedCard);
            }

            else if ((Enemy.Field.Cards.Count != 1) && ((Enemy.Field.Cards.Count - Enemy.Field.InvulnerabilityCards.Count) > 0))
            {
                botChoosedCard = ChooseCard(false, true);
                _cardMechanics.SwapPoints(card, botChoosedCard);
            }
        }

        if (card.SelfCard.StatusEffects.EnduranceOrBleedingOther != 0)
        {
            if (card.SelfCard.StatusEffects.IsEnemyTargetEnduranceOrBleeding && card.SelfCard.BoostOrDamage.NearBoost != -1 && 
                ((Player.Field.Cards.Count - Player.Field.InvulnerabilityCards.Count) > 0))
            {
                botChoosedCard = ChooseCard(false, false);
                _cardMechanics.BleedingOrEndurance(card, botChoosedCard);
                _uiManager.CheckBleeding(botChoosedCard);
            }

            else if (card.SelfCard.BoostOrDamage.NearDamage != -1 && (Enemy.Field.Cards.Count - Enemy.Field.InvulnerabilityCards.Count) > 1)
            {
                botChoosedCard = ChooseCard(false, true);
                _cardMechanics.BleedingOrEndurance(card, botChoosedCard);
                _uiManager.CheckBleeding(botChoosedCard);
            }

        }

        if (card.SelfCard.UniqueMechanics.TransformationCardName != "")
        {
            _cardMechanics.Transformation(card);
        }

        ChangeEnemyPoints();
        ChangePlayerPoints();
    }

    private void PlayerDropCardStartCoroutine(CardInfoScript card)
    {
        AllCoroutine.Add(StartCoroutine(PlayerDropCard(card)));
    }

    private IEnumerator PlayerDropCard(CardInfoScript card)
    {
        IsHandCardPlaying = true;

        CardMove cardMove = card.GetComponent<CardMove>();
        cardMove.MoveTopHierarchy();

        if (!card.SelfCard.StatusEffects.IsInvisibility)
            cardMove.PlayerMoveToField(_playerField.GetComponent<DropField>(), _playerHand.GetComponent<DropField>().EmptyHandCard);

        else if (card.SelfCard.StatusEffects.IsInvisibility)
            cardMove.PlayerMoveToField(_enemyField.GetComponent<DropField>(), _playerHand.GetComponent<DropField>().EmptyHandCard, true);

        yield return new WaitForSeconds(0.6f);

        card.IsAnimationCard = false;
        cardMove.MoveBackHierarchy();

        Player.Hand.Remove(card);

        if (!card.SelfCard.StatusEffects.IsInvisibility)
        {
            Player.Field.Add(card);
            ChangePlayerPoints();

            if (card.SelfCard.StatusEffects.IsInvulnerability)
                Player.Field.InvulnerabilityCards.Add(card);
        }

        else if (card.SelfCard.StatusEffects.IsInvisibility)
        {
            Enemy.Field.Add(card);
            ChangeEnemyPoints();

            if (card.SelfCard.StatusEffects.IsInvulnerability)
                Enemy.Field.InvulnerabilityCards.Add(card);
        }

        _cardMechanics.CheckStatusEffects(card);

        _soundManager.PlayerDeploymentSound(card);

        if (card.SelfCard.BoostOrDamage.NearBoost == -1)
        {
            for (int i = Player.Field.Cards.Count - 1; i >= 0; i--)
            {
                _cardMechanics.Deployment(Player.Field.Cards[i], card);

                if (card.SelfCard.StatusEffects.EnduranceOrBleedingOther != 0 && !card.SelfCard.StatusEffects.IsEnemyTargetEnduranceOrBleeding)
                {
                    _cardMechanics.BleedingOrEndurance(card, Player.Field.Cards[i]);
                    _uiManager.CheckBleeding(Player.Field.Cards[i]);
                }

                if (card.SelfCard.EndTurnActions.ArmorOther > 0)
                {
                    Player.Field.Cards[i].SelfCard.BaseCard.ArmorPoints += card.SelfCard.EndTurnActions.ArmorOther;
                    _uiManager.CheckArmor(Player.Field.Cards[i]);
                }
            }

            _soundManager.PlayerStartEffectSound(card);
        }

        else if ((card.SelfCard.BoostOrDamage.Boost != 0) && Player.Field.Cards.Count != 1 && ((Player.Field.Cards.Count - Player.Field.InvulnerabilityCards.Count) > 1))
        {
            if (card.SelfCard.BoostOrDamage.NearBoost != -1)
            {
                PrepareToChoseCard(card, false);

                AllCoroutine.Add(StartCoroutine(ChoseCardCoroutine(card, isBoost: true)));
            }
        }

        if (card.SelfCard.BoostOrDamage.NearDamage == -1)
        {
            for (int i = Enemy.Field.Cards.Count - 1; i >= 0; i--)
            {
                _cardMechanics.Deployment(Enemy.Field.Cards[i], card);

                if (card.SelfCard.StatusEffects.IsStunOther)
                {
                    Enemy.Field.Cards[i].SelfCard.StatusEffects.IsSelfStunned = true;
                    _cardMechanics.CheckStatusEffects(Enemy.Field.Cards[i]);
                }

                if (card.SelfCard.StatusEffects.EnduranceOrBleedingOther != 0 && card.SelfCard.StatusEffects.IsEnemyTargetEnduranceOrBleeding)
                {
                    _cardMechanics.BleedingOrEndurance(card, Enemy.Field.Cards[i]);
                }
            }

            _soundManager.PlayerStartEffectSound(card);
        }

        else if ((card.SelfCard.BoostOrDamage.Damage != 0) && (Enemy.Field.Cards.Count != 0) && ((Enemy.Field.Cards.Count - Enemy.Field.InvulnerabilityCards.Count) > 0))
        {
            if (card.SelfCard.BoostOrDamage.NearDamage != -1)
            {
                PrepareToChoseCard(card, true);

                AllCoroutine.Add(StartCoroutine(ChoseCardCoroutine(card, isDamage: true)));
            }
        }

        if (((card.SelfCard.BoostOrDamage.SelfBoost != 0) || (card.SelfCard.BoostOrDamage.SelfDamage != 0)) && (!card.SelfCard.BoostOrDamage.AddictionWithAlliedField && !card.SelfCard.BoostOrDamage.AddictionWithEnemyField))
        {
            _cardMechanics.Self(card, card);

            ChangePlayerPoints();

            _soundManager.PlayerStartEffectSound(card);
        }

        if (card.SelfCard.Spawns.SpawnCardCount != 0 && !card.SelfCard.BoostOrDamage.AddictionWithEnemyField)
        {
            _soundManager.PlayerStartEffectSound(card);
            _cardMechanics.SpawnCard(card, true);
            ChangePlayerPoints();
        }

        if (card.SelfCard.DrawCard.DrawCardCount != 0)
        {
            _uiManager.ChangeEndTurnButtonInteractable(false);
            for (int i = 0; i < card.SelfCard.DrawCard.DrawCardCount; i++)
            {
                yield return StartCoroutine(GiveCardtoHand(CurrentGame.PlayerDeck, _playerHand, TimeDrawCard, true));
            }
            _uiManager.ChangeEndTurnButtonInteractable(true);
        }

        if (card.SelfCard.UniqueMechanics.DestroyCardPoints != 0)
        {
            if (card.SelfCard.UniqueMechanics.DestroyCardPoints == -1)
            {
                PrepareToChoseCard(card, true);

                AllCoroutine.Add(StartCoroutine(ChoseCardCoroutine(card, isDestroy: true)));
            }

            else
            {
                List<CardInfoScript> possibleCards = new List<CardInfoScript>();

                foreach (CardInfoScript enemyFieldCard in Enemy.Field.Cards)
                {
                    if (enemyFieldCard.SelfCard.BaseCard.Points <= card.SelfCard.UniqueMechanics.DestroyCardPoints && !enemyFieldCard.SelfCard.StatusEffects.IsInvulnerability)
                    {
                        possibleCards.Add(enemyFieldCard);
                    }
                }

                if (possibleCards.Count > 0)
                {
                    foreach (CardInfoScript possibleChoseCard in possibleCards)
                    {
                        possibleChoseCard.GetComponent<ChoseCard>().enabled = true;
                    }

                    AllCoroutine.Add(StartCoroutine(ChoseCardCoroutine(card, isDestroy: true)));
                }
            }
        }

        if (card.SelfCard.UniqueMechanics.SwapPoints && (((Enemy.Field.Cards.Count - Enemy.Field.InvulnerabilityCards.Count) > 0) || 
            ((Player.Field.Cards.Count - Player.Field.InvulnerabilityCards.Count) > 1)))
        {
            PrepareToChoseCard(card, true);
            PrepareToChoseCard(card, false);

            AllCoroutine.Add(StartCoroutine(ChoseCardCoroutine(card, isSwapPoints: true)));
        }

        if (card.SelfCard.StatusEffects.EnduranceOrBleedingOther != 0)
        {
            if (card.SelfCard.StatusEffects.IsEnemyTargetEnduranceOrBleeding && card.SelfCard.BoostOrDamage.NearDamage != -1 && 
                ((Enemy.Field.Cards.Count - Enemy.Field.InvulnerabilityCards.Count) > 0))
            {
                PrepareToChoseCard(card, true);
                AllCoroutine.Add(StartCoroutine(ChoseCardCoroutine(card, isEnduranceOrBleeding: true, isEnduranceOrBleedingEnemy: true)));
            }

            else if (card.SelfCard.BoostOrDamage.NearBoost != -1 && (Player.Field.Cards.Count - Player.Field.InvulnerabilityCards.Count) > 1)
            {
                PrepareToChoseCard(card, false);
                AllCoroutine.Add(StartCoroutine(ChoseCardCoroutine(card, isEnduranceOrBleeding: true, isEnduranceOrBleedingEnemy: false)));
            }
        }

        if (card.SelfCard.UniqueMechanics.TransformationCardName != "")
        {
            _cardMechanics.Transformation(card);
        }

        ChangeEnemyPoints();
        ChangePlayerPoints();
    }

    private void PrepareToChoseCard(CardInfoScript playedCard, bool isEnemyField)
    {
        if (isEnemyField)
        {
            foreach (CardInfoScript enemyFieldCard in Enemy.Field.Cards)
            {
                if (!enemyFieldCard.SelfCard.StatusEffects.IsInvulnerability)
                {
                    enemyFieldCard.transform.GetComponent<ChoseCard>().enabled = true;
                    enemyFieldCard.IsOrderCard = true;
                    CardsCanChooseOnWickEnd.Add(enemyFieldCard);
                }

                enemyFieldCard.IsOrderCard = true;
            }

            CardsCanChooseOnWickEnd.Remove(playedCard);

            _uiManager.ChangeLineColor(Color.white, Color.red);

        }

        else
        {
            foreach (CardInfoScript playerFieldCard in Player.Field.Cards)
            {
                if (!playerFieldCard.SelfCard.StatusEffects.IsInvulnerability)
                {
                    playerFieldCard.transform.GetComponent<ChoseCard>().enabled = true;
                    playerFieldCard.IsOrderCard = true;
                    CardsCanChooseOnWickEnd.Add(playerFieldCard);
                }

                playerFieldCard.IsOrderCard = true;
            }

            playedCard.transform.GetComponent<ChoseCard>().enabled = false;
            CardsCanChooseOnWickEnd.Remove(playedCard);

            _uiManager.ChangeLineColor(Color.white, Color.green);
        }
    }

    private void RemovePrepareToChoseCard(bool isEnemyField)
    {
        if (isEnemyField)
        {
            foreach (CardInfoScript enemyFieldCard in Enemy.Field.Cards)
            {
                enemyFieldCard.transform.GetComponent<ChoseCard>().enabled = false;
                CardsCanChooseOnWickEnd.Remove(enemyFieldCard);
                enemyFieldCard.ImageEdge1.color = Color.white;
                enemyFieldCard.IsOrderCard = false;
            }
        }

        else
        {
            foreach (CardInfoScript playerFieldCard in Player.Field.Cards)
            {
                playerFieldCard.transform.GetComponent<ChoseCard>().enabled = false;
                CardsCanChooseOnWickEnd.Remove(playerFieldCard);
                playerFieldCard.ImageEdge1.color = Color.white;
                playerFieldCard.IsOrderCard = false;
            }
        }

    }

    private void ChangeEnemyPoints()
    {
        _enemyPoints = 0;

        foreach (CardInfoScript card in Enemy.Field.Cards)
        {
            _enemyPoints += card.ShowPoints(card.SelfCard);
        }

        _uiManager.ChangePoints(_playerPoints, _enemyPoints);
    }

    private void ChangePlayerPoints()
    {
        _playerPoints = 0;

        foreach (CardInfoScript card in Player.Field.Cards)
        {
            _playerPoints += card.ShowPoints(card.SelfCard);
        }

        _uiManager.ChangePoints(_playerPoints, _enemyPoints);
    }

    private CardInfoScript ChooseCard(bool isPlayerChoose, bool isFriendlyCard = true)
    {
        if (isPlayerChoose)
        {
            return _choosenCard;
        }

        else
        {
            if (!isFriendlyCard)
            {
                List<CardInfoScript> choosenCardList = new List<CardInfoScript>(Player.Field.Cards);

                foreach (CardInfoScript card in Player.Field.InvulnerabilityCards)
                {
                    if (choosenCardList.Contains(card))
                        choosenCardList.Remove(card);
                }

                return choosenCardList[Random.Range(0, choosenCardList.Count - 1)];
            }

            else
            {
                List<CardInfoScript> choosenCardList = new List<CardInfoScript>(Enemy.Field.Cards);

                foreach (CardInfoScript card in Enemy.Field.InvulnerabilityCards)
                {
                    if (choosenCardList.Contains(card))
                        choosenCardList.Remove(card);
                }

                return choosenCardList[Random.Range(0, choosenCardList.Count - 1)];
            }
        }
    }

    private IEnumerator ChoseCardCoroutine(CardInfoScript playedCard, bool isBoost = false, bool isDamage = false, bool isDestroy = false, bool isSwapPoints = false, bool isEnduranceOrBleeding = false, bool isEnduranceOrBleedingEnemy = false)
    {
        StartChoseCard = playedCard;
        playedCard.ImageEdge1.color = Color.green;
        _uiManager.ChangeEndTurnButtonInteractable(false);

        yield return StartCoroutine(WaitForChoseCard(playedCard));
        IsChooseCard = false;

        if (isBoost)
        {
            if ((playedCard.SelfCard.BoostOrDamage.AddictionWithAlliedField && (Player.Field.Cards.Count != 1 && 
                (Player.Field.Cards.Count - Player.Field.InvulnerabilityCards.Count) > 1)) ||
            (playedCard.SelfCard.BoostOrDamage.AddictionWithEnemyField && (Enemy.Field.Cards.Count != 0) &&
            (Enemy.Field.Cards.Count - Enemy.Field.InvulnerabilityCards.Count != 0)))
            {
                _cardMechanics.Self(playedCard, playedCard);
            }

            if (playedCard.SelfCard.StatusEffects.IsShieldOther)
                ChooseCard(true).SelfCard.StatusEffects.IsSelfShielded = true;

            _cardMechanics.Deployment(ChooseCard(true), playedCard);

            RemovePrepareToChoseCard(false);

            if (playedCard.SelfCard.BoostOrDamage.NearBoost > 0)
            {
                ChooseCard(true).CheckSiblingIndex();

                if (_cardMechanics.ReturnNearCard(ChooseCard(true), playedCard.SelfCard.BoostOrDamage.NearBoost, true) != null)
                {
                    for (int i = 0; i < _cardMechanics.ReturnNearCard(ChooseCard(true), playedCard.SelfCard.BoostOrDamage.NearBoost, true).Count; i++)
                    {
                        _cardMechanics.Deployment(_cardMechanics.ReturnNearCard(ChooseCard(true), playedCard.SelfCard.BoostOrDamage.NearBoost, true)[i], playedCard, i + 1);
                    }
                }

                if (_cardMechanics.ReturnNearCard(ChooseCard(true), playedCard.SelfCard.BoostOrDamage.NearBoost, false) != null)
                {
                    for (int i = 0; i < _cardMechanics.ReturnNearCard(ChooseCard(true), playedCard.SelfCard.BoostOrDamage.NearBoost, false).Count; i++)
                    {
                        _cardMechanics.Deployment(_cardMechanics.ReturnNearCard(ChooseCard(true), playedCard.SelfCard.BoostOrDamage.NearBoost, false)[i], playedCard, i + 1);
                    }
                }
            }
        }

        if (isDamage)
        {
            if ((playedCard.SelfCard.BoostOrDamage.AddictionWithAlliedField && (Player.Field.Cards.Count != 1 && (Player.Field.Cards.Count - Player.Field.InvulnerabilityCards.Count) > 1)) ||
            (playedCard.SelfCard.BoostOrDamage.AddictionWithEnemyField && (Enemy.Field.Cards.Count != 0)) &&
            (Enemy.Field.Cards.Count - Enemy.Field.InvulnerabilityCards.Count != 0))
            {
                _cardMechanics.Self(playedCard, playedCard);
            }

            _cardMechanics.Deployment(ChooseCard(true, false), playedCard);
            if (playedCard.SelfCard.StatusEffects.IsStunOther)
            {
                ChooseCard(true, false).SelfCard.StatusEffects.IsSelfStunned = true;
                _cardMechanics.CheckStatusEffects(ChooseCard(true, false));
            }

            RemovePrepareToChoseCard(true);

            if (playedCard.SelfCard.BoostOrDamage.NearDamage > 0)
            {
                ChooseCard(true, false).CheckSiblingIndex();

                if (_cardMechanics.ReturnNearCard(ChooseCard(true), playedCard.SelfCard.BoostOrDamage.NearDamage, true) != null)
                {
                    for (int i = 0; i < _cardMechanics.ReturnNearCard(ChooseCard(true), playedCard.SelfCard.BoostOrDamage.NearDamage, true).Count; i++)
                    {
                        _cardMechanics.Deployment(_cardMechanics.ReturnNearCard(ChooseCard(true), playedCard.SelfCard.BoostOrDamage.NearDamage, true)[i], playedCard, i + 1);

                        if (playedCard.SelfCard.StatusEffects.IsStunOther)
                        {
                            _cardMechanics.ReturnNearCard(ChooseCard(true), playedCard.SelfCard.BoostOrDamage.NearDamage, true)[i].SelfCard.StatusEffects.IsSelfStunned = true;
                            _cardMechanics.CheckStatusEffects(_cardMechanics.ReturnNearCard(ChooseCard(true), playedCard.SelfCard.BoostOrDamage.NearDamage, true)[i]);
                        }
                    }
                }

                if (_cardMechanics.ReturnNearCard(ChooseCard(true), playedCard.SelfCard.BoostOrDamage.NearDamage, false) != null)
                {
                    for (int i = 0; i < _cardMechanics.ReturnNearCard(ChooseCard(true), playedCard.SelfCard.BoostOrDamage.NearDamage, false).Count; i++)
                    {
                        _cardMechanics.Deployment(_cardMechanics.ReturnNearCard(ChooseCard(true), playedCard.SelfCard.BoostOrDamage.NearDamage, false)[i], playedCard, i + 1);

                        if (playedCard.SelfCard.StatusEffects.IsStunOther)
                        {
                            _cardMechanics.ReturnNearCard(ChooseCard(true), playedCard.SelfCard.BoostOrDamage.NearDamage, false)[i].SelfCard.StatusEffects.IsSelfStunned = true;
                            _cardMechanics.CheckStatusEffects(_cardMechanics.ReturnNearCard(ChooseCard(true), playedCard.SelfCard.BoostOrDamage.NearDamage, false)[i]);
                        }
                    }
                }
            }
        }

        if (playedCard.SelfCard.Spawns.SpawnCardCount != 0 && playedCard.SelfCard.BoostOrDamage.AddictionWithEnemyField &&
            Enemy.Field.Cards.Count > 0 && (Enemy.Field.Cards.Count - Enemy.Field.InvulnerabilityCards.Count) > 0)
        {
            _cardMechanics.SpawnCard(playedCard, true);
        }

        if (isDestroy)
        {
            _cardMechanics.DestroyCard(ChooseCard(true), playedCard);

            RemovePrepareToChoseCard(true);
        }

        if (isSwapPoints)
        {
            _cardMechanics.SwapPoints(playedCard, ChooseCard(true));

            RemovePrepareToChoseCard(true);

            RemovePrepareToChoseCard(false);
        }

        if (isEnduranceOrBleeding)
        {
            _cardMechanics.BleedingOrEndurance(playedCard, ChooseCard(true));
            _uiManager.CheckBleeding(ChooseCard(true));

            if (isEnduranceOrBleedingEnemy)
                RemovePrepareToChoseCard(true);
            else
                RemovePrepareToChoseCard(false);
        }

        _uiManager.ChangeLinePosition(0, Vector3.zero);
        _uiManager.ChangeLinePosition(1, Vector3.zero);

        playedCard.ImageEdge1.color = Color.white;
        _uiManager.ChangeEndTurnButtonInteractable(true);

        ChangeEnemyPoints();
        ChangePlayerPoints();

        _soundManager.PlayerStartEffectSound(playedCard);
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
        _choosenCard = card;
    }

    private void ClearDestroyedInEndTurnCards()
    {
        foreach (CardInfoScript destroyedCard in Player.Field.DestroyedInEndTurnCards)
        {
            _cardMechanics.DestroyCard(destroyedCard);
        }

        foreach (CardInfoScript destroyedCard in Enemy.Field.DestroyedInEndTurnCards)
        {
            _cardMechanics.DestroyCard(destroyedCard);
        }

        Player.Field.DestroyedInEndTurnCards.Clear();
        Enemy.Field.DestroyedInEndTurnCards.Clear();
    }

    public List<CardInfoScript> EndTurnOrderCard(List<CardInfoScript> cardsInField, bool isPlayerField)
    {
        List<CardInfoScript> temporyList = new List<CardInfoScript>(cardsInField);

        if (isPlayerField)
        {
            for (int i = 0; i < _playerField.childCount; i++)
            {
                temporyList[i] = _playerField.GetChild(i).GetComponent<CardInfoScript>();
            }
        }
        else
        {
            for (int i = 0; i < _enemyField.childCount; i++)
            {
                temporyList[i] = _enemyField.GetChild(i).GetComponent<CardInfoScript>();
            }
        }

        return temporyList;
    }

    public void NewGame()
    {
        HideEndGamePanel.Invoke();

        StopAllCoroutines();

        IsHandCardPlaying = false;

        _turn = 0;

        _playerPoints = 0;
        _enemyPoints = 0;

        _uiManager.ChangePoints(_playerPoints, _enemyPoints);

        for (int i = Enemy.Hand.Cards.Count - 1; i >= 0; i--)
        {
            Destroy(Enemy.Hand.Cards[i].gameObject);
            Enemy.Hand.Remove(Enemy.Hand.Cards[i]);
        }

        for (int i = Player.Hand.Cards.Count - 1; i >= 0; i--)
        {
            Destroy(Player.Hand.Cards[i].gameObject);
            Player.Hand.Remove(Player.Hand.Cards[i]);
        }

        for (int i = Enemy.Field.Cards.Count - 1; i >= 0; i--)
        {
            Destroy(Enemy.Field.Cards[i].gameObject);
            Enemy.Field.Remove(Enemy.Field.Cards[i]);
        }

        for (int i = Player.Field.Cards.Count - 1; i >= 0; i--)
        {
            Destroy(Player.Field.Cards[i].gameObject);
            Player.Field.Remove(Player.Field.Cards[i]);
        }

        Player.Field.InvulnerabilityCards.Clear();
        Enemy.Field.InvulnerabilityCards.Clear();

        _enemyHandPass.SetActive(false);
        _playerHandPass.SetActive(false);
        _isPlayerPassed = false;
        _isEnemyPassed = false;

        CurrentGame = new Game(this);

        Deck.Instance.DeleteDeck();
        Deck.Instance.CreateDeck(CurrentGame.PlayerDeck);

        StartCoroutine(GiveHandCards(CurrentGame.EnemyDeck, _enemyHand, false));
        StartCoroutine(GiveHandCards(CurrentGame.PlayerDeck, _playerHand, true));

        _uiManager.ChangeEndTurnButtonInteractable(true);

        AllCoroutine.Add(StartCoroutine(TurnFunk()));
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
            Player.Hand.Remove(card);
            IsHandCardPlaying = true;
        }
        else
        {
            Enemy.Hand.Remove(card);
        }

        Destroy(card.transform.gameObject);
    }

    private void EndGame()
    {
        StopAllCoroutines();
    }
}
