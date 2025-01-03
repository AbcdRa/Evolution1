
using System;
using Unity.Collections;

public struct GameInteractionStruct
{


    //Возвращает sideTurns при разыгровке свойства 
    //-2 -> оставить ход у игрока
    //-1 -> передать следующему по обычным правилам
    //0-100 -> Насильно передать ход игроку sideTurns
    public static PairAnimalId PlayProp(in PlayerManangerStruct playerMananger, int playerId, in CardStruct card, 
                                        in AnimalId target1, in AnimalId target2, bool isRotated)
    {
        AnimalProp prop = isRotated ? card.second : card.main;
        switch(prop.name)
        {
            case AnimalPropName.Sleep:
                return PlaySleep(playerMananger, target1);
            case AnimalPropName.Fasciest:
                return PlayFasciest(playerMananger, target1);
            case AnimalPropName.Piracy:
                return PlayPiracy(playerMananger, target1, target2);
            case AnimalPropName.Predator:
                return PlayPredator(playerMananger, target1, target2);
        }
        throw new Exception("GameBreaking Rule trying to play a strange prop " + prop);
    }

    private static PairAnimalId PlaySleep(in PlayerManangerStruct playerMananger, in AnimalId target)
    {
        playerMananger.players[target.ownerId].animalArea.spots[target.localId].animal.ActivateSleepProp();
        //TODO именнованым статическим переменным не учили, че за magic number
        return PairAnimalId.NOT_DOING_NEXT_TURN;
    }

    private static PairAnimalId PlayFasciest(in PlayerManangerStruct playerMananger, in AnimalId target)
    {
        playerMananger.players[target.ownerId].animalArea.spots[target.localId].animal.ActivateFasciestProp();
        return PairAnimalId.DESTROY_FOOD;
    }

    private static PairAnimalId PlayPiracy(in PlayerManangerStruct playerMananger, in AnimalId pirate, in AnimalId victim)
    {
        //TODO Ну ты хоть бы проверил возможно ли пиратство
        //Я знаю что двойные проверки не супер круто, но все таки
        playerMananger.players[pirate.ownerId].animalArea.spots[pirate.localId].animal.ActivatePiraceProp();
        PairAnimalId sideTurnsInfo = PairAnimalId.PIRACY_FOOD;
        sideTurnsInfo.second=victim;
        return sideTurnsInfo;
    }

    private static PairAnimalId PlayPredator(in PlayerManangerStruct playerMananger, in AnimalId predatorId, in AnimalId victimId)
    {
        PairAnimalId sideTurnsInfo = new(predatorId, victimId);
        if (!IsCanAttack(playerMananger.players[predatorId.ownerId].animalArea.spots[predatorId.localId].animal,
                         playerMananger.players[victimId.ownerId].animalArea.spots[victimId.localId].animal)) 
           throw new Exception("GameBreaking Trying to attack immortal victim");

        AnimalPropName victimFlags = playerMananger.players[victimId.ownerId].animalArea.spots[victimId.localId].animal.propFlags;
        NativeList<AnimalProp> sideProps = new NativeList<AnimalProp>(3, Allocator.Temp);
        for(int i = 0; i < playerMananger.players[victimId.ownerId].animalArea.spots[victimId.localId].animal.singleProps.Length; i++)
        {
            AnimalProp prop = playerMananger.players[victimId.ownerId].animalArea.spots[victimId.localId].animal.singleProps[i];
            if (!prop.IsActivable) continue;
            if(IsSideInteractable(prop.name)) sideProps.Add(prop);
        }
        if (sideProps.Length == 0)
        {
            playerMananger.KillById(predatorId, victimId);
            return PairAnimalId.DOING_NEXT_TURN;
        }
        return sideTurnsInfo;
    }

    private static PairAnimalId PlaySleep(in IPlayerMananger playerMananger, in AnimalId target)
    {
        playerMananger.players[target.ownerId].animalArea.spots[target.localId].animal.ActivateSleepProp();
        //TODO именнованым статическим переменным не учили, че за magic number
        return PairAnimalId.NOT_DOING_NEXT_TURN;
    }

    private static PairAnimalId PlayFasciest(in IPlayerMananger playerMananger, in AnimalId target)
    {
        playerMananger.players[target.ownerId].animalArea.spots[target.localId].animal.ActivateFasciestProp();
        return PairAnimalId.DESTROY_FOOD;
    }

    private static PairAnimalId PlayPiracy(in IPlayerMananger playerMananger, in AnimalId pirate, in AnimalId victim)
    {
        //TODO Ну ты хоть бы проверил возможно ли пиратство
        //Я знаю что двойные проверки не супер круто, но все таки
        playerMananger.players[pirate.ownerId].animalArea.spots[pirate.localId].animal.ActivatePiraceProp();
        PairAnimalId sideTurnsInfo = PairAnimalId.PIRACY_FOOD;
        sideTurnsInfo.second = victim;
        return sideTurnsInfo;
    }

    private static PairAnimalId PlayPredator(in IPlayerMananger playerMananger, in AnimalId predatorId, in AnimalId victimId)
    {
        PairAnimalId sideTurnsInfo = new(predatorId, victimId);
        if (!IsCanAttack(playerMananger.players[predatorId.ownerId].animalArea.spots[predatorId.localId].animal,
                         playerMananger.players[victimId.ownerId].animalArea.spots[victimId.localId].animal))
            throw new Exception("GameBreaking Trying to attack immortal victim");

        AnimalPropName victimFlags = playerMananger.players[victimId.ownerId].animalArea.spots[victimId.localId].animal.propFlags;
        NativeList<AnimalProp> sideProps = new NativeList<AnimalProp>(3, Allocator.Temp);
        for (int i = 0; i < playerMananger.players[victimId.ownerId].animalArea.spots[victimId.localId].animal.singleProps.Length; i++)
        {
            AnimalProp prop = playerMananger.players[victimId.ownerId].animalArea.spots[victimId.localId].animal.singleProps[i];
            if (!prop.IsActivable) continue;
            if (IsSideInteractable(prop.name)) sideProps.Add(prop);
        }
        if (sideProps.Length == 0)
        {
            playerMananger.KillById(predatorId, victimId);
            return PairAnimalId.DOING_NEXT_TURN;
        }
        return sideTurnsInfo;
    }


    public static PairAnimalId PlayProp(IPlayerMananger playerMananger, int playerId, ICard card, in AnimalId target1, in AnimalId target2, bool isRotated)
    {
        AnimalProp prop = isRotated ? card.second : card.main;
        switch (prop.name)
        {
            case AnimalPropName.Sleep:
                return PlaySleep(playerMananger, target1);
            case AnimalPropName.Fasciest:
                return PlayFasciest(playerMananger, target1);
            case AnimalPropName.Piracy:
                return PlayPiracy(playerMananger, target1, target2);
            case AnimalPropName.Predator:
                return PlayPredator(playerMananger, target1, target2);
        }
        throw new Exception("GameBreaking Rule trying to play a strange prop " + prop);
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

