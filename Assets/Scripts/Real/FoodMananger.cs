
using UnityEngine;

public class FoodMananger : MonoBehaviour, IFoodMananger
{
    private int _food = 0;
    public int food => _food;

    public void Consume(int foodDecrease)
    {
        if (foodDecrease > _food) throw new System.Exception("GameBreaking Rule");
        _food -= foodDecrease;
    }

    public FoodManangerStruct GetStruct()
    {
        return new FoodManangerStruct(food);
    }

    public void SetupGame()
    {
        _food = 0;
    }

    public void SpawnFood(int playersAmount)
    {
        if(playersAmount == 4)
        {
            _food = Random.Range(1, 7) + Random.Range(1, 7) + 2;
            return;
        }
        throw new System.Exception("TODO Exception");
    }
}

