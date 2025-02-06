
using System;
using Unity.Collections;

public struct GameInteractionStruct
{


    ////Возвращает sideTurns при разыгровке свойства 
    ////-2 -> оставить ход у игрока
    ////-1 -> передать следующему по обычным правилам
    ////0-100 -> Насильно передать ход игроку sideTurns
    //public static PairAnimalId PlayProp(in PlayerManangerStruct playerMananger, int playerId, in CardStruct card, 
    //                                    in AnimalId target1, in AnimalId target2, bool isRotated)
    //{
    //    AnimalProp prop = isRotated ? card.second : card.main;
    //    switch(prop.name)
    //    {
    //        case AnimalPropName.Sleep:
    //            return PlaySleep(playerMananger, target1);
    //        case AnimalPropName.Fasciest:
    //            return PlayFasciest(playerMananger, target1);
    //        case AnimalPropName.Piracy:
    //            return PlayPiracy(playerMananger, target1, target2);
    //        case AnimalPropName.Predator:
    //            return PlayPredator(playerMananger, target1, target2);
    //    }
    //    throw new Exception("GameBreaking Rule trying to play a strange prop " + prop.ToFString());
    //}

    //private static PairAnimalId PlaySleep(in PlayerManangerStruct playerMananger, in AnimalId target)
    //{
    //    playerMananger.players[target.ownerId].animalArea.spots[target.localId].animal.ActivateSleepProp();
    //    //TODO именнованым статическим переменным не учили, че за magic number
    //    return PairAnimalId.NOT_DOING_NEXT_TURN;
    //}

    //private static PairAnimalId PlayFasciest(in PlayerManangerStruct playerMananger, in AnimalId target)
    //{
    //    playerMananger.players[target.ownerId].animalArea.spots[target.localId].animal.ActivateFasciestProp();
    //    return PairAnimalId.DESTROY_FOOD;
    //}

    //private static PairAnimalId PlayPiracy(in PlayerManangerStruct playerMananger, in AnimalId pirate, in AnimalId victim)
    //{
    //    //TODO Ну ты хоть бы проверил возможно ли пиратство
    //    //Я знаю что двойные проверки не супер круто, но все таки
    //    playerMananger.players[pirate.ownerId].animalArea.spots[pirate.localId].animal.ActivatePiraceProp();
    //    PairAnimalId sideTurnsInfo = PairAnimalId.PIRACY_FOOD;
    //    sideTurnsInfo.second=victim;
    //    return sideTurnsInfo;
    //}

    //private static PairAnimalId PlayPredator(in PlayerManangerStruct playerMananger, in AnimalId predatorId, in AnimalId victimId)
    //{
    //    PairAnimalId sideTurnsInfo = new(predatorId, victimId);
    //    if (!IsCanAttack(playerMananger.players[predatorId.ownerId].animalArea.spots[predatorId.localId].animal,
    //                     playerMananger.players[victimId.ownerId].animalArea.spots[victimId.localId].animal)) 
    //       throw new Exception("GameBreaking Trying to attack immortal victim");

    //    AnimalPropName victimFlags = playerMananger.players[victimId.ownerId].animalArea.spots[victimId.localId].animal.propFlags;
    //    NativeList<AnimalProp> sideProps = new NativeList<AnimalProp>(3, Allocator.Persistent);
    //    for(int i = 0; i < playerMananger.players[victimId.ownerId].animalArea.spots[victimId.localId].animal.props.singlesLength; i++)
    //    {
    //        AnimalProp prop = playerMananger.players[victimId.ownerId].animalArea.spots[victimId.localId].animal.props.singles[i];
    //        if (!prop.IsActivable) continue;
    //        if(IsSideInteractable(prop.name)) sideProps.Add(prop);
    //    }
    //    if (sideProps.Length == 0)
    //    {
    //        playerMananger.KillById(predatorId, victimId);
    //        sideProps.Dispose();
    //        return PairAnimalId.DOING_NEXT_TURN;
    //    } else
    //    {
    //        sideProps.Dispose();
    //    }
    //    return sideTurnsInfo;
    //}

    private static SideTurnInfo PlaySleep(in IPlayerMananger playerMananger, in AnimalId target)
    {
        playerMananger.players[target.ownerId].animalArea.spots[target.localId].animal.ActivateSleepProp();
        //TODO именнованым статическим переменным не учили, че за magic number
        return SideTurnInfo.GetNotSideRegularInfo();
    }

    private static SideTurnInfo PlayFasciest(in IPlayerMananger playerMananger, in AnimalId target)
    {
        playerMananger.players[target.ownerId].animalArea.spots[target.localId].animal.ActivateFasciestProp();
        return SideTurnInfo.GetNotSideDestroyFood();
    }

    private static SideTurnInfo PlayPiracy(in IPlayerMananger playerMananger, in AnimalId pirate, in AnimalId victim)
    {
        //TODO Ну ты хоть бы проверил возможно ли пиратство
        //Я знаю что двойные проверки не супер круто, но все таки
        playerMananger.players[pirate.ownerId].animalArea.spots[pirate.localId].animal.ActivatePiraceProp();
        var sideTurnsInfo = SideTurnInfo.GetNotSidePiracy(pirate, victim);
        return sideTurnsInfo;
    }

    private static SideTurnInfo PlayPredator(in IPlayerMananger playerMananger, in AnimalId predatorId, in AnimalId victimId)
    {
        if (!IsCanAttack(playerMananger.players[predatorId.ownerId].animalArea.spots[predatorId.localId].animal,
                         playerMananger.players[victimId.ownerId].animalArea.spots[victimId.localId].animal))
            throw new Exception("GameBreaking Trying to attack immortal victim");

        AnimalPropName victimFlags = playerMananger.players[victimId.ownerId].animalArea.spots[victimId.localId].animal.propFlags;
        NativeList<AnimalProp> sideProps = new NativeList<AnimalProp>(3, Allocator.Persistent);
        for (int i = 0; i < playerMananger.players[victimId.ownerId].animalArea.spots[victimId.localId].animal.props.singlesLength; i++)
        {
            AnimalProp prop = playerMananger.players[victimId.ownerId].animalArea.spots[victimId.localId].animal.props.singles[i];
            if (!prop.IsActivable) continue;
            if (IsSideInteractable(prop.name)) sideProps.Add(prop);
        }
        if (sideProps.Length == 0)
        {
            playerMananger.KillById(predatorId, victimId);
            sideProps.Dispose();
            return SideTurnInfo.GetSuccessAttackInfo();
        } else
        {
            sideProps.Dispose();
        }
        return SideTurnInfo.GetWaitingSideInfo(predatorId, victimId);
    }


    public static SideTurnInfo PlayProp(IPlayerMananger playerMananger, int playerId, ICard card, in AnimalId target1, in AnimalId target2, bool isRotated)
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
        if(victim.propFlags.HasFlagFast(AnimalPropName.Camouflage) && !predator.propFlags.HasFlagFast(AnimalPropName.SharpEye)) return false;
        if(victim.propFlags.HasFlagFast(AnimalPropName.Aqua) && !predator.propFlags.HasFlagFast(AnimalPropName.Aqua)) return false;
        if(victim.propFlags.HasFlagFast(AnimalPropName.Big) && !predator.propFlags.HasFlagFast(AnimalPropName.Big)) return false;
        if(victim.propFlags.HasFlagFast(AnimalPropName.RIsSymbiontSlave)) return false;
        if(victim.propFlags.HasFlagFast(AnimalPropName.Borrow) && (victim.food >= victim.maxFood)) return false;
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

