
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
    public AnimalProp main;
    public AnimalProp second;
    public bool isRotated;

    public CardStruct(in AnimalProp main, in AnimalProp second, bool isRotated)
    {
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


    bool ICard.isRotated { get => this.isRotated; set => this.isRotated = value; }
    AnimalProp ICard.main { get => this.main; set => this.main = value; }
    AnimalProp ICard.second { get => this.second; set => this.second = value; }
    public AnimalProp current => isRotated ? second : main;
    public Transform transform => null;
    public Sprite sprite => null;
    public bool IsSpecial() => false;

    public CardStruct GetStruct() => this;
}

