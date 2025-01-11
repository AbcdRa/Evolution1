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

    internal void Pass()
    {
        isAbleToMove = false;
    }

    internal void ResetPass()
    {
        isAbleToMove = true;
    }
}