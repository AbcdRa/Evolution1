


using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Hand : MonoBehaviour, IHand
{
    private List<ICard> _cards = new();
    public int amount => _cards.Count + (selected!=null?1:0);
    private float CARDS_WIDTH = 0.55f;
    [SerializeField] private int ownerId = -1;
    

    public ICard selected { get; private set; }

    public List<ICard> cards => _cards;

    public HandStruct GetStruct()
    {
        List<CardStruct> cards = new List<CardStruct>();
        foreach (var card in this._cards)
        {
            cards.Add(card.GetStruct());
        }
        return new HandStruct(cards);
    }

    public void InitReset(int ownerId)
    {
        this.ownerId = ownerId;
        _cards = new(6);
    }

    public void TakeCardsFromDeck(IDeck deck, int cardsAmount)
    {
        for (int i = 0; i < cardsAmount; i++)
        {
            if (deck.amount <= 0) return;
            _cards.Add(deck.TakeLast());
        }
        OrganizateCards();
    }

    public void OrganizateCards()
    {
        for (int i = 0; i < _cards.Count; i++) {
            _cards[i].transform.parent = transform;

            _cards[i].transform.GetComponent<SelectionableObject>().SetSpecificationAndId(SOSpecification.HandCard, ownerId);

            Vector3 pos = new(i * CARDS_WIDTH-(_cards.Count*CARDS_WIDTH/2), 0, 0.001f*i);
            _cards[i].transform.localPosition = pos;
            _cards[i].transform.localRotation = Quaternion.Euler(0f,0f,_cards[i].isRotated?180f:0f);
        }
    }

    public void Select(Card selected)
    {
        ICard selectedCard = null;
        foreach(var card in _cards)
        {
            if(card.id == selected.id)
            {
                selectedCard = card;
                break;
            }
        }
        if (selectedCard == null) return;
        _cards.Remove(selectedCard);
        selectedCard.transform.gameObject.SetActive(false);
        //По сути копия кода из UnSelect сдеалано это, чтобы не вызывать два раза Organizate
        if(this.selected != null)
        {
            this.selected.transform.gameObject.SetActive(true);
            _cards.Add(this.selected);
        }
        this.selected = selectedCard;
        OrganizateCards();
        
    }

    public void Unselect()
    {
        if (this.selected != null)
        {
            this.selected.transform.gameObject.SetActive(true);
            _cards.Add(this.selected);
            this.selected = null;
            OrganizateCards();
        }


    }

    public void RemoveCard(ICard card)
    {
        if(selected != null && selected.id == card.id)
        {
            selected = null;
            return;
        }
        for(int i = 0; i < _cards.Count; i++)
        {
            if (_cards[i].id == card.id)
            {
                _cards.Remove(_cards[i]);
                break;
            }
        }
        //TODO НЕТ ПРОВЕРКИ ЧТО УДАЛЕНИЕ ПРОИЗОШЛО УСПЕШНО, МОЖНО ИСПОЛЬЗОВАТЬ ХАК
        OrganizateCards();
    }
}

