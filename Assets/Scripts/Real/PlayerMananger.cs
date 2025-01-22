using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
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
            players[nearScavenger.ownerId].animalArea.spots[nearScavenger.localId].animal.ActivateScavenger();
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

    public Hands GetHandsStruct(int targetId, List<CardStruct> deck)
    {
        List<CardStruct>[] hands = new List<CardStruct>[4] { new(), new(), new(), new() };
        int[] handLs = new int[4] { players[0].hand.amount, players[1].hand.amount, players[2].hand.amount, players[3].hand.amount };
        for (int i = 0; i < handLs[0]; i++) { 
            if(targetId == 0) hands[0].Add(players[0].hand.cards[i].GetStruct());
            else deck.Add(players[0].hand.cards[i].GetStruct());
        }
        for (int i = 0; i < handLs[1]; i++)
        {
            if (targetId == 1) hands[1].Add(players[1].hand.cards[i].GetStruct());
            else deck.Add(players[1].hand.cards[i].GetStruct());

        }
        for (int i = 0; i < handLs[2]; i++)
        {
            if (targetId == 2) hands[2].Add(players[2].hand.cards[i].GetStruct());
            else deck.Add(players[2].hand.cards[i].GetStruct());

        }
        for (int i = 0; i < handLs[3]; i++)
        {
            if (targetId == 3) hands[3].Add(players[3].hand.cards[i].GetStruct());
            else deck.Add(players[3].hand.cards[i].GetStruct());
        }

        deck = DevExtension.Shuffle(deck, GameMananger.rng);
        for(int i = 0; i < 4; i++)
        {
            if (i == targetId) continue;
            for(int j = 0; j < handLs[i]; j++)
            {
                CardStruct card = deck.Last();
                deck.Remove(card);
                hands[i].Add(card);
            }
        }
        return new Hands(hands[0], hands[1], hands[2], hands[3]);
    }

    public FixedArr4<bool> GetPlayerInfoStruct()
    {
        FixedArr4<bool> playerInfos = new();

        for(int i = 0; i < players.Length; i++) {
            playerInfos[i] = (players[i].isAbleToMove);
        }
        return playerInfos;
    }

    public PlayerSpots GetPlayerSpotStruct()
    {
        List<AnimalSpotStruct>[] spots = new List<AnimalSpotStruct>[4] { new(), new(), new(), new() };
        FixedArr4<bool> isAbleToMove = new();
        for(int i = 0; i < playerAmount; i++)
        {
            isAbleToMove[i] = players[i].isAbleToMove;
            for(int j = 0; j < players[i].animalArea.amount; j++)
            {
                spots[i].Add(players[i].animalArea.spots[j].GetStruct(i));
            }
        }
        return new PlayerSpots(spots[0], spots[1], spots[2], spots[3], isAbleToMove);
    }

    public IAnimalSpot GetSpot(AnimalId target)
    {
        return players[target.ownerId].animalArea.spots[target.localId];
    }

    public VPlayerMananger GetVirtual(Player player, List<CardStruct> deck)
    {
        List<List<AnimalSpotStruct>> spots = new(players.Length);
        List<List<CardStruct>> hands = new();
        List<VPlayer> vPlayers = new();
        for (int i = 0; i < players.Length; i++)
        {
            if (i == player.id) continue;
            for(int j = 0; j < players[i].hand.amount; j++)
            {
                deck.Add(players[i].hand.cards[j].GetStruct());
            }
        }

        deck = DevExtension.Shuffle(deck, GameMananger.rng);

        for(int i = 0; i < players.Length; i++)
        {
            List<AnimalSpotStruct> spotsIn = new();
            List<CardStruct> handsIn = new();
            for(int j = 0; j < players[i].animalArea.amount; j++)
            {
                spotsIn.Add(players[i].animalArea.spots[j].GetStruct(i));
            }
            for(int j = 0; j < players[i].hand.cards.Count; j++)
            {
                if(j == player.id) handsIn.Add(players[i].hand.cards[j].GetStruct());
                else
                {
                    handsIn.Add(deck[deck.Count - 1]);
                    deck.RemoveAt(deck.Count - 1);
                }

            }
            spots.Add(spotsIn);
            hands.Add(handsIn);
            vPlayers.Add(new VPlayer(players[i].isAbleToMove, players[i].id));
        }
        return new VPlayerMananger(spots, hands, vPlayers);
    }

}
