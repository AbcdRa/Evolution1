using System;
using Unity.Collections;
using Unity.Burst;

[BurstCompile(DisableDirectCall = true)]
public struct PairAnimalId
{
    internal static readonly PairAnimalId NULL = new(-1,-1,-1,-1);
    public static readonly PairAnimalId DOING_NEXT_TURN = NULL;
    public static readonly PairAnimalId NOT_DOING_NEXT_TURN = new(-2, -1, -1, -1);
    public static readonly PairAnimalId DESTROY_FOOD = new(-3, 1, -1, -1);
    public static readonly PairAnimalId PIRACY_FOOD = new(-4, -1, -1, -1);
    //public static PairAnimalId PLAY_INSTANT_SIDE_MOVE = new(-5, -1, -1, -1);
    //public static PairAnimalId TRY_TO_PLAY_INSTANT_SIDE_MOVE = new(-6, -1, -1, -1);
    public AnimalId first;
    public AnimalId second;

    public PairAnimalId(in AnimalId first, in AnimalId second) 
    {
        this.first = first;
        this.second = second;
    }

    public PairAnimalId(int owner1, int local1, int owner2, int local2) 
    {
        first = new(owner1, local1);
        second = new(owner2, local2);
    }

    public bool Equals(PairAnimalId other)
    {
        return other.first.Equals(first) && other.second.Equals(second);
    }
}
