

using NUnit.Framework;
using System;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.SocialPlatforms.Impl;

[BurstCompile]
public struct AnimalStruct
{
    public static readonly AnimalStruct NULL = new AnimalStruct() { localId = -1, food = -1, fat = -1, maxFat = -2, maxFood = -2 };
    public bool isNull => localId == -1 && food == -1;

    public int localId;
    public int food;
    public int maxFood;
    public int fat;
    public int maxFat;
    public NativeList<AnimalProp> singleProps;
    public NativeList<AnimalProp> pairProps;
    public AnimalPropName propFlags;


    public AnimalStruct(int localId)
    {
        this.localId = localId;
        food = 0;
        fat = 0;
        maxFat = 0;
        maxFood = 1;
        singleProps = new NativeList<AnimalProp>(4, Allocator.TempJob);
        pairProps = new NativeList<AnimalProp>(4, Allocator.TempJob);
        propFlags = new AnimalPropName();
    }

    public bool CanSurvive()
    {
        if (food >= maxFood) return true;
        TransformFat();
        return food >= maxFood;
    }

    private void TransformFat()
    {
        int newFood = food+ (fat < maxFood - food ? fat : maxFood-food);
        fat = newFood- food;
        food = newFood;

    }

    public bool isFull() { return food == maxFood && fat == maxFat; }

    internal bool IsPiracyTarget()
    {
        return food > 0 && food < maxFood;
    }

    internal bool AddProp(in CardStruct card, bool isRotated)
    {
        AnimalProp prop = isRotated ? card.second : card.main;
        bool isPossibleToAdd = IsPossibleToAdd(prop);
        if(!isPossibleToAdd) return false;
        if(prop.isPair) pairProps.Add(prop);
        else singleProps.Add(prop);
        maxFood += prop.hungerIncrease;
        if (prop.name == AnimalPropName.Fat) maxFat++;
        propFlags |= prop.name;
        return true;

    }

    public bool IsPossibleToAdd(in AnimalProp prop)
    {
        if(prop.name == AnimalPropName.Fat) return true;
        AnimalPropName confictPropName = AnimalPropName.ERROR;
        if (prop.name == AnimalPropName.Predator) confictPropName = AnimalPropName.Piracy;
        if (prop.name == AnimalPropName.Piracy) confictPropName = AnimalPropName.Predator;
        if(prop.isPair)
        {
            for(int i = 0; i < pairProps.Length; i++)
            {
                if(prop.SoftEquals(pairProps[i])) return false;
                if (pairProps[i].name == confictPropName) return false;
            }
        } else
        {
            for (int i = 0; i < singleProps.Length; i++)
            {
                if (prop.SoftEquals(singleProps[i])) return false;
                if (singleProps[i].name == confictPropName) return false;
            }
        }
        return true;
    }

    internal bool IsPossibleToAddProp(in AnimalProp prop)
    {
        if(prop.name == AnimalPropName.Fat) return true;
        if(prop.isPair) {
            for (int i = 0; i < pairProps.Length; i++) {
                if (prop.SoftEquals(pairProps[i])) return false;
            }
            return true;
        }
        if(propFlags.HasFlagFast(prop.name)) return false;
        if(prop.name == AnimalPropName.Predator && propFlags.HasFlagFast(AnimalPropName.Piracy)) return false;
        if(prop.name == AnimalPropName.Piracy && propFlags.HasFlagFast(AnimalPropName.Predator)) return false;
        return true;
    }

    public int Feed(int foodIncrease=1)
    {
        if(food < maxFood) { food+=foodIncrease; return 1; }
        if (fat < maxFat) { fat+=foodIncrease; return 1; }
        return 0;
    }

    internal void RemoveProp(in AnimalProp animalProp)
    {
        for (int i = 0; i < pairProps.Length; i++) {
            if (animalProp.SoftEquals(pairProps[i]))
            {
                pairProps.RemoveAt(i);
                return;
            }
        }
        for (int i = 0; i < singleProps.Length; i++)
        {
            if (animalProp.SoftEquals(singleProps[i]))
            {
                singleProps.RemoveAt(i);
                return;
            }
        }
    }

    internal int GetScore()
    {
        int score = 2;
        for (int i = 0; i < pairProps.Length; i++) {
            score++;
            score += pairProps[i].hungerIncrease;
        }
        for (int i = 0; i < singleProps.Length; i++)
        {
            score++;
            score += singleProps[i].hungerIncrease;
        }
        return score;
    }

    internal void UpdatePhaseCooldown()
    {
        for (int i = 0; i < pairProps.Length; i++)
        {
            pairProps[i].UpdatePhaseCooldown();
        }
        for (int i = 0; i < singleProps.Length; i++)
        {
            singleProps[i].UpdatePhaseCooldown();
        }
    }

    internal void UpdateTurnCooldown()
    {
        for (int i = 0; i < pairProps.Length; i++)
        {
            pairProps[i].UpdateTurnCooldown();
        }
        for (int i = 0; i < singleProps.Length; i++)
        {
            singleProps[i].UpdateTurnCooldown();
        }
    }

    internal void ActivateSleepProp()
    {
        throw new NotImplementedException();
    }

    internal void ActivateFasciestProp()
    {
        throw new NotImplementedException();
    }

    internal void DecreaseFood()
    {
        throw new NotImplementedException();
    }

    internal void ActivatePiraceProp()
    {
        throw new NotImplementedException();
    }

    internal void ActivateFastProp()
    {
        throw new NotImplementedException();
    }

    internal void ActivateMimicProp()
    {
        throw new NotImplementedException();
    }

    internal void ActivateDropTailProp()
    {
        throw new NotImplementedException();
    }

    internal void AddFlag(AnimalPropName rIsPoisoned)
    {
        throw new NotImplementedException();
    }
}