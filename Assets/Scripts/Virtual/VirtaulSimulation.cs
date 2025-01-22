
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Jobs;
using UnityEngine.UIElements;



public class VirtualSimulation
{
    VGame vGame;
    public UnityEvent onMoveReady;

    public VirtualSimulation(Player player)
    {
        vGame = GameMananger.instance.GetVirtual(player);
        onMoveReady = new();
    }




    public void WaitingMove()
    {
        //List<VMove> moves = vGame.GetLegalMoves();
        //float[] ratings = new float[4];
        //for(int i = 0; i < 1000; i++)
        //{
        //    int j = 0;
        //    VGame vGameCopy = vGame.DeepCopy();
        //    while (!vGameCopy.IsGameOver())
        //    {
        //        vGameCopy.MakeRandomMove();
        //        if (j > 2000) throw new Exception("Inf game");
        //    }
        //}

        VMove move = GameAI.Algorithms.MonteCarlo.RandomSimulation<VGame, VMove, VPlayer>.Search(vGame, 100);
        Debug.Log("Finisging");
        onMoveReady.Invoke();
    }

}
