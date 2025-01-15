using System;
using Unity.Collections;
using Unity.Burst;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;



[BurstCompile(DisableDirectCall = true)]
public struct VGMstructXL
{
    public PlayerSpots spots;
    public Hands hands;

    public NativeList<CardStruct> deck;
    Unity.Mathematics.Random random;

    public int food;
    public int playersLength => 4;

    public int currentPivot;

    public int currentPhase;

    private int _currentTurn;

    public int _currentSideTurn;
    public int currentTurn => (_currentSideTurn == -1) ? _currentTurn : _currentSideTurn;
    public bool isOver => currentPhase == -1;
    public bool isSideTurns => _currentSideTurn != -1;
    private PairAnimalId _sideTurnsInfo;

    public VGMstructXL(int currentPivot, int currentPhase, int currentTurn, int currentSideTurn, in PlayerSpots spots, in Hands hands,
                       List<CardStruct> deck, int food)
    {
        this.currentPivot = currentPivot;
        this.currentPhase = currentPhase;
        _currentSideTurn = currentSideTurn;
        _currentTurn = currentTurn;
        _sideTurnsInfo = new PairAnimalId();
        this.spots = spots;
        this.deck = new(deck.Count, Allocator.TempJob);
        for(int i = 0; i < deck.Count; i++) { this.deck.Add(deck[i]); }
        this.hands = hands;
        random = new Unity.Mathematics.Random(10u);
        this.food = food;

    }


    public int GetWinner()
    {
        return spots.GetWinner();
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
        spots.UpdateTurnCooldown();
    }

    private void NextPhase()
    {
        if (currentPhase == -1) return;
        if (currentPhase + 1 >= 3)
        {
            currentPhase = 0;
            currentPivot = (currentPivot + 1) % playersLength;
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

    private void UpdatePhaseCooldown()
    {
        spots.UpdatePhaseCooldown();
    }

    private void ResetPass()
    {
        spots.ResetPass();
    }

    internal bool AddPropToAnimal(int playerId, in CardStruct card, in AnimalId target, bool isRotated)
    {
        if (target.ownerId != playerId) throw new Exception("RuleBreaker as you like");
        AnimalSpotStruct spot = spots.GetSpot(target);
        bool isAdded = spot.AddPropToAnimal(card, isRotated);
        if(!isAdded) return false;
        spots.SetSpot(target, spot);
        return true;
    }

    private AnimalSpotStruct CreateFreeSpot(int playerId)
    {
        return new AnimalSpotStruct(new(playerId, spots.GetSpotsLength(playerId)), AnimalStruct.NULL);
    }

    internal bool CreateAnimal(int playerId, in CardStruct card)
    {
        bool isAddedSuccesful = false;
        AnimalSpotStruct freeSpot = CreateFreeSpot(playerId);
        isAddedSuccesful = freeSpot.CreateAnimal(card, spots.GetSpotsLength(playerId));
        if (!isAddedSuccesful) return false;
        freeSpot.SetLocalAndOwnerId(new(playerId, spots.GetSpotsLength(playerId)));
        spots.SetSpot(freeSpot);
        if (isAddedSuccesful) NextTurn();
        return isAddedSuccesful;
    }

    internal void Feed(int playerId, in AnimalId target)
    {
        if (food <= 0) throw new Exception("Trying to feed without food");
        int foodConsumed = 0;
        if (target.ownerId != playerId) throw new Exception("RuleBreaker as you like");

        foodConsumed += spots.Feed(target, food, false);

        if (foodConsumed > 0)
        {
            NextTurn();
            food -= foodConsumed;
        }
    }



    internal void Pass(int playerId)
    {
        spots.Pass(playerId);
        NextTurn();
    }


    internal void PlayProp(int playerId, in CardStruct card, in AnimalId target1, in AnimalId target2, bool isRotated = false)
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
            AnimalSpotStruct piracyTarget = spots.GetSpot(_sideTurnsInfo.second);
            piracyTarget.animal.DecreaseFood();
            spots.SetSpot(piracyTarget);
            return;
        }
        NativeList<MoveStruct> moves = GetAllPossibleSidesMoves(_sideTurnsInfo);
        if (moves.Length == 1)
        {
            MoveStruct.ExecuteMove(ref this, moves[0]);
        }
        NextTurn(_sideTurnsInfo.second.ownerId);

    }



