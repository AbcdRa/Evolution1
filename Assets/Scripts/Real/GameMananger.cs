using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GameMananger : MonoBehaviour, IGameMananger
{
    public static GameMananger instance { get; private set; }
    public UnityEvent onNextTurn;

    const int FIRST_TURN_CARDS_AMOUNT = 8;

    [SerializeField] private IPlayerMananger _playerMananger;
    [SerializeField] private IFoodMananger _foodMananger;
    [SerializeField] private IDeck _deck;

    public IPlayerMananger playerMananger => _playerMananger;
    public IDeck deck => _deck;
    public IFoodMananger foodMananger => _foodMananger;
    public int currentPivot { get; private set; }
    public int currentPhase { get; private set; }
    public int currentTurn => _currentSideTurn == -1 ? _currentTurn : _currentSideTurn;
    public IPlayer currentPlayer => playerMananger.players[currentTurn];
    public bool isOver => currentPhase == -1;

    private int _currentTurn;
    private int _currentSideTurn;
    private PairAnimalId _sideTurnsInfo;

    private void Awake()
    {
        if (instance != null) throw new System.Exception("НАРУШЕНИЕ СИНГЛТОНА");
        instance = this;
    }


    public void SetupGame()
    {
        currentPivot = 0;
        _currentSideTurn = -1;
        _currentTurn = 0;
        currentPhase = 0;
        foodMananger.SetupGame();
        deck.SetupGame();
        playerMananger.SetupGame(deck, FIRST_TURN_CARDS_AMOUNT);
    }


    public void NextTurn(int sideTurn = -1)
    {
        if (sideTurn != -1)
        {
            _currentSideTurn = sideTurn;
            onNextTurn.Invoke();
            return;
        }
        if (_currentSideTurn != -1)
        {
            _currentSideTurn = -1;
            onNextTurn.Invoke();
            return;
        }
        int nextTurn = FindNextTurn();
        if (nextTurn == -1) NextPhase();
        else
        {
            if (currentPhase == 1 && nextTurn == currentPivot) playerMananger.UpdateTurnCooldown();
            _currentTurn = nextTurn;
        }
        onNextTurn.Invoke();
    }

    private int FindNextTurn()
    {
        int nextTurn = (currentTurn + 1) % playerMananger.players.Length;
        while (!playerMananger.players[nextTurn].isAbleToMove)
        {
            nextTurn = (nextTurn + 1) % playerMananger.players.Length;
            if (nextTurn == currentTurn && !playerMananger.players[nextTurn].isAbleToMove) return -1;
        }
        return nextTurn;
    }

    private void NextPhase()
    {
        if (currentPhase == -1) return;
        if (currentPhase + 1 >= 3)
        {
            currentPhase = 0;
            currentPivot = (currentPivot + 1) % playerMananger.players.Length;
            _currentTurn = currentPivot;

        }
        else
        {
            currentPhase++;
            _currentTurn = currentPivot;
        }
        playerMananger.ResetPass();
        if (currentPhase == 1) playerMananger.UpdatePhaseCooldown();
        SetupPhase(currentPhase);
    }

    private void SetupPhase(int phase)
    {
        if (phase == 1)
        {
            foodMananger.SpawnFood(playerMananger.players.Length);

        }
        else if (phase == 2)
        {
            StartSurvivingPhase();
            if (deck.amount == 0) currentPhase = -1;
            else StartPreDevelopPhase();

        }
    }

    private void StartSurvivingPhase()
    {
        playerMananger.StartSurvivingPhase();
    }


    private void StartPreDevelopPhase()
    {
        playerMananger.StartPreDevelopPhase(currentPivot, deck);
        NextPhase();
    }


    public void AddPropToAnimal(int playerId, ICard card, in AnimalId target, bool isRotated)
    {
        bool isAddedSuccesful = playerMananger.AddPropToAnimal(playerId, card, target, isRotated);
        if (isAddedSuccesful) NextTurn();
    }


    public void CreateAnimal(int playerId, ICard card)
    {
        bool isCreatedSuccesful = playerMananger.CreateAnimal(playerId, card);
        if (isCreatedSuccesful) NextTurn();
    }

    public void Feed(int playerId, in AnimalId target)
    {
        if (foodMananger.food <= 0) throw new Exception("Trying to feed without food");
        int isFeeded = playerMananger.Feed(playerId, target, foodMananger);
        if (isFeeded > 0)
        {
            NextTurn();
            foodMananger.Consume(isFeeded);
        }
    }

    public void Pass(int playerId)
    {
        playerMananger.Pass(playerId);
        NextTurn();
    }


    public void PlayProp(int playerId, ICard card, in AnimalId target1, in AnimalId target2, bool isRotated = false)
    {
        _sideTurnsInfo = GameInteractionStruct.PlayProp(playerMananger, playerId, card, target1, target2, isRotated);
        if (_sideTurnsInfo.Equals(PairAnimalId.DOING_NEXT_TURN))
        {
            NextTurn(); return;
        }
        if (_sideTurnsInfo.Equals(PairAnimalId.NOT_DOING_NEXT_TURN)) return;
        if (_sideTurnsInfo.Equals(PairAnimalId.DESTROY_FOOD)) { foodMananger.Consume(1); return; }
        if (_sideTurnsInfo.first.Equals(PairAnimalId.PIRACY_FOOD.first))
        {
            playerMananger.players[_sideTurnsInfo.second.ownerId].animalArea.spots[_sideTurnsInfo.second.localId].animal.DecreaseFood();
            return;
        }
        List<MoveStruct> moves = GetAllPossibleSidesMoves(_sideTurnsInfo);
        if (moves.Count == 1)
        {
            MoveStruct.ExecuteMove(this, moves[0]);
        }
        NextTurn(_sideTurnsInfo.second.ownerId);
    }

    public List<MoveStruct> GetAllPossibleSidesMoves(in PairAnimalId sideTurnsInfo)
    {
        AnimalId myAnimalId = sideTurnsInfo.second;
        AnimalId enemyId = sideTurnsInfo.first;
        List<MoveStruct> moves = new();
        for (int i = 0; i < playerMananger.players[currentTurn].animalArea.spots[myAnimalId.localId].animal.singleProps.Length; i++)
        {
            AnimalProp prop = playerMananger.players[currentTurn].animalArea.spots[myAnimalId.localId].animal.singleProps[i];
            if (!prop.IsActivable) continue;
            if (prop.name == AnimalPropName.Fast)
            {
                moves.Add(MoveStruct.GetResponceToAttackMove(currentTurn, myAnimalId, enemyId, prop));
            }
            else if (prop.name == AnimalPropName.DropTail)
            {
                for (int j = 0; j < playerMananger.players[currentTurn].animalArea.spots[myAnimalId.localId].animal.singleProps.Length; j++)
                {
                    prop.mainAnimalId = myAnimalId;
                    prop.secondAnimalId = new(0, j);
                    moves.Add(MoveStruct.GetResponceToAttackMove(currentTurn, myAnimalId, enemyId, prop));
                }
                for (int j = 0; j < playerMananger.players[currentTurn].animalArea.spots[myAnimalId.localId].animal.pairProps.Length; j++)
                {
                    prop.mainAnimalId = myAnimalId;
                    prop.secondAnimalId = new(1, j);
                    moves.Add(MoveStruct.GetResponceToAttackMove(currentTurn, myAnimalId, enemyId, prop));
                }

            }
            else if (prop.name == AnimalPropName.Mimic)
            {
                for (int j = 0; j < playerMananger.players[currentTurn].animalArea.amount; j++)
                {
                    if (j == myAnimalId.localId) continue;
                    bool isCanMimic = GameInteractionStruct.
                        IsCanAttack(playerMananger.players[enemyId.ownerId].animalArea.spots[enemyId.localId].animal,
                            playerMananger.players[currentTurn].animalArea.spots[j].animal);
                    if (isCanMimic)
                    {
                        prop.mainAnimalId = myAnimalId;
                        prop.secondAnimalId = new(currentTurn, j);
                        moves.Add(MoveStruct.GetResponceToAttackMove(currentTurn, myAnimalId, enemyId, prop));
                    }
                }
            }
        }
        return moves;

    }

    internal VGMstruct GetStruct(Player player)
    {
        throw new NotImplementedException();
    }
}
