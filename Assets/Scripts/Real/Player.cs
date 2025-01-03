
using UnityEngine;

public class Player : MonoBehaviour, IPlayer
{
    [SerializeField] private IAnimalArea _animalArea;
    [SerializeField] private bool _isBot;
    [SerializeField] private int _id;
    private bool _isAbleToMove = false;
    public bool isAbleToMove => _isAbleToMove;
    public IAnimalArea animalArea => _animalArea;
    public int id => _id;

    public bool AddPropToAnimal(int playerId, ICard card, int localId, bool isRotated)
    {
        bool isHostileProp = isRotated ? card.second.isHostile() : card.main.isHostile();
        //Круто вычислил да ?
        bool isPossibleToAdd = (isHostileProp ^ playerId == _id); 
        if(!isPossibleToAdd) return false;
        return animalArea.AddPropToAnimal(card, localId, isRotated);
    }

    public bool CreateAnimal(ICard card)
    {
        return animalArea.CreateAnimal(card);
    }

    public int Feed(int localId, IFoodMananger foodMananger)
    {
        return animalArea.Feed(localId, foodMananger);
    }

    public void InitReset(int id)
    {
        this._id = id;
        _isAbleToMove = true;
        _animalArea.InitReset();
    }

    public void Pass()
    {
        throw new System.NotImplementedException();
    }

    public void ResetPass()
    {
        throw new System.NotImplementedException();
    }

    public void TakeCardsFromDeck(IDeck deck, int cardsAmount)
    {
        throw new System.NotImplementedException();
    }
}
