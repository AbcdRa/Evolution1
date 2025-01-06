


using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Hand : MonoBehaviour, IHand
{
    public List<ICard> cards;
    public int amount => cards.Count;

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

    public void TakeCardsFromDeck(IDeck deck, int cardsAmount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (deck.amount <= 0) return;
            cards.Add(deck.TakeLast());
        }
    }

}

