using Unity.VisualScripting;

public interface IPlayer
{
    public int id { get; }
    public IAnimalArea animalArea { get; }
    public IHand hand { get; }
    bool isAbleToMove { get; }
    bool isBot { get; }

    bool AddPropToAnimal(int playerId, ICard card, int localId);
    bool CreateAnimal(ICard card);
    int Feed(int localId, IFoodMananger foodMananger);
    PlayerStruct GetStruct();
    void InitReset(int i);
    void MakeVirtualTurn();
    void Pass();
    void ResetPass();
    void TakeCardsFromDeck(IDeck deck, int cardsAmount);
}