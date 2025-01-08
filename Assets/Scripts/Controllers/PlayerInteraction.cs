
using System;
using UnityEditor.ShaderGraph;
using UnityEngine;


public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private int playerId;
    private PlayerControls controls;
    private SelectionableObject selection;

    [SerializeField] private UIMananger uiMananger;
    private IPlayer player => GameMananger.instance.playerMananger.players[playerId];

    private void OnEnable()
    {
        if (controls == null) controls = new PlayerControls();
        controls.Enable();

        controls.Player.RotateCard.performed += _ => OnRotateCard();
        controls.Player.Interact.performed += _ => OnInteract();
        controls.Player.UnselectCard.performed += _ => OnUnselect();
    }

    private void OnUnselect()
    {
        player.hand.Unselect();
        uiMananger.UpdateSelectedCardUI(player.hand.selected);
    }

    private void OnInteract()
    {
        if (selection == null) return;
        if(selection.specification == SOSpecification.HandCard && selection.ownerId == playerId)
        {
            Card card = selection.parent.GetComponent<Card>();
            InteractWithHandCard(card);
            return;
        }
            
    }

    private void InteractWithHandCard(Card card)
    {
        player.hand.Select(card);
        uiMananger.UpdateSelectedCardUI(player.hand.selected);
    }

    private void OnRotateCard()
    {
        if (selection == null || selection.specification != SOSpecification.HandCard || selection.ownerId != playerId) return;
        selection.parent.GetComponent<Card>().Rotate(); 
    }

    public void Pass()
    {
        GameMananger.instance.Pass(playerId);
    }

    public void HandleSelection(SelectionableObject selection)
    {
        this.selection = selection;
    }
}

