using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Module : MonoBehaviour {
    public int HitPoints { get; protected set; }
    public int Attack { get; protected set; }
    public int Mass { get; protected set; }
    public int range { get; protected set; }
    public ModuleName ModuleName { get; protected set; }

    //basic constructor (give name)
    public Module(ModuleName name)
    {
        //give stats to module based on type          
        if (name == ModuleName.shortRange)
        {
            HitPoints = 25;
            Attack = 15;
            Mass = 100;
            range = 1;
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