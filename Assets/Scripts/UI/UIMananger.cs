using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMananger: MonoBehaviour
{
    [SerializeField] private UIPlayerInfoMananger playerInfoMananger;
    [SerializeField] private TextMeshProUGUI currentPhaseText;
    [SerializeField] private TextMeshProUGUI currentFoodText;

    private void Awake()
    {
        GameMananger.instance.onNextTurn.AddListener(UpdateUI);
    }

    public void UpdateUI()
    {
        playerInfoMananger.UpdateUI();
        currentFoodText.text = GameMananger.instance.foodMananger.food.ToString();
        currentFoodText.text = GameMananger.instance.currentPhase.ToString();
    }

    public void OnStartButtonPress()
    {

    }
    public void OnPassButtonPress()
    {

    }
    public void OnDebugButtonPress()
    {

    }

}
