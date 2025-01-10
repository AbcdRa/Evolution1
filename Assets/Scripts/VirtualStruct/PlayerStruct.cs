
using System;
using Unity.Burst;
using fstring = Unity.Collections.FixedString32Bytes;


[BurstCompile(DisableDirectCall = true)]
public struct PlayerStruct
{
    public bool isAbleToMove;
    public AnimalAreaStruct animalArea;
    public HandStruct hand;
    public int id;

    public PlayerStruct(int id, HandStruct hand, AnimalAreaStruct animalArea, bool isAbleToMove)
    {
        this.id = id;
        this.hand = hand;
        this.animalArea = animalArea;
        this.isAbleToMove = isAbleToMove;
    }

    internal bool AddPropToAnimal(in CardStruct card, in AnimalId target, bool isRotated)
    {
        return animalArea.spots[target.localId].AddPropToAnimal(card,isRotated);
    }

    public bool CreateAnimal(in CardStruct card)
    {
        return animalArea.CreateAnimal(card);
    }

    internal void GetCardsFromDeck(ref DeckStruct deck, int cardAmount)
    {
        hand.TakeCardsFromDeck(ref deck, cardAmount);
    }

    public int Feed(in AnimalId target, in FoodManangerStruct foodMananger)
    {
        if(target.ownerId != id) 
            return 0;

        return animalArea.Feed(target.localId, foodMananger);
    }

    internal int GetScore()
    {
        return animalArea.GetScore();
    }

    internal void Pass()
    {
        isAbleToMove = false;
    }

    internal void ResetPass()
    {
        isAbleToMove = true;
    }

    internal void UpdatPhaseCooldown()
    {
        animalArea.UpdatePhaseCooldown();
    }

    internal void UpdatTurnCooldown()
    {
        animalArea.UpdateTurnCooldown();
    }

    public PropId GetNextInteractablPropId(in PropId prv)
    {
        for(int i = prv.spotlId; i < animalArea.amount; i++)
        {
            for (int j = prv.proplId; j < animalArea.spots[i].animal.props.singlesLength; j++)
            {
                bool isInteractable = animalArea.spots[i].animal.props.singles[j].IsInteractable();
                if(isInteractable) return new PropId(i, j);
            }
        }
        return PropId.NULL;
    }
}

