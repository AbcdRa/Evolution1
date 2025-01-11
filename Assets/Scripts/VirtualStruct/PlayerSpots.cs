
using System;
using Unity.Collections;

public struct PlayerSpots
{
    public NativeList<AnimalSpotStruct> spots1;
    public NativeList<AnimalSpotStruct> spots2;
    public NativeList<AnimalSpotStruct> spots3;
    public NativeList<AnimalSpotStruct> spots4;

    public AnimalSpotStruct GetSpot(AnimalId id)
    {
        switch (id.ownerId)
        {
            case 0: return spots1[id.localId];
            case 1: return spots1[id.localId];
            case 2: return spots1[id.localId];
            case 3: return spots1[id.localId];
        }
        throw new Exception("WTF");
    }

    public void SetSpot(AnimalId id, in AnimalSpotStruct spot)
    {
        switch (id.ownerId)
        {
            case 0: spots1[id.localId] = spot; break;
            case 1: spots1[id.localId] = spot; break;
            case 2: spots1[id.localId] = spot; break;
            case 3: spots1[id.localId] = spot; break;
        }
    }

    internal void SetSpot(in AnimalSpotStruct spot)
    {
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



}