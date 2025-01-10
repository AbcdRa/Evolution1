using System;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using fstring = Unity.Collections.FixedString32Bytes;

public enum MoveType { Pass, PlayProp, CreateAnimal, AddPropToAnimal, Feed, ResponseToAttack}
[BurstCompile(DisableDirectCall = true)]
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

[BurstCompile(DisableDirectCall = true)]
public struct MoveData
{
    public MoveType type;
    public int playerId;
    public bool isRotated;
    public CardStruct card;
    public AnimalId target1;
    public AnimalId target2;
    public AnimalProp prop;


    public MoveData(MoveType type, int playerId, in CardStruct card, AnimalProp prop, in AnimalId t1, in AnimalId t2, bool isRotated=false)
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

[BurstCompile(DisableDirectCall = true)]
public struct MoveStruct
{
    public float rating;
    public MoveData data;
    public fstring notation;

    public MoveStruct(in fstring notation, in MoveData data, float rating)
    {
        this.notation = notation;
        this.data = data;
        this.rating = rating;
    }

    public static void ExecuteMove(ref VGMstruct vgm, in MoveStruct move)
    {
        switch(move.data.type)
        {
            case MoveType.Pass:
                vgm.Pass(move.data.playerId); break;
            case MoveType.CreateAnimal:
                vgm.CreateAnimal(move.data.playerId, move.data.card); break;
            case MoveType.AddPropToAnimal:
                vgm.AddPropToAnimal(move.data.playerId, move.data.card, move.data.target1, move.data.isRotated); break;
            case MoveType.Feed:
                vgm.Feed(move.data.playerId, move.data.target1); break;
            case MoveType.PlayProp:
                vgm.PlayProp(move.data.playerId, move.data.card, move.data.target1, move.data.target2); break;
            case MoveType.ResponseToAttack:
                vgm.PlaySideProp(move.data.playerId, move.data.prop, move.data.target1, move.data.target2); break;
        }
    }


    public static void ExecuteMove(IGameMananger gm, in MoveStruct move)
    {
        switch (move.data.type)
        {
            case MoveType.Pass:
                gm.Pass(move.data.playerId); break;
            case MoveType.CreateAnimal:
                gm.CreateAnimal(move.data.playerId, move.data.card); break;
            case MoveType.AddPropToAnimal:
                gm.AddPropToAnimal(move.data.playerId, move.data.card, move.data.target1, move.data.isRotated); break;
            case MoveType.Feed:
                gm.Feed(move.data.playerId, move.data.target1); break;
            case MoveType.PlayProp:
                gm.PlayProp(move.data.playerId, move.data.card, move.data.target1, move.data.target2); break;
        }
    }

    public static MoveStruct GetPassMove(int playerId)
    {
        fstring notation = playerId.ToFString();
        notation.Append(MoveNotationMG.GetName(MoveType.Pass));
        MoveData data = new MoveData(MoveType.Pass, playerId, CardStruct.NULL, AnimalProp.NULL, AnimalId.NULL, AnimalId.NULL);
        MoveStruct move = new MoveStruct(notation, data, 0f);
        return move;
    }

    public static MoveStruct GetResponceToAttackMove(int playerId, in AnimalId friendId, in AnimalId enemyId, in AnimalProp prop)
    {
        fstring notation = playerId.ToFString();
        notation.Append(MoveNotationMG.GetName(MoveType.ResponseToAttack));
        //TODO Подробней нотацию расписать
        MoveData data = new MoveData(MoveType.ResponseToAttack, playerId, CardStruct.NULL, prop, friendId, enemyId);
        MoveStruct move = new MoveStruct(notation, data, 0f);
        return move;
    }

    public static MoveStruct GetCreateAnimalMove(int playerId, in CardStruct card)
    {
        fstring notation = playerId.ToFString();
        notation.Append(MoveNotationMG.GetName(MoveType.CreateAnimal));
        notation.Append(card.ToFString());
        MoveData data = new MoveData(MoveType.CreateAnimal, playerId, card, AnimalProp.NULL, AnimalId.NULL, AnimalId.NULL);
        MoveStruct move = new MoveStruct(notation, data, 0f);
        return move;
    }

    public static MoveStruct GetAddPropMove(int playerId, in CardStruct card, in AnimalId target1, in AnimalId target2, bool isRotated)
    {
        fstring notation = playerId.ToFString();
        notation.Append(MoveNotationMG.GetName(MoveType.AddPropToAnimal));
        notation.Append(card.ToFString());
        MoveData data = new MoveData(MoveType.AddPropToAnimal, playerId, card, AnimalProp.NULL, target1, target2, isRotated);
        MoveStruct move = new MoveStruct(notation, data, 0f);
        return move;
    }


    public static MoveStruct GetFeedMove(int playerId, in AnimalId target)
    {
        fstring notation = playerId.ToFString();
        notation.Append(MoveNotationMG.GetName(MoveType.Feed));
        notation.Append(target.ToFString());
        MoveData data = new MoveData(MoveType.Feed, playerId, CardStruct.NULL, AnimalProp.NULL, target, AnimalId.NULL);
        MoveStruct move = new MoveStruct(notation, data, 0f);
        return move;
    }

    public static MoveStruct GetPlayPropMove(int playerId, in AnimalProp prop, in AnimalId target1, in AnimalId target2)
    {
        fstring notation = playerId.ToFString();
        notation.Append(MoveNotationMG.GetName(MoveType.PlayProp));
        notation.Append(prop.ToFString());
        notation.Append('>');
        notation.Append(target1.ToFString());
        notation.Append('|');
        notation.Append(target2.ToFString());
        MoveData data = new MoveData(MoveType.PlayProp, playerId, CardStruct.NULL, prop, target1, target2);
        MoveStruct move = new MoveStruct(notation, data, 0f);
        return move;
    }
}
