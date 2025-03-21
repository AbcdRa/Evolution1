﻿using Newtonsoft.Json;
using System;
using Unity.Burst;

[BurstCompile(DisableDirectCall = true)]
//public struct FixedArr10<T> where T : unmanaged
//{
//    private T _field0;
//    private T _field1;
//    private T _field2;
//    private T _field3;
//    private T _field4;
//    private T _field5;
//    private T _field6;
//    private T _field7;
//    private T _field8;
//    private T _field9;

//    // Индексатор для доступа по индексу
//    public T this[int index]
//    {
//        get
//        {
//            return index switch
//            {
//                0 => _field0,
//                1 => _field1,
//                2 => _field2,
//                3 => _field3,
//                4 => _field4,
//                5 => _field5,
//                6 => _field6,
//                7 => _field7,
//                8 => _field8,
//                9 => _field9,
//                _ => throw new IndexOutOfRangeException($"Index {index} is out of range for FixedArr10.")
//            };
//        }
//        set
//        {
//            switch (index)
//            {
//                case 0: _field0 = value; break;
//                case 1: _field1 = value; break;
//                case 2: _field2 = value; break;
//                case 3: _field3 = value; break;
//                case 4: _field4 = value; break;
//                case 5: _field5 = value; break;
//                case 6: _field6 = value; break;
//                case 7: _field7 = value; break;
//                case 8: _field8 = value; break;
//                case 9: _field9 = value; break;
//                default: throw new IndexOutOfRangeException($"Index {index} is out of range for FixedArr10.");
//            }
//        }
//    }

//    // Длина массива
//    public int Length => 10;
//}


public struct FixedArr4<T> where T : unmanaged
{
    private T _field0;
    private T _field1;
    private T _field2;
    private T _field3;


    // Индексатор для доступа по индексу
    public T this[int index]
    {
        get
        {
            return index switch
            {
                0 => _field0,
                1 => _field1,
                2 => _field2,
                3 => _field3,
                _ => throw new IndexOutOfRangeException($"Index {index} is out of range for FixedArr10.")
            };
        }
        set
        {
            switch (index)
            {
                case 0: _field0 = value; break;
                case 1: _field1 = value; break;
                case 2: _field2 = value; break;
                case 3: _field3 = value; break;
                default: throw new IndexOutOfRangeException($"Index {index} is out of range for FixedArr10.");
            }
        }
    }

    // Длина массива
    public int Length => 4;
}




//[BurstCompile(DisableDirectCall = true)]
//public struct FixedList20<T> where T : unmanaged
//{
//    private T _field0;
//    private T _field1;
//    private T _field2;
//    private T _field3;
//    private T _field4;
//    private T _field5;
//    private T _field6;
//    private T _field7;
//    private T _field8;
//    private T _field9;
//    private T _field10;
//    private T _field11;
//    private T _field12;
//    private T _field13;
//    private T _field14;
//    private T _field15;
//    private T _field16;
//    private T _field17;
//    private T _field18;
//    private T _field19;


//    // Индексатор для доступа по индексу
//    public T this[int index]
//    {
//        get
//        {
//            return index switch
//            {
//                0 => _field0,
//                1 => _field1,
//                2 => _field2,
//                3 => _field3,
//                4 => _field4,
//                5 => _field5,
//                6 => _field6,
//                7 => _field7,
//                8 => _field8,
//                9 => _field9,
//                10 => _field10,
//                11 => _field11,
//                12 => _field12,
//                13 => _field13,
//                14 => _field14,
//                15 => _field15,
//                16 => _field16,
//                17 => _field17,
//                18 => _field18,
//                19 => _field19,
//                _ => throw new IndexOutOfRangeException($"Index {index} is out of range for FixedArr10.")
//            };
//        }
//        set
//        {
//            switch (index)
//            {
//                case 0: _field0 = value; break;
//                case 1: _field1 = value; break;
//                case 2: _field2 = value; break;
//                case 3: _field3 = value; break;
//                case 4: _field4 = value; break;
//                case 5: _field5 = value; break;
//                case 6: _field6 = value; break;
//                case 7: _field7 = value; break;
//                case 8: _field8 = value; break;
//                case 9: _field9 = value; break;
//                case 10: _field10 = value; break;
//                case 11: _field11 = value; break;
//                case 12: _field12 = value; break;
//                case 13: _field13 = value; break;
//                case 14: _field14 = value; break;
//                case 15: _field15 = value; break;
//                case 16: _field16 = value; break;
//                case 17: _field17 = value; break;
//                case 18: _field18 = value; break;
//                case 19: _field19 = value; break;
//                default: throw new IndexOutOfRangeException($"Index {index} is out of range for FixedArr10.");
//            }
//        }
//    }

