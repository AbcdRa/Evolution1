﻿
using System;
using Unity.Burst;
using UnityEngine;
using fstring = Unity.Collections.FixedString32Bytes;

[BurstCompile]
public struct CardStruct : ICard
{
    public static CardStruct NULL = new CardStruct() { main = AnimalProp.NULL };
    public bool IsNull() => main.isNull();
    public AnimalProp main;
    public AnimalProp second;
    public bool isRotated;


    internal fstring ToFString()
    {
        return new fstring("cs["+main.ToFString()+"/"+second.ToFString()+"]");
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
    public bool IsSpecial() => false;
    public ICard Copy() => this;
}

