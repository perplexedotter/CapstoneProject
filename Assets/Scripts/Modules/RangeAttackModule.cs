using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeAttackModule : Module {

    //basic constructor (give name)
    //We can change stats as necessary for balance
    public RangeAttackModule()
    {
        ModuleType = ModuleType.longRange;
        HitPoints = 0;
        Mass = 200;
        Action = new Action(ActionType.LongAttack, 25, 2);
    }

    public override Action GetAction()
    {
        return Action;
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
