using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Module : MonoBehaviour {
    //public int HitPoints { get; protected set; }
    //public int Mass { get; protected set; }
    //public Target ModuleTarget { get; protected set; }
    //public ModuleType ModuleType { get; set; }
    public Action Action { get; protected set; }

    [SerializeField] protected ModuleType type;
    [SerializeField] protected int value;
    [SerializeField] protected int threat = 0;
    [SerializeField] protected int mass;

    public int Mass
    {
        get
        {
            return mass;
        }

        set
        {
            mass = value;
        }
    }

    public ModuleType Type
    {
        get
        {
            return type;
        }
    }

    public int Value
    {
        get
        {
            return value;
        }
    }

    public int Threat
    {
        get
        {
            return threat;
        }
    }

    //virtual GetAction
    public virtual Action GetAction()
    {
        return null;
    }

    public virtual void DisplayAction(Action action)
    {
        return;
    }

    public virtual Buff GetBuff()
    {
        return null;
    }

}

//used to name module
public enum ModuleType
{
    shortRange, longRange, heal, slow, engine, shields
}

//used for choosing target
