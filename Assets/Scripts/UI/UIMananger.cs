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
    [SerializeField] private TextMeshProUGUI currentDeckText;
    [SerializeField] private Image selectedCard;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private PlayerController playerController;

    public void UpdateSelectedCardUI(ICard card)
    {
        if(card == null)
        {
            selectedCard.sprite = null;
            selectedCard.color = new Color(0f, 0f, 0f, 0f);
        } else
        {
            selectedCard.sprite = card.sprite;
            selectedCard.rectTransform.rotation = Quaternion.Euler(0f, 0f, card.isRotated ? 180f : 0f);
            selectedCard.color = new Color(1f, 1f, 1f, 1f);
        }
        
    }

    public void UpdateUI()
    {
        playerInfoMananger.UpdateUI(GameMananger.instance);
        currentFoodText.text = GameMananger.instance.foodMananger.food.ToString();
        currentFoodText.text = GameMananger.instance.currentPhase.ToString();
        currentDeckText.text = GameMananger.instance.deck.amount.ToString();
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
