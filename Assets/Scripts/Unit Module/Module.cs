using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Module{
    public int equipped { get; private set; }
    private modCharStat healthMod;
    private modCharStat attackMod;
    private modCharStat massMod;

    public Module()
    {
        equipped = 0;
    }
    public void EquipShortRange(Unit c)
    {
        equipped += 1;

        healthMod = new modCharStat(0.25f, StatIdentifier.Addition_Percent, 200, this);
        c.Health.PlusModStat(healthMod);
        //c.Health.PlusModStat(new modCharStat(0.1f, StatIdentifier.Multiply_Percent));

        attackMod = new modCharStat(15, StatIdentifier.basic, 100, this);
        c.Attack.PlusModStat(attackMod);

        massMod = new modCharStat(100, StatIdentifier.basic, 100, this);
        c.Mass.PlusModStat(massMod);
        
    }

    public void UnequipAll(Unit c)
    {
        c.shortRange1.equipped = 0;
        c.shortRange2.equipped = 0;
        c.Health.DeleteAllMods(this);
        c.Attack.DeleteAllMods(this);
        c.Mass.DeleteAllMods(this);
    }
}

public enum ModuleName
{
    shortRange,
}

