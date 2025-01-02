
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.VisualScripting;

[BurstCompile]
public struct AnimalSpotStruct
{
    public int localId;
    public AnimalStruct animal;
    public bool isFree => animal.isNull;
    public NativeList<CardStruct> cards;

    internal bool AddPropToAnimal(in CardStruct card, bool isRotated)
    {
        bool isAdded = animal.AddProp(card, isRotated);
        if (isAdded) { cards.Add(card); }
        return isAdded;
    }

    internal void Kill()
    {
        animal = AnimalStruct.NULL;
        cards.Dispose();
    }

    internal void SetLocalId(int i)
    {
        localId = i;
        animal.localId = i;
    }

    internal bool IsPossibleToAddProp(in AnimalProp prop)
    {
        return animal.IsPossibleToAddProp(prop);
    }

    internal bool CreateAnimal(in CardStruct card, int localId)
    {
        animal = new AnimalStruct(localId);
        cards.Add(card);
        return true;
    }

    public int Feed(int food)
    {
        if(food <= 0) return 0;
        return animal.Feed();
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

    internal void RemoveProp(in AnimalProp animalProp)
    {
        animal.RemoveProp(animalProp);
    }
}

