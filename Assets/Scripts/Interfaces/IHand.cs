
public interface IHand
{
    public int amount { get; }

    void TakeCardsFromDeck(IDeck deck, int cardsAmount);
}