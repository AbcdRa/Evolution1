using Unity.VisualScripting;

public interface IPlayer
{
    bool isAbleToMove { get; }
    IAnimalArea animalArea { get; }
    public int id { get; }

    bool AddPropToAnimal(int playerId, ICard card, int localId, bool isRotated);
    bool CreateAnimal(ICard card);
    int Feed(int localId, IFoodMananger foodMananger);
    void InitReset(int i);
    void Pass();
    void ResetPass();
    void TakeCardsFromDeck(IDeck deck, int cardsAmount);
}