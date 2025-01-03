
using System;
using Unity.Collections;

public struct PlayerManangerStruct
{
    public NativeArray<PlayerStruct> players;

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
        if (players[victimId.ownerId].animalArea.spots[victimId.localId].animal.propFlags.HasFlag(AnimalPropName.Poison))
            players[predatorId.ownerId].animalArea.spots[predatorId.localId].animal.AddFlag(AnimalPropName.RIsPoisoned);
        players[victimId.ownerId].animalArea.Kill(victimId.localId);
        players[victimId.ownerId].animalArea.OrganizateSpots();
        players[predatorId.ownerId].animalArea.spots[predatorId.localId].animal.Feed(2);
    }
}