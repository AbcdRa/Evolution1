using System;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
public struct FoodManangerStruct
{
    public int food;

    public void SpawnFood(int length)
    {
        food = UnityEngine.Random.Range(1, 7) + UnityEngine.Random.Range(1, 7) + length/2;
    }

    internal void Consume(int amount)
    {
        food -= amount;
        if (food < 0) throw new Exception("Gamebreaking soo much food eated");
    }
}