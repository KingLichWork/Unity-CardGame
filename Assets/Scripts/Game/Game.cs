using System.Collections.Generic;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class Game
{
    public List<Card> EnemyDeck;
    public List<Card> PlayerDeck;

    public Game()
    {
        EnemyDeck = GiveDeckCard();

        if (Object.FindObjectOfType<DeckManager>() != null && DeckManager.Instance.Deck != null)
        {
            DeckManager.Instance.Deck = ShuffleDeck();
            PlayerDeck = new List<Card>(DeckManager.Instance.Deck);
        }
        else
            PlayerDeck = GiveDeckCard();
    }

    private List<Card> ShuffleDeck()
    {
        List<Card> shuffleDeck = new List<Card>(DeckManager.Instance.Deck);

        for (int i = shuffleDeck.Count - 1; i > 0; i--)
        {
            int random = Random.Range(0, i + 1);

            Card temp = shuffleDeck[i];
            shuffleDeck[i] = shuffleDeck[random];
            shuffleDeck[random] = temp;
        }

        return shuffleDeck;
    }

    private List<Card> GiveDeckCard()
    {
        List<Card> DeckList = new List<Card>();
        for (int i = 0; i < GameManager.Instance.ValueDeckCards; i++)
        {
            DeckList.Add(CardManagerList.AllCards[Random.Range(0, CardManagerList.AllCards.Count)]);
            //DeckList.Add(CardManagerList.AllCards[45]);
        }
        return DeckList;
    }
}