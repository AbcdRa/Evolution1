﻿
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class AnimalSpot : MonoBehaviour, IAnimalSpot
{
    public AnimalStruct animal { get; private set; }
    private int _localId;
    private List<ICard> _cards = new();
    private AnimalInfoDisplay animalInfo;


    public int localId => _localId;
    public List<ICard> cards => _cards;
    public bool isFreeSpot => animal.isNull;
    public bool isFree => animal.isNull;

    ~AnimalSpot()
    {
        animal.Dispose();
    }

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

    public bool CreateAnimal(ICard card, int ownerId)
    {
        animal = new AnimalStruct(localId);
        card.transform.parent = transform;
        card.transform.gameObject.SetActive(true);
        card.transform.GetComponent<SelectionableObject>().SetSpecificationAndId(SOSpecification.AnimalCard, ownerId);
        card.transform.localPosition = Vector3.zero;
        card.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
        animalInfo = AnimalInfoDisplay.CreateInfoDisplay(card);
        animalInfo.UpdateUI(animal);
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

    public AnimalSpotStruct GetStruct(int ownerId)
    {
        return new AnimalSpotStruct(new(ownerId, localId), animal);
    }

    public ICard FindCard(CardStruct card)
    {
        for (int i = 0; i < cards.Count; i++) { 
            if(card.SoftEqual(cards[i])) return cards[i];
        }
        throw new Exception("Cant find");
    }
}

