using TMPro;
using UnityEngine;

public class TextInfoCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI foodText;
    [SerializeField] private TextMeshProUGUI fatText;

    public void UpdateUI(in AnimalStruct animal)
    {
        foodText.text = $"{animal.food}/{animal.maxFood}";
        fatText.text = $"{animal.fat}/{animal.maxFat}";
    }
}