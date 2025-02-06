using GameAI.Algorithms.MonteCarlo;
using GameAI.GameInterfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Unity.Collections;
using UnityEditorInternal.VR;
using UnityEngine;
using SRandom = Unity.Mathematics.Random;

public enum SideTurnType
{
    SuccessAttack, UnSuccessAttack, NotSideRegular, NotSideDestroyFood, NotSidePiracy, WaitingSide, AfterDropTail, Error
}

public struct SideTurnInfo
{
    public SideTurnType type;
    public AnimalId first;
    public AnimalId second;

    public SideTurnInfo(SideTurnType type)
    {
        this.type = type;
        first = AnimalId.NULL;
        second = AnimalId.NULL;
    }

    public SideTurnInfo(SideTurnType type, AnimalId first, AnimalId second) 
    {
        this.type = type;
        this.first = first;
        this.second = second;
    }

    public static SideTurnInfo GetNotSideRegularInfo()
    {
        return new SideTurnInfo(SideTurnType.NotSideRegular);
    }

    public static SideTurnInfo GetNotSideDestroyFood()
    {
        return new SideTurnInfo(SideTurnType.NotSideDestroyFood);
    }

    public static SideTurnInfo GetNotSidePiracy(in AnimalId first, in AnimalId second)
    {
        return new SideTurnInfo(SideTurnType.NotSidePiracy, first, second);
    }

    internal static SideTurnInfo GetSuccessAttackInfo()
    {
        return new SideTurnInfo(SideTurnType.SuccessAttack);
    }

    public static SideTurnInfo GetWaitingSideInfo(in AnimalId predator, in AnimalId victim)
    {
        return new SideTurnInfo(SideTurnType.WaitingSide, predator, victim);
    }

    internal static SideTurnInfo GetUnsuccessAttackInfo(AnimalId enemyId, AnimalId friendId)
    {
        return new SideTurnInfo(SideTurnType.UnSuccessAttack, enemyId, friendId);
    }

    public static SideTurnInfo GetAfterDropTailInfo()
    {
        return new SideTurnInfo(SideTurnType.AfterDropTail);
    }

    public static SideTurnInfo GetErrorInfo() {
        return new SideTurnInfo(SideTurnType.Error);
    }
}

public enum MoveType { Pass, PlayProp, CreateAnimal, AddPropToAnimal, Feed, ResponseToAttack}

public class VGame : RandomSimulation<VGame, VMove, VPlayer>.IGame
{
    [JsonProperty] private int s = 0;
    private SRandom random;
    public VPlayerMananger vPM;
    public VMove m;

    public List<CardStruct> deck;
    public int food;

    public int currentPivot;
    public int currentPhase;
    [JsonProperty] private int _currentTurn;
    [JsonProperty] private int _currentSideTurn;
    public int currentTurn => (_currentSideTurn == -1) ? _currentTurn : _currentSideTurn;
    public bool isSideTurns => _currentSideTurn != -1;
    VPlayer ICurrentPlayer<VPlayer>.CurrentPlayer => vPM.GetCurrentPlayer(currentTurn);
    private string debugJson => CompressString(ToJson());

    [JsonProperty] private SideTurnInfo _sideTurnsInfo;



