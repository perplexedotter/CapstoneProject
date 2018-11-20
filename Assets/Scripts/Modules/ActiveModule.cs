using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveModule : Module {



    [SerializeField] protected Target Target;
    [SerializeField] protected ActionType ActionType;
    [SerializeField] protected int Range;
    [SerializeField] protected int Power;

    [SerializeField] protected ParticleSystem particleSystem;

    public void Start()
    {
        particleSystem = GetComponentInChildren<ParticleSystem>();
    }

    public override Action GetAction()
    {
        return new Action(ActionType, Target, Power, Range);
    }

    public void SetActive(Target Target, ActionType ActionType, int Range, int Power, int mass, ModuleType type, int value, int threat)
    {
        this.Target = Target;
        this.ActionType = ActionType;
        this.Range = Range;
        this.Power = Power;
        this.mass = mass;
        this.type = type;
        this.value = value;
        this.threat = threat;
    }

    //This function will activate a modules particle system if it is present 
    //and the correct action type is passed
    public override void DisplayAction(Action action, Transform target)
    {
        if(ActionType == action.Type && particleSystem)
        {
            //var emission = particleSystem.emission;
            if (target)
                gameObject.transform.LookAt(target);
            particleSystem.Play();
        }
    }
}
