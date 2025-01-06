using System.Collections.Generic;
using UnityEngine;

public interface IAnimalSpot
{
    AnimalStruct animal { get; }
    bool isFreeSpot { get; }
    GameObject gameObject { get; }

    void Destroy();
    void MakeFree();
    void SetLocalId(int i);
}