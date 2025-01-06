
using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public struct PlayerManangerStruct
{
    public NativeArray<PlayerStruct> players;

    public PlayerManangerStruct(List<PlayerStruct> players)
    {
        this.players = new(players.Count, Allocator.Persistent);
        for (int i = 0; i < players.Count; i++) { 
            this.players[i] = (players[i]);
        }
    }

    public bool AddPropToAnimal(int playerId, in CardStruct card, in AnimalId target, bool isRotated)
    {
        if (target.ownerId != playerId) throw new Exception("RuleBreaker as you like");
        return players[playerId].AddPropToAnimal(card, target, isRotated);
    }

    public bool CreateAnimal(int playerId, in CardStruct card)
    {
        return players[playerId].CreateAnimal(card);
    }

    public int Feed(int playerId, in AnimalId target, in FoodManangerStruct foodMananger)
    {
        if (target.ownerId != playerId) throw new Exception("RuleBreaker as you like");
        return players[playerId].Feed(target, foodMananger);
    }

    public int GetWinner()
    {
        int bestScore = 0;
        int winnerId = 0;
        for (int i = 0; i < players.Length; i++) {
            int score = players[i].GetScore();
            if (score > bestScore) {
                bestScore = score;
                winnerId = i;
            }
        }
        return winnerId;
    }

    public void Pass(int playerId)
    {
        players[playerId].Pass();
    }

    public void ResetPass()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].ResetPass();
        }
    }

    public void UpdatePhaseCooldown()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].UpdatPhaseCooldown();
        }
    }

    public void UpdateTurnCooldown()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].UpdatTurnCooldown();
        }
    }

    public void KillById(in AnimalId predatorId, in AnimalId victimId)
    {
        if (players[victimId.ownerId].animalArea.spots[victimId.localId].animal.propFlags.HasFlagFast(AnimalPropName.Poison))
            players[predatorId.ownerId].animalArea.spots[predatorId.localId].animal.AddFlag(AnimalPropName.RIsPoisoned);
        players[victimId.ownerId].animalArea.Kill(victimId.localId);
        players[victimId.ownerId].animalArea.OrganizateSpots();
        AnimalId nearScavenger = FindNearScavenger(predatorId.ownerId);
        if (!nearScavenger.isNull)
        {
            players[nearScavenger.ownerId].animalArea.spots[nearScavenger.localId].animal.PlayScavenger();
            players[nearScavenger.ownerId].animalArea.Feed(nearScavenger.localId, FoodManangerStruct.NULL);
        }
        players[predatorId.ownerId].animalArea.spots[predatorId.localId].animal.Feed(2);
    }

    private AnimalId FindNearScavenger(int ownerId)
    {
        for (int i = 0; i < players.Length; i++)
        {
            int playerId = (ownerId + i) % players.Length;
            for (int j = 0; j < players[playerId].animalArea.spots.Length; j++)
            {
                if (players[playerId].animalArea.spots[j].animal.propFlags.HasFlagFast(AnimalPropName.Scavenger)
                    && !players[playerId].animalArea.spots[j].animal.isFull()) return new(playerId, j);
            }
        }
        return AnimalId.NULL;
    }
}