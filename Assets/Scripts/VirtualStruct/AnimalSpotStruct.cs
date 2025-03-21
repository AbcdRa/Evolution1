﻿
using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.VisualScripting;

[BurstCompile(DisableDirectCall = true)]
public struct AnimalSpotStruct
{
    public AnimalId id;
    public AnimalStruct animal;
    public bool isFree => animal.isNull;
    //public NativeList<CardStruct> cards;

    public AnimalSpotStruct(AnimalId id, in AnimalStruct animal)
    {
        this.id = id;
        this.animal = animal;

    }


    internal bool AddPropToAnimal(in CardStruct card, bool isRotated)
    {
        bool isAdded = animal.AddProp(card, isRotated);
        //if (isAdded) { cards.Add(card); }
        return isAdded;
    }

    internal void Kill()
    {
        animal = AnimalStruct.NULL;
        //cards.Dispose();
    }

    internal void SetLocalAndOwnerId(in AnimalId id)
    {
        this.id = id;
    }

    internal void SetOwnerAndLocalId(int ownerId, int localId)
    {
        this.id.ownerId = ownerId;
        this.id.localId = localId;
    }


    internal bool IsPossibleToAddProp(in AnimalProp prop)
    {
        return animal.IsPossibleToAdd(prop);
    }

    internal bool CreateAnimal(in CardStruct card, int localId)
    {
        animal = new AnimalStruct(localId);
        //cards.Add(card);
        return true;
    }

    public int Feed(int food, int foodIncrease=1)
    {
        if(food <= 0) return 0;
        return animal.Feed(foodIncrease);
    }


    internal int GetScore()
    {
        return animal.isNull ? 0 : animal.GetScore();
    }

    internal void UpdatePhaseCooldown()
    {
        animal.UpdatePhaseCooldown();
    }

    internal void UpdateTurnCooldown()
    {
        animal.UpdateTurnCooldown();
    }

    public void RemoveProp(in AnimalProp animalProp)
    {
        animal.RemoveProp(animalProp);
    }

    public override string ToString()
    {
        return $"SPOT[{id.ownerId}~{id.localId}]={animal.ToString()}";
    }

    internal void DecreaseFood(int v)
    {
        animal.food -= v;
    }

    internal void UpdateIdWhenRemove(int ownerId, int localId)
    {
        if(id.localId > localId)
        {
            id.localId--;
        }
        animal.UpdateIdWhenRemove(ownerId, localId);
    }
}

