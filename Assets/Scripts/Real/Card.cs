using System;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour, ICard
{
    public int id { get; private set; }
    public bool isRotated { get; private set; }
    public AnimalProp main { get; set; }
    public AnimalProp second { get; set; }
    public AnimalProp current => isRotated ? second : main;

    [SerializeField] private Image image;
    public Sprite sprite => image.sprite;

    public void CreateCard(CardSO cardSO)
    {
        this.id = cardSO.id;
        this.main = cardSO.main;
        this.second = cardSO.second;
        this.isRotated = false;
        image.sprite = PrefabDataSingleton.instance.GetFaces()[cardSO.id]; 
    }

    public CardStruct GetStruct()
    {
        return new CardStruct(main, second, isRotated, id);
    }

    public bool IsSpecial() => false;

    public bool SoftEqual(ICard card)
    {
        return card.main.name == main.name && card.second.name == second.name;
    }

    public void Rotate()
    {
        isRotated = !isRotated;
        transform.localRotation = Quaternion.Euler(0f, 0f, (isRotated ? 180f : 0f));
    }
}

