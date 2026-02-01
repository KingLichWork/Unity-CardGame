using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class UIManager : MonoBehaviour
{
    [SerializeField] private EndGamePanel _endGamePanel;

    [SerializeField] private TextMeshProUGUI _playerPointsTMPro;
    [SerializeField] private TextMeshProUGUI _enemyPointsTMPro;
    [SerializeField] private TextMeshProUGUI _playerDeckTMPro;
    [SerializeField] private TextMeshProUGUI _enemyDeckTMPro;

    [SerializeField] private Image[] _imageTurnTime = new Image[2];
    [SerializeField] private Button _endTurnButton;

    [SerializeField] private LineRenderer _line;

    private bool _isPause;
    public bool IsPause => _isPause;

    private void OnEnable()
    {
        GameManager.ChangePoints += ChangePoints;
    }

    private void OnDisable()
    {
        GameManager.ChangePoints -= ChangePoints;
    }

    public void ChangeEndTurnButtonInteractable(bool isInteractable)
    {
        _endTurnButton.interactable = isInteractable;
    }

    public bool ReturnEndTurnButtonInteractable()
    {
        return _endTurnButton.interactable;
    }

    public void ChangeDeckCount(Game currentGame)
    {
        _playerDeckTMPro.text = currentGame.PlayerDeck.Count.ToString();
        _enemyDeckTMPro.text = currentGame.EnemyDeck.Count.ToString();
    }

    public void ChangePoints(int playerPoints, int enemyPoints)
    {
        _playerPointsTMPro.text = playerPoints.ToString();
        _enemyPointsTMPro.text = enemyPoints.ToString();

        _enemyPointsTMPro.color = (playerPoints >= enemyPoints) ? Color.white : Color.red;
        _enemyPointsTMPro.fontSize = (playerPoints >= enemyPoints) ? 36 : 50;
        _playerPointsTMPro.color = (playerPoints >= enemyPoints) ? Color.red : Color.white;
        _playerPointsTMPro.fontSize = (playerPoints >= enemyPoints) ? 50 : 36;
    }

    public void ChangeWick(int currentTime)
    {
        _imageTurnTime[0].fillAmount = (float)currentTime / GameManager.TurnDuration;
        _imageTurnTime[1].fillAmount = (float)currentTime / GameManager.TurnDuration;
    }

    public void ChangeLineColor(Color firstColor, Color secondColor)
    {
        _line.startColor = firstColor;
        _line.endColor = secondColor;
    }

    public void ChangeLinePosition(int point, Vector3 position)
    {
        _line.SetPosition(point, position);
    }

    public void CheckColorPointsCard(CardInfoScript card)
    {
        if (card.SelfCard.BaseCard.Points == card.SelfCard.BaseCard.MaxPoints)
            card.Point.colorGradient = new VertexGradient(Color.white, Color.white, Color.white, Color.white);
        else if (card.SelfCard.BaseCard.Points < card.SelfCard.BaseCard.MaxPoints)
            card.Point.colorGradient = new VertexGradient(Color.red, Color.red, Color.white, Color.white);
        else
            card.Point.colorGradient = new VertexGradient(Color.green, Color.green, Color.white, Color.white);
    }

    public void CheckTimer(CardInfoScript card)
    {
        card.TimerObject.SetActive(card.SelfCard.EndTurnActions.Timer > 0);

        if (card.SelfCard.EndTurnActions.Timer > 0)
            card.TimerText.text = card.SelfCard.EndTurnActions.Timer.ToString();
    }

    public void CheckBleeding(CardInfoScript card)
    {
        int count = card.SelfCard.StatusEffects.SelfEnduranceOrBleeding;

        card.BleedingPanel.SetActive(count != 0);
        card.BleedingPanel.GetComponent<Image>().color = count < 0 ? Color.red : Color.green;
        card.BleedingPanelText.text = count < 0 ? (-count).ToString() : count.ToString();
    }

    public void CheckArmor(CardInfoScript card)
    {
        card.ArmorObject.SetActive(card.SelfCard.BaseCard.ArmorPoints > 0);

        if (card.SelfCard.BaseCard.ArmorPoints > 0)
            card.ArmorPoints.text = card.SelfCard.BaseCard.ArmorPoints.ToString();
    }

    public void EndGame(int playerPoints, int enemyPoint)
    {
        StopAllCoroutines();

        _endGamePanel.EndGame(playerPoints,enemyPoint);
    }

    public void Pause()
    {
        _isPause = true;
        _endGamePanel.Pause();
    }

    public void UnPause()
    {
        _isPause = false;
        _endGamePanel.Hide();
    }
}
