
using System;
using Unity.Collections;

public struct Hands
{
    public NativeList<CardStruct> hand1;
    public NativeList<CardStruct> hand2;
    public NativeList<CardStruct> hand3;
    public NativeList<CardStruct> hand4;

    public CardStruct GetHandCard(AnimalId id)
    {
        switch (id.ownerId)
        {
            case 0: return hand1[id.localId];
            case 1: return hand2[id.localId];
            case 2: return hand3[id.localId];
            case 3: return hand4[id.localId];
        }
        throw new Exception("WTF");
    }

    public int GetHandAmount(int playerId)
    {
        switch (playerId)
        {
            case 0: return hand1.Length;
            case 1: return hand2.Length;
            case 2: return hand3.Length;
            case 3: return hand4.Length;
        }
        return 0;
    }

    public void SetCard(AnimalId id, in CardStruct card)
    {
        switch (id.ownerId)
        {
            case 0: hand1[id.localId] = card; break;
            case 1: hand2[id.localId] = card; break;
            case 2: hand3[id.localId] = card; break;
            case 3: hand4[id.localId] = card; break;
        }
    }

    internal void AddCard(int playerId, in CardStruct card)
    {
        switch (playerId)
        {
            case 0: hand1.Add(card); break;
            case 1: hand2.Add(card); break;
            case 2: hand3.Add(card); break;
            case 3: hand4.Add(card); break;
        }
    }
}
