using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Image selectedCardImage;
    [SerializeField] private Transform textTransform;
    [SerializeField] private TextMeshProUGUI textCurrentPlayer;
    [SerializeField] private TextMeshProUGUI logInfoText;
    [SerializeField] private Button startButton;
    private PlayerMananger playerMananger;

    private void Start()
    {
        playerMananger = GameMananger.instance.GetRawPlayerMananger();
    }


    private void Update()
    {
        textCurrentPlayer.text = $"Текущий игрок: {playerMananger.GetInteractablePlayer().id}";
    }

    public void OnPassButtonClicked()
    {
        playerMananger.GetInteractablePlayer().Pass(GameMananger.instance);
    }

    public void OnStartGameButtonClicked()
    {
        GameMananger.instance.SetuptFirstTurnGame();
        startButton.GetComponent<Button>().interactable = false;

    }
    public void UpdateSelectedCardImage()
    {
        UpdateSelectedCardImage(playerMananger.GetInterectableRawPlayer().GetSelectedRawCard());
    }


    public void UpdateSelectedCardImage(Card selectedCard)
    {
        if (selectedCard == null)
        {
            selectedCardImage.enabled = false;
            return;
        }
        selectedCardImage.GetComponent<RectTransform>().localRotation = Quaternion.identity;
        if (selectedCard.baseCard.isRotated)
        {

            selectedCardImage.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 180f);
        }
        selectedCardImage.sprite = selectedCard.GetImage().sprite;
        selectedCardImage.enabled = true;
    }

    public void SetText(string text, bool isError = false)
    {
        if (string.IsNullOrEmpty(text)) return; 
        TextMeshProUGUI uiText = textTransform.GetComponent<TextMeshProUGUI>();

        uiText.text = text;
        uiText.color = isError ? Color.red : Color.white;
    }

    internal void UpdateLogInfo(GameMananger gameMananger)
    {
        List<IPlayer> activePlayers = gameMananger.GetRawPlayerMananger().GetActivePlayers();
        string activePlayersInfo = "";
        for (int i = 0; i < activePlayers.Count; i++) {
            activePlayersInfo += " " + activePlayers[i].id ;
        }
        logInfoText.text = $"Текущая фаза: {gameMananger.currentPhase}\r\n" +
            $"Ждем хода игрока: {gameMananger.GetCurrentPlayer().id}\r\n" +
            $"Aктивные игроки: {activePlayersInfo}\r\n" +
            $"Еда: {gameMananger.foodMananger.food}";
    }

    public void MakeVirtualTurn()
    {
        GameMananger.instance.GetCurrentPlayer().MakeVirtualTurn();
    }
}
