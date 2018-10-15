using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Added to units to alter stats / abilities
public class Module{
    public int equipped { get; protected set; }
    public ModuleName moduleName;

    //basic constructor (give name)
    public Module(ModuleName name)
    {
        equipped = 0;
        moduleName = name;
    }

    //increases how many of current module is equipped
    public void equip()
    {

        equipped += 1;
    }

    //unequips module
    //used for unequipping all modules of this type
    public void unequip()
    {
        equipped = 0;
    }
}

public enum ModuleName
{
    shortRange,
}

