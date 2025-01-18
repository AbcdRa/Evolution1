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
    public void Pass(int playerId);
    public void CreateAnimal(int playerId, ICard card);
    public void AddPropToAnimal(int playerId, ICard card, in AnimalId target1, bool isRotated);
    public void Feed(int playerId, in AnimalId target1);
    public void PlayProp(int playerId, ICard card, in AnimalId target1, in AnimalId target2, bool isRotated = false);
    public ICard FindCard(CardStruct card);
}