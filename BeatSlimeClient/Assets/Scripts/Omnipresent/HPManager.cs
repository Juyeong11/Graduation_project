using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPManager
{
    public int MaxHp;
    public int CurrentHP;
    public float prevHP;
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
        if (prevHP <= 0)
            isAlive = false;
    }

    public void hpUpdate(float deltaTime)
    {
        if (prevHP > CurrentHP)
        {
            prevHP -= 30 * deltaTime;
        }
    }
}
