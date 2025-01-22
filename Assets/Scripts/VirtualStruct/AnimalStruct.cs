

using JetBrains.Annotations;
using NUnit.Framework;
using System;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.SocialPlatforms.Impl;

[BurstCompile(DisableDirectCall = true)]
public struct PropArray
{
    public FixedListProps20 singles;
    public FixedListProps20 pairs;
    public int singlesLength => singles.Length;
    public int pairsLength => pairs.Length;

    public PropArray(int capacity)
    {
        singles = new (capacity);
        pairs = new (capacity);
        //singlesLength = 0;
        //pairsLength = 0;
    }

    internal void Add(in AnimalProp prop)
    {
        if (prop.isPair)
        {
            pairs.Add(prop);
        }
        else
        {
            singles.Add(prop);
        }
        //if (prop.isPair)
        //{
        //    pairs[pairsLength++] = prop;
        //}
        //else
        //{
        //    singles[singlesLength++] = prop;
        //}
    }

    internal bool Remove(in AnimalProp animalProp)
    {
        if (animalProp.isPair) {
            return pairs.Remove(animalProp);
        } else
        {
            return singles.Remove(animalProp);
        }
    }

    internal bool HasPropName(in AnimalProp animalProp)
    {
        if(animalProp.isPair)
        {
            for (int i = 0; i < pairsLength; i++)
            {
                if(pairs[i].name == animalProp.name) return true;
            }
        } else
        {
            for (int i = 0; i < singlesLength; i++)
            {
                if (singles[i].name == animalProp.name) return true;
            }
        }
        return false;


    }

    internal int GetScore()
    {
        int score = pairsLength + singlesLength;

        for (int i = 0; i < pairsLength; i++)
        {
            score += pairs[i].hungerIncrease;
        }
        for (int i = 0; i < singlesLength; i++)
        {
            score += singles[i].hungerIncrease;
        }
        return score;
    }

    internal void UpdatePhaseCooldown()
    {
        for (int i = 0; i < pairsLength; i++)
        {
            pairs[i].UpdatePhaseCooldown();
        }
        for (int i = 0; i < singlesLength; i++)
        {
            singles[i].UpdatePhaseCooldown();
        }

    }

    internal void UpdateTurnCooldown()
    {
        for (int i = 0; i < pairsLength; i++)
        {
            pairs[i].UpdateTurnCooldown();
        }
        for (int i = 0; i < singlesLength; i++)
        {
            singles[i].UpdateTurnCooldown();
        }

    }

    internal bool ActivateSleepProp()
    {
        for (int i = 0; i < singlesLength; i++)
        {
            if (singles[i].name == AnimalPropName.Sleep)
            {
                singles[i].Activate();

                return true;
            }
        }
        return false;
    }

    internal bool ActivateFasciestProp()
    {
        for (int i = 0; i < singlesLength; i++)
        {
            if (singles[i].name == AnimalPropName.Fasciest)
            {
                singles[i].Activate();
                return true;
            }
        }
        return false;
    }

    internal bool ActivatePiraceProp()
    {
        for (int i = 0; i < singlesLength; i++)
        {
            if (singles[i].name == AnimalPropName.Piracy)
            {
                singles[i].Activate();
                return true;
            }
        }
        return false;
    }

    internal bool ActivateFastProp()
    {
        for (int i = 0; i < singlesLength; i++)
        {
            if (singles[i].name == AnimalPropName.Fast)
            {
                singles[i].Activate();
                return true;
            }
        }
        return false;
    }

    internal bool ActivateMimicProp()
    {
        for (int i = 0; i < singlesLength; i++)
        {
            if (singles[i].name == AnimalPropName.Mimic)
            {
                singles[i].Activate();
                return true;
            }
        }
        return false;
    }

    internal bool ActivateScavengerProp()
    {
        for (int i = 0; i < singlesLength; i++)
        {
            if (singles[i].name == AnimalPropName.Scavenger)
            {
                singles[i].Activate();
                return true;
            }
        }
        return false;
    }

