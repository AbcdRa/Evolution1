
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

    public void SetSpot(AnimalId id, in CardStruct card)
    {
        switch (id.ownerId)
        {
            case 0: hand1[id.localId] = card; break;
            case 1: hand2[id.localId] = card; break;
            case 2: hand3[id.localId] = card; break;
            case 3: hand4[id.localId] = card; break;
        }
    }


}
