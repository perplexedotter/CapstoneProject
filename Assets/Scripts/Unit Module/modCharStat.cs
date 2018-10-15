using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//class for modified stats
public class modCharStat{

    public readonly float Stat;
    public readonly StatIdentifier Type;
    public readonly int Order;
    public readonly object Source;

    public modCharStat(float value, StatIdentifier type, int order, object source)
    {
        Stat = value;
        Type = type;
        Order = order;
        Source = source;
    }
    public modCharStat(float value, StatIdentifier type) : this(value, type, (int)type, null) { }
    public modCharStat(float value, StatIdentifier type, int order) : this(value, type, order, null) { }
    public modCharStat(float value, StatIdentifier type, object source) : this(value, type, (int)type, source) { }

}
