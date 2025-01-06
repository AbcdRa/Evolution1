using UnityEngine;

public interface ICard
{
    public bool isRotated { get; set; }
    public AnimalProp main { get; set; }
    public AnimalProp second { get; set; }
    public AnimalProp current { get; }
    public Transform transform { get; }
    Sprite sprite { get; }

    public bool IsSpecial();
    public bool SoftEqual(ICard card);
}