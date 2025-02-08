
using System;
using Unity.Collections;
using static UnityEngine.Rendering.HDROutputUtils;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEditorInternal.VR;
using fstring = Unity.Collections.FixedString32Bytes;
using Unity.Burst;





[BurstCompile(DisableDirectCall = true)]
public static class AnimalPropExtensions
{
    // Метод-расширение для проверки флага
    public static bool HasFlagFast(this AnimalPropName value, in AnimalPropName flag)
    {
        return (value & flag) != 0;
    }

    //TODO ПО ФАКТУ НЕ НУЖЕН
    public static fstring ToFString(this int value)
    {
        fstring fixedString = default;
        fixedString.Append(value); // Добавляет число в строку
        return fixedString;
    }
}

//[Flags]
//public enum AnimalPropNameFlags
//{
//    ERROR, Aqua, Big, Borrow, Camouflage, Cooperation, DropTail, Fasciest, Fast, Fat, Interaction,
//    Mimic, Parasite, Piracy, Poison, Predator, Scavenger, SharpEye, Sleep, Symbiosis, VIRTUAL
//}

[Flags]
public enum AnimalPropName
{
    Empty =            0b0000000000000000000000000000001, 
    Aqua =             0b0000000000000000000000000000010, 
    Big =              0b0000000000000000000000000000100, 
    Borrow =           0b0000000000000000000000000001000, 
    Camouflage =       0b0000000000000000000000000010000,
    Cooperation =      0b0000000000000000000000000100000,
    DropTail =         0b0000000000000000000000001000000, 
    Fasciest =         0b0000000000000000000000010000000,
    Fast =             0b0000000000000000000000100000000,
    Fat =              0b0000000000000000000001000000000,
    Interaction =      0b0000000000000000000010000000000,
    Mimic =            0b0000000000000000000100000000000,
    Parasite =         0b0000000000000000001000000000000, 
    Piracy =           0b0000000000000000010000000000000,
    Poison =           0b0000000000000000100000000000000,
    Predator =         0b0000000000000001000000000000000,
    Scavenger =        0b0000000000000010000000000000000,
    SharpEye =         0b0000000000000100000000000000000,
    Sleep =            0b0000000000001000000000000000000,
    Symbiosis =        0b0000000000010000000000000000000,
    ERROR =            0b0000000000100000000000000000000,
    RIsPoisoned =      0b0000000001000000000000000000000,
    RIsSymbiontSlave = 0b0000000010000000000000000000000
}



public struct AnimalProp
{
    public static readonly AnimalProp NULL = new AnimalProp() { name = AnimalPropName.ERROR };
    public bool isNull() => name == AnimalPropName.ERROR;
    public AnimalPropName name;
    public int hungerIncrease;
    public bool isPair;
    public AnimalId secondAnimalId;
    public AnimalId mainAnimalId;
    public int phaseCooldown;
    public int turnCooldown;

    public AnimalProp(AnimalPropSO animalPropSO) 
    {
        this.name = animalPropSO.PropName;
        this.hungerIncrease = animalPropSO.HungerIncrease;
        this.isPair = animalPropSO.IsPair;
        this.mainAnimalId = AnimalId.NULL;
        this.secondAnimalId = AnimalId.NULL;
        this.phaseCooldown = 0;
        this.turnCooldown = 0;
    }

    public bool IsActivable => phaseCooldown == 0 && turnCooldown == 0;

    public void Activate()
    {
        switch(name)
        {
            case AnimalPropName.Interaction:
                turnCooldown++;
                break;
            case AnimalPropName.Cooperation:
                turnCooldown++;
                break;
            case AnimalPropName.Predator:
                phaseCooldown++;
                break;
            case AnimalPropName.Sleep:
                phaseCooldown += 2;
                break;
            case AnimalPropName.Fasciest:
                turnCooldown++;
                break;
            case AnimalPropName.Piracy:
                phaseCooldown++;
                break;
            case AnimalPropName.Fast:
                turnCooldown++;
                break;
            case AnimalPropName.Mimic:
                turnCooldown++;
                break;
        }
    }

    public void UpdateTurnCooldown()
    {
        if (turnCooldown == 0) return;
        turnCooldown--;
    }

    public void UpdatePhaseCooldown()
    {
        if (phaseCooldown == 0) return;
        phaseCooldown--;
    }

    public AnimalId GetOtherAnimalId(in AnimalId id)
    {
        if (id.Equals(mainAnimalId)) return secondAnimalId;
        if (id.Equals(secondAnimalId)) return mainAnimalId;
        return AnimalId.NULL;
    }

    internal fstring ToFString()
    {
        
        return new fstring(name.ToString());
    }

    public override string ToString()
    {
        return ToFString().Value;
    }

    internal bool isHostile()
    {
        return name == AnimalPropName.Parasite;
    }

    internal bool IsInteractable()
    {
        if(!IsActivable) return false;
        return GameInteractionStruct.IsInteractable(name);
    }

    public bool SoftEquals(in AnimalProp oth)
    {
        if(oth.name != name) return false;
        //TODO CAN OPTIMIZED
        if(oth.mainAnimalId.Equals(mainAnimalId) && oth.secondAnimalId.Equals(secondAnimalId)) return true;
        if (oth.secondAnimalId.Equals(mainAnimalId) && oth.mainAnimalId.Equals(secondAnimalId)) return true;
        return false;
    }


}

