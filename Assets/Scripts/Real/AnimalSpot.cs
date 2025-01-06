
using UnityEngine;

public class AnimalSpot : MonoBehaviour, IAnimalSpot
{
    public AnimalStruct animal { get; private set; }
    private int _localId;
    public bool isFreeSpot => animal.isNull;

    public void Destroy()
    {
        throw new System.NotImplementedException();
    }

    public void MakeFree()
    {
        throw new System.NotImplementedException();
    }

    public void SetLocalId(int i)
    {
        _localId = i;
        if(!isFreeSpot) animal.SetLocalId(i);
    }
}

