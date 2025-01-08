using TMPro;
using UnityEngine;

public class AnimalInfoDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI foodText;
    [SerializeField] private TextMeshProUGUI fatText;

    public void UpdateUI(in AnimalStruct animal)
    {
        foodText.text = $"{animal.food}/{animal.maxFood}";
        fatText.text = $"{animal.fat}/{animal.maxFat}";
    }

    public static AnimalInfoDisplay CreateInfoDisplay(ICard card)
    {
        var animalInfo = Instantiate(PrefabDataSingleton.instance.GetAnimalInfoCardPrefab(), card.transform.GetChild(0));
        animalInfo.transform.localPosition = Vector3.zero;
        animalInfo.transform.localRotation = Quaternion.Euler(0f,0f,180f);
        return animalInfo; 
    }
}