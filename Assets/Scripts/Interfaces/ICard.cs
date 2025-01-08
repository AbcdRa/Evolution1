using UnityEngine;

public interface ICard
{
    public bool isRotated { get;}
    public AnimalProp main { get; }
    public AnimalProp second { get;  }
    public AnimalProp current { get; }
    public Transform transform { get; }
    Sprite sprite { get; }
    public int id { get; }

    CardStruct GetStruct();
    public bool IsSpecial();
    public bool SoftEqual(ICard card);
    public void Rotate();
}