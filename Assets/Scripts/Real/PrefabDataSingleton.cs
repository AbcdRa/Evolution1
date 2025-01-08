using System;
using UnityEngine;

public class PrefabDataSingleton : MonoBehaviour
{
    [SerializeField] private Sprite[] faces;
    [SerializeField] private Transform cardTemplate;
    [SerializeField] private DrawArc drawArcPrefab;
    [SerializeField] private AnimalInfoDisplay animalInfoCardPrefab;
    [SerializeField] private AnimalPropSO errorPropSO;
    [SerializeField] private AnimalSpot animalSpotPrefab;
    [SerializeField] private CardSO[] cardsVariants;
    private AnimalProp errorProp;
    public static PrefabDataSingleton instance;

    public CardSO[] GetAllCardVariants() {  return cardsVariants; }

    public AnimalSpot GetAnimalSpotPrefab() { return animalSpotPrefab; }

    public AnimalProp GetErrorAnimalProp() {  return errorProp; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("SINGLETON DESTROYED");
        }
        instance = this;
        instance.errorProp = errorPropSO.BuildProp();
    }

    public static PrefabDataSingleton GetInstance()
    {
        return instance;
    }

    public Sprite[] GetFaces() { return faces; }
    public Transform GetCardTemplate() { return cardTemplate; }

    public DrawArc GetArcWithArrowsPrefab() { return drawArcPrefab; }

    public AnimalInfoDisplay GetAnimalInfoCardPrefab() { return animalInfoCardPrefab; }

    internal Sprite GetSpriteForCardSO(CardSO cardSO)
    {
        return cardsVariants[cardSO.id].sprite;
    }
}
