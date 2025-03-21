﻿
using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.SocialPlatforms.Impl;

[BurstCompile(DisableDirectCall = true)]
public struct AnimalAreaStruct
{
    public int ownerId;
    public int amount => spots.Length;
    public NativeList<AnimalSpotStruct> spots;
    public AnimalSpotStruct freeSpot;

    public AnimalAreaStruct(int ownerId, List<AnimalSpotStruct> spots, AnimalSpotStruct freeSpot) { 
        this.ownerId = ownerId;
        this.spots = new(10, Allocator.Persistent);
        this.freeSpot = freeSpot;
        for (int i = 0; i < spots.Count; i++) { 
            this.spots.Add(spots[i]);
        }
    }


    public void StartSurvivingPhase()
    {
        for(int i = 0; i < spots.Length; i++)
        {
            if (spots[i].isFree) continue;
            if (!spots[i].animal.CanSurvive()) Kill(i);
        }
        OrganizateSpots();
    }

    public void Kill(int localId)
    {
        AnimalId myId = new(ownerId, localId);
        for (int i = 0; i < spots[localId].animal.props.pairsLength; i++)
        {
            AnimalId oth = spots[localId].animal.props.pairs[i].GetOtherAnimalId(myId);
            if (oth.ownerId != ownerId) throw new Exception("GAMEBREAKING RULE животное было связано парным свойством с другим !!!");
            spots[oth.localId].RemoveProp(spots[localId].animal.props.pairs[i]);
        }
        spots[localId].Kill();
    }
 


    public void OrganizateSpots()
    {
        //TODO Можно оптимизировать!! (Пришлось)
        for (int i = 0; i < this.spots.Length;)
        {
            if (this.spots[i].isFree)
            {
                this.spots.RemoveAt(i);

            }
            else
            {
                i++;
            }
        }
        //for (int i = 0; i < amount; i++) {
        //    spots[i].SetLocalId(i);
        //}
    }

    internal bool CreateAnimal(in CardStruct card)
    {
        bool isCreated = freeSpot.CreateAnimal(card, spots.Length);
        if(!isCreated) return false;
        //freeSpot.SetLocalId(spots.Length);
        spots.Add(freeSpot);
        freeSpot = new();
        return isCreated; 
    }

    public int Feed(int localId, in FoodManangerStruct foodMananger, int foodIncrease=1)
    {
        if(foodMananger.isNull)
        {
            spots[localId].Feed(foodIncrease);
            PairFeed(localId, localId, foodMananger.food, true, false);
            return 0;
        }

        if (spots[localId].animal.propFlags.HasFlagFast(AnimalPropName.Interaction))
        {
            int consumedAmount = foodMananger.food - PairFeed(localId, localId, foodMananger.food, true);
            return consumedAmount;
        }

        else if (spots[localId].animal.propFlags.HasFlagFast(AnimalPropName.Cooperation))
        {
            int consumedAmount = foodMananger.food - PairFeed(localId, localId, foodMananger.food, true);
            return consumedAmount;
        }
        return spots[localId].Feed(foodMananger.food, foodIncrease);
    }

    private int PairFeed(int localId, int breakingId, int food, bool isFirstInit=false, bool isConsumedFood=true)
    {
        if ((!isFirstInit) && localId == breakingId) return 0;
        if(isConsumedFood && food == 0) return 0;
        int foodConsumed = 0;
        foodConsumed = spots[localId].Feed(food);
        if (foodConsumed == 0) return 0;
        if(isConsumedFood) food -= foodConsumed;

        for (int i = 0; i < spots[localId].animal.props.pairsLength; i++) {
            if (isConsumedFood && spots[localId].animal.props.pairs[i].name == AnimalPropName.Interaction && food > 0)
            { 
                AnimalId oth = spots[localId].animal.props.pairs[i].GetOtherAnimalId(new(ownerId, localId));
                if (oth.ownerId != ownerId) throw new Exception("GAMEBREAKING RULE Trying to feed another animal");
                food -= PairFeed(oth.localId, breakingId, food);
                
            } else if(spots[localId].animal.props.pairs[i].name == AnimalPropName.Cooperation) {
                AnimalId oth = spots[localId].animal.props.pairs[i].GetOtherAnimalId(new(ownerId, localId));
                if (oth.ownerId != ownerId) throw new Exception("GAMEBREAKING RULE Trying to feed another animal");
                food -= PairFeed(oth.localId, breakingId, food, false, true);
            }
        }
        return food;
    }


    internal int GetScore()
    {
        int score = 0;
        for (int i = 0; i < spots.Length; i++) {
            score += spots[i].GetScore();
        }
        return score;
    }

    internal void UpdatePhaseCooldown()
    {
        for (int i = 0; i < spots.Length; i++)
        {
            spots[i].UpdatePhaseCooldown();
        }
    }

    internal void UpdateTurnCooldown()
    {
        for (int i = 0; i < spots.Length; i++)
        {
            spots[i].UpdateTurnCooldown();
        }
    }
}

