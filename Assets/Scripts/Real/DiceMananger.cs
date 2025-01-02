using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DiceMananger : MonoBehaviour
{
    [SerializeField] private Dice dicePrefab;
    public UnityEvent OnDicesReadyForResult;
    private Dice[] dices;
    private int countWaitingDices = 0;
    private int result = 0;

    public int RollDiceForFood()
    {
        
        Dice dice1 = Instantiate<Dice>(dicePrefab);
        Dice dice2 = Instantiate<Dice>(dicePrefab);
        dices = new Dice[2] {dice1, dice2 };
        dice1.transform.position = Vector3.one;
        dice2.transform.position = Vector3.one;
        dice1.OnStopFlying.AddListener(UnWaitDice);
        dice2.OnStopFlying.AddListener(UnWaitDice);
        dice1.Roll();
        dice2.Roll();
        result = 2;
        countWaitingDices = 2;
        return 0;
    }


    public int GetResult()
    {
        return result;
    }

    public void UnWaitDice()
    {
        countWaitingDices--;
        if(countWaitingDices == 0)
        {
            foreach (Dice dice in dices)
            {
                result += dice.GetResult();
            }
            OnDicesReadyForResult.Invoke();
        }
    }
}
