


using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour, IHand
{
    public List<ICard> cards;

    public int amount => throw new System.NotImplementedException();

    public ICard selected => throw new System.NotImplementedException();

    public void TakeCardsFromDeck(IDeck deck, int cardsAmount)
    {
        throw new System.NotImplementedException();
    }
}

