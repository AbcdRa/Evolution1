using UnityEngine;


[CreateAssetMenu()]
public class AnimalPropSO : ScriptableObject
{
    public AnimalPropName PropName;
    public int HungerIncrease;
    public bool IsPair;

    public AnimalProp BuildProp()
    {
        AnimalProp animalPropStruct = new AnimalProp();
        return animalPropStruct;
    }
}
