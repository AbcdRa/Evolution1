public interface IFoodMananger
{
    public int food {  get; }
    public void Consume(int food);
    FoodManangerStruct GetStruct();
    void SetupGame();
    void SpawnFood(int length);
}