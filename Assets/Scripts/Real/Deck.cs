

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Deck : MonoBehaviour, IDeck
{
    private List<ICard> _cards;
    public int amount => _cards.Count;

    public DeckStruct GetStruct()
    {
        List<CardStruct> cardstructs = new List<CardStruct>();
        for (int i = 0; i < _cards.Count; i++) cardstructs.Add(_cards[i].GetStruct());
        return new DeckStruct(cardstructs);
    }

    public void SetupGame()
    {
        CardSO[] cardVariants = PrefabDataSingleton.instance.GetAllCardVariants();
        _cards = new List<ICard>();
        foreach (var cardVariant in cardVariants) { 
            for(int i = 0; i < 4; i++)
            {
                _cards.Add(cardVariant);
            }
        }
        Shuffle();
    }

    public void Shuffle()
    {
        for (int i = 0; i < _cards.Count; i++) {
            _cards[i] = _cards[Random.Range(0, _cards.Count)];
        }
    }

    public ICard TakeLast()
    {
        ICard card = _cards[_cards.Count - 1];
        _cards.RemoveAt(_cards.Count - 1);
        return card;
    }
}

