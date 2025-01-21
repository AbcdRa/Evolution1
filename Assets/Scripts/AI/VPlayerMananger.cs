using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime.Misc;


public class VPlayerMananger
{
    public int playersAmount => players.Count;
    List<List<AnimalSpotStruct>> spots;
    List<List<CardStruct>> hands;
    List<VPlayer> players;

    public VPlayerMananger(List<List<AnimalSpotStruct>> spots, List<List<CardStruct>> hands, List<VPlayer> players)
    {
        this.spots = spots;
        this.hands = hands;
        this.players = players;
    }
    //public FixedArr4<bool> isAbleToMove { get; internal set; }

    internal void AddCard(int playerId, CardStruct card)
    {
        throw new NotImplementedException();
    }

    internal bool AddPropToAnimal(CardStruct card, AnimalId target, bool isRotated)
    {
        throw new NotImplementedException();
    }

    internal bool CreateAnimal(int playerId, CardStruct card)
    {
        AnimalSpotStruct freeSpot = CreateFreeSpot(playerId);
        bool isAddedSuccesful = freeSpot.CreateAnimal(card, GetSpotsLength(playerId));
        if (!isAddedSuccesful) return false;
        freeSpot.SetLocalAndOwnerId(new(playerId, GetSpotsLength(playerId)));
        SetSpot(freeSpot);
        return isAddedSuccesful;
    }

    internal void DecreaseFood(object piracyTarget, int v)
    {
        throw new NotImplementedException();
    }

    internal VPlayerMananger DeepCopy()
    {
        List<List<AnimalSpotStruct>> spotsCopy = new(spots.Count);
        List<List<CardStruct>> handsCopy = new(hands.Count);
        for(int i = 0; i < spots.Count; i++)
        {
            List<AnimalSpotStruct> spotsCopyIn = new(spots[i].Count);
            for(int j = 0; j < spots[i].Count; j++)
            {
                spotsCopyIn.Add(spots[i][j]);
            }
            spotsCopy.Add(spotsCopyIn);
        }

        for(int i = 0; i < hands.Count; i++)
        {
            List<CardStruct> cardsCopyIn = new(hands[i].Count);
            for(int j = 0; j < hands[i].Count; j++)
            {
                cardsCopyIn.Add(hands[i][j]);
            }
            hands.Add(cardsCopyIn);
        }
        List<VPlayer> playersCopy = new(players.Count);
        for(int i =0; i < players.Count; i++)
        {
            playersCopy.Add(players[i]);
        }
        return new VPlayerMananger(spotsCopy, handsCopy, playersCopy);
    }

    internal int Feed(AnimalId target, int food, bool v)
    {
        throw new NotImplementedException();
    }

    internal VPlayer GetCurrentPlayer(int currentTurn)
    {
        return players[currentTurn];
    }

    internal int GetHandAmount(int playerId)
    {
        return hands[playerId].Count;
    }

    internal CardStruct GetHandCard(AnimalId id)
    {
        return hands[id.ownerId][id.localId];
    }

    public AnimalSpotStruct GetSpot(AnimalId target)
    {
        return spots[target.ownerId][target.localId];
    }

    public AnimalSpotStruct GetSpot(int ownerId, int localId)
    {
        return spots[ownerId][localId];
    }


    public int GetSpotsLength(int playerId)
    {
        return spots[playerId].Count;
    }

    internal bool IsWinner(VPlayer player)
    {
        throw new NotImplementedException();
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

    private int Feed(in AnimalId id, int foodAmount, bool isBlueFood = true, int foodConsume = 1)
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

    internal void Pass(int playerId)
    {
        players[playerId].Pass();
    }

    internal PairAnimalId Play(AnimalId target, AnimalPropName propName)
    {
        switch(propName)
        {
            case AnimalPropName.Sleep:
                AnimalSpotStruct spot = GetSpot(target);
                spot.animal.ActivateSleepProp();
                SetSpot(spot);
                return PairAnimalId.NOT_DOING_NEXT_TURN;
            case AnimalPropName.Fasciest:
                spot = GetSpot(target);
                spot.animal.ActivateFasciestProp();
                SetSpot(spot);
                return PairAnimalId.DESTROY_FOOD;
            case AnimalPropName.Piracy:
                //TODO Ну ты хоть бы проверил возможно ли пиратство
                //Я знаю что двойные проверки не супер круто, но все таки
                AnimalSpotStruct pirateSpot = GetSpot(target);

                pirateSpot.animal.ActivatePiraceProp();
                SetSpot(pirateSpot);
                PairAnimalId sideTurnsInfo = PairAnimalId.PIRACY_FOOD;
                return sideTurnsInfo;
        }
        throw new NotImplementedException();
    }

    internal void ResetPass()
    {
        for (int i = 0; i < players.Count; i++) {
            players[i].ResetPass();
        }
    }

    internal void SetSpot(AnimalSpotStruct friendSpot)
    {
        spots[friendSpot.id.ownerId][friendSpot.id.localId] = friendSpot;
    }

    internal void SetSpot(in AnimalId id, AnimalSpotStruct friendSpot)
    {
        spots[id.ownerId][id.localId] = friendSpot;
    }

    internal void StartSurvivingPhase()
    {
        for (int j = 0; j < spots.Count; j++)
        {
            for (int i = 0; i < spots[j].Count;)
            {
                if (!spots[j][i].animal.CanSurvive()) spots[j].RemoveAt(i);
                else
                {
                    AnimalSpotStruct spot = spots[j][i];
                    spot.SetLocalAndOwnerId(j, i);
                    spots[j][i] = spot;
                    i++;

                }
            }
        }
        
    }

    internal void UpdatePhaseCooldown()
    {
        for (int j = 0; j < spots.Count; j++)
        {
            for (int i = 0; i < spots[j].Count;)
            {
                spots[j][i].UpdatePhaseCooldown();
            }
        }
    }

    internal void UpdateTurnCooldown()
    {
        for (int j = 0; j < spots.Count; j++)
        {
            for (int i = 0; i < spots[j].Count;)
            {
                spots[j][i].UpdateTurnCooldown();
            }
        }
    }
    private AnimalSpotStruct CreateFreeSpot(int playerId)
    {
        return new AnimalSpotStruct(new(playerId, GetSpotsLength(playerId)), AnimalStruct.NULL);
    }

    internal bool IsAbleToMove(int playerId)
    {
        return players[playerId].isAbleToMove;
    }
}
