
using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;

[BurstCompile(DisableDirectCall = true)]
public struct HandStruct
{
    public int amount => cards.Length;
    public NativeList<CardStruct> cards;

    public HandStruct(List<CardStruct> cards)
    {
        this.cards = new(cards.Count, Allocator.TempJob);
        for (int i = 0; i < cards.Count; i++)
        {
            this.cards.Add(cards[i]);
        }
    }

    public void TakeCardsFromDeck(ref DeckStruct deck, int cardAmount)
    {
        for (int i = 0; i < cardAmount; i++) {
            if (deck.amount <= 0) return;
            cards.Add(deck.TakeLast());
        }
    }
}

