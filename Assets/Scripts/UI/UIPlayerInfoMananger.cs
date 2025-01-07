using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerInfoMananger: MonoBehaviour
{
    [SerializeField] private List<UIPlayerInfo> playerInfos;

    internal void UpdateUI(IGameMananger gm)
    {
        for (int i = 0; i < gm.playerMananger.playerAmount; i++) {
            playerInfos[i].UpdateUI(GameMananger.instance, gm.playerMananger.players[i]);
        }
        Canvas.ForceUpdateCanvases();
    }
}
