using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerInfo : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI animalAmountText;
    [SerializeField] private TextMeshProUGUI handCardsAmountText;
    [SerializeField] private TextMeshProUGUI statusText;

    public Color bgColor;
    private float smoothTransitionTime = 1f;

    internal void UpdateUI(IGameMananger gm, IPlayer player)
    {
        if (background == null)
        {
            Debug.LogError("Background is not assigned!");
            return;
        }

        playerNameText.text = "Player" + player.id;
        animalAmountText.text = player.animalArea.amount.ToString();
        handCardsAmountText.text = player.hand.amount.ToString();
        statusText.text = GetPlayerStatus(gm, player);
        Color bgColor;
        if (gm.currentTurn == player.id)
        {
            bgColor = ColorPallette.green;
        }
        else if (player.isAbleToMove)
        {
            bgColor = ColorPallette.dark;
        }
        else {
            bgColor = ColorPallette.neon;
        }
        if(bgColor != this.bgColor)
        {
            StartCoroutine(SmoothMoveToPoint(bgColor));
           
        }


    }

    private IEnumerator SmoothMoveToPoint(Color targetColor)
    {
        float elapsedTime = 0f;
        Color startColor = background.GetComponent<Image>().color;

        while (elapsedTime < smoothTransitionTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / smoothTransitionTime;
            background.color = Color.Lerp(startColor, targetColor, t);
            Canvas.ForceUpdateCanvases();
            yield return null;
        }
        this.bgColor = targetColor;
        background.color = targetColor;

    }



    public string GetPlayerStatus(IGameMananger gm, IPlayer player)
    {
        if (!player.isAbleToMove) return "Пасс";
        int currentTurn = (int)(gm.turnInfo >> 32);
        int currentSideTurn = (int)(gm.turnInfo);
        if (currentSideTurn == -1)
        {
            return player.id == gm.currentTurn ? "Текущий ход" : "Ожидает ход";
        }
        else
        {
            return player.id == currentSideTurn ? "Отвечает на атаку" : player.id == currentTurn ? "Текущий ход" : "Ожидает ход";
        }
    }
}
