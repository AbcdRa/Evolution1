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
        VGMstructXL vgm = GameMananger.instance.GetStruct(player);
        NativeList<MoveStruct> moves = vgm.GetAllPossibleMoves();

        List<CalculateMoveJob> jobs = new();
        List<JobHandle> jobHandles = new();
        for (int i = 0; i < 1; i++)
        {
            MoveStruct.ExecuteMove(ref vgm, moves[0]);
            CalculateMoveJob job = new CalculateMoveJob(vgm, moves[i], GameMananger.instance.currentTurn);
            jobs.Add(job);
            jobHandles.Add(job.Schedule());
        }
        foreach (var job in jobHandles)
        {
            job.Complete();
        }
        for (int i = 0; i < 1; i++)
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

[BurstCompile(DisableDirectCall = true)]
public struct CalculateMoveJob : IJob
{

    VGMstructXL vgmStruct;
    [ReadOnly] MoveStruct vMove;
    [ReadOnly] int targetPlayer;
    public float rating;

    public CalculateMoveJob(VGMstructXL vgmStruct, MoveStruct move, int targetPlayerId)
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
            VGMstructXL vgmInit = vgmStruct;
            MoveStruct.ExecuteMove(ref vgmInit, vMove);
            winRank += vgmInit.MakeRandomMovesUntilTerminate(targetPlayer) ? 1 : 0;
        }
        rating = (winRank + 0f) / attempts;
    }
}