    private PairAnimalId PlaySleep(in AnimalId target)
    {
        AnimalSpotStruct spot = spots.GetSpot(target);
        spot.animal.ActivateSleepProp();
        spots.SetSpot(spot);
        return PairAnimalId.NOT_DOING_NEXT_TURN;
    }

    private PairAnimalId PlayFasciest(in AnimalId target)
    {
        AnimalSpotStruct spot = spots.GetSpot(target);
        spot.animal.ActivateFasciestProp();
        spots.SetSpot(spot);
        return PairAnimalId.DESTROY_FOOD;
    }

    private PairAnimalId PlayPiracy(in AnimalId pirate, in AnimalId victim)
    {
        //TODO Ну ты хоть бы проверил возможно ли пиратство
        //Я знаю что двойные проверки не супер круто, но все таки
        AnimalSpotStruct pirateSpot = spots.GetSpot(pirate);

        pirateSpot.animal.ActivatePiraceProp();
        spots.SetSpot(pirateSpot);
        PairAnimalId sideTurnsInfo = PairAnimalId.PIRACY_FOOD;
        sideTurnsInfo.second = victim;
        return sideTurnsInfo;
    }

    private PairAnimalId PlayPredator(in AnimalId predatorId, in AnimalId victimId)
    {
        PairAnimalId sideTurnsInfo = new(predatorId, victimId);
        AnimalSpotStruct predator = spots.GetSpot(predatorId);
        AnimalSpotStruct victim = spots.GetSpot(victimId);


        if (!GameInteractionStruct.IsCanAttack(predator.animal, victim.animal))
            throw new Exception("GameBreaking Trying to attack immortal victim");

        AnimalPropName victimFlags = victim.animal.propFlags;
        NativeList<AnimalProp> sideProps = new NativeList<AnimalProp>(3, Allocator.Temp);
        for (int i = 0; i < victim.animal.props.singlesLength; i++)
        {
            AnimalProp prop = victim.animal.props.singles[i];
            if (!prop.IsActivable) continue;
            if (GameInteractionStruct.IsSideInteractable(prop.name)) sideProps.Add(prop);
        }
        if (sideProps.Length == 0)
        {
            spots.KillById(predatorId, victimId);
            return PairAnimalId.DOING_NEXT_TURN;
        }
        return sideTurnsInfo;
    }



    internal void PlaySideProp(int playerId, in AnimalProp prop, in AnimalId friendId, in AnimalId enemyId)
    {
        AnimalSpotStruct friendSpot = spots.GetSpot(friendId);
        AnimalSpotStruct enemySpot = spots.GetSpot(enemyId);

        switch (prop.name)
        {
            case AnimalPropName.Fast:
                friendSpot.animal.ActivateFastProp();
                spots.SetSpot(friendSpot);
                int diceResult = GetDiceResult();
                if (diceResult >= 4)
                {
                    _sideTurnsInfo = PairAnimalId.NULL;
                    return;
                }
                if (GetAllPossibleSidesMoves(_sideTurnsInfo).Length == 0)
                {
                    spots.KillById(enemyId, friendId);
                }
                break;
            case AnimalPropName.Mimic:
                friendSpot.animal.ActivateMimicProp();
                spots.SetSpot(friendSpot);
                _sideTurnsInfo.second = prop.secondAnimalId;
                break;
            case AnimalPropName.DropTail:
                friendSpot.animal.ActivateDropTailProp();
                _sideTurnsInfo = PairAnimalId.NULL;

                enemySpot.animal.Feed();
                spots.SetSpot(enemySpot);
                AnimalProp targetProp = prop.secondAnimalId.ownerId == 0 ?
                    friendSpot.animal.props.singles[prop.secondAnimalId.localId] :
                    friendSpot.animal.props.pairs[prop.secondAnimalId.localId];
                friendSpot.animal.RemoveProp(targetProp);
                spots.SetSpot(friendSpot);
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
            if (deck.Length == 0) currentPhase = -1;
            else StartPreDevelopPhase();

        }
    }


    private void StartSurvivingPhase()
    {
        spots.StartSurvivingPhase();
    }


