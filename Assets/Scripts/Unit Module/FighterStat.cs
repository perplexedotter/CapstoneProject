using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterStat : MonoBehaviour {
    public List<BaseStat> stats = new List<BaseStat>();
    void Start()
    {
        stats.Add(new BaseStat(100, "Health", "Amount of damage a unit can recieve before destruction"));
        stats[0].addStatBonus(new BonusStat(5));
        stats[0].addStatBonus(new BonusStat(15));
        Debug.Log(stats[0].getFinalValue());
    }
}
