public interface IFoodMananger
{
    public int food {  get; }
    public void Consume(int food);
    void SetupGame();
    void SpawnFood(int length);
}