
using System;
using Unity.Collections;

public struct PlayerManangerStruct
{
    public NativeArray<PlayerStruct> players;

    internal bool AddPropToAnimal(int playerId, in CardStruct card, in AnimalId target, bool isRotated)
    {
        if (target.ownerId != playerId) throw new Exception("RuleBreaker as you like");
        return players[playerId].AddPropToAnimal(card, target, isRotated);
    }

    internal bool CreateAnimal(int playerId, in CardStruct card)
    {
        return players[playerId].CreateAnimal(card);
    }

    public int Feed(int playerId, in AnimalId target, in FoodManangerStruct foodMananger)
    {
        if (target.ownerId != playerId) throw new Exception("RuleBreaker as you like");
        return players[playerId].Feed(target, foodMananger);
    }

    internal int GetWinner()
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

    internal void Pass(int playerId)
    {
        players[playerId].Pass();
    }

    internal void ResetPass()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].ResetPass();
        }
    }

    internal void UpdatePhaseCooldown()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].UpdatPhaseCooldown();
        }
    }

    internal void UpdateTurnCooldown()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].UpdatTurnCooldown();
        }
    }
}