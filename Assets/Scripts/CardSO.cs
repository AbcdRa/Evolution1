using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu()]
public class CardSO : ScriptableObject, ICard
{
    public int ID;
    public string cardName;
    public AnimalPropSO mainPropSO;
    public AnimalPropSO secondPropSO;
    
    public bool isRotated { get; set; }
    public AnimalProp main { get => mainPropSO.BuildProp(null); set => main = value; }
    public AnimalProp second { get => secondPropSO.BuildProp(null); set => second = value; }
    public AnimalProp current => isRotated ? second : main;

    public Transform transform => GetTransform();

    private Transform GetTransform(Transform parent=null)
    {
        Transform cardTemplate = PrefabDataSingleton.instance.GetCardTemplate();
        Transform cardTransform = Instantiate(cardTemplate, parent);
        Card card = cardTransform.GetComponent<Card>();
        card.CreateCard(this);
        return cardTransform;
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
}