    private void StartPreDevelopPhase()
    {
        for (int i = currentPivot; i <= currentPivot + playersLength; i++)
        {
            int playerId = i % playersLength;
            int cardAmount = spots.GetSpotsLength(playerId) + 1;
            if (cardAmount == 1 && hands.GetHandAmount(playerId) == 0)
                cardAmount = 6;

            for(int j = 0; j < cardAmount; j++)
            {
                if(deck.Length == 0) break;
                CardStruct card = deck[deck.Length - 1];
                hands.AddCard(playerId, card);
                deck.RemoveAt(deck.Length - 1);
            }
        }
        NextPhase();
    }


    private int FindNextTurn()
    {
        int nextTurn = (currentTurn + 1) % playersLength;
        while (!spots.isAbleToMove[nextTurn])
        {
            nextTurn = (nextTurn + 1) % playersLength;
            if (nextTurn == currentTurn && !spots.isAbleToMove[nextTurn]) return -1;
        }
        return nextTurn;
    }


    public NativeList<MoveStruct> GetAllPossibleSidesMoves(in PairAnimalId sideTurnsInfo)
    {
        AnimalId myAnimalId = sideTurnsInfo.second;
        AnimalId enemyId = sideTurnsInfo.first;
        NativeList<MoveStruct> moves = new NativeList<MoveStruct>(4, Allocator.TempJob);
        for (int i = 0; i < spots.GetSpot(myAnimalId).animal.props.singlesLength; i++)
        {
            AnimalProp prop = spots.GetSpot(myAnimalId).animal.props.singles[i];
            if (!prop.IsActivable) continue;
            if (prop.name == AnimalPropName.Fast)
            {
                moves.Add(MoveStruct.GetResponceToAttackMove(currentTurn, myAnimalId, enemyId, prop));
            }
            else if (prop.name == AnimalPropName.DropTail)
            {
                for (int j = 0; j < spots.GetSpot(myAnimalId).animal.props.singlesLength; j++)
                {
                    prop.mainAnimalId = myAnimalId;
                    prop.secondAnimalId = new(0, j);
                    moves.Add(MoveStruct.GetResponceToAttackMove(currentTurn, myAnimalId, enemyId, prop));
                }
                for (int j = 0; j < spots.GetSpot(myAnimalId).animal.props.pairsLength; j++)
                {
                    prop.mainAnimalId = myAnimalId;
                    prop.secondAnimalId = new(1, j);
                    moves.Add(MoveStruct.GetResponceToAttackMove(currentTurn, myAnimalId, enemyId, prop));
                }

            }
            else if (prop.name == AnimalPropName.Mimic)
            {
                for (int j = 0; j < spots.GetSpotsLength(currentTurn); j++)
                {
                    if (j == myAnimalId.localId) continue;
                    bool isCanMimic = GameInteractionStruct.
                        IsCanAttack(spots.GetSpot(enemyId).animal,
                            spots.GetSpot(new(currentTurn, j)).animal);
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

    public NativeList<MoveStruct> GetAllPossibleMoves()
    {
        NativeList<AnimalId> enemySpots = new NativeList<AnimalId>(5, Allocator.Temp);
        NativeList<AnimalId> friendSpots = new NativeList<AnimalId>(5, Allocator.Temp);
        NativeList<PairAnimalId> pairEnemySpots = new NativeList<PairAnimalId>(5, Allocator.Temp);
        NativeList<PairAnimalId> pairFriendSpots = new NativeList<PairAnimalId>(5, Allocator.Temp);

        for (int i = 0; i < playersLength; i++)
        {
            for (int j = 0; j < spots.GetSpotsLength(i) ; j++)
            {
                if (i == currentTurn)
                {
                    friendSpots.Add(new(i, j));
                    for (int k = j; k < spots.GetSpotsLength(i); k++)
                        pairFriendSpots.Add(new(i, j, i, k));
                }
                else
                {
                    enemySpots.Add(new(i, j));
                    for (int k = j; k < spots.GetSpotsLength(i); k++)
                        pairEnemySpots.Add(new(i, j, i, k));
                }
            }
        }


        NativeList<MoveStruct> moves = new NativeList<MoveStruct>(64, Allocator.TempJob);
        switch (currentPhase)
        {
            case 0:
                for (int i = 0; i < hands.GetHandAmount(currentTurn); i++)
                {
                    CardStruct card = hands.GetHandCard(new(currentTurn, i));
                    moves.Add(MoveStruct.GetCreateAnimalMove(currentTurn, card));
                    if (card.main.isHostile())
                    {
                        if (card.main.isPair)
                        {
                            for (int j = 0; j < pairEnemySpots.Length; j++)
                            {
                                
                                bool isPossibleToAdd = spots.GetSpot(pairEnemySpots[j].first).IsPossibleToAddProp(card.main);
                                if (!isPossibleToAdd) continue;
                                isPossibleToAdd = spots.GetSpot(pairEnemySpots[j].second).IsPossibleToAddProp(card.main);
                                if (!isPossibleToAdd) continue;
                                moves.Add(MoveStruct.
                                    GetAddPropMove(currentTurn, card, pairEnemySpots[j].first, pairEnemySpots[j].second, false));
                            }
                        }
                        else
                        {
                            for (int j = 0; j < enemySpots.Length; j++)
                            {
                                bool isPossibleToAdd = spots.GetSpot(enemySpots[j]).IsPossibleToAddProp(card.main);
                                if (!isPossibleToAdd) continue;
                                moves.Add(MoveStruct.GetAddPropMove(currentTurn, card, enemySpots[j], AnimalId.NULL, false));
                            }
                        }

                    }
                    else
                    {
                        if (card.main.isPair)
                        {
                            for (int j = 0; j < pairFriendSpots.Length; j++)
                            {
                                bool isPossibleToAdd = spots.GetSpot(pairFriendSpots[j].first).
                                    IsPossibleToAddProp(card.main);
                                if (!isPossibleToAdd) continue;
                                isPossibleToAdd = spots.GetSpot(pairFriendSpots[j].second).
                                    IsPossibleToAddProp(card.main);
                                if (!isPossibleToAdd) continue;
                                moves.Add(MoveStruct.
                                    GetAddPropMove(currentTurn, card, pairFriendSpots[j].first, pairFriendSpots[j].second, false));
                            }

                        }
                        else
                        {
                            for (int j = 0; j < friendSpots.Length; j++)
                            {
                                bool isPossibleToAdd = spots.GetSpot(friendSpots[j]).
                                    IsPossibleToAddProp(card.main);
                                if (!isPossibleToAdd) continue;
                                moves.Add(MoveStruct.GetAddPropMove(currentTurn, card, friendSpots[j], AnimalId.NULL, false));
                            }
                        }

                    }
                    if (card.second.isNull()) continue;
                    if (card.second.isHostile())
                    {
                        if (card.second.isPair)
                        {
                            for (int j = 0; j < pairEnemySpots.Length; j++)
                            {

                                bool isPossibleToAdd = spots.GetSpot(pairEnemySpots[j].first).IsPossibleToAddProp(card.second);
                                if (!isPossibleToAdd) continue;
                                isPossibleToAdd = spots.GetSpot(pairEnemySpots[j].second).IsPossibleToAddProp(card.second);
                                if (!isPossibleToAdd) continue;
                                moves.Add(MoveStruct.
                                    GetAddPropMove(currentTurn, card, pairEnemySpots[j].first, pairEnemySpots[j].second, true));
                            }
                        }
                        else
                        {
                            for (int j = 0; j < enemySpots.Length; j++)
                            {
                                bool isPossibleToAdd = spots.GetSpot(enemySpots[j]).IsPossibleToAddProp(card.second);
                                if (!isPossibleToAdd) continue;
                                moves.Add(MoveStruct.GetAddPropMove(currentTurn, card, enemySpots[j], AnimalId.NULL, true));
                            }
                        }

                    }
                    else
                    {
                        if (card.second.isPair)
                        {
                            for (int j = 0; j < pairFriendSpots.Length; j++)
                            {
                                bool isPossibleToAdd = spots.GetSpot(pairFriendSpots[j].first).
                                    IsPossibleToAddProp(card.second);
                                if (!isPossibleToAdd) continue;
                                isPossibleToAdd = spots.GetSpot(pairFriendSpots[j].second).
                                    IsPossibleToAddProp(card.second);
                                if (!isPossibleToAdd) continue;
                                moves.Add(MoveStruct.
                                    GetAddPropMove(currentTurn, card, pairFriendSpots[j].first, pairFriendSpots[j].second, true));
                            }

                        }
                        else
                        {
                            for (int j = 0; j < friendSpots.Length; j++)
                            {
                                bool isPossibleToAdd = spots.GetSpot(friendSpots[j]).
                                    IsPossibleToAddProp(card.second);
                                if (!isPossibleToAdd) continue;
                                moves.Add(MoveStruct.GetAddPropMove(currentTurn, card, friendSpots[j], AnimalId.NULL, true));
                            }
                        }

                    }
                }
                moves.Add(MoveStruct.GetPassMove(currentTurn));
                break;
            case 1:
                for (int i = 0; i < spots.GetSpotsLength(currentTurn); i++)
                {
                    
                    bool isFull = spots.GetSpot(new(currentTurn, i)).animal.isFull();
                    if (!isFull) moves.Add(MoveStruct.GetFeedMove(currentTurn, new(currentTurn, i)));
                }
                if (moves.Length == 0) moves.Add(MoveStruct.GetPassMove(currentTurn));
                PropId propId = new PropId();
                while (!propId.isNull())
                {
                    propId = GetNextInteractablPropId(currentTurn, propId);
                    if (propId.isNull()) break;
                    
                    AnimalProp prop = spots.GetSpot(new(currentTurn, propId.spotlId)).animal.props.singles[propId.proplId];
                    if (prop.name == AnimalPropName.Predator)
                    {
                        for (int i = 0; i < enemySpots.Length; i++)
                        {
                            if (GameInteractionStruct.IsCanAttack(spots.GetSpot(new(currentTurn, propId.spotlId)).animal,
                                spots.GetSpot(enemySpots[i]).animal))
                                moves.Add(MoveStruct.GetPlayPropMove(currentTurn, prop, new(currentTurn, propId.spotlId), enemySpots[i]));
                        }
                    }
                    else if (prop.name == AnimalPropName.Sleep)
                    {
                        //TODO Да тут можно сооптимизировать с помощью case проброса
                        moves.Add(MoveStruct.GetPlayPropMove(currentTurn, prop, new(currentTurn, propId.spotlId), AnimalId.NULL));
                    }
                    else if (prop.name == AnimalPropName.Fasciest)
                    {
                        moves.Add(MoveStruct.GetPlayPropMove(currentTurn, prop, new(currentTurn, propId.spotlId), AnimalId.NULL));
                    }
                    else if (prop.name == AnimalPropName.Piracy)
                    {
                        for (int i = 0; i < enemySpots.Length; i++)
                        {
                            if (spots.GetSpot(enemySpots[i]).animal.IsPiracyTarget())
                                moves.Add(MoveStruct.GetPlayPropMove(currentTurn, prop, new(currentTurn, propId.spotlId), enemySpots[i]));
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
        for (int i = prv.spotlId; i < spots.GetSpotsLength(playerId); i++)
        {
            for (int j = prv.proplId; j < spots.GetSpot(playerId, i).animal.props.singlesLength; j++)
            {
                bool isInteractable = spots.GetSpot(playerId, i).animal.props.singles[j].IsInteractable();
                if (isInteractable) return new PropId(i, j);
            }
        }
        return PropId.NULL;
    }

    public bool MakeRandomMovesUntilTerminate(int targetPlayer)
    {
        int k = 0;
        while (!isOver)
        {
            NativeList<MoveStruct> moves = isSideTurns ? GetAllPossibleSidesMoves(_sideTurnsInfo) : GetAllPossibleMoves();
            //МДА НЕ ИДЕАЛЬНО КОНЕЧНО TODO оптимизировать
            MoveStruct randomMove = moves[random.NextInt(0, moves.Length)];
            MoveStruct.ExecuteMove(ref this, randomMove);
            k++;
            if (k > 3200) throw new Exception("Бесконечная игра");
        }

        return targetPlayer == GetWinner();
    }

    private int GetDiceResult()
    {
        return random.NextInt(1, 7);
    }


}
