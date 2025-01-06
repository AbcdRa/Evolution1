using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMananger : MonoBehaviour, IPlayerMananger
{
    [SerializeField] private Player[] _players;


    public IPlayer[] players => _players;
    public int playerAmount => _players.Length;
    


    public bool AddPropToAnimal(int playerId, ICard card, AnimalId target)
    {
        bool isAdded = players[target.ownerId].AddPropToAnimal(playerId, card, target.localId);
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



    public void KillById(AnimalId predatorId, AnimalId victimId)
    {
        if (players[victimId.ownerId].animalArea.spots[victimId.localId].animal.propFlags.HasFlagFast(AnimalPropName.Poison))
            players[predatorId.ownerId].animalArea.spots[predatorId.localId].animal.AddFlag(AnimalPropName.RIsPoisoned);
        players[victimId.ownerId].animalArea.Kill(victimId.localId);
        players[victimId.ownerId].animalArea.OrganizateSpots();
        AnimalId nearScavenger = FindNearScavenger(predatorId.ownerId);
        if (!nearScavenger.isNull)
        {
            players[nearScavenger.ownerId].animalArea.spots[nearScavenger.localId].animal.PlayScavenger();
            players[nearScavenger.ownerId].animalArea.Feed(nearScavenger.localId, null);
        }
        players[predatorId.ownerId].animalArea.spots[predatorId.localId].animal.Feed(2);
    }

    private AnimalId FindNearScavenger(int ownerId)
    {
        for (int i = 0; i < players.Length; i++)
        {
            int playerId = (ownerId + i) % players.Length;
            for (int j = 0; j < players[playerId].animalArea.amount; j++)
            {
                if (players[playerId].animalArea.spots[j].animal.propFlags.HasFlagFast(AnimalPropName.Scavenger)
                    && !players[playerId].animalArea.spots[j].animal.isFull()) return new(playerId, j);
            }
        }
        return AnimalId.NULL;
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

    public PlayerManangerStruct GetStruct()
    {
        List<PlayerStruct> list = new List<PlayerStruct>();
        for (int i = 0; i < players.Length; i++)
        {
            list.Add(players[i].GetStruct());
        }
        return new PlayerManangerStruct(list);
    }
}
