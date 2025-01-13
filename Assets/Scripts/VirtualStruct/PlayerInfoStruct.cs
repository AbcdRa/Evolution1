using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct PlayerInfo
{
    public bool isAbleToMove;
    public int id;
    public int animalAmount;

    public PlayerInfo(bool isAbleToMove, int id, int animalAmount)
    {
        this.id = id;
        this.isAbleToMove = isAbleToMove;
        this.animalAmount = animalAmount;
    }


    internal void Pass()
    {
        isAbleToMove = false;
    }

    internal void ResetPass()
    {
        isAbleToMove = true;
    }
}