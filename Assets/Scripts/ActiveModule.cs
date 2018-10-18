using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveModule : Module {

    public override Action GetAction()
    {
        return action;
    }

    //basic constructor (give name)
    //We can change stats as necessary for balance
    public ActiveModule(ModuleName name)
    {
        ModuleName = name;

        //give stats to module based on type          
        if (name == ModuleName.shortRange)
        {
            HitPoints = 25;
            Mass = 100;
            action = new Action(ActionType.ShortAttack, 25, 1);
        }
        if (name == ModuleName.longRange)
        {
            HitPoints = 0;
            Mass = 200;
            action = new Action(ActionType.LongAttack, 25, 2);
        }
        if (name == ModuleName.heal)
        {
            HitPoints = 50;
            Mass = 100;
            action = new Action(ActionType.Heal, 20, 1);
        }
        if (name == ModuleName.slow)
        {
            HitPoints = 25;
            Mass = 100;
            action = new Action(ActionType.Slow, 0, 1);
        }
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
