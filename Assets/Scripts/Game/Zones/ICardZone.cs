using System.Collections.Generic;

public interface ICardZone
{
    List<CardInfoScript> Cards { get; }

    bool CanAdd(CardInfoScript card);
    void Add(CardInfoScript card);
    void Remove(CardInfoScript card);
}
