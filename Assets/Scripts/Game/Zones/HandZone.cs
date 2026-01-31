using System;
using System.Collections.Generic;
using UnityEngine;

public class HandZone : ICardZone
{
    private readonly List<CardInfoScript> _cards = new();
    private readonly int _maxCards;

    public List<CardInfoScript> Cards => _cards;

    public HandZone(int maxCards)
    {
        _maxCards = maxCards;
    }

    public bool CanAdd(CardInfoScript card)
        => _cards.Count < _maxCards;

    public void Add(CardInfoScript card)
    {
        if (!CanAdd(card))
            Debug.Log("Hand is full");

        _cards.Add(card);
    }

    public void Remove(CardInfoScript card)
    {
        _cards.Remove(card);
    }
}
