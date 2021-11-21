using System;
using UnityEngine;


public class GameVariable<T> : ScriptableObject
{
    [SerializeField] private T _value;
    public T Value
    {
        get => _value;
        set
        {
            if (isReadOnly)
                throw new Exception("Variable [" + this.name + "] is read-only.");
            else
                _value = value;
        }
    }

    public bool isReadOnly = false;
}

