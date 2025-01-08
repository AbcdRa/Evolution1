
public interface IHand
{
    public int amount { get; }
    public ICard selected {  get; }

    HandStruct GetStruct();
    void InitReset(int ownerId);
    void Select(Card card);
    void TakeCardsFromDeck(IDeck deck, int cardsAmount);
    void Unselect();
}