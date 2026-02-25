using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSelectionService
{
    private UIManager _ui;
    private Camera _camera;

    public CardInfoScript StartChoseCard { get; private set; }
    public bool IsChoosing { get; private set; }

    private CardInfoScript _chosenCard;
    private readonly List<CardInfoScript> _cards = new();

    public CardSelectionService(UIManager ui)
    {
        _ui = ui;
        _camera = Camera.main;
    }

    public void SetChosen(CardInfoScript card)
    {
        _chosenCard = card;
    }

    public CardInfoScript GetChosen() => _chosenCard;

    public void Prepare(IEnumerable<CardInfoScript> fieldCards, CardInfoScript playedCard, bool enemyField)
    {
        _cards.Clear();

        foreach (var c in fieldCards)
        {
            if (!c.SelfCard.StatusEffects.IsInvulnerability)
            {
                c.GetComponent<ChoseCard>().enabled = true;
                _cards.Add(c);
            }

            c.IsOrderCard = true;
        }

        _cards.Remove(playedCard);

        _ui.ChangeLineColor(Color.white, enemyField ? Color.red : Color.green);
    }

    public void Clear(IEnumerable<CardInfoScript> fieldCards)
    {
        foreach (var c in fieldCards)
        {
            c.GetComponent<ChoseCard>().enabled = false;
            c.IsOrderCard = false;
            c.ImageEdge1.color = Color.white;
        }

        _cards.Clear();
    }

    public IEnumerator WaitForChoose(CardInfoScript playedCard)
    {
        StartChoseCard = playedCard;
        playedCard.ImageEdge1.color = Color.green;

        _chosenCard = null;
        IsChoosing = true;

        while (_chosenCard == null)
        {
            _ui.ChangeLinePosition(0, playedCard.transform.position);
            _ui.ChangeLinePosition(
                1,
                _camera.ScreenToWorldPoint(Input.mousePosition)
            );

            yield return null;
        }

        IsChoosing = false;
    }
}