    public VGame(int currentPivot, int currentPhase, int currentTurn, int currentSideTurn, VPlayerMananger vPM,
                List<CardStruct> deck, int food, SideTurnInfo sideTurnsInfo)
    {
        this.currentPivot = currentPivot;
        this.currentPhase = currentPhase;
        this._currentSideTurn = currentSideTurn;
        this._currentTurn = currentTurn;
        this._sideTurnsInfo = sideTurnsInfo;
        this.food = food;
        this.deck = deck;
        this.vPM = vPM;
        random = new(10u);
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
        m = move;
        //SaveJson();
        switch (move.data.type)
        {
            case MoveType.Pass:
                Pass(move.data.playerId); break;
            case MoveType.CreateAnimal:
                CreateAnimal(move.data.playerId, move.data.card); break;
            case MoveType.AddPropToAnimal:
                AddPropToAnimal(move.data.playerId, move.data.card, move.data.target1, move.data.target2, move.data.isRotated); break;
            case MoveType.Feed:
                Feed(move.data.playerId, move.data.target1); break;
            case MoveType.PlayProp:
                PlayProp(move.data.playerId, move.data.prop, move.data.target1, move.data.target2); break;
            case MoveType.ResponseToAttack:
                PlaySideProp(move.data.playerId, move.data.prop, move.data.target1, move.data.target2); break;
        }
        s++;
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

    public bool AddPropToAnimal(int playerId, in CardStruct card, in AnimalId target, in AnimalId pairTarget, bool isRotated)
    {
        if (!(isRotated ? card.second : card.main).isHostile() && target.ownerId != playerId) 
            throw new Exception($"RuleBreaker as you like Add prop {card.ToFString()} isRotated = {isRotated} {this.ToString()} plId = {playerId}, target = {target.ToFString()} ");
        bool isAdded = vPM.AddPropToAnimal(card, target, pairTarget, isRotated);
        if (!isAdded) return false;
        if (isAdded)
        {
            NextTurn();
            vPM.RemoveCardFromHand(playerId, card);
            if (vPM.hands[playerId].Count == 0) vPM.players[playerId].isAbleToMove = false;
        }
        return true;
    }



    public bool CreateAnimal(int playerId, in CardStruct card)
    {
        bool isAdded = vPM.CreateAnimal(playerId, card);
        if (isAdded)
        {
            NextTurn();
            vPM.RemoveCardFromHand(playerId, card);
            if (vPM.hands[playerId].Count == 0) vPM.players[playerId].isAbleToMove = false;
        }
        return isAdded;
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


    public void PlayProp(int playerId, in AnimalProp prop, in AnimalId target1, in AnimalId target2)
    {
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
        if (_sideTurnsInfo.type == SideTurnType.SuccessAttack)
        {
            NextTurn(); return;
        }
        //if (_sideTurnsInfo.Equals(PairAnimalId.NOT_DOING_NEXT_TURN)) return;
        //if (_sideTurnsInfo.Equals(PairAnimalId.DESTROY_FOOD)) { food -= 1; return; }
        //if (_sideTurnsInfo.first.Equals(PairAnimalId.PIRACY_FOOD.first))
        //{
        //    vPM.DecreaseFood(_sideTurnsInfo.second, 1);
        //    return;
        //}
        //var sideMoves = GetAllPossibleSidesMoves(_sideTurnsInfo);
        //if (sideMoves.Count == 1)
        //{
        //    DoMove(sideMoves[0]);

        //}
        //NextTurn(_sideTurnsInfo.second.ownerId);
    }



    private SideTurnInfo PlaySleep(in AnimalId target)
    {
        return vPM.Play(target, AnimalPropName.Sleep);

    }

    private SideTurnInfo PlayFasciest(in AnimalId target)
    {
        return vPM.Play(target, AnimalPropName.Fasciest);
    }

    private SideTurnInfo PlayPiracy(in AnimalId pirate, in AnimalId victim)
    {
        var result = vPM.Play(pirate, AnimalPropName.Piracy);
        result.second = victim;
        return result;
    }

    private SideTurnInfo PlayPredator(in AnimalId predatorId, in AnimalId victimId)
    {

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
            return SideTurnInfo.GetSuccessAttackInfo();
        }
        else
        {
            sideProps.Dispose();
        }
        return SideTurnInfo.GetWaitingSideInfo(predatorId, victimId); 
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
                    _sideTurnsInfo = SideTurnInfo.GetUnsuccessAttackInfo(enemyId, friendId);
                    return;
                }
                var sideMoves = GetAllPossibleSidesMoves(_sideTurnsInfo);
                if (sideMoves.Count == 0)
                {
                    vPM.KillById(enemyId, friendId);
                    _sideTurnsInfo = SideTurnInfo.GetSuccessAttackInfo();
                } else
                {
                    _sideTurnsInfo = SideTurnInfo.GetWaitingSideInfo(enemyId, friendId);
                }

                break;
            case AnimalPropName.Mimic:
                friendSpot.animal.ActivateMimicProp();
                vPM.SetSpot(friendSpot);
                _sideTurnsInfo = SideTurnInfo.GetWaitingSideInfo(enemyId, prop.secondAnimalId);
                break;
            case AnimalPropName.DropTail:
                friendSpot.animal.ActivateDropTailProp();
                _sideTurnsInfo = SideTurnInfo.GetAfterDropTailInfo();

                enemySpot.animal.Feed();
                vPM.SetSpot(enemySpot);
                AnimalProp targetProp = prop.secondAnimalId.ownerId == 0 ?
                    friendSpot.animal.props.singles[prop.secondAnimalId.localId] :
                    friendSpot.animal.props.pairs[prop.secondAnimalId.localId];
                friendSpot.animal.RemoveProp(targetProp);
                vPM.SetSpot(friendSpot);
                break;
            case AnimalPropName.ERROR:
                _sideTurnsInfo = SideTurnInfo.GetErrorInfo();
                break;
        }
        if(_sideTurnsInfo.Equals(PairAnimalId.DOING_NEXT_TURN))
        {
            NextTurn();
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
            for(int j = 0; j < vPM.spots[playerId].Count; j++)
            {
                AnimalSpotStruct spot = vPM.spots[playerId][j];
                spot.animal.food = 0;
                vPM.SetSpot(spot);
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

    public static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }

    private string filePath;

    public static string CompressString(string text)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(text);
        using (var memoryStream = new MemoryStream())
        {
            using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gzipStream.Write(buffer, 0, buffer.Length);
            }
            return Convert.ToBase64String(memoryStream.ToArray());
        }
    }

    public static string DecompressString(byte[] compressedData)
    {
        using (var memoryStream = new MemoryStream(compressedData))
        {
            using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            {
                using (var decompressedStream = new MemoryStream())
                {
                    gzipStream.CopyTo(decompressedStream);
                    byte[] decompressedData = decompressedStream.ToArray();
                    return Encoding.UTF8.GetString(decompressedData);
                }
            }
        }
    }

    private void SaveJson()
    {

        string json = JsonConvert.SerializeObject(this, Formatting.Indented);
        //string baseJson = Base64Encode(json);
        // Записываем JSON строку в файл
        filePath = Path.Combine(Application.persistentDataPath, "vgame/s" + s.ToString() + ".json");
        File.WriteAllText(filePath, json);
       
    }

    private string ToJson()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented); 
    }


    public List<VMove> GetAllPossibleSidesMoves(SideTurnInfo sideTurnsInfo)
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

        if(sideMoves.Count == 0)
        {
            sideMoves.Add(VMove.GetResponceToAttackMove(myAnimalId.ownerId, myAnimalId, enemyId, AnimalProp.NULL));
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

                        bool canFeed = vPM.CanFeed(new(currentTurn, i));
                        if (canFeed) moves.Add(VMove.GetFeedMove(currentTurn, new(currentTurn, i)));
                    }
                if (moves.Count == 0) moves.Add(VMove.GetPassMove(currentTurn));
                PropId propId = new PropId(0, -1);
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
        prv.proplId++;
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

    internal void MakeRandomMove()
    {
        List<VMove> moves = GetLegalMoves();
        if (moves.Count == 0) 
            throw new Exception("WTF");
        VMove move = moves[random.NextInt(0, moves.Count)];
        DoMove(move);
        moves.Clear();
    }
}
