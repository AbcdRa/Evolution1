
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

        //this.cards = new(cards.Count, Allocator.TempJob);
        //foreach(CardStruct card in cards)
        //{
        //    this.cards.Add(card);
        //}
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

    internal void RemoveProp(in AnimalProp animalProp)
    {
        animal.RemoveProp(animalProp);
    }
}

