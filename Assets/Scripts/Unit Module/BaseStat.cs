using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseStat{

    public List<BonusStat> baseStatPlus { get; set; }
    public int baseValue { get; set; }
    public int endValue { get; set; }
    public string statName { get; set; }
    public string statDesc { get; set; }

    public BaseStat(int base_Value, string stat_Name, string stat_Desc)
    {
        this.baseStatPlus = new List<BonusStat>();
        this.baseValue = base_Value;
        this.statName = stat_Name;
        this.statDesc = stat_Desc;
    }

    public void addStatBonus(BonusStat statBonus)
    {
        this.baseStatPlus.Add(statBonus);
    }

    public void removeStatBonus(BonusStat statBonus)
    {
        this.baseStatPlus.Remove(baseStatPlus.Find(x => x.extraStat == statBonus.extraStat));
    }

    public int getFinalValue()
    {
        this.endValue = 0;
        this.baseStatPlus.ForEach(x => this.endValue += x.extraStat);
        endValue += baseValue;
        return endValue;
    }
}
