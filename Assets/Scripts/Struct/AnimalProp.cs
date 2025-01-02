﻿
using System;
using Unity.Collections;
using static UnityEngine.Rendering.HDROutputUtils;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEditorInternal.VR;
using fstring = Unity.Collections.FixedString32Bytes;

[Flags]
public enum AnimalPropName
{
    Empty, Aqua, Big, Borrow, Camouflage, Cooperation, DropTail, Fasciest, Fast, Fat, Interaction,
    Mimic, Parasite, Piracy, Poison, Predator, Scavenger, SharpEye, Sleep, Symbiosis, ERROR, RIsPoisoned, RIsSymbiontSlave
}

//[Flags]
//public enum AnimalPropNameFlags
//{
//    ERROR, Aqua, Big, Borrow, Camouflage, Cooperation, DropTail, Fasciest, Fast, Fat, Interaction,
//    Mimic, Parasite, Piracy, Poison, Predator, Scavenger, SharpEye, Sleep, Symbiosis, VIRTUAL
//}



public struct AnimalPropNameToFString
{
    public readonly static FixedString32Bytes[] runes = new FixedString32Bytes[] 
    { "Aqua", "Big", "Borrow", "Camouflage", "Cooperation", "DropTail", "Fasciest", "Fast", "Fat", "Interaction",
      "Mimic", "Parasite", "Piracy", "Poison", "Predator", "Scavenger", "SharpEye", "Sleep", "Symbiosis", "ERROR", "VIRTUAL" };
    
    public static fstring GetName(in AnimalPropName name)
    {
        //TODO Сложнее но Можно оптимизировать
        return runes[(int)name];
    }

}

public struct AnimalProp
{
    public static AnimalProp NULL = new AnimalProp() { name = AnimalPropName.ERROR };
    public bool isNull() => name == AnimalPropName.ERROR;
    public AnimalPropName name;
    public int hungerIncrease;
    public bool isPair;
    public AnimalId secondAnimalId;
    public AnimalId mainAnimalId;
    public int phaseCooldown;
    public int turnCooldown;

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
        }
    }

    public void UpdateTurnCooldown()
    {
        turnCooldown--;
    }

    public void UpdatePhaseCooldown()
    {
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
        
        return AnimalPropNameToFString.GetName(name);
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

