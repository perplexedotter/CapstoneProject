using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveModule : Module {

    [SerializeField] protected Target Target;
    [SerializeField] protected ActionType ActionType;
    [SerializeField] protected int Range;
    [SerializeField] protected int Power;
    public override Action GetAction()
    {
        return new Action(ActionType, Target, Power, Range);
    }
}
