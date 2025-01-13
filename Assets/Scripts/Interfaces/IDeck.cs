using System.Collections.Generic;

public interface IDeck
{
    int amount { get; }

    List<CardStruct> GetCardStruct();
    DeckStruct GetStruct();
    void SetupGame();

    void Shuffle();
    ICard TakeLast();
}