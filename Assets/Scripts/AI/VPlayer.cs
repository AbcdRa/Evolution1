using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class VPlayer
{
    public bool isAbleToMove;
    public int id;

    public VPlayer(bool isAbleToMove, int id)
    {
        this.isAbleToMove = isAbleToMove;
        this.id = id;
    }

    internal void Pass()
    {
        isAbleToMove = false;
    }

    internal void ResetPass()
    {
        isAbleToMove = true;
    }

    public override string ToString()
    {
        return $"p{id}{(isAbleToMove ? 'A':'P')}";
    }
}