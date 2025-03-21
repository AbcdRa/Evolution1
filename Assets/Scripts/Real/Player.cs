﻿
using System.Threading.Tasks;
using UnityEditor.Media;
using UnityEngine;

public class Player : MonoBehaviour, IPlayer
{
    [SerializeField] private AnimalArea _animalArea;
    [SerializeField] private Hand _hand;
    [SerializeField] private bool _isBot;
    [SerializeField] private int _id;

    private bool _isAbleToMove = false;

    public bool isAbleToMove => _isAbleToMove;
    public IAnimalArea animalArea => _animalArea;
    public int id => _id;
    public IHand hand => _hand;

    public bool isBot => _isBot;

    public bool AddPropToAnimal(int playerId, ICard card, int localId)
    {
        bool isHostileProp = card.current.isHostile();
        //Круто вычислил да ?
        bool isPossibleToAdd = (isHostileProp ^ playerId == _id); 
        if(!isPossibleToAdd) return false;
        return animalArea.AddPropToAnimal(card, localId);
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
        _animalArea.InitReset(id);
        hand.InitReset(id);
    }

    public void MakeVirtualTurn()
    {
        VirtualSimulation vr = new VirtualSimulation(this);
        vr.onMoveReady.AddListener(onMoveReady);
        //Task.Factory.StartNew(() => vr.WaitingMove());
        vr.WaitingMove();
        //VMove bestMove = 
        //vr.GetBestMove(this);
        //VMove.ExecuteMove(GameMananger.instance, bestMove);
    }

    private void onMoveReady()
    {
        Debug.Log("Ураа!");
    }

    public void Pass()
    {
        _isAbleToMove = false;
    }

    public void ResetPass()
    {
        _isAbleToMove = true;
    }

    public void TakeCardsFromDeck(IDeck deck, int cardsAmount)
    {
        hand.TakeCardsFromDeck(deck, cardsAmount);
    }
}
