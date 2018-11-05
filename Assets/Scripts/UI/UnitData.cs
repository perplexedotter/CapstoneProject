using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//holds data for Unit class 
//Used by UnitSave class to store this data
[Serializable]
public class UnitData {

    public string unitName;
    public UnitType type;
    public ModuleType module1;
    public ModuleType module2;
    public ModuleType module3;
    public UnitData()
    {
        this.unitName = "";
        this.type = UnitType.none;
    }
    public UnitData(string unitName, UnitType type, ModuleType module1, ModuleType module2)
    {
        this.unitName = unitName;
        this.type = type;
        this.module1 = module1;
        this.module2 = module2;
    }

    public UnitData(string unitName, UnitType type, ModuleType module1, ModuleType module2, ModuleType module3)
    {
        this.unitName = unitName;
        this.type = type;
        this.module1 = module1;
        this.module2 = module2;
        this.module3 = module3;

    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
