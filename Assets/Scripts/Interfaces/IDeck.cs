using System.Collections.Generic;

public interface IDeck
{
    int amount { get; }

    List<CardStruct> GetCardStruct();
    void SetupGame();

    void Shuffle();
    ICard TakeLast();
}