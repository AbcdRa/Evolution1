

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Deck : MonoBehaviour, IDeck
{
    private List<CardSO> _cards;
    public int amount => _cards.Count;

    public List<CardStruct> GetCardStruct()
    {
        List<CardStruct> cards = new(_cards.Count);
        for(int i = 0; i < _cards.Count; i++)
        {
            cards.Add(_cards[i].GetStruct());
        }
        return cards;
    }


    public void SetupGame()
    {
        CardSO[] cardVariants = PrefabDataSingleton.instance.GetAllCardVariants();
        _cards = new List<CardSO>(80);
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
        _cards = DevExtension.Shuffle(_cards, GameMananger.rng);
    }

    public ICard TakeLast()
    {
        ICard card = _cards[_cards.Count - 1].CreateCard();
        _cards.RemoveAt(_cards.Count - 1);
        return card;
    }
}

