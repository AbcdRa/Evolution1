using System;
using Unity.Burst;
using Unity.Collections;
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
        fstring result = ownerId.ToFString();
        result.Append('~');
        result.Append(localId);
        return result;
    }

    public bool Equals(AnimalId other) { 
        return ownerId == other.ownerId && localId == other.localId;
    }
}

[BurstCompile]
public struct PropId
{

    public int spotlId;
    public int proplId;

    public static readonly PropId NULL = new() { proplId = -1, spotlId = -1 };

    public PropId(int spotlId, int proplId)
    {
        this.spotlId = spotlId;
        this.proplId = proplId;
    }

    public bool isNull() => proplId == -1 && spotlId == -1;


    public fstring ToFString()
    {
        return new fstring(spotlId + "~" + proplId);
    }
}
