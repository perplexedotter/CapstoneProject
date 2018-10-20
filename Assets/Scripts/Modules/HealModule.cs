using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealModule : Module {

    //basic constructor (give name)
    //We can change stats as necessary for balance
    public HealModule()
    {
        ModuleName = ModuleName.heal;
        HitPoints = 50;
        Mass = 100;
        action = new Action(ActionType.Heal, 20, 1);
    }

    public override Action GetAction()
    {
        return action;
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
