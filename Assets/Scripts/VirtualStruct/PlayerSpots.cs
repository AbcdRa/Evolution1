﻿
using Mono.Cecil;
using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

[BurstCompile]
public struct PlayerSpots : IDisposable
{
    public NativeList<AnimalSpotStruct> spots1;
    public NativeList<AnimalSpotStruct> spots2;
    public NativeList<AnimalSpotStruct> spots3;
    public NativeList<AnimalSpotStruct> spots4;
    public FixedArr4<bool> isAbleToMove;

    public PlayerSpots(List<AnimalSpotStruct> spots1, List<AnimalSpotStruct> spots2, List<AnimalSpotStruct> spots3, List<AnimalSpotStruct> spots4, FixedArr4<bool> isAbleToMove)
    {
        //Todo можно заранее 8 capacity
        this.spots1 = spots1.ToNativeList(Allocator.Persistent);
        this.spots2 = spots2.ToNativeList(Allocator.Persistent);
        this.spots3 = spots3.ToNativeList(Allocator.Persistent);
        this.spots4 = spots4.ToNativeList(Allocator.Persistent);
        this.isAbleToMove = isAbleToMove;
    }

    public AnimalSpotStruct GetSpot(AnimalId id)
    {
        switch (id.ownerId)
        {
            case 0: return spots1[id.localId];
            case 1: return spots2[id.localId];
            case 2: return spots3[id.localId];
            case 3: return spots4[id.localId];
        }
        throw new Exception("WTF");
    }


    public AnimalSpotStruct GetSpot(int ownerId, int localId)
    {
        switch (ownerId)
        {
            case 0: return spots1[localId];
            case 1: return spots2[localId];
            case 2: return spots3[localId];
            case 3: return spots4[localId];
        }
        throw new Exception("WTF");
    }


    public int GetSpotsLength(int playerId)
    {
        switch (playerId)
        {
            case 0: return spots1.Length;
            case 1: return spots2.Length;
            case 2: return spots3.Length;
            case 3: return spots4.Length;
        }
        return 0;
    }


    public void SetSpot(AnimalId id, in AnimalSpotStruct spot)
    {
        switch (id.ownerId)
        {
            case 0: spots1[id.localId] = spot; break;
            case 1: spots2[id.localId] = spot; break;
            case 2: spots3[id.localId] = spot; break;
            case 3: spots4[id.localId] = spot; break;
        }
    }

    internal void SetSpot(in AnimalSpotStruct spot)
    {
        if (spot.id.localId == GetSpotsLength(spot.id.ownerId)) {
            switch (spot.id.ownerId) {
                case 0: spots1.Add(spot); break;
                case 1: spots2.Add(spot); break;
                case 2: spots3.Add(spot); break;
                case 3: spots4.Add(spot); break;
            }
        } else 
        SetSpot(spot.id, spot);
    }

    internal int GetWinner()
    {
        FixedArr4<int> scores = new();
        for (int i = 0; i < spots1.Length; i++)
        {
            scores[0] += spots1[i].GetScore();
        }
        for (int i = 0; i < spots2.Length; i++)
        {
            scores[1] += spots2[i].GetScore();
        }
        for (int i = 0; i < spots3.Length; i++)
        {
            scores[2] += spots3[i].GetScore();
        }
        for (int i = 0; i < spots4.Length; i++)
        {
            scores[3] += spots4[i].GetScore();
        }
        //TODO Можно сразу макс искать, но для дебага полезно посмотреть у остальных счет
        int wiinerId = 0;
        int maxScore = 0;
        for (int i = 0; i < scores.Length; i++)
        {
            if (maxScore < scores[i])
            {
                wiinerId = i;
                maxScore = scores[i];
            }
        }
        return wiinerId;
    }

    internal void UpdateTurnCooldown()
    {
        for (int i = 0; i < spots1.Length; i++)
        {
            AnimalSpotStruct spot = spots1[i];
            spot.UpdateTurnCooldown();
            spots1[i] = spot;
        }
        for (int i = 0; i < spots2.Length; i++)
        {
            AnimalSpotStruct spot = spots2[i];
            spot.UpdateTurnCooldown();
            spots2[i] = spot;
        }
        for (int i = 0; i < spots3.Length; i++)
        {
            AnimalSpotStruct spot = spots3[i];
            spot.UpdateTurnCooldown();
            spots3[i] = spot;
        }
        for (int i = 0; i < spots4.Length; i++)
        {
            AnimalSpotStruct spot = spots4[i];
            spot.UpdateTurnCooldown();
            spots4[i] = spot;
        }
    }

    internal void UpdatePhaseCooldown()
    {
        for (int i = 0; i < spots1.Length; i++)
        {
            AnimalSpotStruct spot = spots1[i];
            spot.UpdatePhaseCooldown();
            spots1[i] = spot;
        }
        for (int i = 0; i < spots2.Length; i++)
        {
            AnimalSpotStruct spot = spots2[i];
            spot.UpdatePhaseCooldown();
            spots2[i] = spot;
        }
        for (int i = 0; i < spots3.Length; i++)
        {
            AnimalSpotStruct spot = spots3[i];
            spot.UpdatePhaseCooldown();
            spots3[i] = spot;
        }
        for (int i = 0; i < spots4.Length; i++)
        {
            AnimalSpotStruct spot = spots4[i];
            spot.UpdatePhaseCooldown();
            spots4[i] = spot;
        }
    }


