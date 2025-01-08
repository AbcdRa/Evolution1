
using System;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using fstring = Unity.Collections.FixedString32Bytes;

[BurstCompile]
public struct CardStruct : ICard
{
    public static readonly CardStruct NULL = new CardStruct() { main = AnimalProp.NULL };
    public bool IsNull() => main.isNull();

    public int id;
    public AnimalProp main;
    public AnimalProp second;
    public bool isRotated;

    public CardStruct(in AnimalProp main, in AnimalProp second, bool isRotated, int id)
    {
        this.id = id;
        this.main = main;
        this.second = second;
        this.isRotated = isRotated;
    }

    internal fstring ToFString()
    {
        fstring result = new();
        result.Append('c');
        result.Append('[');
        result.Append(main.ToFString());
        result.Append('/');
        result.Append(second.ToFString());
        result.Append(']');
        return result;
    }



    public bool SoftEqual(ICard card)
    {
        if(card.main.name != main.name) return false;
        if(card.second.name != second.name) return false;
        return true;
    }


    bool ICard.isRotated { get => this.isRotated;}
    AnimalProp ICard.main { get => this.main;  }
    AnimalProp ICard.second { get => this.second; }
    public AnimalProp current => isRotated ? second : main;
    public Transform transform => null;
    public Sprite sprite => null;

    int ICard.id => id;

    public bool IsSpecial() => false;

    public CardStruct GetStruct() => this;

    public void Rotate()
    {
        isRotated = !isRotated;
    }
}

