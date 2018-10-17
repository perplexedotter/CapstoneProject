using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Module {
    public int hitPoints { get; protected set; }
    public int attack { get; protected set; }
    public int mass { get; protected set; }
    public ModuleName moduleName { get; protected set; }

    //basic constructor (give name)
    public Module(ModuleName name)
    {
        //give stats to module based on type          
        if (name == ModuleName.shortRange)
        {
            hitPoints = 25;
            attack = 15;
            mass = 100;
        }
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

//used to name module
public enum ModuleName
{
    shortRange,
}