using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowModule : Module {

    //basic constructor (give name)
    //We can change stats as necessary for balance
    public SlowModule()
    {
        ModuleName = ModuleName.slow;
        HitPoints = 25;
        Mass = 100;
        action = new Action(ActionType.Slow, 0, 1);
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
