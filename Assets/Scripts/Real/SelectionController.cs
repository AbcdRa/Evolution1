using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public class SelectionController : MonoBehaviour
{
    [SerializeField] private LayerMask layerMaskSelectable;
    //[SerializeField] private Dice dice;
    private PlayerMananger playerMananger;
    private SelectionableObject selectionableObject;

    private void Start()
    {
        playerMananger = GameMananger.instance.GetRawPlayerMananger();
    }

    private void UpdateSelectionableObject(SelectionableObject selectionableObject)
    {
        SafeSelectChange(selectionableObject, true);
        if (selectionableObject != this.selectionableObject)
        {
            SafeSelectChange(this.selectionableObject, false);
            this.selectionableObject = selectionableObject;
            SafeSelectChange(this.selectionableObject, true);

        }
    }

    private void SafeSelectChange(SelectionableObject selectionableObject, bool isSelected)
    {
        if (selectionableObject != null)
        {
            if (isSelected)
            {
                selectionableObject.Selection();
            }
            else
            {
                selectionableObject.UnSelection();
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            playerMananger.GetInterectableRawPlayer().UnselectCard();
        }
        Ray userRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(userRay, out RaycastHit hitInfo, 10000f, layerMaskSelectable))
        {
            SelectController(hitInfo);
        }
        else
        {
            SafeSelectChange(selectionableObject, false);
        }
    }


    private void DeckSelectController(Deck deck, SelectionableObject so)
    {
        UpdateSelectionableObject(so);
    }


    private void CardController(Card card, SelectionableObject so)
    {
        
        if (so.GetSpecification() == SelectionableObjectSpecification.HandCard)
        {
            if (so.idOwner != playerMananger.GetInteractablePlayer().id) return;
            UpdateSelectionableObject(so);
            if (Input.GetKeyDown(KeyCode.F))
                card.SwitchCard();
            if (Input.GetKeyDown(KeyCode.R))
                card.RotateCard();
            if (Input.GetMouseButtonDown(0))
                playerMananger.GetInterectableRawPlayer().SelectCard(card);
            
        } else if(so.GetSpecification() == SelectionableObjectSpecification.PropCard)
        {
            UpdateSelectionableObject(so);
            if (Input.GetMouseButton(0))
                GameMananger.instance.InteractProp(playerMananger.GetInteractablePlayer(), card);
            
        }
    }


    private void SelectController(RaycastHit hitInfo)
    {

        if (hitInfo.transform.TryGetComponent(out SelectionableObject selectionableObject))
        {
            //UpdateSelectionableObject(selectionableObject);
            if (selectionableObject.GetLogicParent().TryGetComponent(out Deck deck))
                DeckSelectController(deck, selectionableObject);
 
            else if (selectionableObject.GetLogicParent().TryGetComponent<Card>(out Card card))
                CardController(card, selectionableObject);
            else if (selectionableObject.GetLogicParent().TryGetComponent<AnimalSpot>(out AnimalSpot animalSpot))
            {
                UpdateSelectionableObject(selectionableObject);
                if (Input.GetMouseButtonDown(0))
                {
                    playerMananger.GetInterectableRawPlayer().AddCardToSpot(animalSpot);
                }


            }
            else if (selectionableObject.GetLogicParent().TryGetComponent<FoodMananger>(out FoodMananger foodMananger))
            {
                UpdateSelectionableObject(selectionableObject);
                if (Input.GetMouseButton(0))
                {
                    playerMananger.GetInterectableRawPlayer().InteractWithFood(foodMananger);
                }
            }
            else SafeSelectChange(this.selectionableObject, false);
            
        }

    }



}
