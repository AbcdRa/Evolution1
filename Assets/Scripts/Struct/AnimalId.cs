using System;
using Unity.Burst;
using fstring = Unity.Collections.FixedString32Bytes;

[BurstCompile]
public struct AnimalId
{

    public int ownerId;
    public int localId;

    public static readonly AnimalId NULL = new() { localId = -1, ownerId = -1 };

    public AnimalId(int ownerId, int localId)
    {
        this.ownerId = ownerId;
        this.localId = localId;
    }

    public bool isNull() => localId == -1 && ownerId == -1;


    public fstring ToFString()
    {
        return new fstring(ownerId+"~"+localId);
    }

    public bool Equals(AnimalId other) { 
        return ownerId == other.ownerId && localId == other.localId;
    }
}
