
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class AnimalSpot : MonoBehaviour, IAnimalSpot
{
    public AnimalStruct animal { get; private set; }
    private int _localId;
    private List<ICard> _cards;
    public int localId => _localId;
    public List<ICard> cards => _cards;
    public bool isFreeSpot => animal.isNull;

    public bool isFree => animal.isNull;

    public void Destroy()
    {
        foreach (var card in _cards) {
            Destroy(card.transform);
        }
    }

    public void MakeFree()
    {
        animal = AnimalStruct.NULL;
    }

    public void SetLocalId(int i)
    {
        _localId = i;
        if(!isFreeSpot) animal.SetLocalId(i);
    }

    public bool CreateAnimal(ICard card)
    {
        animal = new AnimalStruct(localId);
        cards.Add(card);
        return true;
    }

    public bool AddPropToAnimal(ICard card)
    {
        bool isAdded = animal.AddProp(card.GetStruct(), card.isRotated);
        if (isAdded) { cards.Add(card); }
        return isAdded;
    }

    public int Feed(int food, int foodIncrease = 1)
    {
        if (food <= 0) return 0;
        return animal.Feed(foodIncrease);
    }

    public void Kill()
    {
        animal = AnimalStruct.NULL;
    }

    public void RemoveProp(AnimalProp animalProp)
    {
        animal.RemoveProp(animalProp);
    }

    public void UpdatePhaseCooldown()
    {
        animal.UpdatePhaseCooldown();
    }

    public void UpdateTurnCooldown()
    {
        animal.UpdateTurnCooldown();
    }

    public AnimalSpotStruct GetStruct()
    {
        List<CardStruct> cardstructs = new List<CardStruct>();
        for (int i = 0; i < cards.Count; i++) cardstructs.Add(cards[i].GetStruct());
        return new AnimalSpotStruct(localId, animal, cardstructs);
    }

}

