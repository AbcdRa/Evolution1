
using System;

public struct GameInteractionStruct
{


    //Возвращает sideTurns при разыгровке свойства 
    //-2 -> оставить ход у игрока
    //-1 -> передать следующему по обычным правилам
    //0-100 -> Насильно передать ход игроку sideTurns
    public static int PlayProp(in PlayerManangerStruct playerMananger, int playerId, in CardStruct card, in AnimalId target1, in AnimalId target2, bool isRotated)
    {
        AnimalProp
    }

    public static int PlayProp(IPlayerMananger playerMananger, int playerId, ICard card, in AnimalId target1, in AnimalId target2, bool isRotated)
    {
        throw new NotImplementedException();
    }

    public static bool IsCanAttack(in AnimalStruct predator, in AnimalStruct victim)
    {
        if(predator.isFull()) return false;
        if(victim.propFlags.HasFlag(AnimalPropName.Camouflage) && !predator.propFlags.HasFlag(AnimalPropName.SharpEye)) return false;
        if(victim.propFlags.HasFlag(AnimalPropName.Aqua) && !predator.propFlags.HasFlag(AnimalPropName.Aqua)) return false;
        if(victim.propFlags.HasFlag(AnimalPropName.Big) && !predator.propFlags.HasFlag(AnimalPropName.Big)) return false;
        if(victim.propFlags.HasFlag(AnimalPropName.RIsSymbiontSlave)) return false;
        if(victim.propFlags.HasFlag(AnimalPropName.Borrow) && (victim.food >= victim.maxFood)) return false;
        return true;

    }

    public static bool IsInteractable(in AnimalPropName name)
    {
        switch(name)
        {
            case AnimalPropName.Predator: return true;
            case AnimalPropName.Sleep: return true;
            case AnimalPropName.Piracy: return true;
            case AnimalPropName.Fasciest: return true;
            default: return false;
        }
    }

    public static bool IsSideInteractable(in AnimalPropName name)
    {
        switch (name)
        {
            case AnimalPropName.Fast: return true;
            case AnimalPropName.Mimic: return true;
            case AnimalPropName.DropTail: return true;
            default: return false;
        }
    }
}

