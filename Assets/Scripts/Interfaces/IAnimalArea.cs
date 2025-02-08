using System.Collections.Generic;
using UnityEngine;

public interface IAnimalArea
{
    int amount { get; }
    List<IAnimalSpot> spots { get; }

    bool AddPropToAnimal(ICard card, int localId);
    bool CreateAnimal(ICard card);
    int Feed(int localId, IFoodMananger foodMananger, int foodIncrease = 1);
    void InitReset();
    void Kill(int localId);
    void OrganizateSpots();
    void StartSurvivingPhase();
    void UpdatePhaseCooldown();
    void UpdateTurnCooldown();
}