
public interface IPlayerMananger
{
    public IPlayer[] players { get; }
    public int playerAmount { get; }

    bool AddPropToAnimal(int playerId, ICard card, AnimalId target, bool isRotated);
    bool CreateAnimal(int playerId, ICard card);
    int Feed(int playerId, AnimalId target, IFoodMananger foodMananger);
    public IPlayer interactablePlayer { get; }
    void KillById(AnimalId predatorId, AnimalId victimId);
    void Pass(int playerId);
    void ResetPass();
    void SetupGame(IDeck deck, int fIRST_TURN_CARDS_AMOUNT);
    void StartPreDevelopPhase(int currentPivot, IDeck deck);
    void StartSurvivingPhase();
    void UpdatePhaseCooldown();
    void UpdateTurnCooldown();
}