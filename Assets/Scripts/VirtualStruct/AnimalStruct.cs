

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
            AnimalProp prop = pairs[i];
            prop.UpdatePhaseCooldown();
            pairs[i] = prop;
        }
        for (int i = 0; i < singlesLength; i++)
        {
            AnimalProp prop = singles[i];
            prop.UpdatePhaseCooldown();
            singles[i] = prop;
        }

    }

    internal void UpdateTurnCooldown()
    {
        for (int i = 0; i < pairsLength; i++)
        {
            AnimalProp prop = pairs[i];
            prop.UpdateTurnCooldown();
            pairs[i] = prop;
        }
        for (int i = 0; i < singlesLength; i++)
        {
            AnimalProp prop = singles[i];
            prop.UpdateTurnCooldown();
            singles[i] = prop;
        }

    }

    public bool ActivateProp(AnimalPropName name)
    {
        for (int i = 0; i < singlesLength; i++)
        {
            if (singles[i].name == name)
            {
                //TODO можно сооптимизировать singles.Activate(i)
                AnimalProp prop = singles[i];
                prop.Activate();
                singles[i] = prop;
                return true;
            }
        }
        return false;
    }

    public bool ActivateProp(int i, bool isPairs=true)
    {
        if(isPairs)
        {
            if (i >= pairsLength) return false;
            AnimalProp prop = pairs[i];
            prop.Activate();
            pairs[i] = prop;
        }
        else
        {
            if (i >= singlesLength) return false;
            AnimalProp prop = singles[i];
            prop.Activate();
            singles[i] = prop;
        }
        return true;
    }

    internal void UpdateIdWnenRemove(int ownerId, int localId)
    {
        for (int i = 0; i < singlesLength; i++) {
            if (singles[i].mainAnimalId.ownerId != ownerId && singles[i].secondAnimalId.ownerId != localId) continue;
            AnimalProp prop = singles[i];
            if (prop.mainAnimalId.ownerId == ownerId && prop.mainAnimalId.localId > localId)
            {
                prop.mainAnimalId.localId--;
                
            }
            if (prop.secondAnimalId.ownerId == ownerId && prop.secondAnimalId.localId > localId)
            {
                prop.secondAnimalId.localId--;
            }
            singles[i] = prop;
        }
        for (int i = 0; i < pairsLength; i++)
        {
            if (pairs[i].mainAnimalId.ownerId != ownerId && pairs[i].secondAnimalId.ownerId != localId) continue;
            AnimalProp prop = pairs[i];
            if (prop.mainAnimalId.ownerId == ownerId && prop.mainAnimalId.localId > localId)
            {
                prop.mainAnimalId.localId--;

            }
            if (prop.secondAnimalId.ownerId == ownerId && prop.secondAnimalId.localId > localId)
            {
                prop.secondAnimalId.localId--;
            }
            pairs[i] = prop;
        }
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
        if(prop.name == AnimalPropName.Symbiosis && prop.secondAnimalId.localId == localId)
        {
            propFlags |= AnimalPropName.RIsSymbiontSlave;
        }
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
        if(animalProp.name == AnimalPropName.Symbiosis && propFlags.HasFlag(AnimalPropName.RIsSymbiontSlave))
        {
            RemoveSymbiontSlaveFlag();
        }
    }

    private void RemoveSymbiontSlaveFlag()
    {
        if (!propFlags.HasFlagFast(AnimalPropName.Symbiosis))
        {
            propFlags &= ~AnimalPropName.RIsSymbiontSlave;
            return;
        }

        for (int i = 0; i < props.pairsLength; i++)
        {
            if (props.pairs[i].name == AnimalPropName.Symbiosis && props.pairs[i].secondAnimalId.localId == localId) return;
        }
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
        if(props.ActivateProp(AnimalPropName.Sleep)) food = maxFood;
    }

    internal void ActivateFasciestProp()
    {
        props.ActivateProp(AnimalPropName.Fasciest);
    }

    internal void DecreaseFood()
    {
        food--;
    }

    internal void ActivatePiraceProp()
    {
        if(props.ActivateProp(AnimalPropName.Piracy)) food++;
    }

    internal void ActivateFastProp()
    {
        props.ActivateProp(AnimalPropName.Fast);
    }

    internal void ActivateMimicProp()
    {
        props.ActivateProp(AnimalPropName.Mimic);
    }

    internal void ActivateDropTailProp()
    {
        props.ActivateProp(AnimalPropName.DropTail);
    }

    internal void AddFlag(AnimalPropName flag)
    {
        propFlags |= flag;
    }

    internal void ActivateScavenger()
    {
        if(props.ActivateProp(AnimalPropName.Scavenger)) food++;
    }

    public void Dispose()
    {
        
    }

    public override string ToString()
    {
        return $"a[{food}/{maxFood}][{fat}/{maxFat}]{propFlags}";
    }

    internal void ActivatePredator()
    {
        props.ActivateProp(AnimalPropName.Predator);
    }

    internal void UpdateIdWhenRemove(int ownerId, int localId)
    {
        if (localId < this.localId) localId--;
        props.UpdateIdWnenRemove(ownerId, localId);
    }
}