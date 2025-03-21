using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu()]
public class CardSO : ScriptableObject, ICard
{
    [SerializeField] private int _id;
    public string cardName;
    public AnimalPropSO mainPropSO;
    public AnimalPropSO secondPropSO;

    public int id => _id;
    public bool isRotated { get; set; }
    public AnimalProp main { get => mainPropSO.BuildProp(); }
    public AnimalProp second { get => secondPropSO==null ? AnimalProp.NULL : secondPropSO.BuildProp(); }
    public AnimalProp current => isRotated ? second : main;

    public Transform transform => GetTransform();

    public Sprite sprite => PrefabDataSingleton.instance.GetFaces()[id];


    private Transform GetTransform()
    {
        return CreateCard().transform;
    }

    public Card CreateCard(Transform parent=null)
    {
        Transform cardTemplate = PrefabDataSingleton.instance.GetCardTemplate();
        Transform cardTransform = Instantiate(cardTemplate, parent);
        Card card = cardTransform.GetComponent<Card>();
        card.CreateCard(this);
        return card;
    }

    public bool IsSpecial()
    {
        return false;
    }

    public bool SoftEqual(ICard card)
    {
        return card.main.name == main.name && card.second.name == second.name;
    }

    public override string ToString()
    {
        return $"soc{(isRotated ? 'R' : "")}[{main}{(second.isNull() ? null : "/" + second)}]";
    }

    public CardStruct GetStruct()
    {
        return new CardStruct(main, second, isRotated, id);
    }

    public void Rotate()
    {
        isRotated = !isRotated;
    }
}
