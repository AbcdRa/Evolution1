

using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimalArea : MonoBehaviour, IAnimalArea
{
    [SerializeField] private AnimalSpot _freeSpot;

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
            _freeSpot = CreateNewFreeSpot();
        }
        for (int i = 0; i < newSpots.Count; i++)
        {
            newSpots[i].SetLocalId(i);
        }
        _spots = newSpots;
    }

    private AnimalSpot CreateNewFreeSpot()
    {
        throw new NotImplementedException();
    }

    public bool AddPropToAnimal(ICard card, int localId)
    {
        throw new System.NotImplementedException();
    }

    public bool CreateAnimal(ICard card)
    {
        throw new System.NotImplementedException();
    }

    public int Feed(int localId, IFoodMananger foodMananger)
    {
        throw new System.NotImplementedException();
    }

    public void InitReset()
    {
        for (int i = 0; i < spots.Count; i++) {
            _spots[i].MakeFree();
        }
        OrganizateSpots();
    }

    public void StartSurvivingPhase()
    {
        throw new System.NotImplementedException();
    }

    public void UpdatePhaseCooldown()
    {
        throw new System.NotImplementedException();
    }

    public void UpdateTurnCooldown()
    {
        throw new System.NotImplementedException();
    }
}

