

using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimalArea : MonoBehaviour, IAnimalArea
{
    [SerializeField] private AnimalSpot _freeSpot;
    public int ownerId { get; private set; } 

    private List<IAnimalSpot> _spots = new();

    public int amount => _spots.Count;
    public List<IAnimalSpot> spots => _spots;

    public void OrganizateSpots()
    {
        List<IAnimalSpot> newSpots = new List<IAnimalSpot>();
        List<IAnimalSpot> destroySpots = new List<IAnimalSpot>();
        for (int i = 0; i < amount; i++) {
            if (spots[i].isFreeSpot) destroySpots.Add(spots[i]);
            newSpots.Add(spots[i]);
        }
        foreach (var spot in destroySpots) {
            
            Destroy(spot.gameObject);
        }
        if (!_freeSpot.isFreeSpot)
        {
            newSpots.Add(_freeSpot);
            _freeSpot = CreateNewFreeSpot(newSpots.Count);
        }
        for (int i = 0; i < newSpots.Count; i++)
        {
            newSpots[i].SetLocalId(i);
        }
        _spots = newSpots;
    }

    private AnimalSpot CreateNewFreeSpot(int i)
    {
        AnimalSpot freeSpot = PrefabDataSingleton.instance.GetAnimalSpotPrefab();
        freeSpot.transform.SetParent(transform);
        freeSpot.SetLocalId(i);
        return freeSpot;
    }

    public bool AddPropToAnimal(ICard card, int localId)
    {
        bool isAdded = spots[localId].AddPropToAnimal(card);
        return isAdded;
    }

    public bool CreateAnimal(ICard card)
    {
        bool isCreated = _freeSpot.CreateAnimal(card);
        spots.Add(_freeSpot);
        _freeSpot = CreateNewFreeSpot(spots.Count);
        return isCreated;

    }

    public int Feed(int localId, IFoodMananger foodMananger, int foodIncrease=1)
    {
        if (foodMananger == null)
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


    private int PairFeed(int localId, int breakingId, int food, bool isFirstInit = false, bool isConsumedFood = true)
    {
        if ((!isFirstInit) && localId == breakingId) return 0;
        if (isConsumedFood && food == 0) return 0;
        int foodConsumed = 0;
        foodConsumed = spots[localId].Feed(food);
        if (foodConsumed == 0) return 0;
        if (isConsumedFood) food -= foodConsumed;

        for (int i = 0; i < spots[localId].animal.pairProps.Length; i++)
        {
            if (isConsumedFood && spots[localId].animal.pairProps[i].name == AnimalPropName.Interaction && food > 0)
            {
                AnimalId oth = spots[localId].animal.pairProps[i].GetOtherAnimalId(new(ownerId, localId));
                if (oth.ownerId != ownerId) throw new Exception("GAMEBREAKING RULE Trying to feed another animal");
                food -= PairFeed(oth.localId, breakingId, food);

            }
            else if (spots[localId].animal.pairProps[i].name == AnimalPropName.Cooperation)
            {
                AnimalId oth = spots[localId].animal.pairProps[i].GetOtherAnimalId(new(ownerId, localId));
                if (oth.ownerId != ownerId) throw new Exception("GAMEBREAKING RULE Trying to feed another animal");
                food -= PairFeed(oth.localId, breakingId, food, false, true);
            }
        }
        return food;
    }




    public void InitReset(int id)
    {
        ownerId = id;
        for (int i = 0; i < spots.Count; i++) {
            _spots[i].MakeFree();
        }
        OrganizateSpots();
    }

    public void StartSurvivingPhase()
    {
        for (int i = 0; i < spots.Count; i++)
        {
            if (spots[i].isFree) continue;
            if (!spots[i].animal.CanSurvive()) Kill(i);
        }
        OrganizateSpots();
    }

    public void UpdatePhaseCooldown()
    {
        for (int i = 0; i < spots.Count; i++)
        {
            spots[i].UpdatePhaseCooldown();
        }
    }

    public void UpdateTurnCooldown()
    {
        for (int i = 0; i < spots.Count; i++)
        {
            spots[i].UpdateTurnCooldown();
        }
    }

    public void Kill(int localId)
    {
        AnimalId myId = new(ownerId, localId);
        for (int i = 0; i < spots[localId].animal.pairProps.Length; i++)
        {
            AnimalId oth = spots[localId].animal.pairProps[i].GetOtherAnimalId(myId);
            if (oth.ownerId != ownerId) throw new Exception("GAMEBREAKING RULE животное было связано парным свойством с другим !!!");
            spots[oth.localId].RemoveProp(spots[localId].animal.pairProps[i]);
        }
        spots[localId].Kill();
    }

    public AnimalAreaStruct GetStruct()
    {
        List<AnimalSpotStruct> animalSpotStructs = new List<AnimalSpotStruct>();
        for(int i = 0; i < spots.Count; i++)
        {
            animalSpotStructs.Add(spots[i].GetStruct());
        }
        return new AnimalAreaStruct(ownerId, animalSpotStructs, _freeSpot.GetStruct());
    }

    public void InitReset()
    {
        //TODO Нужно улчушить этот код
        spots.Clear();
        _freeSpot.SetLocalId(0);
    }
}

