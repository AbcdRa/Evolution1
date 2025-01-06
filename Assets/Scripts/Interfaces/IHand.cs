
public interface IHand
{
    public int amount { get; }
    public ICard selected {  get; }

    HandStruct GetStruct();
    void TakeCardsFromDeck(IDeck deck, int cardsAmount);
}