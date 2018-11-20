using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffects{
    public float Duration { get; protected set; }
    public float Amount { get; protected set; }
    public statusType Type{ get; protected set; }

    // constructor
    public StatusEffects(float duration, float amount, statusType statusName) 
    {
        this.Duration = duration;
        this.Amount = amount;
        this.Type = statusName;
	}

    //used for decreasing by 1 each turn
    public void DecrementDuration()
    {
        this.Duration -= 1;
    }
}

public enum statusType
{
    hitPoints,
    mass,
    speed,
    attack,
    shields,
    range,
}