    internal bool ActivateDropTailProp()
    {
        for (int i = 0; i < singlesLength; i++)
        {
            if (singles[i].name == AnimalPropName.DropTail)
            {
                singles[i].Activate();
                return true;
            }
        }
        return false;
    }
}


[BurstCompile(DisableDirectCall = true)]
public struct AnimalStruct : IDisposable
{
    public static readonly AnimalStruct NULL = new AnimalStruct() { localId = -1, food = -1, fat = -1, maxFat = -2, maxFood = 0 };
    public bool isNull => maxFood == 0;

    public int localId;
    public int food;
    public int maxFood;
    public int fat;
    public int maxFat;
    public PropArray props;
    public AnimalPropName propFlags;


    public AnimalStruct(int localId)
    {
        this.localId = localId;
        food = 0;
        fat = 0;
        maxFat = 0;
        maxFood = 1;
        props = new PropArray(20);
        propFlags = new AnimalPropName();
    }

    public void SetLocalId(int i)
    {
        localId = i;
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
        if(prop.name == AnimalPropName.Fat)
        {
            maxFat++;
            if(!props.HasPropName(prop))
            {
                props.Add(prop);
                
            }
            propFlags |= prop.name;
            return true;
        }
        props.Add(prop);
        maxFood += prop.hungerIncrease;
        propFlags |= prop.name;
        return true;

    }

    public bool IsPossibleToAdd(in AnimalProp prop)
    {
        if (prop.name == AnimalPropName.Fat) return true;
        if (prop.isPair)
        {
            for (int i = 0; i < props.pairsLength; i++)
            {
                if (prop.SoftEquals(props.pairs[i])) return false;
            }
            return true;
        }
        if (propFlags.HasFlagFast(prop.name)) return false;
        if (prop.name == AnimalPropName.Predator && propFlags.HasFlagFast(AnimalPropName.Piracy)) return false;
        if (prop.name == AnimalPropName.Piracy && propFlags.HasFlagFast(AnimalPropName.Predator)) return false;
        return true;
    }


    public int Feed(int foodIncrease=1)
    {
        if(food < maxFood) { food+=foodIncrease; return 1; }
        if (fat < maxFat) { fat+=foodIncrease; return 1; }
        return 0;
    }

    public void RemoveProp(in AnimalProp animalProp)
    {
        bool isRemoved = props.Remove(animalProp);
        
        if(!props.HasPropName(animalProp)) propFlags &= ~animalProp.name;
    }

    internal int GetScore()
    {
        int score = 2;
        score += props.GetScore();
        return score;
    }

    internal void UpdatePhaseCooldown()
    {
         props.UpdatePhaseCooldown();
    }

    internal void UpdateTurnCooldown()
    {
        props.UpdateTurnCooldown();
    }

    internal void ActivateSleepProp()
    {
        if(props.ActivateSleepProp()) food = maxFood;
    }

    internal void ActivateFasciestProp()
    {
        props.ActivateFasciestProp();
    }

    internal void DecreaseFood()
    {
        food--;
    }

    internal void ActivatePiraceProp()
    {
        if(props.ActivatePiraceProp()) food++;
    }

    internal void ActivateFastProp()
    {
        props.ActivateFastProp();
    }

    internal void ActivateMimicProp()
    {
        props.ActivateMimicProp();
    }

    internal void ActivateDropTailProp()
    {
        props.ActivateDropTailProp();
    }

    internal void AddFlag(AnimalPropName flag)
    {
        propFlags |= flag;
    }

    internal void ActivateScavenger()
    {
        if(props.ActivateScavengerProp()) food++;
    }

    public void Dispose()
    {
        
    }

    public override string ToString()
    {
        return $"a[{food}/{maxFood}][{fat}/{maxFat}]{propFlags}";
    }
}