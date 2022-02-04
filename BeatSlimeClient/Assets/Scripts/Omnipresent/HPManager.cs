using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPManager
{
    public int MaxHp;
    public int CurrentHP;
    public int prevHP;
    public bool isAlive;

    public void Initialized(bool isBoss)
    {
        isAlive = true;
        if (isBoss)
        {
            MaxHp = 1000;
            CurrentHP = MaxHp;
            prevHP = CurrentHP;
        }
        else
        {
            MaxHp = 100;
            CurrentHP = MaxHp;
            prevHP = CurrentHP;
        }

    }
    public void Damage(int damage)
    {
        CurrentHP -= damage;
        //Debug.Log("Damaged");
        if (CurrentHP <= 0)
            isAlive = false;
    }

    public void hpUpdate()
    {
        if (prevHP > CurrentHP)
        {
            prevHP -= 3;
        }
    }
}
