using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory
{
    static public int actualMoney;

    static public void EarnMoney(int amountEarned)
    {
        actualMoney += amountEarned;
        WinCondition.Instance.myAnalytics.moneyEarned += amountEarned;
    }

    static public void LooseMoney(int amountEarned)
    {
        actualMoney -= amountEarned;
    }

    static public int GetActualMoney()
    {
        return actualMoney;
    }
}
