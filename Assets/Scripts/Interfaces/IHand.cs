
using NUnit.Framework;
using System.Collections.Generic;

public interface IHand
{
    public int amount { get; }
    public ICard selected {  get; }
    public List<ICard> cards { get; }

    void InitReset(int ownerId);
    void RemoveCard(ICard card);
    void Select(Card card);
    void TakeCardsFromDeck(IDeck deck, int cardsAmount);
    void Unselect();
}