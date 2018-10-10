using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusStat{
    public int extraStat { get; set; }
    public BonusStat(int extra_Stat)
    {
        this.extraStat = extra_Stat;
        Debug.Log("Extra Stat has been added");
    }
}
