using System.Linq;
using Unity.Collections;
using fstring = Unity.Collections.FixedString32Bytes;

public struct MoveNotationMG
{
    public readonly static FixedString32Bytes[] runes = new FixedString32Bytes[]
      { "pass", "play", "newa", "addp", "feed", "sply"};

    public static fstring GetName(in MoveType name)
    {
        //TODO Сложнее но Можно оптимизировать
        return runes[(int)name];
    }
}

public struct MoveData
{
    public MoveType type;
    public int playerId;
    public bool isRotated;
    public CardStruct card;
    public AnimalId target1;
    public AnimalId target2;
    public AnimalProp prop;


    public MoveData(MoveType type, int playerId, in CardStruct card, AnimalProp prop, in AnimalId t1, in AnimalId t2, bool isRotated = false)
    {
        this.type = type;
        this.playerId = playerId;
        this.card = card;
        this.target1 = t1;
        this.target2 = t2;
        this.isRotated = isRotated;
        this.prop = prop;
    }

}


public class VMove
{
    public MoveData data;
    public fstring notation;

    public VMove(in fstring notation, in MoveData data)
    {
        this.notation = notation;
        this.data = data;
    }

    public static void ExecuteMove(IGameMananger gm, in VMove move)
    {

        switch (move.data.type)
        {
            case MoveType.Pass:
                gm.Pass(move.data.playerId); break;
            case MoveType.CreateAnimal:
                ICard card = gm.FindCard(move.data.card, move.data.playerId, AnimalId.NULL);
                gm.CreateAnimal(move.data.playerId, card); break;
            case MoveType.AddPropToAnimal:
                card = gm.FindCard(move.data.card, move.data.playerId, AnimalId.NULL);
                gm.AddPropToAnimal(move.data.playerId, card, move.data.target1, move.data.isRotated); break;
            case MoveType.Feed:
                gm.Feed(move.data.playerId, move.data.target1); break;
            case MoveType.PlayProp:
                card = gm.FindCard(move.data.card, move.data.playerId, move.data.target1);
                gm.PlayProp(move.data.playerId, card, move.data.target1, move.data.target2); break;
        }
    }

    public static VMove GetPassMove(int playerId)
    {
        fstring notation = playerId.ToFString();
        notation.Append(MoveNotationMG.GetName(MoveType.Pass));
        MoveData data = new MoveData(MoveType.Pass, playerId, CardStruct.NULL, AnimalProp.NULL, AnimalId.NULL, AnimalId.NULL);
        return new(notation, data);
    }

    public static VMove GetResponceToAttackMove(int playerId, in AnimalId friendId, in AnimalId enemyId, in AnimalProp prop)
    {
        fstring notation = playerId.ToFString();
        notation.Append(MoveNotationMG.GetName(MoveType.ResponseToAttack));
        //TODO Подробней нотацию расписать
        MoveData data = new MoveData(MoveType.ResponseToAttack, playerId, CardStruct.NULL, prop, friendId, enemyId);
        return new(notation, data );
    }

    public static VMove GetCreateAnimalMove(int playerId, in CardStruct card)
    {
        fstring notation = playerId.ToFString();
        notation.Append(MoveNotationMG.GetName(MoveType.CreateAnimal));
        notation.Append(card.ToFString());
        MoveData data = new MoveData(MoveType.CreateAnimal, playerId, card, AnimalProp.NULL, AnimalId.NULL, AnimalId.NULL);
        return new(notation, data);
    }

    public static VMove GetAddPropMove(int playerId, in CardStruct card, in AnimalId target1, in AnimalId target2, bool isRotated)
    {
        if (playerId != target1.ownerId && !(isRotated ? card.second : card.main).isHostile()) 
            throw new System.Exception("WTF!");
        fstring notation = playerId.ToFString();
        notation.Append(MoveNotationMG.GetName(MoveType.AddPropToAnimal));
        notation.Append(card.ToFString());
        MoveData data = new MoveData(MoveType.AddPropToAnimal, playerId, card, AnimalProp.NULL, target1, target2, isRotated);
        return new(notation, data);
    }


    public static VMove GetFeedMove(int playerId, in AnimalId target)
    {
        fstring notation = playerId.ToFString();
        notation.Append(MoveNotationMG.GetName(MoveType.Feed));
        notation.Append(target.ToFString());
        MoveData data = new MoveData(MoveType.Feed, playerId, CardStruct.NULL, AnimalProp.NULL, target, AnimalId.NULL);
        return new(notation, data);
    }

    public static VMove GetPlayPropMove(int playerId, in AnimalProp prop, in AnimalId target1, in AnimalId target2)
    {
        if (prop.name == AnimalPropName.ERROR)
            throw new System.Exception("WTF Error prop!");
        fstring notation = playerId.ToFString();
        notation.Append(MoveNotationMG.GetName(MoveType.PlayProp));
        notation.Append(prop.ToFString());
        notation.Append('>');
        notation.Append(target1.ToFString());
        notation.Append('|');
        notation.Append(target2.ToFString());
        MoveData data = new MoveData(MoveType.PlayProp, playerId, CardStruct.NULL, prop, target1, target2);
        return new(notation, data);
    }

    public override string ToString()
    {
        return "M{" + notation + "}";
    }
}

 

