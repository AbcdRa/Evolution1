using System.Collections.Generic;

public interface IGameMananger
{
    public IPlayerMananger playerMananger { get; }
    public IDeck deck { get;  }
    public IFoodMananger foodMananger { get;  }
    public int currentPivot { get; }
    public int currentPhase { get;  }
    public int currentTurn { get; }
    public long turnInfo { get; }
    public bool isOver {  get; }
    void Pass(int playerId);
    void CreateAnimal(int playerId, ICard card);
    void AddPropToAnimal(int playerId, ICard card, in AnimalId target1, bool isRotated);
    void Feed(int playerId, in AnimalId target1);
    void PlayProp(int playerId, ICard card, in AnimalId target1, in AnimalId target2, bool isRotated = false);
}