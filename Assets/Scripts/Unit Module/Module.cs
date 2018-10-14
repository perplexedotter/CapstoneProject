using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Module{
    public int equipped = 0;
    public Module()
    {
        equipped = 0;
    }
    public void EquipShortRange(Unit c)
    {
        equipped += 1;
        //c.Health.AddModifier(new modCharStat(100, StatModType.Flat));
        c.Health.PlusModStat(new modCharStat(0.25f, StatIdentifier.Addition_Percent));
        //c.Health.AddModifier(new modCharStat(0.1f, StatModType.PercentMult));
        c.Attack.PlusModStat(new modCharStat(15, StatIdentifier.basic));
        c.Mass.PlusModStat(new modCharStat(100, StatIdentifier.basic));
        
    }

    public void UnequipShortRange(Unit c)
    {
        equipped = 0;
        c.Health.DeleteAllMods(this);
        c.Attack.DeleteAllMods(this);
        c.Mass.DeleteAllMods(this);
    }
}

