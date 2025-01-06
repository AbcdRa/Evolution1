using System;
using UnityEngine;

public class Card : MonoBehaviour, ICard
{
    public bool isRotated {  get; set; }
    public AnimalProp main { get; set; }
    public AnimalProp second { get; set; }
    public AnimalProp current => isRotated ? second : main;

    public Sprite sprite => throw new NotImplementedException();

    public void CreateCard(CardSO cardSO)
    {
        this.main = cardSO.main;
        this.second = cardSO.second;
        this.isRotated = false;
    }

    public bool IsSpecial() => false;

    public bool SoftEqual(ICard card)
    {
        return card.main.name == main.name && card.second.name == second.name;
    }


}

