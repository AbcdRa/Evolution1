
using System;
using Unity.Collections;

public struct HandStruct
{
    public int amount => cards.Length;
    public NativeList<CardStruct> cards;

    internal void GetCardsFromDeck(in DeckStruct deck, int cardAmount)
    {
        for (int i = 0; i < cardAmount; i++) {
            if (deck.amount <= 0) return;
            cards.Add(deck.TakeLast());
        }
    }
}

