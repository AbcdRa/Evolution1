using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerInfo : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI animalAmountText;
    [SerializeField] private TextMeshProUGUI handCardsAmountText;
    [SerializeField] private TextMeshProUGUI statusText;

    internal void UpdateUI(IGameMananger gm, IPlayer player)
    {
        playerNameText.text = "Player" + player.id;
        animalAmountText.text = player.animalArea.amount.ToString();
        handCardsAmountText.text = player.hand.amount.ToString();
        statusText.text = GetPlayerStatus(gm, player);
        Color bgColor = player.isAbleToMove ? ColorPalette.dark : ColorPalette.neon;
        bgColor = player.id == gm.currentTurn ? ColorPalette.green : bgColor;
        background.color = bgColor;
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
