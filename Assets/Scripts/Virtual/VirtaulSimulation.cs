using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;



public class VirtualSimulation
{
    public MoveStruct GetBestMove(Player player)
    {
        VGMstruct vgm = GameMananger.instance.GetStruct(player);
        NativeList<MoveStruct> moves = vgm.GetAllPossibleMoves();
        NativeArray<VGMstruct> vgms = new(1, Allocator.Persistent);
        vgms[0] = vgm;
        MoveStruct.ExecuteMove(vgm, moves[0]);
        List<CalculateMoveJob> jobs = new();
        List<JobHandle> jobHandles = new();
        for (int i = 0; i < moves.Length; i++)
        {

            CalculateMoveJob job = new CalculateMoveJob(vgm, moves[i], GameMananger.instance.currentTurn);
            jobs.Add(job);
            jobHandles.Add(job.Schedule());
        }
        foreach (var job in jobHandles)
        {
            job.Complete();
        }
        for (int i = 0; i < moves.Length; i++)
        {
            Debug.Log(jobs[i].rating);
        }
        return moves[0];
        

        //VirtualMove bestMove = moves[0];
        //for (int i = 0; i < moves.Count; i++)
        //{
        //    if (moves[i].rating > bestMove.rating) bestMove = moves[i];
        //}
        //return bestMove;
    }


}

[BurstCompile]
public struct CalculateMoveJob : IJob
{

    [ReadOnly] VGMstruct vgmStruct;
    [ReadOnly] MoveStruct vMove;
    [ReadOnly] int targetPlayer;
    public float rating;

    public CalculateMoveJob(VGMstruct vgmStruct, MoveStruct move, int targetPlayerId)
    {
        this.vgmStruct = vgmStruct;
        this.vMove = move;
        this.targetPlayer = targetPlayerId;
        rating = 0;
    }

    public void Execute()
    {
        int winRank = 0;
        int attempts = 100;
        for (int i = 0; i < attempts; i++)
        {
            VGMstruct vgmInit = vgmStruct;
            MoveStruct.ExecuteMove(vgmInit, vMove);
            winRank += vgmInit.MakeRandomMovesUntilTerminate(targetPlayer) ? 1 : 0;
        }
        rating = (winRank + 0f) / attempts;
    }
}