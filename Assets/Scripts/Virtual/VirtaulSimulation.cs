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
        MoveStruct.ExecuteMove(vgm, moves[0]);
        int result = vgm.MakeRandomMovesUntilTerminate(player.id) ? 1 : 0;
        return moves[0];

        //VirtualGameMananger vgm = VirtualGameMananger.Virtualize(GameMananger.Instance(), player);
        //List<VirtualMove> moves = vgm.GetAllPossibleMoves();
        //List<JobHandle> jobHandles = new();
        //for (int i = 0; i < moves.Count; i++) {

        //    NativeArray<VGMstruct> vgmStruct = new NativeArray<VGMstruct>(1, Allocator.TempJob);
        //    NativeArray<VirtualMove> vMove = new NativeArray<VirtualMove>(1, Allocator.TempJob);
        //    NativeArray<float> rating = new NativeArray<float>(1, Allocator.TempJob);
        //    NativeArray<int> targetPlayer = new NativeArray<int>(1, Allocator.TempJob);
        //    targetPlayer[0] = vgm.GetCurrentPlayer().id
        //    vgmStruct[0] = vgm.GetStruct();
        //    vMove[0] = moves[i];
        //    CalculateMoveJob job = new CalculateMoveJob() { vgmStruct = vgmStruct };
        //    jobs.Add(job);
        //    jobHandles.Add(job.Schedule());
        //}
        //foreach (var job in jobHandles) {
        //    job.Complete();
        //}
        //for (int i = 0; i < moves.Count; i++)
        //{
        //    moves[i].rating = jobs[i].rating;
        //}

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

    [ReadOnly] NativeArray<VGMstruct> vgmStruct;
    [ReadOnly] NativeArray<MoveStruct> vMove;
    [ReadOnly] NativeArray<int> targetPlayer;
    NativeArray<float> rating;


    public void Execute()
    {
        int winRank = 0;
        int attempts = 100;
        for (int i = 0; i < attempts; i++)
        {
            VGMstruct vgmInit = vgmStruct[0];
            MoveStruct.ExecuteMove(vgmInit, vMove[0]);
            winRank += vgmInit.MakeRandomMovesUntilTerminate(targetPlayer[0]) ? 1 : 0;
        }
        rating[0] = (winRank + 0f) / attempts;
    }
}