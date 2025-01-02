using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameMananger : MonoBehaviour, IGameMananger
{
    public static GameMananger instance { get; private set; }

    const int FIRST_TURN_CARDS_AMOUNT = 8;

    [SerializeField] private IPlayerMananger _playerMananger;
    [SerializeField] private IFoodMananger _foodMananger;
    [SerializeField] private IDeck _deck;
    [SerializeField] private PlayerUI _playerUI;

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
            return;
        }
        if (_currentSideTurn != -1)
        {
            _currentSideTurn = -1;
            return;
        }
        int nextTurn = FindNextTurn();
        if (nextTurn == -1) NextPhase();
        else
        {
            if (currentPhase == 1 && nextTurn == currentPivot) playerMananger.UpdateTurnCooldown();
            _currentTurn = nextTurn;
        }
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
        playerMananger.StartPreDevelopPhase(currentPivot);
        NextPhase();
    }


    private void Update()
    {
        _playerUI.UpdateLogInfo(this);
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
        int sideNextTurn = GameInteractionStruct.PlayProp(playerMananger, playerId, card, target1, target2, isRotated);
        if (sideNextTurn != -2)
        {
            NextTurn(sideNextTurn);
        }
    }

}
