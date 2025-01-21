﻿using GameAI.Algorithms.MonteCarlo;
using GameAI.GameInterfaces;
using System;
using System.Collections.Generic;

using Unity.Collections;

using SRandom = Unity.Mathematics.Random;


public class VGame : RandomSimulation<VGame, VMove, VPlayer>.IGame
{
    private SRandom random;
    private VPlayerMananger vPM;

    public List<CardStruct> deck;
    public int food;

    public int currentPivot;
    public int currentPhase;
    private int _currentTurn;
    private int _currentSideTurn;
    public int currentTurn => (_currentSideTurn == -1) ? _currentTurn : _currentSideTurn;
    public bool isSideTurns => _currentSideTurn != -1;
    VPlayer ICurrentPlayer<VPlayer>.CurrentPlayer => vPM.GetCurrentPlayer(currentTurn);

    private PairAnimalId _sideTurnsInfo;



    public VGame(int currentPivot, int currentPhase, int currentTurn, int currentSideTurn, VPlayerMananger vPM,
                List<CardStruct> deck, int food, PairAnimalId sideTurnsInfo)
    {
        this.currentPivot = currentPivot;
        this.currentPhase = currentPhase;
        this._currentSideTurn = currentSideTurn;
        this._currentTurn = currentTurn;
        this._sideTurnsInfo = sideTurnsInfo;
        this.food = food;
        this.deck = deck;
        this.vPM = vPM;
    }


        

    public VGame DeepCopy()
    {
        List<CardStruct> deckCopy = new List<CardStruct>(deck.Count);
        for(int i = 0; i < deck.Count; i++)
        {
            deckCopy.Add(deck[i]);
        }
        return new VGame(currentPivot, currentPhase, _currentTurn, _currentSideTurn, vPM.DeepCopy(), deckCopy, food, _sideTurnsInfo);
    }

    public void DoMove(VMove move)
    {
        switch (move.data.type)
        {
            case MoveType.Pass:
                Pass(move.data.playerId); break;
            case MoveType.CreateAnimal:
                CreateAnimal(move.data.playerId, move.data.card); break;
            case MoveType.AddPropToAnimal:
                AddPropToAnimal(move.data.playerId, move.data.card, move.data.target1, move.data.isRotated); break;
            case MoveType.Feed:
                Feed(move.data.playerId, move.data.target1); break;
            case MoveType.PlayProp:
                PlayProp(move.data.playerId, move.data.card, move.data.target1, move.data.target2); break;
            case MoveType.ResponseToAttack:
                PlaySideProp(move.data.playerId, move.data.prop, move.data.target1, move.data.target2); break;
        }
    }

    public List<VMove> GetLegalMoves()
    {
        return isSideTurns ? GetAllPossibleSidesMoves(_sideTurnsInfo) : GetAllPossibleMoves();
    }

    public bool IsGameOver()
    {
        return currentPhase == -1;
    }

    bool IWinner<VPlayer>.IsWinner(VPlayer player)
    {
        return vPM.IsWinner(player);
    }
   

   


