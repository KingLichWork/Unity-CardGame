using System;
using System.Collections.Generic;

public class FieldZone : ICardZone
{
    private readonly int _maxCards;
    private List<CardInfoScript> _cards = new();

    public List<CardInfoScript> DestroyedInEndTurnCards { get; } = new();
    public List<CardInfoScript> InvulnerabilityCards { get; } = new();

    public List<CardInfoScript> Cards => _cards;

    public FieldZone(int maxCards)
    {
        _maxCards = maxCards;
    }

    public bool CanAdd(CardInfoScript card)
        => _cards.Count < _maxCards;

    public void SetCards(List<CardInfoScript> cards)
    {
        _cards = cards;
    }

    public void Add(CardInfoScript card)
    {
        if (!CanAdd(card))
            throw new InvalidOperationException("Field is full");

        _cards.Add(card);
    }

    public void Remove(CardInfoScript card)
    {
        _cards.Remove(card);
    }
}
