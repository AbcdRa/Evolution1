using System;
using Unity.Collections;
using Unity.Burst;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;



[BurstCompile(DisableDirectCall = true)]
public struct VGMstructXL
{
    public PlayerSpots spots;
    public Hands hands;

    public NativeList<CardStruct> deck;
    public NativeList<PlayerInfo> players;
    Unity.Mathematics.Random random;

    public int food;
    public int playersLength;

    public int currentPivot;

    public int currentPhase;

    private int _currentTurn;

    public int _currentSideTurn;
    public int currentTurn => (_currentSideTurn == -1) ? _currentTurn : _currentSideTurn;
    public bool isOver => currentPhase == -1;
    public bool isSideTurns => _currentSideTurn != -1;
    private PairAnimalId _sideTurnsInfo;

    //public VGMstructXL(in PlayerManangerStruct pm, in DeckStruct deck, in FoodManangerStruct foodMananger, int currentPivot, int currentPhase, int currentTurn, int currentSideTurn)
    //{
    //    playerMananger = pm;
    //    this.deck = deck;
    //    this.foodMananger = foodMananger;
    //    this.currentPivot = currentPivot;
    //    this.currentPhase = currentPhase;
    //    _currentSideTurn = currentSideTurn;
    //    _currentTurn = currentTurn;
    //    _sideTurnsInfo = new PairAnimalId();
    //}


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
        for (int i = 0; i < players.Length; i++)
        {
            PlayerInfo p = players[i];
            p.ResetPass();
            players[i] = p;
        }
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
        return new AnimalSpotStruct(new(playerId, players[playerId].animalAmount), AnimalStruct.NULL);
    }

    internal bool CreateAnimal(int playerId, in CardStruct card)
    {
        bool isAddedSuccesful = false;
        AnimalSpotStruct freeSpot = CreateFreeSpot(playerId);
        PlayerInfo player = players[playerId];
        isAddedSuccesful = freeSpot.CreateAnimal(card, player.animalAmount);
        if (!isAddedSuccesful) return false;
        freeSpot.SetLocalAndOwnerId(new(playerId, players[playerId].animalAmount));
        spots.SetSpot(freeSpot);

        player.animalAmount++;
        players[playerId] = player;
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
        PlayerInfo player = players[playerId];
        player.Pass();
        players[playerId] = player;
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
        for (int i = currentPivot; i <= currentPivot + players.Length; i++)
        {
            int cardAmount = players[i % players.Length].animalAmount + 1;
            if (cardAmount == 1 && playerMananger.players[i % playerMananger.players.Length].hand.amount == 0)
                cardAmount = 6;
            playerMananger.players[i % playerMananger.players.Length].GetCardsFromDeck(ref deck, cardAmount);
        }
        NextPhase();
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


    public NativeList<MoveStruct> GetAllPossibleSidesMoves(in PairAnimalId sideTurnsInfo)
    {
        AnimalId myAnimalId = sideTurnsInfo.second;
        AnimalId enemyId = sideTurnsInfo.first;
        NativeList<MoveStruct> moves = new NativeList<MoveStruct>(4, Allocator.TempJob);
        for (int i = 0; i < playerMananger.players[currentTurn].animalArea.spots[myAnimalId.localId].animal.props.singlesLength; i++)
        {
            AnimalProp prop = playerMananger.players[currentTurn].animalArea.spots[myAnimalId.localId].animal.props.singles[i];
            if (!prop.IsActivable) continue;
            if (prop.name == AnimalPropName.Fast)
            {
                moves.Add(MoveStruct.GetResponceToAttackMove(currentTurn, myAnimalId, enemyId, prop));
            }
            else if (prop.name == AnimalPropName.DropTail)
            {
                for (int j = 0; j < playerMananger.players[currentTurn].animalArea.spots[myAnimalId.localId].animal.props.singlesLength; j++)
                {
                    prop.mainAnimalId = myAnimalId;
                    prop.secondAnimalId = new(0, j);
                    moves.Add(MoveStruct.GetResponceToAttackMove(currentTurn, myAnimalId, enemyId, prop));
                }
                for (int j = 0; j < playerMananger.players[currentTurn].animalArea.spots[myAnimalId.localId].animal.props.pairsLength; j++)
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

    public NativeList<MoveStruct> GetAllPossibleMoves()
    {
        NativeList<AnimalId> enemySpots = new NativeList<AnimalId>(5, Allocator.Temp);
        NativeList<AnimalId> friendSpots = new NativeList<AnimalId>(5, Allocator.Temp);
        NativeList<PairAnimalId> pairEnemySpots = new NativeList<PairAnimalId>(5, Allocator.Temp);
        NativeList<PairAnimalId> pairFriendSpots = new NativeList<PairAnimalId>(5, Allocator.Temp);

        for (int i = 0; i < playerMananger.players.Length; i++)
        {
            for (int j = 0; j < playerMananger.players[i].animalArea.amount; j++)
            {
                if (i == currentTurn)
                {
                    friendSpots.Add(new(i, j));
                    for (int k = j; k < playerMananger.players[i].animalArea.amount; k++)
                        pairFriendSpots.Add(new(i, j, i, k));
                }
                else
                {
                    enemySpots.Add(new(i, j));
                    for (int k = j; k < playerMananger.players[i].animalArea.amount; k++)
                        pairEnemySpots.Add(new(i, j, i, k));
                }
            }
        }


        NativeList<MoveStruct> moves = new NativeList<MoveStruct>(64, Allocator.TempJob);
        switch (currentPhase)
        {
            case 0:
                for (int i = 0; i < playerMananger.players[currentTurn].hand.amount; i++)
                {
                    moves.Add(MoveStruct.GetCreateAnimalMove(currentTurn, playerMananger.players[currentTurn].hand.cards[i]));
                    if (playerMananger.players[currentTurn].hand.cards[i].main.isHostile())
                    {
                        if (playerMananger.players[currentTurn].hand.cards[i].main.isPair)
                        {
                            for (int j = 0; j < pairEnemySpots.Length; j++)
                            {
                                bool isPossibleToAdd = playerMananger.players[pairEnemySpots[j].first.ownerId].animalArea.spots[pairEnemySpots[j].first.localId].
                                    IsPossibleToAddProp(playerMananger.players[currentTurn].hand.cards[i].main);
                                if (!isPossibleToAdd) continue;
                                isPossibleToAdd = playerMananger.players[pairEnemySpots[j].second.ownerId].animalArea.spots[pairEnemySpots[j].second.localId].
                                    IsPossibleToAddProp(playerMananger.players[currentTurn].hand.cards[i].main);
                                if (!isPossibleToAdd) continue;
                                moves.Add(MoveStruct.
                                    GetAddPropMove(currentTurn, playerMananger.players[currentTurn].hand.cards[i], pairEnemySpots[j].first, pairEnemySpots[j].second, false));
                            }
                        }
                        else
                        {
                            for (int j = 0; j < enemySpots.Length; j++)
                            {
                                bool isPossibleToAdd = playerMananger.players[enemySpots[j].ownerId].animalArea.spots[enemySpots[j].localId].
                                    IsPossibleToAddProp(playerMananger.players[currentTurn].hand.cards[i].main);
                                if (!isPossibleToAdd) continue;
                                moves.Add(MoveStruct.GetAddPropMove(currentTurn, playerMananger.players[currentTurn].hand.cards[i], enemySpots[j], AnimalId.NULL, false));
                            }
                        }

                    }
                    else
                    {
                        if (playerMananger.players[currentTurn].hand.cards[i].main.isPair)
                        {
                            for (int j = 0; j < pairFriendSpots.Length; j++)
                            {
                                bool isPossibleToAdd = playerMananger.players[pairFriendSpots[j].first.ownerId].animalArea.spots[pairFriendSpots[j].first.localId].
                                    IsPossibleToAddProp(playerMananger.players[currentTurn].hand.cards[i].main);
                                if (!isPossibleToAdd) continue;
                                isPossibleToAdd = playerMananger.players[pairFriendSpots[j].second.ownerId].animalArea.spots[pairFriendSpots[j].second.localId].
                                    IsPossibleToAddProp(playerMananger.players[currentTurn].hand.cards[i].main);
                                if (!isPossibleToAdd) continue;
                                moves.Add(MoveStruct.
                                    GetAddPropMove(currentTurn, playerMananger.players[currentTurn].hand.cards[i], pairFriendSpots[j].first, pairFriendSpots[j].second, false));
                            }

                        }
                        else
                        {
                            for (int j = 0; j < friendSpots.Length; j++)
                            {
                                bool isPossibleToAdd = playerMananger.players[friendSpots[j].ownerId].animalArea.spots[friendSpots[j].localId].
                                    IsPossibleToAddProp(playerMananger.players[currentTurn].hand.cards[i].main);
                                if (!isPossibleToAdd) continue;
                                moves.Add(MoveStruct.GetAddPropMove(currentTurn, playerMananger.players[currentTurn].hand.cards[i], friendSpots[j], AnimalId.NULL, false));
                            }
                        }

                    }
                    if (playerMananger.players[currentTurn].hand.cards[i].second.isNull()) continue;


                    if (playerMananger.players[currentTurn].hand.cards[i].second.isHostile())
                    {
                        if (playerMananger.players[currentTurn].hand.cards[i].second.isPair)
                        {
                            for (int j = 0; j < pairEnemySpots.Length; j++)
                            {
                                bool isPossibleToAdd = playerMananger.players[pairEnemySpots[j].first.ownerId].animalArea.spots[pairEnemySpots[j].first.localId].
                                    IsPossibleToAddProp(playerMananger.players[currentTurn].hand.cards[i].second);
                                if (!isPossibleToAdd) continue;
                                isPossibleToAdd = playerMananger.players[pairEnemySpots[j].second.ownerId].animalArea.spots[pairEnemySpots[j].second.localId].
                                    IsPossibleToAddProp(playerMananger.players[currentTurn].hand.cards[i].second);
                                if (!isPossibleToAdd) continue;
                                moves.Add(MoveStruct.
                                    GetAddPropMove(currentTurn, playerMananger.players[currentTurn].hand.cards[i], pairEnemySpots[j].first, pairEnemySpots[j].second, false));
                            }
                        }
                        else
                        {
                            for (int j = 0; j < enemySpots.Length; j++)
                            {
                                bool isPossibleToAdd = playerMananger.players[enemySpots[j].ownerId].animalArea.spots[enemySpots[j].localId].
                                    IsPossibleToAddProp(playerMananger.players[currentTurn].hand.cards[i].second);
                                if (!isPossibleToAdd) continue;
                                moves.Add(MoveStruct.GetAddPropMove(currentTurn, playerMananger.players[currentTurn].hand.cards[i], enemySpots[j], AnimalId.NULL, false));
                            }

                        }

                    }
                    else
                    {
                        if (playerMananger.players[currentTurn].hand.cards[i].second.isPair)
                        {
                            for (int j = 0; j < pairFriendSpots.Length; j++)
                            {
                                bool isPossibleToAdd = playerMananger.players[pairFriendSpots[j].first.ownerId].animalArea.spots[pairFriendSpots[j].first.localId].
                                    IsPossibleToAddProp(playerMananger.players[currentTurn].hand.cards[i].second);
                                if (!isPossibleToAdd) continue;
                                isPossibleToAdd = playerMananger.players[pairFriendSpots[j].second.ownerId].animalArea.spots[pairFriendSpots[j].second.localId].
                                    IsPossibleToAddProp(playerMananger.players[currentTurn].hand.cards[i].second);
                                if (!isPossibleToAdd) continue;
                                moves.Add(MoveStruct.
                                    GetAddPropMove(currentTurn, playerMananger.players[currentTurn].hand.cards[i], pairFriendSpots[j].first, pairFriendSpots[j].second, false));
                            }

                        }
                        else
                        {
                            for (int j = 0; j < friendSpots.Length; j++)
                            {
                                bool isPossibleToAdd = playerMananger.players[friendSpots[j].ownerId].animalArea.spots[friendSpots[j].localId].
                                    IsPossibleToAddProp(playerMananger.players[currentTurn].hand.cards[i].second);
                                if (!isPossibleToAdd) continue;
                                moves.Add(MoveStruct.GetAddPropMove(currentTurn, playerMananger.players[currentTurn].hand.cards[i], friendSpots[j], AnimalId.NULL, false));

                            }
                        }
                    }
                }
                moves.Add(MoveStruct.GetPassMove(currentTurn));
                break;
            case 1:
                for (int i = 0; i < playerMananger.players[currentTurn].animalArea.amount; i++)
                {
                    bool isFull = playerMananger.players[currentTurn].animalArea.spots[i].animal.isFull();
                    if (!isFull) moves.Add(MoveStruct.GetFeedMove(currentTurn, new(currentTurn, i)));
                }
                if (moves.Length == 0) moves.Add(MoveStruct.GetPassMove(currentTurn));
                PropId propId = new PropId();
                while (!propId.isNull())
                {
                    propId = playerMananger.players[currentTurn].GetNextInteractablPropId(propId);
                    if (propId.isNull()) break;
                    AnimalProp prop = playerMananger.players[currentTurn].animalArea.spots[propId.spotlId].animal.props.singles[propId.proplId];
                    if (prop.name == AnimalPropName.Predator)
                    {
                        for (int i = 0; i < enemySpots.Length; i++)
                        {
                            if (GameInteractionStruct.IsCanAttack(playerMananger.players[currentTurn].animalArea.spots[propId.spotlId].animal,
                                playerMananger.players[enemySpots[i].ownerId].animalArea.spots[enemySpots[i].localId].animal))
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
                            if (playerMananger.players[enemySpots[i].ownerId].animalArea.spots[enemySpots[i].localId].animal.IsPiracyTarget())
                                moves.Add(MoveStruct.GetPlayPropMove(currentTurn, prop, new(currentTurn, propId.spotlId), enemySpots[i]));
                        }
                    }
                    else throw new Exception("GameBreaking player give me a shit card");
                }
                break;
        }
        return moves;
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
