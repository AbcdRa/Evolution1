
public interface IHand
{
    public int amount { get; }
    public ICard selected {  get; }

    HandStruct GetStruct();
    void InitReset();
    void TakeCardsFromDeck(IDeck deck, int cardsAmount);
}