using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffects{
    public float duration { get; protected set; }
    public float amount { get; protected set; }
    public statusType statusName{ get; protected set; }

    // constructor
    public StatusEffects(float duration, float amount, statusType statusName) 
    {
        this.duration = duration;
        this.amount = amount;
        this.statusName = statusName;
	}

    //used for decreasing by 1 each turn
    public void DecrementDuration()
    {
        this.duration -= 1;
    }

	// Update is called once per frame
	void Update () {
		
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