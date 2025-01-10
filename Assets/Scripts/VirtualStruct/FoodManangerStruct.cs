using System;
using Unity.Burst;
using UnityEngine;

[BurstCompile(DisableDirectCall = true)]
public struct FoodManangerStruct
{
    public static readonly FoodManangerStruct NULL = new FoodManangerStruct() { food = -1 };
    internal readonly bool isNull => food == -1;
    public int food;

    public FoodManangerStruct(int food)
    {
        this.food = food;
    }

    public void SpawnFood(int length)
    {
        var random = new Unity.Mathematics.Random(10);
        food = random.NextInt(1, 7) + random.NextInt(1, 7) + length/2;
    }

    internal void Consume(int amount)
    {
        food -= amount;
        if (food < 0) throw new Exception("Gamebreaking soo much food eated");
    }
}