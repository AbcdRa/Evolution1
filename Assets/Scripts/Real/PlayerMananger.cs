using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMananger : MonoBehaviour, IPlayerMananger
{
    [SerializeField] private Player[] _players;
    [SerializeField] private PlayerUI ui;

    public IPlayer[] players => _players;
    public int playerAmount => _players.Length;


    public bool AddPropToAnimal(int playerId, ICard card, AnimalId target, bool isRotated)
    {
        bool isAdded = players[target.ownerId].AddPropToAnimal(playerId, card, target.localId, isRotated);
        return isAdded;
    }

    public bool CreateAnimal(int playerId, ICard card)
    {
        return players[playerId].CreateAnimal(card);
    }

    public int Feed(int playerId, AnimalId target, IFoodMananger foodMananger)
    {
        if (playerId != target.ownerId) throw new Exception("RULEBREAKING trying to feed otherAnimal");
        return players[playerId].Feed(target.localId, foodMananger);
    }

    public void Pass(int playerId)
    {
        players[playerId].Pass();
    }

    public void ResetPass()
    {
        foreach (var player in players) {
            player.ResetPass();
        }
    }

    public void SetupGame(IDeck deck, int cardsAmount)
    {
        for (int i = 0; i < playerAmount; i++) {
            players[i].InitReset(i); 
        }
        foreach (var player in players) { 
            player.TakeCardsFromDeck(deck,cardsAmount);
        }
    }

    public void StartPreDevelopPhase(int currentPivot, IDeck deck)
    {
        for (int i = 0; i < players.Length; i++) { 
            int playerId = (currentPivot + i) % players.Length;
            int newCardAmount = players[playerId].animalArea.amount + 1;
            players[playerId].TakeCardsFromDeck(deck, newCardAmount);
        }
    }

    public void StartSurvivingPhase()
    {
        foreach (var player in players) {
            player.animalArea.StartSurvivingPhase();
        }
    }

    public void UpdatePhaseCooldown()
    {
        foreach (var player in players)
        {
            player.animalArea.UpdatePhaseCooldown();
        }
    }

    public void UpdateTurnCooldown()
    {
        foreach (var player in players)
        {
            player.animalArea.UpdateTurnCooldown();
        }
    }
}