//    // Длина массива
//    public int Capacity => 20;
//    public int Length;

//    internal void Add(in T animalSpotStruct)
//    {
//        this[Length++] = animalSpotStruct;
//    }

//}



public struct FixedListProps20
{
    [JsonProperty] private AnimalProp _field0;
    [JsonProperty] private AnimalProp _field1;
    [JsonProperty] private AnimalProp _field2;
    [JsonProperty] private AnimalProp _field3;
    [JsonProperty] private AnimalProp _field4;
    [JsonProperty] private AnimalProp _field5;
    [JsonProperty] private AnimalProp _field6;
    [JsonProperty] private AnimalProp _field7;
    [JsonProperty] private AnimalProp _field8;
    [JsonProperty] private AnimalProp _field9;
    [JsonProperty] private AnimalProp _field10;
    [JsonProperty] private AnimalProp _field11;
    [JsonProperty] private AnimalProp _field12;
    [JsonProperty] private AnimalProp _field13;
    [JsonProperty] private AnimalProp _field14;
    [JsonProperty] private AnimalProp _field15;
    [JsonProperty] private AnimalProp _field16;
    [JsonProperty] private AnimalProp _field17;
    [JsonProperty] private AnimalProp _field18;
    [JsonProperty] private AnimalProp _field19;

    public FixedListProps20(int cap)
    {
        _field0 = AnimalProp.NULL;
        _field1 = AnimalProp.NULL;
        _field2 = AnimalProp.NULL;
        _field3 = AnimalProp.NULL;
        _field4 = AnimalProp.NULL;
        _field5 = AnimalProp.NULL;
        _field6 = AnimalProp.NULL;
        _field7 = AnimalProp.NULL;
        _field8 = AnimalProp.NULL;
        _field9 = AnimalProp.NULL;
        _field10 = AnimalProp.NULL;
        _field11 = AnimalProp.NULL;
        _field12 = AnimalProp.NULL;
        _field13 = AnimalProp.NULL;
        _field14 = AnimalProp.NULL;
        _field15 = AnimalProp.NULL;
        _field16 = AnimalProp.NULL;
        _field17 = AnimalProp.NULL;
        _field18 = AnimalProp.NULL;
        _field19 = AnimalProp.NULL;
        Length = 0;
    }


    // Индексатор для доступа по индексу
    public AnimalProp this[int index]
    {
        get
        {
            return index switch
            {
                0 => _field0,
                1 => _field1,
                2 => _field2,
                3 => _field3,
                4 => _field4,
                5 => _field5,
                6 => _field6,
                7 => _field7,
                8 => _field8,
                9 => _field9,
                10 => _field10,
                11 => _field11,
                12 => _field12,
                13 => _field13,
                14 => _field14,
                15 => _field15,
                16 => _field16,
                17 => _field17,
                18 => _field18,
                19 => _field19,
                _ => throw new IndexOutOfRangeException($"Index {index} is out of range for FixedArr10.")
            };
        }
        set
        {
            switch (index)
            {
                case 0: _field0 = value; break;
                case 1: _field1 = value; break;
                case 2: _field2 = value; break;
                case 3: _field3 = value; break;
                case 4: _field4 = value; break;
                case 5: _field5 = value; break;
                case 6: _field6 = value; break;
                case 7: _field7 = value; break;
                case 8: _field8 = value; break;
                case 9: _field9 = value; break;
                case 10: _field10 = value; break;
                case 11: _field11 = value; break;
                case 12: _field12 = value; break;
                case 13: _field13 = value; break;
                case 14: _field14 = value; break;
                case 15: _field15 = value; break;
                case 16: _field16 = value; break;
                case 17: _field17 = value; break;
                case 18: _field18 = value; break;
                case 19: _field19 = value; break;
                default: throw new IndexOutOfRangeException($"Index {index} is out of range for FixedArr10.");
            }
        }
    }

    // Длина массива
    public int Capacity => 20;
    public int Length;

    internal void Add(in AnimalProp animalSpotStruct)
    {
        this[Length++] = animalSpotStruct;
    }

    internal bool Remove(AnimalProp el)
    {
        for (int i = 0; i < Length; i++)
        { 
            if (el.SoftEquals(this[i]))
            {
                for(int j = i+1; j < Length; j++ )
                {
                    this[j-1] = this[j]; 
                }
                Length--;
                return true;
            }
        }
        return false;
    }
}