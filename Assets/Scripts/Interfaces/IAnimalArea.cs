using System.Collections.Generic;
using UnityEngine;

public interface IAnimalArea
{
    int amount { get; }
    List<IAnimalSpot> spots { get; }

    bool AddPropToAnimal(ICard card, int localId);
    bool CreateAnimal(ICard card);
    int Feed(int localId, IFoodMananger foodMananger);
    void InitReset();
    void StartSurvivingPhase();
    void UpdatePhaseCooldown();
    void UpdateTurnCooldown();
}