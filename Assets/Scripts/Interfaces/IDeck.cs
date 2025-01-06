using System.Collections.Generic;

public interface IDeck
{
    int amount { get; }

    DeckStruct GetStruct();
    void SetupGame();

    void Shuffle();
    ICard TakeLast();
}