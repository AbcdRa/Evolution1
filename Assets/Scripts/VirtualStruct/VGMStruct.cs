using System;
using Unity.Collections;
using Unity.Burst;

[BurstCompile(DisableDirectCall = true)]
public struct PairAnimalId
{
    internal static readonly PairAnimalId NULL = new(-1,-1,-1,-1);
    public static readonly PairAnimalId DOING_NEXT_TURN = NULL;
    public static readonly PairAnimalId NOT_DOING_NEXT_TURN = new(-2, -1, -1, -1);
    public static readonly PairAnimalId DESTROY_FOOD = new(-3, 1, -1, -1);
    public static readonly PairAnimalId PIRACY_FOOD = new(-4, -1, -1, -1);
    //public static PairAnimalId PLAY_INSTANT_SIDE_MOVE = new(-5, -1, -1, -1);
    //public static PairAnimalId TRY_TO_PLAY_INSTANT_SIDE_MOVE = new(-6, -1, -1, -1);
    public AnimalId first;
    public AnimalId second;

    public PairAnimalId(in AnimalId first, in AnimalId second) 
    {
        this.first = first;
        this.second = second;
    }

    public PairAnimalId(int owner1, int local1, int owner2, int local2) 
    {
        first = new(owner1, local1);
        second = new(owner2, local2);
    }

    public bool Equals(PairAnimalId other)
    {
        return other.first.Equals(first) && other.second.Equals(second);
    }
}



[BurstCompile(DisableDirectCall = true)]
public struct VGMstruct
{
    public PlayerManangerStruct playerMananger;

    public DeckStruct deck;

    public FoodManangerStruct foodMananger;

    public int currentPivot;

    public int currentPhase;

    private int _currentTurn;

    public int _currentSideTurn;
    public int currentTurn => (_currentSideTurn == -1) ? _currentTurn : _currentSideTurn;
    public bool isOver => currentPhase == -1;
    public bool isSideTurns => _currentSideTurn != -1;
    private PairAnimalId _sideTurnsInfo;

    public VGMstruct(in PlayerManangerStruct pm, in DeckStruct deck,  in FoodManangerStruct foodMananger, int currentPivot, int currentPhase, int currentTurn, int currentSideTurn)
    {
        playerMananger = pm;
        this.deck = deck;
        this.foodMananger = foodMananger;
        this.currentPivot = currentPivot;
        this.currentPhase = currentPhase;
        _currentSideTurn = currentSideTurn;
        _currentTurn = currentTurn;
        _sideTurnsInfo = new PairAnimalId();
    }


    public int GetWinner()
    {
        return playerMananger.GetWinner();
    }


