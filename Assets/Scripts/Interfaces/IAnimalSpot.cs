using System.Collections.Generic;
using UnityEngine;

public interface IAnimalSpot
{
    AnimalStruct animal { get; }
    bool isFreeSpot { get; }
    GameObject gameObject { get; }
    bool isFree { get; }
    public Transform transform { get; }

    public bool AddPropToAnimal(ICard card);
    void Destroy();
    public int Feed(int food, int foodIncrease = 1);
    AnimalSpotStruct GetStruct(int ownerId);
    void Kill();
    void MakeFree();
    void RemoveProp(AnimalProp animalProp);
    void SetLocalId(int i);
    void UpdatePhaseCooldown();
    void UpdateTurnCooldown();
}