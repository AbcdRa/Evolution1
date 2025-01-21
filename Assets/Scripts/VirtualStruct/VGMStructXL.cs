using System;
using Unity.Collections;
using Unity.Burst;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;



//[BurstCompile(DisableDirectCall = true)]
[BurstCompile]
public struct VGMstructXL : IDisposable
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

    private NativeList<MoveStruct> moves;
    private NativeList<MoveStruct> sideMoves;

    public VGMstructXL(int currentPivot, int currentPhase, int currentTurn, int currentSideTurn, in PlayerSpots spots, in Hands hands,
                       List<CardStruct> deck, int food)
    {
        this.currentPivot = currentPivot;
        this.currentPhase = currentPhase;
        _currentSideTurn = currentSideTurn;
        _currentTurn = currentTurn;
        _sideTurnsInfo = new PairAnimalId();
        this.spots = spots;
        
        this.deck = new(deck.Count, Allocator.Persistent);
        for(int i = 0; i < deck.Count; i++) { this.deck.Add(deck[i]); }
        this.hands = hands;
        random = new Unity.Mathematics.Random(10u);
        this.food = food;
        moves = new NativeList<MoveStruct>(128, Allocator.Persistent);
        sideMoves = new NativeList<MoveStruct>(24, Allocator.Persistent);
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
        if (target.ownerId != playerId) throw new Exception($"RuleBreaker as you like Add prop {card.ToFString()} isRotated = {isRotated} {this.ToString()} plId = {playerId}, target = {target.ToFString()} ");
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
        if (food <= 0) 
            throw new Exception("Trying to feed without food");
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
        sideMoves = GetAllPossibleSidesMoves(_sideTurnsInfo);
        if (sideMoves.Length == 1)
        {
            ExecuteMove(sideMoves[0]);
            
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
        NativeList<AnimalProp> sideProps = new NativeList<AnimalProp>(3, Allocator.Persistent);
        for (int i = 0; i < victim.animal.props.singlesLength; i++)
        {
            AnimalProp prop = victim.animal.props.singles[i];
            if (!prop.IsActivable) continue;
            if (GameInteractionStruct.IsSideInteractable(prop.name)) sideProps.Add(prop);
        }
        if (sideProps.Length == 0)
        {
            spots.KillById(predatorId, victimId);
            sideProps.Dispose();
            return PairAnimalId.DOING_NEXT_TURN;
        } else
        {
            sideProps.Dispose();
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
                sideMoves.Clear();
                sideMoves = GetAllPossibleSidesMoves(_sideTurnsInfo);
                if (sideMoves.Length == 0)
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
        sideMoves.Clear();
        for (int i = 0; i < spots.GetSpot(myAnimalId).animal.props.singlesLength; i++)
        {
            AnimalProp prop = spots.GetSpot(myAnimalId).animal.props.singles[i];
            if (!prop.IsActivable) continue;
            if (prop.name == AnimalPropName.Fast)
            {
                sideMoves.Add(MoveStruct.GetResponceToAttackMove(currentTurn, myAnimalId, enemyId, prop));
            }
            else if (prop.name == AnimalPropName.DropTail)
            {
                for (int j = 0; j < spots.GetSpot(myAnimalId).animal.props.singlesLength; j++)
                {
                    prop.mainAnimalId = myAnimalId;
                    prop.secondAnimalId = new(0, j);
                    sideMoves.Add(MoveStruct.GetResponceToAttackMove(currentTurn, myAnimalId, enemyId, prop));
                }
                for (int j = 0; j < spots.GetSpot(myAnimalId).animal.props.pairsLength; j++)
                {
                    prop.mainAnimalId = myAnimalId;
                    prop.secondAnimalId = new(1, j);
                    sideMoves.Add(MoveStruct.GetResponceToAttackMove(currentTurn, myAnimalId, enemyId, prop));
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
                        sideMoves.Add(MoveStruct.GetResponceToAttackMove(currentTurn, myAnimalId, enemyId, prop));
                    }
                }
            }
        }
        return sideMoves;

    }

    public NativeList<MoveStruct> GetAllPossibleMoves()
    {
        NativeList<AnimalId> enemySpots = new NativeList<AnimalId>(5, Allocator.Persistent);
        NativeList<AnimalId> friendSpots = new NativeList<AnimalId>(5, Allocator.Persistent);
        NativeList<PairAnimalId> pairEnemySpots = new NativeList<PairAnimalId>(5, Allocator.Persistent);
        NativeList<PairAnimalId> pairFriendSpots = new NativeList<PairAnimalId>(5, Allocator.Persistent);

        for (int i = 0; i < playersLength; i++)
        {
            for (int j = 0; j < spots.GetSpotsLength(i) ; j++)
            {
                if (i == currentTurn)
                {
                    friendSpots.Add(new(i, j));
                    for (int k = j+1; k < spots.GetSpotsLength(i); k++)
                        pairFriendSpots.Add(new(i, j, i, k));
                }
                else
                {
                    enemySpots.Add(new(i, j));
                    for (int k = j+1; k < spots.GetSpotsLength(i); k++)
                        pairEnemySpots.Add(new(i, j, i, k));
                }
            }
        }

        
        
        moves.Clear();
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
                if(food > 0)
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
        enemySpots.Dispose();
        friendSpots.Dispose();
        pairEnemySpots.Dispose();
        pairFriendSpots.Dispose();
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
            moves.Clear();
            ExecuteMove(randomMove);
            k++;
            if (k > 3200) throw new Exception("Бесконечная игра");
        }

        return targetPlayer == GetWinner();
    }

    private int GetDiceResult()
    {
        return random.NextInt(1, 7);
    }

    public override string ToString()
    {
        return $"VGM[{currentPhase}/{currentTurn}][D{deck.Length}][F{food}]";
    }

    public void Dispose()
    {
        spots.Dispose();
        hands.Dispose();
        deck.Dispose();
        moves.Dispose();
        sideMoves.Dispose();
    }



    public void ExecuteMove(in MoveStruct move)
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
}