    internal void NextTurn(int sideTurn = -1)
    {
        if(sideTurn != -1)
        {
            _currentSideTurn = sideTurn;
            return;
        }
        if(_currentSideTurn != -1)
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

 

    internal void AddPropToAnimal(int playerId, in CardStruct card, in AnimalId target, bool isRotated)
    {
        bool isAddedSuccesful = playerMananger.AddPropToAnimal(playerId, card, target, isRotated);
        if (isAddedSuccesful) NextTurn();
    }

    internal void CreateAnimal(int playerId, in CardStruct card)
    {
        bool isCreatedSuccesful = playerMananger.CreateAnimal(playerId, card);
        if (isCreatedSuccesful) NextTurn();
    }

    internal void Feed(int playerId, in AnimalId target)
    {
        if (foodMananger.food <= 0) throw new Exception("Trying to feed without food");
        int isFeeded = playerMananger.Feed(playerId, target, foodMananger);
        if (isFeeded > 0)
        {
            NextTurn();
            foodMananger.Consume(isFeeded);
        }
    }

    internal void Pass(int playerId)
    {
        playerMananger.Pass(playerId);
        NextTurn();
    }


    internal void PlayProp(int playerId, in CardStruct card, in AnimalId target1, in AnimalId target2, bool isRotated=false)
    {
        _sideTurnsInfo = GameInteractionStruct.PlayProp(playerMananger, playerId, card, target1, target2, isRotated);
        if(_sideTurnsInfo.Equals(PairAnimalId.DOING_NEXT_TURN))
        {
            NextTurn(); return;
        }
        if (_sideTurnsInfo.Equals(PairAnimalId.NOT_DOING_NEXT_TURN)) return;
        if(_sideTurnsInfo.Equals(PairAnimalId.DESTROY_FOOD)) { foodMananger.Consume(1); return; }
        if(_sideTurnsInfo.first.Equals(PairAnimalId.PIRACY_FOOD.first))
        {
            playerMananger.players[_sideTurnsInfo.second.ownerId].animalArea.spots[_sideTurnsInfo.second.localId].animal.DecreaseFood();
            return;
        }
        NativeList<MoveStruct> moves = GetAllPossibleSidesMoves(_sideTurnsInfo);
        if(moves.Length == 1)
        {
            MoveStruct.ExecuteMove(ref this, moves[0]);
        } 
        NextTurn(_sideTurnsInfo.second.ownerId);

    }

    internal void PlaySideProp(int playerId, in AnimalProp prop, in AnimalId friendId, in AnimalId enemyId)
    {
        switch (prop.name)
        {
            case AnimalPropName.Fast:
                playerMananger.players[playerId].animalArea.spots[friendId.localId].animal.ActivateFastProp();
                int diceResult = GetDiceResult();
                if (diceResult >= 4)
                {
                    _sideTurnsInfo = PairAnimalId.NULL;
                    return;
                }
                if(GetAllPossibleSidesMoves(_sideTurnsInfo).Length == 0)
                {
                    playerMananger.KillById(enemyId, friendId);
                }
                break;
            case AnimalPropName.Mimic:
                playerMananger.players[playerId].animalArea.spots[friendId.localId].animal.ActivateMimicProp();
                _sideTurnsInfo.second = prop.secondAnimalId;
                break;
            case AnimalPropName.DropTail:
                playerMananger.players[playerId].animalArea.spots[friendId.localId].animal.ActivateDropTailProp();
                _sideTurnsInfo = PairAnimalId.NULL;
                playerMananger.players[enemyId.ownerId].animalArea.spots[enemyId.localId].animal.Feed();
                AnimalProp targetProp = prop.secondAnimalId.ownerId == 0 ?
                    playerMananger.players[playerId].animalArea.spots[friendId.localId].animal.props.singles[prop.secondAnimalId.localId] :
                    playerMananger.players[playerId].animalArea.spots[friendId.localId].animal.props.pairs[prop.secondAnimalId.localId];
                playerMananger.players[playerId].animalArea.spots[friendId.localId].animal.RemoveProp(targetProp);
                break;
        }
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
        for (int i = 0; i < playerMananger.players.Length; i++)
        {
            playerMananger.players[i].animalArea.StartSurvivingPhase();
        }
    }


    private void StartPreDevelopPhase()
    {
        for (int i = currentPivot; i <= currentPivot + playerMananger.players.Length; i++)
        {
            int cardAmount = playerMananger.players[i % playerMananger.players.Length].animalArea.amount + 1;
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
        for(int i = 0; i < playerMananger.players[currentTurn].animalArea.spots[myAnimalId.localId].animal.props.singlesLength; i++)
        {
            AnimalProp prop = playerMananger.players[currentTurn].animalArea.spots[myAnimalId.localId].animal.props.singles[i];
            if(!prop.IsActivable) continue;
            if(prop.name == AnimalPropName.Fast)
            {
                moves.Add(MoveStruct.GetResponceToAttackMove(currentTurn, myAnimalId, enemyId, prop));
            } else if(prop.name == AnimalPropName.DropTail)
            {
                for(int j = 0; j < playerMananger.players[currentTurn].animalArea.spots[myAnimalId.localId].animal.props.singlesLength;j++)
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

            } else if(prop.name == AnimalPropName.Mimic)
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

        for(int i = 0; i <  playerMananger.players.Length; i++)
        {
            for(int j = 0; j < playerMananger.players[i].animalArea.amount; j++)
            {
                if(i==currentTurn)
                {
                    friendSpots.Add(new(i, j));
                    for (int k = j; k < playerMananger.players[i].animalArea.amount; k++)
                        pairFriendSpots.Add(new(i,j, i, k)); 
                } else
                {
                    enemySpots.Add(new(i, j));
                    for (int k = j; k < playerMananger.players[i].animalArea.amount; k++)
                        pairEnemySpots.Add(new(i, j, i, k));
                }
            }
        }


        NativeList<MoveStruct> moves = new NativeList<MoveStruct>(64, Allocator.TempJob);
        switch (currentPhase) {
            case 0:
                for(int i = 0; i < playerMananger.players[currentTurn].hand.amount; i++)
                {
                    moves.Add(MoveStruct.GetCreateAnimalMove(currentTurn, playerMananger.players[currentTurn].hand.cards[i]));
                    if (playerMananger.players[currentTurn].hand.cards[i].main.isHostile())
                    {
                        if(playerMananger.players[currentTurn].hand.cards[i].main.isPair)
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
                        } else
                        {
                            for (int j = 0; j < enemySpots.Length; j++)
                            {
                                bool isPossibleToAdd = playerMananger.players[enemySpots[j].ownerId].animalArea.spots[enemySpots[j].localId].
                                    IsPossibleToAddProp(playerMananger.players[currentTurn].hand.cards[i].main);
                                if (!isPossibleToAdd) continue;
                                moves.Add(MoveStruct.GetAddPropMove(currentTurn, playerMananger.players[currentTurn].hand.cards[i], enemySpots[j], AnimalId.NULL, false));
                            }
                        }
                        
                    } else
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
                for(int i = 0; i < playerMananger.players[currentTurn].animalArea.amount; i++)
                {
                    bool isFull = playerMananger.players[currentTurn].animalArea.spots[i].animal.isFull();
                    if (!isFull) moves.Add(MoveStruct.GetFeedMove(currentTurn, new(currentTurn, i)));
                }
                if (moves.Length == 0) moves.Add(MoveStruct.GetPassMove(currentTurn));
                PropId propId = new PropId();
                while(!propId.isNull())
                {
                    propId = playerMananger.players[currentTurn].GetNextInteractablPropId(propId);
                    if(propId.isNull()) break;
                    AnimalProp prop = playerMananger.players[currentTurn].animalArea.spots[propId.spotlId].animal.props.singles[propId.proplId];
                    if (prop.name == AnimalPropName.Predator)
                    {
                        for (int i = 0; i < enemySpots.Length; i++)
                        {
                            if(GameInteractionStruct.IsCanAttack(playerMananger.players[currentTurn].animalArea.spots[propId.spotlId].animal, 
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
        var random = new Unity.Mathematics.Random(10);

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
        return UnityEngine.Random.Range(1, 7);
    }


}


























//const int FIRST_TURN_CARDS_AMOUNT = 8;

//public delegate void SetupPhaseDG(int phase, VGMstruct t);
//private SetupPhaseDG setupPhaseDG;
//private IPlayerMananger _playerMananger;
//private IDeck _deck;
//private IFoodMananger _foodMananger;
//private AnimalPropInteractionMananger _animalPropInteractionMananger;
//private int _currentPivot;
//private int _currentPhase;
//private int _currentTurn;
//private int _currentSideTurn;

//public IPlayerMananger playerMananger => _playerMananger;
//public IDeck deck => _deck;
//public IFoodMananger foodMananger => _foodMananger;
//public int currentPivot => _currentPivot;
//public int currentPhase => _currentPhase;
//public int currentTurn => _currentTurn;
//public int currentSideTurn => _currentSideTurn;

//private static SetupPhaseDG GetDefaultSetupPhaseDG()
//{
//    return (phase, t) =>
//    {
//        if (phase == 1)
//        {
//            t.foodMananger.food = (UnityEngine.Random.Range(1, 7) + UnityEngine.Random.Range(1, 7) + 2);
//        }
//        else if (phase == 2)
//        {
//            t.StartSurvivingPhase();
//            if (t.deck.GetCards().Count == 0) t._currentPhase = -1;
//            else t.StartPreDevelopPhase();
//        }
//    };
//}

//public VGMstruct Copy()
//{
//    return new VGMstruct(this);
//}

//public VGMstruct(VirtualGameMananger vgm)
//{
//    _playerMananger = vgm.playerMananger;
//    _deck = vgm.deck;
//    _foodMananger = vgm.foodMananger;
//    _currentPivot = vgm.currentPivot;
//    _currentPhase = vgm.currentPhase;
//    _currentTurn = vgm.currentTurn;
//    _currentSideTurn = vgm.currentSideTurn;
//    setupPhaseDG = VGMstruct.GetDefaultSetupPhaseDG();
//    _animalPropInteractionMananger = null;
//}

//public VGMstruct(VGMstruct vgm) : this()
//    {
//    _playerMananger = vgm.playerMananger;
//    _deck = vgm.deck;
//    _foodMananger = vgm.foodMananger;
//    _currentPivot = vgm.currentPivot;
//    _currentPhase = vgm.currentPhase;
//    _currentTurn = vgm.currentTurn;
//    _currentSideTurn = vgm.currentSideTurn;
//    setupPhaseDG = vgm.setupPhaseDG;
//    _animalPropInteractionMananger = vgm._animalPropInteractionMananger;
//}

//public void Init()
//{
//    _animalPropInteractionMananger = new AnimalPropInteractionMananger(this);
//}

//public IPlayer GetCurrentPlayer()
//{
//    if (currentSideTurn != -1) return playerMananger.GetPlayers()[currentSideTurn];
//    return playerMananger.GetPlayers()[currentTurn];
//}


////Вычислить ход следующего игрока, или передать его другому
//public void NextTurn(int sideTurn = -1)
//{
//    if (sideTurn != -1)
//    {
//        _currentSideTurn = sideTurn;
//        return;
//    }
//    if (_currentSideTurn != -1)
//    {
//        _currentSideTurn = -1;
//        return;
//    }

//    int nextTurn = FindNextTurn();
//    if (nextTurn == -1) NextPhase();
//    else
//    {
//        if (currentPhase == 1 && nextTurn == currentPivot) foreach (var p in playerMananger.GetPlayers()) p.UpdateTurnCooldown();
//        _currentTurn = nextTurn;
//    }
//}

//public void NextPhase()
//{
//    if (currentPhase == -1) return;
//    if (currentPhase + 1 >= 3)
//    {
//        _currentPhase = 0;
//        _currentPivot = (currentPivot + 1) % playerMananger.GetPlayers().Length;
//        _currentTurn = currentPivot;

//    }
//    else
//    {
//        _currentPhase++;
//        _currentTurn = currentPivot;
//    }
//    foreach (var p in playerMananger.GetPlayers()) p.ResetPass();
//    if (currentPhase == 1) foreach (var p in playerMananger.GetPlayers()) p.UpdatePhaseCooldown();
//    setupPhaseDG(currentPhase, this);
//}


//private void StartSurvivingPhase()
//{
//    foreach (var p in playerMananger.GetPlayers())
//    {
//        foreach (var animalSpot in p.animalArea.animalSpots)
//        {
//            BaseAnimal animal = animalSpot.animal;
//            if (!animal.CanSurvive()) animalSpot.Remove();
//        }
//        p.animalArea.OrganizateSpots();
//    }
//}

//private void StartPreDevelopPhase()
//{
//    for (int i = currentPivot; i <= currentPivot + playerMananger.GetPlayers().Length; i++)
//    {
//        IPlayer player = playerMananger.GetPlayers()[i % playerMananger.GetPlayers().Length];
//        for (int j = 0; j < player.animalArea.animalSpots.Count + 1; j++)
//        {
//            if (deck.GetCards().Count > 0) deck.GiveLastCardToPlayer(player);
//        }

//    }
//    NextPhase();
//}

//private int FindNextTurn()
//{
//    int nextTurn = (currentTurn + 1) % playerMananger.GetPlayers().Length;
//    while (!playerMananger.GetPlayers()[nextTurn].IsAbleToMove())
//    {
//        nextTurn = (nextTurn + 1) % playerMananger.GetPlayers().Length;
//        if (nextTurn == currentTurn && !playerMananger.GetPlayers()[nextTurn].IsAbleToMove()) return -1;
//    }
//    return nextTurn;
//}

//public bool InteractProp(IPlayer player, ICard card)
//{
//    if (currentPhase != 1) return false;
//    BaseAnimal linkedAnimal = player.animalArea.GetLinkedAnimalByCard(card);
//    if (linkedAnimal == null) return false;
//    List<AnimalProp> interactableProps = AnimalPropInteractionMananger.GetInteractablePropsIdle(linkedAnimal);
//    bool isInteractable = AnimalPropInteractionMananger.IsInteractableCard(card, interactableProps);
//    if (!isInteractable) return false;
//    if (card.currentProp.name == AnimalPropName.Sleep)
//        return _animalPropInteractionMananger.InteractSleep(player, card, linkedAnimal);
//    if (card.currentProp.name == AnimalPropName.Fasciest)
//        return _animalPropInteractionMananger.InteractFasciest(player, card, linkedAnimal);
//    if (card.currentProp.name == AnimalPropName.Predator)
//        return _animalPropInteractionMananger.ActivatePredator(player, card, linkedAnimal);
//    return false;
//}

//public bool IsOver()
//{
//    return currentPhase == -1;
//}

//public int GetWinner()
//{
//    List<int> scores = GetScores();
//    return scores.IndexOf(scores.Max());
//}

//public List<int> GetScores()
//{
//    List<int> scores = new();
//    foreach (var player in playerMananger.GetPlayers())
//    {
//        scores.Add(player.animalArea.GetScore());
//    }
//    return scores;
//}

//public override string ToString()
//{
//    return $"VGMSTRUCT_ph{currentPhase}t{currentTurn}_{deck}";
//}


//public List<VirtualMove> GetAllPossibleMoves()
//{
//    List<VirtualMove> moves = new();
//    IPlayer player = GetCurrentPlayer();
//    List<Pair<BaseAnimal>> hostileAnimalSingle = GetAllHostileAnimals(player, 0);
//    List<Pair<BaseAnimal>> hostileAnimalPair = GetAllHostileAnimals(player, 1);
//    List<Pair<BaseAnimal>> friendlyAnimalSingle = GetAllFriendlyAnimals(player, 0);
//    List<Pair<BaseAnimal>> friendlyAnimalPair = GetAllFriendlyAnimals(player, 1);
//    if (currentPhase == 0)
//    {
//        foreach (var card in player.hand.GetCards())
//        {
//            VirtualMove createAnimalMove = VirtualMove.GetCreateAnimalMove(GetCurrentPlayer(), card);
//            moves.Add(createAnimalMove);

//            Pair<List<Pair<BaseAnimal>>> animalCollections = new(null, null);
//            if (IsHostileProp(card.mainProp))
//            {
//                animalCollections.first = card.mainProp.isPair ? hostileAnimalPair : hostileAnimalSingle;
//            }
//            else
//            {
//                animalCollections.first = card.mainProp.isPair ? friendlyAnimalPair : friendlyAnimalSingle;
//            }
//            if (card.secondProp != null && IsHostileProp(card.secondProp))
//            {
//                animalCollections.second = card.secondProp.isPair ? hostileAnimalPair : hostileAnimalSingle;
//            }
//            else if (card.secondProp != null)
//            {
//                animalCollections.second = card.secondProp.isPair ? friendlyAnimalPair : friendlyAnimalSingle;
//            }
//            moves.AddRange(GetAllAddPropCardMoves(card, player, animalCollections));
//        }
//        moves.Add(VirtualMove.GetPassMove(GetCurrentPlayer()));
//    }
//    else if (currentPhase == 1)
//    {
//        List<ICard> cards = player.animalArea.GetAllInteractableCards(this);
//        foreach (var card in cards)
//        {
//            if (card.currentProp.name != AnimalPropName.Predator)
//            {
//                moves.Add(VirtualMove.GetPlayPropMove(player, card, new Pair<BaseAnimal>(null, null)));
//            }
//            else
//            {
//                BaseAnimal predator = player.animalArea.GetLinkedAnimalByCard(card);
//                foreach (var target in hostileAnimalSingle)
//                {
//                    if (!AnimalPropInteractionMananger.IsCanAttack(predator, target.first)) continue;
//                    moves.Add(VirtualMove.GetPlayPropMove(player, card, target));
//                }
//            }
//        }
//        if (foodMananger.food > 0)
//        {
//            int movesCount = moves.Count;
//            foreach (var pAnimal in friendlyAnimalSingle)
//            {
//                BaseAnimal animal = pAnimal.first;
//                if (!animal.isFull())
//                    moves.Add(VirtualMove.GetEatFoodPropMove(player, animal));
//            }
//            //Ну то есть все животинки покормлены то открываем опцию паса
//            if (moves.Count == movesCount)
//            {
//                moves.Add(VirtualMove.GetPassMove(GetCurrentPlayer()));
//            }
//        }
//        else
//        {
//            moves.Add(VirtualMove.GetPassMove(GetCurrentPlayer()));
//        }

//    }

//    return moves;
//}



//private List<VirtualMove> GetAllAddPropCardMoves(ICard card, IPlayer player, Pair<List<Pair<BaseAnimal>>> animalCollections)
//{
//    List<VirtualMove> virtualMoves = new();

//    foreach (var animalPair in animalCollections.first)
//    {
//        VirtualMove move1 = VirtualMove.GetAddPropMove(player, card, animalPair);
//        if (!move1.isNull()) virtualMoves.Add(move1);
//    }
//    if (animalCollections.second == null) return virtualMoves;
//    foreach (var animalPair in animalCollections.second)
//    {
//        VirtualMove move2 = VirtualMove.GetAddPropMove(player, card, animalPair, true);
//        if (!move2.isNull()) virtualMoves.Add(move2);
//    }
//    return virtualMoves;

//}

//private bool IsHostileProp(AnimalProp prop)
//{
//    if (prop.name == AnimalPropName.Parasite) return true;
//    return false;
//}

//private List<Pair<BaseAnimal>> GetAllHostileAnimals(IPlayer player, int isPair)
//{
//    List<Pair<BaseAnimal>> animals = new();
//    foreach (var p in playerMananger.GetPlayers())
//    {
//        if (p.id != player.id)
//        {
//            for (int i = 0; i < p.animalArea.animalSpots.Count; i++)
//            {
//                BaseAnimal first = p.animalArea.animalSpots[i].animal;
//                if (first == null) continue;
//                Pair<BaseAnimal> pair = new(first, null);
//                for (int j = i + 1; j < p.animalArea.animalSpots.Count * isPair; j++)
//                {
//                    BaseAnimal second = p.animalArea.animalSpots[j].animal;
//                    if (second == null) continue;
//                    pair.second = second;
//                    animals.Add(new Pair<BaseAnimal>(first, pair.second));
//                }
//                if (isPair == 0) animals.Add(pair);
//            }
//        }
//    }
//    return animals;
//}

//private List<Pair<BaseAnimal>> GetAllFriendlyAnimals(IPlayer player, int isPair)
//{
//    List<Pair<BaseAnimal>> animals = new();

//    for (int i = 0; i < player.animalArea.animalSpots.Count; i++)
//    {
//        BaseAnimal first = player.animalArea.animalSpots[i].animal;
//        if (first == null) continue;
//        Pair<BaseAnimal> pair = new(first, null);
//        for (int j = i + 1; j < player.animalArea.animalSpots.Count * isPair; j++)
//        {
//            pair.second = player.animalArea.animalSpots[j].animal;
//            if (pair.second == null) continue;
//            animals.Add(new Pair<BaseAnimal>(first, pair.second));
//        }
//        if (isPair == 0) animals.Add(pair);
//    }

//    return animals;
//}

//public bool MakeRandomMovesUntilTerminate(int targetPlayer)
//{
//    int k = 0;

//    while (!IsOver())
//    {
//        List<VirtualMove> moves = GetAllPossibleMoves();
//        int randomIndex = UnityEngine.Random.Range(0, moves.Count);
//        VirtualMove randomMove = moves[randomIndex];
//        randomMove.GetMoveFunction()(this);
//        k++;
//        if (k > 1600) throw new Exception("Бесконечная игра");
//    }

//    return targetPlayer == GetWinner();
//}