    internal void NextTurn(int sideTurn = -1)
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
            if (currentPhase == 1 && nextTurn == currentPivot) UpdateTurnCooldown();
            _currentTurn = nextTurn;
        }
    }

    private void UpdateTurnCooldown()
    {
        vPM.UpdateTurnCooldown();
    }

    private void UpdatePhaseCooldown()
    {
        vPM.UpdatePhaseCooldown();
    }


    private void NextPhase()
    {
        if (currentPhase == -1) return;
        if (currentPhase + 1 >= 3)
        {
            currentPhase = 0;
            currentPivot = (currentPivot + 1) % vPM.playersAmount;
            _currentTurn = currentPivot;
        }
        else
        {
            currentPhase++;
            _currentTurn = currentPivot;
        }
        ResetPass();
        if (currentPhase == 1) UpdatePhaseCooldown();
        SetupPhase(currentPhase);
    }


    private void ResetPass()
    {
        vPM.ResetPass();
    }

    public bool AddPropToAnimal(int playerId, in CardStruct card, in AnimalId target, bool isRotated)
    {
        if (target.ownerId != playerId) throw new Exception($"RuleBreaker as you like Add prop {card.ToFString()} isRotated = {isRotated} {this.ToString()} plId = {playerId}, target = {target.ToFString()} ");
        bool isAdded = vPM.AddPropToAnimal(card, target, isRotated);
        if (!isAdded) return false;
        return true;
    }



    public bool CreateAnimal(int playerId, in CardStruct card)
    {
        bool isAddedSuccesful = vPM.CreateAnimal(playerId, card);
        if (isAddedSuccesful) NextTurn();
        return isAddedSuccesful;
    }

    public void Feed(int playerId, in AnimalId target)
    {
        if (food <= 0)
            throw new Exception("Trying to feed without food");
        int foodConsumed = 0;
        if (target.ownerId != playerId) throw new Exception("RuleBreaker as you like");

        foodConsumed += vPM.Feed(target, food, false);

        if (foodConsumed > 0)
        {
            NextTurn();
            food -= foodConsumed;
        }
    }


    public void Pass(int playerId)
    {
        vPM.Pass(playerId);
        NextTurn();
    }


    public void PlayProp(int playerId, in CardStruct card, in AnimalId target1, in AnimalId target2, bool isRotated = false)
    {
        AnimalProp prop = isRotated ? card.second : card.main;
        switch (prop.name)
        {
            case AnimalPropName.Sleep:
                _sideTurnsInfo = PlaySleep(target1); break;
            case AnimalPropName.Fasciest:
                _sideTurnsInfo = PlayFasciest(target1); break;
            case AnimalPropName.Piracy:
                _sideTurnsInfo = PlayPiracy(target1, target2); break;
            case AnimalPropName.Predator:
                _sideTurnsInfo = PlayPredator(target1, target2); break;
        }
        if (_sideTurnsInfo.Equals(PairAnimalId.DOING_NEXT_TURN))
        {
            NextTurn(); return;
        }
        if (_sideTurnsInfo.Equals(PairAnimalId.NOT_DOING_NEXT_TURN)) return;
        if (_sideTurnsInfo.Equals(PairAnimalId.DESTROY_FOOD)) { food -= 1; return; }
        if (_sideTurnsInfo.first.Equals(PairAnimalId.PIRACY_FOOD.first))
        {
            vPM.DecreaseFood(_sideTurnsInfo.second, 1);
            return;
        }
        var sideMoves = GetAllPossibleSidesMoves(_sideTurnsInfo);
        if (sideMoves.Count == 1)
        {
            DoMove(sideMoves[0]);

        }
        NextTurn(_sideTurnsInfo.second.ownerId);
    }



    private PairAnimalId PlaySleep(in AnimalId target)
    {
        return vPM.Play(target, AnimalPropName.Sleep);

    }

    private PairAnimalId PlayFasciest(in AnimalId target)
    {
        return vPM.Play(target, AnimalPropName.Fasciest);
    }

    private PairAnimalId PlayPiracy(in AnimalId pirate, in AnimalId victim)
    {
        var result = vPM.Play(pirate, AnimalPropName.Piracy);
        result.second = victim;
        return result;
    }

    private PairAnimalId PlayPredator(in AnimalId predatorId, in AnimalId victimId)
    {
        PairAnimalId sideTurnsInfo = new(predatorId, victimId);
        AnimalSpotStruct predator = vPM.GetSpot(predatorId);
        AnimalSpotStruct victim = vPM.GetSpot(victimId);


        if (!GameInteractionStruct.IsCanAttack(predator.animal, victim.animal))
            throw new Exception("GameBreaking Trying to attack immortal victim");

        AnimalPropName victimFlags = victim.animal.propFlags;
        NativeList<AnimalProp> sideProps = new NativeList<AnimalProp>(3, Allocator.Persistent);
        for (int i = 0; i < victim.animal.props.singlesLength; i++)
        {
            AnimalProp prop = victim.animal.props.singles[i];
            if (!prop.IsActivable) continue;
            if (GameInteractionStruct.IsSideInteractable(prop.name)) sideProps.Add(prop);
        }
        if (sideProps.Length == 0)
        {
            vPM.KillById(predatorId, victimId);
            sideProps.Dispose();
            return PairAnimalId.DOING_NEXT_TURN;
        }
        else
        {
            sideProps.Dispose();
        }
        return sideTurnsInfo;
    }



    internal void PlaySideProp(int playerId, in AnimalProp prop, in AnimalId friendId, in AnimalId enemyId)
    {
        AnimalSpotStruct friendSpot = vPM.GetSpot(friendId);
        AnimalSpotStruct enemySpot = vPM.GetSpot(enemyId);

        switch (prop.name)
        {
            case AnimalPropName.Fast:
                friendSpot.animal.ActivateFastProp();
                vPM.SetSpot(friendSpot);
                int diceResult = GetDiceResult();
                if (diceResult >= 4)
                {
                    _sideTurnsInfo = PairAnimalId.NULL;
                    return;
                }
                var sideMoves = GetAllPossibleSidesMoves(_sideTurnsInfo);
                if (sideMoves.Count == 0)
                {
                    vPM.KillById(enemyId, friendId);
                }

                break;
            case AnimalPropName.Mimic:
                friendSpot.animal.ActivateMimicProp();
                vPM.SetSpot(friendSpot);
                _sideTurnsInfo.second = prop.secondAnimalId;
                break;
            case AnimalPropName.DropTail:
                friendSpot.animal.ActivateDropTailProp();
                _sideTurnsInfo = PairAnimalId.NULL;

                enemySpot.animal.Feed();
                vPM.SetSpot(enemySpot);
                AnimalProp targetProp = prop.secondAnimalId.ownerId == 0 ?
                    friendSpot.animal.props.singles[prop.secondAnimalId.localId] :
                    friendSpot.animal.props.pairs[prop.secondAnimalId.localId];
                friendSpot.animal.RemoveProp(targetProp);
                vPM.SetSpot(friendSpot);
                break;
        }
    }

    private void SetupPhase(int phase)
    {
        if (phase == 1)
        {
            food = random.NextInt(1, 7) + random.NextInt(1, 7) + 2;

        }
        else if (phase == 2)
        {
            StartSurvivingPhase();
            if (deck.Count == 0) currentPhase = -1;
            else StartPreDevelopPhase();
        }
    }


    private void StartSurvivingPhase()
    {
        vPM.StartSurvivingPhase();
    }


    private void StartPreDevelopPhase()
    {
        for (int i = currentPivot; i <= currentPivot + vPM.playersAmount; i++)
        {
            int playerId = i % vPM.playersAmount;
            int cardAmount = vPM.GetSpotsLength(playerId) + 1;
            if (cardAmount == 1 && vPM.GetHandAmount(playerId) == 0)
                cardAmount = 6;

            for (int j = 0; j < cardAmount; j++)
            {
                if (deck.Count == 0) break;
                CardStruct card = deck[deck.Count - 1];
                vPM.AddCard(playerId, card);
                deck.RemoveAt(deck.Count - 1);
            }
        }
        NextPhase();
    }


    private int FindNextTurn()
    {
        int nextTurn = (currentTurn + 1) % vPM.playersAmount;
        while (!vPM.IsAbleToMove(nextTurn))
        {
            nextTurn = (nextTurn + 1) % vPM.playersAmount;
            if (nextTurn == currentTurn && !vPM.IsAbleToMove(nextTurn)) return -1;
        }
        return nextTurn;
    }


    public List<VMove> GetAllPossibleSidesMoves(in PairAnimalId sideTurnsInfo)
    {
        AnimalId myAnimalId = sideTurnsInfo.second;
        AnimalId enemyId = sideTurnsInfo.first;
        List<VMove> sideMoves = new(4);
        for (int i = 0; i < vPM.GetSpot(myAnimalId).animal.props.singlesLength; i++)
        {
            AnimalProp prop = vPM.GetSpot(myAnimalId).animal.props.singles[i];
            if (!prop.IsActivable) continue;
            if (prop.name == AnimalPropName.Fast)
            {
                sideMoves.Add(VMove.GetResponceToAttackMove(currentTurn, myAnimalId, enemyId, prop));
            }
            else if (prop.name == AnimalPropName.DropTail)
            {
                for (int j = 0; j < vPM.GetSpot(myAnimalId).animal.props.singlesLength; j++)
                {
                    prop.mainAnimalId = myAnimalId;
                    prop.secondAnimalId = new(0, j);
                    sideMoves.Add(VMove.GetResponceToAttackMove(currentTurn, myAnimalId, enemyId, prop));
                }
                for (int j = 0; j < vPM.GetSpot(myAnimalId).animal.props.pairsLength; j++)
                {
                    prop.mainAnimalId = myAnimalId;
                    prop.secondAnimalId = new(1, j);
                    sideMoves.Add(VMove.GetResponceToAttackMove(currentTurn, myAnimalId, enemyId, prop));
                }

            }
            else if (prop.name == AnimalPropName.Mimic)
            {
                for (int j = 0; j < vPM.GetSpotsLength(currentTurn); j++)
                {
                    if (j == myAnimalId.localId) continue;
                    bool isCanMimic = GameInteractionStruct.
                    IsCanAttack(vPM.GetSpot(enemyId).animal,
                            vPM.GetSpot(new(currentTurn, j)).animal);
                    if (isCanMimic)
                    {
                        prop.mainAnimalId = myAnimalId;
                        prop.secondAnimalId = new(currentTurn, j);
                        sideMoves.Add(VMove.GetResponceToAttackMove(currentTurn, myAnimalId, enemyId, prop));
                    }
                }
            }
        }
        return sideMoves;

    }

    public List<VMove> GetAllPossibleMoves()
    {
        List<AnimalId> enemySpots = new(5);
        List<AnimalId> friendSpots = new(5);
        List<PairAnimalId> pairEnemySpots = new(5);
        List<PairAnimalId> pairFriendSpots = new(5);
        

        for (int i = 0; i < vPM.playersAmount ; i++)
        {
            for (int j = 0; j < vPM.GetSpotsLength(i); j++)
            {
                if (i == currentTurn)
                {
                    friendSpots.Add(new(i, j));
                    for (int k = j + 1; k < vPM.GetSpotsLength(i); k++)
                        pairFriendSpots.Add(new(i, j, i, k));
                }
                else
                {
                    enemySpots.Add(new(i, j));
                    for (int k = j + 1; k < vPM.GetSpotsLength(i); k++)
                        pairEnemySpots.Add(new(i, j, i, k));
                }
            }
        }


        List<VMove> moves = new(5);
        switch (currentPhase)
        {
            case 0:
                for (int i = 0; i < vPM.GetHandAmount(currentTurn); i++)
                {
                    CardStruct card = vPM.GetHandCard(new(currentTurn, i));
                    moves.Add(VMove.GetCreateAnimalMove(currentTurn, card));
                    if (card.main.isHostile())
                    {
                        if (card.main.isPair)
                        {
                            for (int j = 0; j < pairEnemySpots.Count; j++)
                            {

                                bool isPossibleToAdd = vPM.GetSpot(pairEnemySpots[j].first).IsPossibleToAddProp(card.main);
                                if (!isPossibleToAdd) continue;
                                isPossibleToAdd = vPM.GetSpot(pairEnemySpots[j].second).IsPossibleToAddProp(card.main);
                                if (!isPossibleToAdd) continue;
                                moves.Add(VMove.
                                    GetAddPropMove(currentTurn, card, pairEnemySpots[j].first, pairEnemySpots[j].second, false));
                            }
                        }
                        else
                        {
                            for (int j = 0; j < enemySpots.Count; j++)
                            {
                                bool isPossibleToAdd = vPM.GetSpot(enemySpots[j]).IsPossibleToAddProp(card.main);
                                if (!isPossibleToAdd) continue;
                                moves.Add(VMove.GetAddPropMove(currentTurn, card, enemySpots[j], AnimalId.NULL, false));
                            }
                        }

                    }
                    else
                    {
                        if (card.main.isPair)
                        {
                            for (int j = 0; j < pairFriendSpots.Count; j++)
                            {
                                bool isPossibleToAdd = vPM.GetSpot(pairFriendSpots[j].first).
                                    IsPossibleToAddProp(card.main);
                                if (!isPossibleToAdd) continue;
                                isPossibleToAdd = vPM.GetSpot(pairFriendSpots[j].second).
                                    IsPossibleToAddProp(card.main);
                                if (!isPossibleToAdd) continue;
                                moves.Add(VMove.
                                    GetAddPropMove(currentTurn, card, pairFriendSpots[j].first, pairFriendSpots[j].second, false));
                            }

                        }
                        else
                        {
                            for (int j = 0; j < friendSpots.Count; j++)
                            {
                                bool isPossibleToAdd = vPM.GetSpot(friendSpots[j]).
                                    IsPossibleToAddProp(card.main);
                                if (!isPossibleToAdd) continue;
                                moves.Add(VMove.GetAddPropMove(currentTurn, card, friendSpots[j], AnimalId.NULL, false));
                            }
                        }

                    }
                    if (card.second.isNull()) continue;
                    if (card.second.isHostile())
                    {
                        if (card.second.isPair)
                        {
                            for (int j = 0; j < pairEnemySpots.Count; j++)
                            {

                                bool isPossibleToAdd = vPM.GetSpot(pairEnemySpots[j].first).IsPossibleToAddProp(card.second);
                                if (!isPossibleToAdd) continue;
                                isPossibleToAdd = vPM.GetSpot(pairEnemySpots[j].second).IsPossibleToAddProp(card.second);
                                if (!isPossibleToAdd) continue;
                                moves.Add(VMove.
                                    GetAddPropMove(currentTurn, card, pairEnemySpots[j].first, pairEnemySpots[j].second, true));
                            }
                        }
                        else
                        {
                            for (int j = 0; j < enemySpots.Count; j++)
                            {
                                bool isPossibleToAdd = vPM.GetSpot(enemySpots[j]).IsPossibleToAddProp(card.second);
                                if (!isPossibleToAdd) continue;
                                moves.Add(VMove.GetAddPropMove(currentTurn, card, enemySpots[j], AnimalId.NULL, true));
                            }
                        }

                    }
                    else
                    {
                        if (card.second.isPair)
                        {
                            for (int j = 0; j < pairFriendSpots.Count; j++)
                            {
                                bool isPossibleToAdd = vPM.GetSpot(pairFriendSpots[j].first).
                                    IsPossibleToAddProp(card.second);
                                if (!isPossibleToAdd) continue;
                                isPossibleToAdd = vPM.GetSpot(pairFriendSpots[j].second).
                                    IsPossibleToAddProp(card.second);
                                if (!isPossibleToAdd) continue;
                                moves.Add(VMove.
                                    GetAddPropMove(currentTurn, card, pairFriendSpots[j].first, pairFriendSpots[j].second, true));
                            }

                        }
                        else
                        {
                            for (int j = 0; j < friendSpots.Count; j++)
                            {
                                bool isPossibleToAdd = vPM.GetSpot(friendSpots[j]).
                                    IsPossibleToAddProp(card.second);
                                if (!isPossibleToAdd) continue;
                                moves.Add(VMove.GetAddPropMove(currentTurn, card, friendSpots[j], AnimalId.NULL, true));
                            }
                        }

                    }
                }
                moves.Add(VMove.GetPassMove(currentTurn));
                break;
            case 1:
                if (food > 0)
                    for (int i = 0; i < vPM.GetSpotsLength(currentTurn); i++)
                    {
                        bool isFull = vPM.GetSpot(new(currentTurn, i)).animal.isFull();
                        if (!isFull) moves.Add(VMove.GetFeedMove(currentTurn, new(currentTurn, i)));
                    }
                if (moves.Count == 0) moves.Add(VMove.GetPassMove(currentTurn));
                PropId propId = new PropId();
                while (!propId.isNull())
                {
                    propId = GetNextInteractablPropId(currentTurn, propId);
                    if (propId.isNull()) break;

                    AnimalProp prop = vPM.GetSpot(new(currentTurn, propId.spotlId)).animal.props.singles[propId.proplId];
                    if (prop.name == AnimalPropName.Predator)
                    {
                        for (int i = 0; i < enemySpots.Count; i++)
                        {
                            if (GameInteractionStruct.IsCanAttack(vPM.GetSpot(new(currentTurn, propId.spotlId)).animal,
                                vPM.GetSpot(enemySpots[i]).animal))
                                moves.Add(VMove.GetPlayPropMove(currentTurn, prop, new(currentTurn, propId.spotlId), enemySpots[i]));
                        }
                    }
                    else if (prop.name == AnimalPropName.Sleep)
                    {
                        //TODO Да тут можно сооптимизировать с помощью case проброса
                        moves.Add(VMove.GetPlayPropMove(currentTurn, prop, new(currentTurn, propId.spotlId), AnimalId.NULL));
                    }
                    else if (prop.name == AnimalPropName.Fasciest)
                    {
                        moves.Add(VMove.GetPlayPropMove(currentTurn, prop, new(currentTurn, propId.spotlId), AnimalId.NULL));
                    }
                    else if (prop.name == AnimalPropName.Piracy)
                    {
                        for (int i = 0; i < enemySpots.Count; i++)
                        {
                            if (vPM.GetSpot(enemySpots[i]).animal.IsPiracyTarget())
                                moves.Add(VMove.GetPlayPropMove(currentTurn, prop, new(currentTurn, propId.spotlId), enemySpots[i]));
                        }
                    }
                    else throw new Exception("GameBreaking player give me a shit card");
                }
                break;
        }
        return moves;
    }

    private PropId GetNextInteractablPropId(int playerId, PropId prv)
    {
        for (int i = prv.spotlId; i < vPM.GetSpotsLength(playerId); i++)
        {
            for (int j = prv.proplId; j < vPM.GetSpot(playerId, i).animal.props.singlesLength; j++)
            {
                bool isInteractable = vPM.GetSpot(playerId, i).animal.props.singles[j].IsInteractable();
                if (isInteractable) return new PropId(i, j);
            }
        }
        return PropId.NULL;
    }

    private int GetDiceResult()
    {
        return random.NextInt(1, 7);
    }

    public override string ToString()
    {
        return $"VGM[{currentPhase}/{currentTurn}][D{deck.Count}][F{food}]";
    }

}
