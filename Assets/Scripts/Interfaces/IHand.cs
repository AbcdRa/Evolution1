
public interface IHand
{
    public int amount { get; }
    public ICard selected {  get; }

    void TakeCardsFromDeck(IDeck deck, int cardsAmount);
}