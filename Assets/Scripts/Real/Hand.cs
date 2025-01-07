


using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Hand : MonoBehaviour, IHand
{
    public List<ICard> cards = new();
    public int amount => cards.Count;
    private float CARDS_WIDTH = 20f;

    public ICard selected { get; set; }

    public HandStruct GetStruct()
    {
        List<CardStruct> cards = new List<CardStruct>();
        foreach (var card in this.cards)
        {
            cards.Add(card.GetStruct());
        }
        return new HandStruct(cards);
    }

    public void InitReset()
    {
        cards = new(6);
    }

    public void TakeCardsFromDeck(IDeck deck, int cardsAmount)
    {
        for (int i = 0; i < cardsAmount; i++)
        {
            if (deck.amount <= 0) return;
            cards.Add(deck.TakeLast());
        }
        OrganizateCards();
    }

    public void OrganizateCards()
    {
        for (int i = 0; i < cards.Count; i++) {
            cards[i].transform.parent = transform;
            Vector3 pos = new(i * CARDS_WIDTH, 0, 0);
            cards[i].transform.localPosition = pos;
        }
    }
}

