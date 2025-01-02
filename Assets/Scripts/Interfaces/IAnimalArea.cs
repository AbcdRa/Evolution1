using System.Collections.Generic;
using UnityEngine;

public interface IAnimalArea
{
    int amount { get; }

    bool AddPropToAnimal(ICard card, int localId, bool isRotated);
    bool CreateAnimal(ICard card);
    int Feed(int localId, IFoodMananger foodMananger);
    void InitReset();
    void StartSurvivingPhase();
    void UpdatePhaseCooldown();
    void UpdateTurnCooldown();
}