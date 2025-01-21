
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.UIElements;



public class VirtualSimulation
{
    VGame vGame;

    public VirtualSimulation(Player player)
    {
        vGame = GameMananger.instance.GetVirtual(player);
    }

    public IEnumerator WaitingMove()
    {
        VMove move = GameAI.Algorithms.MonteCarlo.RandomSimulation<VGame, VMove, VPlayer>.ParallelSearch(vGame, 10);
        Debug.Log(move);
        yield return null;
    }

}
