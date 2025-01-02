using System;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public struct DeckStruct
{
    public NativeList<CardStruct> cards;
    internal int amount => cards.Length;

    internal CardStruct TakeLast()
    {
        //TODO Честно не знаю, но возможно сооптимизировать и это я уверен, НЕНАВИЖУ копирование
        CardStruct card = cards[cards.Length - 1];
        cards.RemoveAt(cards.Length - 1);
        return card;
    }
}