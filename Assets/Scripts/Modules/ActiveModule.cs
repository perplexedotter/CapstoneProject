using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveModule : Module {



    [SerializeField] protected Target Target;
    [SerializeField] protected ActionType ActionType;
    [SerializeField] protected int Range;
    [SerializeField] protected int Power;

    //[SerializeField] protected GameObject moduleEffectFX;
    [SerializeField] protected Vector3 effectFXLocation;
    //[SerializeField] protected GameObject fx;

    protected GameObject fx;

    public void Start()
    {
        fx = GameObject.Find("HealModuleFX");
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
    public override void DisplayAction(Action action)
    {
        if(ActionType == action.Type && fx)
        {
            var parent = transform.parent;
            List<ParticleSystem> parts = new List<ParticleSystem>( GetComponentsInChildren<ParticleSystem>());
            foreach(var p in parts)
            {
                p.Play();
            }
            //if(fx.activeInHierarchy)
            //    fx.SetActive(false);
            //fx.SetActive(true);
            //GetComponentInChildren<ParticleSystem>().Play();

            return;
        }
    }
}
