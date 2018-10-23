using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackModule : Module {

    //basic constructor (give name)
    //We can change stats as necessary for balance
    public MeleeAttackModule()
    {
        ModuleName = ModuleName.shortRange;
        HitPoints = 25;
        Mass = 100;
        Action = new Action(ActionType.ShortAttack, 25, 1);
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
