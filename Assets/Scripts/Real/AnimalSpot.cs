
using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimalSpot : MonoBehaviour, IAnimalSpot
{
    public AnimalStruct animal { get; private set; }
    private int _localId;
    private List<ICard> _cards;
    public bool isFreeSpot => animal.isNull;

    public bool isFree => throw new NotImplementedException();

    public void Destroy()
    {
        foreach (var card in _cards) {
            Destroy(card.transform);
        }
    }

    public void MakeFree()
    {
        throw new System.NotImplementedException();
    }

    public void SetLocalId(int i)
    {
        _localId = i;
        if(!isFreeSpot) animal.SetLocalId(i);
    }

    public bool CreateAnimal(ICard card)
    {
        throw new NotImplementedException();
    }

    public bool AddPropToAnimal(ICard card)
    {
        throw new NotImplementedException();
    }

    public int Feed(int food, int foodIncrease = 1)
    {
        throw new NotImplementedException();
    }

    public void Kill()
    {
        throw new NotImplementedException();
    }

    public void RemoveProp(AnimalProp animalProp)
    {
        throw new NotImplementedException();
    }

    public void UpdatePhaseCooldown()
    {
        throw new NotImplementedException();
    }

    public void UpdateTurnCooldown()
    {
        throw new NotImplementedException();
    }
}

