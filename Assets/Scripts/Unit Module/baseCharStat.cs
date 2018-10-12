using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;


//Contains the base stats for unit
[System.Serializable]
public class baseCharStat{

    public float baseStat;
    private readonly List<modCharStat> statChange;
    protected bool beenModded = true;
    protected float _value;
    public readonly ReadOnlyCollection<modCharStat> StatChange;

    //allow user to see stat modifiers
    //make list of modifications
    public baseCharStat()
    {
        statChange = new List<modCharStat>();
        StatChange = statChange.AsReadOnly();
    }
    
    //constructor with input
    public baseCharStat(float baseValue) : this()
    {
        baseStat = baseValue;
    }
   
    //depending on type of stat modification change order
    private int CheckModOrder(modCharStat a, modCharStat b)
    {
        if (a.Order < b.Order)
            return -1;
        else if (a.Order > b.Order)
            return 1;
        return 0; // if (a.Order == b.Order)
    }

    //add stat to modded list
    public void PlusModStat(modCharStat mod)
    {
        beenModded = true;
        statChange.Add(mod);
        statChange.Sort(CheckModOrder);
    }

    //remove the modification
    public bool MinusModStat(modCharStat mod)
    {
        if (statChange.Remove(mod))
        {
            beenModded = true;
            return true;
        }
        return false;
    }

    //remove every modification
    public bool DeleteAllMods(object source)
    {
        bool removed = false;

        for (int i = statChange.Count - 1; i >= 0; i--)
        {
            if (statChange[i].Source == source)
            {
                beenModded = true;
                removed = true;
                statChange.RemoveAt(i);
            }
        }
        return removed;
    }

    //get value after adding base + modifications
    public float getModdedValue()
    {
        float moddedStat = baseStat;
        float addPercent = 0;
        for (int i = 0; i < statChange.Count; i++)
        {
            modCharStat mod = statChange[i];
            
            if (mod.Type == StatIdentifier.basic)
            {
                moddedStat += statChange[i].Stat;
            }
            else if (mod.Type == StatIdentifier.Multiply_Percent)
            {
                moddedStat *= 1 + mod.Stat;
            }
            else if (mod.Type == StatIdentifier.Addition_Percent)
            {
                addPercent += mod.Stat; 

                if (i + 1 >= statChange.Count || statChange[i + 1].Type != StatIdentifier.Addition_Percent)
                {
                    moddedStat *= 1 + addPercent; 
                    addPercent = 0;
                }
            }
        }
        return (float)Mathf.Round(moddedStat);
    }
}

//Type of stat modifier
public enum StatIdentifier
{
    //flat will add the number specified
    basic = 100,
    //Addition_Percent will take each percentage and add it together before using it on number 
    Addition_Percent = 200,
    //Multiply_Percent will take each percent and apply it independent of other multipliers
    Multiply_Percent = 300,
}
