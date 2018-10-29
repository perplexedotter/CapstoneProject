using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveModule : Module {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public override Action GetAction()
    {
        return new Action(ActionType, Target, Power, Range);
    }
}
