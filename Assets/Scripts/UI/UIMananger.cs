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
    [SerializeField] private Image selectedCard;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private PlayerController playerController;

    public void UpdateSelectedCardUI(ICard card)
    {
        selectedCard.sprite = card.sprite;
    }

    public void UpdateUI()
    {
        playerInfoMananger.UpdateUI(GameMananger.instance);
        currentFoodText.text = GameMananger.instance.foodMananger.food.ToString();
        currentFoodText.text = GameMananger.instance.currentPhase.ToString();
        Canvas.ForceUpdateCanvases();
    }

    public void OnStartButtonPress()
    {
        GameMananger.instance.onNextTurn.AddListener(UpdateUI);
        GameMananger.instance.SetupGame();
        playerController.OnSwitchToHandCardView();
    }
    public void OnPassButtonPress()
    {
        playerInteraction.Pass();
    }
    public void OnDebugButtonPress()
    {
    }

}