    public int Feed(in AnimalId id, int foodAmount, bool isBlueFood = true, int foodConsume = 1)
    {

        AnimalSpotStruct targetSpot = GetSpot(id);
        if (isBlueFood)
        {
            if (foodConsume > 1) targetSpot.Feed(foodConsume - 1, foodConsume - 1);
            PairFeed(id, id, foodAmount, true, false);
            SetSpot(id, targetSpot);
            return 0;
        }
        else
        {
            if (foodConsume > foodAmount) return 0;
            if (targetSpot.animal.propFlags.HasFlagFast(AnimalPropName.Interaction | AnimalPropName.Cooperation))
            {
                int consumedAmount = foodConsume + PairFeed(id, id, foodAmount, true, true);
                return consumedAmount;
            }
            int resultConsume = targetSpot.Feed(foodConsume, foodConsume);
            SetSpot(id, targetSpot);
            return resultConsume;
        }
    }

    private int PairFeed(AnimalId id, AnimalId breaking, int food, bool isFirstInit = false, bool isConsumedFood = true)
    {
        if ((!isFirstInit) && id.Equals(breaking)) return 0;
        if (isConsumedFood && food == 0) return 0;
        int foodConsumed = 0;
        AnimalSpotStruct spot = GetSpot(id);
        if (isConsumedFood)
        {
            foodConsumed = spot.Feed(food);
        }
        else
        {
            foodConsumed = spot.Feed(1, 1);
        }
        if (foodConsumed == 0) return 0;
        SetSpot(id, spot);
        if (isConsumedFood) food -= foodConsumed;

        for (int i = 0; i < spot.animal.props.pairsLength; i++)
        {
            if (isConsumedFood && spot.animal.props.pairs[i].name == AnimalPropName.Interaction && food > 0)
            {
                AnimalId oth = spot.animal.props.pairs[i].GetOtherAnimalId(id);
                if (oth.ownerId != id.ownerId) throw new Exception("GAMEBREAKING RULE Trying to feed another animal");
                food -= PairFeed(oth, breaking, food);

            }
            else if (spot.animal.props.pairs[i].name == AnimalPropName.Cooperation)
            {
                AnimalId oth = spot.animal.props.pairs[i].GetOtherAnimalId(id);
                if (oth.ownerId != id.ownerId) throw new Exception("GAMEBREAKING RULE Trying to feed another animal");
                food -= PairFeed(oth, breaking, food, false, true);
            }
        }
        return food;
    }

    internal void KillById(AnimalId predatorId, AnimalId victimId)
    {
        AnimalSpotStruct predator = GetSpot(predatorId);
        AnimalSpotStruct victim = GetSpot(victimId);

        if (victim.animal.propFlags.HasFlagFast(AnimalPropName.Poison)) 
            predator.animal.AddFlag(AnimalPropName.RIsPoisoned);

        Kill(victimId);
        OrganizateSpots();
        AnimalId nearScavenger = FindNearScavenger(predatorId.ownerId);
        if (!nearScavenger.isNull)
        {
            AnimalSpotStruct scavenger = GetSpot(nearScavenger);
            scavenger.animal.ActivateScavenger();
            Feed(nearScavenger, 1);
            SetSpot(scavenger);
        }
        predator.animal.Feed(2);
        SetSpot(predator);
    }

    private AnimalId FindNearScavenger(int ownerId)
    {
        throw new NotImplementedException();
    }

    private void OrganizateSpots()
    {
        throw new NotImplementedException();
    }

    private void Kill(AnimalId victimId)
    {
        throw new NotImplementedException();
    }

    internal void StartSurvivingPhase()
    {
        for (int i = 0; i < spots1.Length;)
        {
            if (!spots1[i].animal.CanSurvive()) spots1.RemoveAt(i);
            else
            {
                AnimalSpotStruct spot = spots1[i];
                spot.SetLocalAndOwnerId(new(0, i));
                spots1[i] = spot;
                i++;

            }
        }
        for (int i = 0; i < spots2.Length;)
        {
            if (!spots2[i].animal.CanSurvive()) spots2.RemoveAt(i);
            else
            {
                AnimalSpotStruct spot = spots2[i];
                spot.SetLocalAndOwnerId(new(1, i));
                spots2[i] = spot;
                i++;

            }
        }
        for (int i = 0; i < spots3.Length;)
        {
            if (!spots3[i].animal.CanSurvive()) spots3.RemoveAt(i);
            else
            {
                AnimalSpotStruct spot = spots3[i];
                spot.SetLocalAndOwnerId(new(2, i));
                spots3[i] = spot;
                i++;

            }
        }
        for (int i = 0; i < spots4.Length;)
        {
            if (!spots4[i].animal.CanSurvive()) spots4.RemoveAt(i);
            else {
                AnimalSpotStruct spot = spots4[i];
                spot.SetLocalAndOwnerId(new(3, i));
                spots4[i] = spot;
                i++;
                
            }
        }
    }

    internal void ResetPass()
    {
        for (int i = 0; i < isAbleToMove.Length; i++) {
            isAbleToMove[i] = true;    
        }
    }

    internal void Pass(int playerId)
    {
        isAbleToMove[playerId] = false;
    }

    public void Dispose()
    {
        spots1.Dispose();
        spots2.Dispose();
        spots3.Dispose();
        spots4.Dispose();
    }
}   