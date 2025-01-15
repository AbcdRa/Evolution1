
using System.Collections.Generic;

public interface IPlayerMananger
{
    public IPlayer[] players { get; }
    public int playerAmount { get; }

    bool AddPropToAnimal(int playerId, ICard card, AnimalId target);
    bool CreateAnimal(int playerId, ICard card);
    int Feed(int playerId, AnimalId target, IFoodMananger foodMananger);
    Hands GetHandsStruct(int target, List<CardStruct> deck);
    PlayerSpots GetPlayerSpotStruct();
    void KillById(AnimalId predatorId, AnimalId victimId);
    void Pass(int playerId);
    void ResetPass();
    void SetupGame(IDeck deck, int fIRST_TURN_CARDS_AMOUNT);
    void StartPreDevelopPhase(int currentPivot, IDeck deck);
    void StartSurvivingPhase();
    void UpdatePhaseCooldown();
    void UpdateTurnCooldown();
}