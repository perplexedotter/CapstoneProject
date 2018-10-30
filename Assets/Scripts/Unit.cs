using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[SelectionBase]
public class Unit : MonoBehaviour {

    //Needed to Detect Child Collision
    [SerializeField] Collider collider;
    [SerializeField] Shader outlineShader;
    [SerializeField] Shader standardShader;

    [Header("Transform Properties")]
    [SerializeField] protected float moveSpeed = 75.0f;
    [SerializeField] Tile currentTile;

    [Header("Unit Stats")]
    [Tooltip("Which player controls this unit")]
    [SerializeField] protected int playerNumber = 0;
    [SerializeField] private bool aIUnit = false;
    [SerializeField] protected UnitType type;
    [SerializeField] protected float hitPoints = 100;
    [SerializeField] protected float damageTaken = 0;
    [SerializeField] protected float damageDealt = 0;
    [SerializeField] protected float attack = 0;
    [SerializeField] protected float speed = 100;
    [SerializeField] protected float mass = 400;
    [SerializeField] protected float shields = 0;
    [SerializeField] protected float hardPoints = 0;
    [SerializeField] private float threat;
    [SerializeField] protected int movementRange;
    [SerializeField] protected bool destroy = false;

    [Header("Modules and Statuses")]
    [SerializeField] protected List<Module> modules = new List<Module>();
    [SerializeField] protected List<StatusEffects> statuses = new List<StatusEffects>();

    //Movement fields
    private bool isMoving = false;
    protected bool movementFinished = false;
    private List<Tile> path;
    private int pathIndex;

    /**************************************** UNITY FUNCTIONS **********************************/

    void Awake()
    {

        //This code can get components instead of children
        /*
        foreach (MeleeAttackModule m in this.GetComponents<MeleeAttackModule>()){
            Debug.Log(m);
            modules.Add(m);
        }*/


        //gets children
        //Needs own function for supplying a small wait time
        //wait allows for components to be added before they are added to module list
        //StartCoroutine (GetChildren());
        modules = new List<Module>(GetComponentsInChildren<Module>());



    }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ProcessMovement();
    }


    /*********************************************** SETUP FUNCTIONS **********************************/

    IEnumerator GetChildren()
    {
        yield return new WaitForSeconds(.5f);
        Transform[] childModules = GetComponentsInChildren<Transform>();
        foreach (Transform child in childModules)
        {
            if (child.tag == "Melee")
            {
                MeleeAttackModule melee = child.GetComponent<MeleeAttackModule>();
                modules.Add(melee);
            }
            else if (child.tag == "Range")
            {
                RangeAttackModule range = child.GetComponent<RangeAttackModule>();
                modules.Add(range);
            }
            else if (child.tag == "Heal")
            {
                Debug.Log("k");
                HealModule heal = child.GetComponent<HealModule>();
                modules.Add(heal);
            }
            else if (child.tag == "Slow")
            {
                SlowModule slow = child.GetComponent<SlowModule>();
                modules.Add(slow);
            }
        }
    }

    //based on unit type provide this will give base stats to unit
    public void DefineUnit(UnitType Type)
    {
        //modules = new List<Module>();
        //statuses = new List<StatusEffects>();
        if (UnitType.fighter == Type)
        {
            hitPoints = 100;
            mass = 400;
            hardPoints = 2;
            speed = 100;
            damageTaken = 0;
            attack = 20;
            type = Type;
            shields = 0;
            threat = 20;
            damageDealt = 0;
        }

        if (UnitType.frigate == Type)
        {
            hitPoints = 200;
            mass = 600;
            hardPoints = 3;
            speed = 50;
            damageTaken = 0;
            attack = 20;
            type = Type;
            shields = 0;
            threat = 10;
            damageDealt = 0;
        }
    }


    /************************************** BASIC GETTERS / SETTERS ***********************************/
    public Tile CurrentTile
    {
        get
        {
            return currentTile;
        }

        set
        {
            currentTile = value;
        }
    }

    public bool IsMoving
    {
        get
        {
            return isMoving;
        }

        set
        {
            isMoving = value;
        }
    }

    public int PlayerNumber
    {
        get
        {
            return playerNumber;
        }
        set
        {
            playerNumber = value;
        }
    }

    public bool AIUnit
    {
        get
        {
            return aIUnit;
        }
    }

    //returns if unit should be destroyed or not
    public bool Destroyed
    {
        get
        {
            return destroy;
        }
    }

    public float GetDamage()
    {
        return damageTaken;
    }

    //returns total damage Dealt
    public float GetDamageDealt()
    {
        return damageDealt;
    }

    /**************************************CALCULATED GETTERS ***************************************/

    //calculate threat
    public float GetThreat()
    {
        float tempThreat = threat;

        for (int i = 0; i < modules.Count; i++)
        {
            if (modules[i].ModuleType == ModuleType.heal)
            {
                tempThreat += 30;
            }
            else if (modules[i].ModuleType == ModuleType.longRange)
            {
                tempThreat += 25;
            }
            else if (modules[i].ModuleType == ModuleType.shortRange)
            {
                tempThreat += 20;
            }
            else if (modules[i].ModuleType == ModuleType.slow)
            {
                tempThreat += 15;
            }
            else if (modules[i].ModuleType == ModuleType.engine)
            {
                tempThreat += 10;
            }
            else if (modules[i].ModuleType == ModuleType.shields)
            {
                tempThreat += 5;
            }
        }
        return tempThreat;
    }

    //returns list of actions
    //will combine actions into more powerful one
    //public List<Action> GetActions()
    //{
    //    List<Action> actions = new List<Action>();
    //    List<Action> actionsSorted = new List<Action>();

    //    for (int i = 0; i < modules.Count; i++)
    //    {
    //        actions.Add(modules[i].Action);
    //    }

    //    actionsSorted = actions.OrderBy(o => o.Type).ToList();
    //    for (int i = 0; i < actionsSorted.Count; i++)
    //    {
    //        if (i != actionsSorted.Count - 1)
    //        {
    //            if (actionsSorted[i].Type == actionsSorted[i + 1].Type)
    //            {
    //                actionsSorted[i + 1] = actionsSorted[i].Combine(actionsSorted[i + 1]);
    //                actionsSorted.RemoveAt(i);
    //                i -= 1;
    //            }
    //        }
    //    }
    //    //DebugActions(actionsSorted);
    //    return actionsSorted;
    //}

    public List<Action> GetActions()
    {
        Dictionary<ActionType, Action> actions = new Dictionary<ActionType, Action>();
        foreach(var m in modules)
        {
            Action a;
            Action action = m.GetAction();
            if (action != null){
                if(actions.TryGetValue(action.Type, out a)){
                    actions[action.Type] = action.Combine(a);
                }
                else
                {
                    actions.Add(action.Type, action);
                }
            }
        }
        return new List<Action>(actions.Values);
    }

    public List<ModuleType> GetModuleTypes()
    {
        HashSet<ModuleType> moduleTypes = new HashSet<ModuleType>();
        foreach (var m in modules)
        {
            moduleTypes.Add(m.ModuleType);
        }
        return new List<ModuleType>(moduleTypes);
    }

    //returns base hp + bonus from modules
    public float GetHP()
    {
        float moddedStat = hitPoints;

        for (int i = 0; i < modules.Count; i++)
        {
            moddedStat += modules[i].HitPoints;
        }

        for (int i = 0; i < statuses.Count; i++)
        {
            if (statuses[i].StatusName == statusType.hitPoints)
            {
                moddedStat += statuses[i].Amount;
            }
        }
        return moddedStat;
    }

    //returns base mass + bonus from modules
    public float GetMass()
    {
        float moddedStat = mass;

        for (int i = 0; i < modules.Count; i++)
        {
            moddedStat += modules[i].Mass;
        }

        for (int i = 0; i < statuses.Count; i++)
        {
            if (statuses[i].StatusName == statusType.mass)
            {
                moddedStat += statuses[i].Amount;
            }
        }
        return moddedStat;
    }

    /*********************************************** UNIT VALUE EFFECTING FUNCTIONS ***************************/

    //Unit will take damage
    public float DamageUnit(float dmg)
    {
        damageTaken += dmg;
        float currentHP = GetHP() - damageTaken;
        if(currentHP <= 0)
        {
            destroy = true;
        }
        return currentHP;
    }

    public float HealUnit(float healing)
    {
        float healedAmount = damageTaken;
        damageTaken -= healing;
        damageTaken = damageTaken < 0 ? 0 : damageTaken; //Don't let damageTaken go negative
        healedAmount = Mathf.Min(healedAmount, healing); //Determine the actual amount healed
        return healedAmount;
    }

    //Adds status to list
    public void AddStatus(StatusEffects status)
    {
        statuses.Add(status);
    }

    //decrease each status by one -- remove if duration reaches 0
    public void DecrementStatuses()
    {
        for (int i = 0; i < statuses.Count; i++)
        {
            statuses[i].DecrementDuration();
            if (statuses[i].Duration == 1)
            {
                statuses.RemoveAt(i);
                i -= 1;
            }
        }
    }

    //Adds module to unit
    public void AddModule(Module module)
    {
        //can only have modules up to number of hardpoints
        if (!(modules.Count >= hardPoints))
        {
            modules.Add(module);
        }
    }

    //remove specific module from list (if it is in list)
    public void RemoveModule(ModuleType module)
    {
        for(int i = 0; i < modules.Count; i++)
        {
            if(modules[i].ModuleType == module)
            {
                modules.RemoveAt(i);
                break;
            }
        }
    }

    //removes all modules from unit
    public void RemoveAllModules()
    {
        modules.Clear();
    }

    //add to damage dealt
    public void AddDamage(float damage)
    {
        damageDealt += damage;
    }

    /************************************** MOVEMENT FUNCTIONS ***********************************/

    private void ProcessMovement()
    {
        if (IsMoving)
        {
            ContinuePathTraversal();
        }
        if (movementFinished)
        {
            movementFinished = false;
            SendMessageUpwards("FinishedMovement");
        }
    }

    //returns movement after calculation based on unit mass
    public int GetMovementRange()
    {
        float moddedMass = GetMass();
        movementRange = (int)((1000 - moddedMass) / 100);

        if (movementRange <= 0)
        {
            movementRange = 1;
        }
        return movementRange;
    }


    public void TraversePath(List<Tile> path)
    {
        if(path != null)
        {
            this.path = path;
            isMoving = true;
            pathIndex = 0;
        }
    }

    private void ContinuePathTraversal()
    {
        if(currentTile != path[path.Count - 1]) //If the Unit hasn't reached the end of the path
        {
            if (currentTile != path[pathIndex]) //If the current Tile isn't the same as the next tile
            {
                Vector3 destination = path[pathIndex].transform.position;
                float step = moveSpeed * Time.deltaTime;
                //Vector3.RotateTowards(transform.position, destination);
                transform.LookAt(destination);
                transform.position = Vector3.MoveTowards(transform.position, destination, step);
                if (Vector3.Distance(destination, transform.position) <= 0)
                {
                    UpdateTile(path[pathIndex]);
                }
            }
            else
            {
                pathIndex++;
            }
        }
        else //Finish the units movement
        {
            UpdateTile(path[pathIndex]); //MakeSure the unit is on the tile
            movementFinished = true;
            isMoving = false;
        }
    }

    private void UpdateTile(Tile tile)
    {
        //Remove highlight
        Highlighter highlighter = GetComponent<Highlighter>();
        if (highlighter != null)
            highlighter.RemoveHighlight();
        if (currentTile != null && currentTile.UnitOnTile == this) //Don't remove units other then yourself from the tile
            currentTile.UnitOnTile = null;
        currentTile = tile;
        if(currentTile.UnitOnTile == null) //Only add yourself to an empty tile
            currentTile.UnitOnTile = this;
    }

    //Places the unit on a tile instantly (No Visual Movment)
    public void PlaceOnTile(Tile tile)
    {
        if(tile != null)
        {
            transform.position = tile.transform.position;
            UpdateTile(tile);
        }
    }

    public void FaceTile(Tile tile)
    {
        if(tile != null)
        {
            transform.LookAt(tile.transform);
        }
    }

    public Vector2Int GetCoords()
    {
        return new Vector2Int(
            (int)Mathf.Floor(transform.position.x / Tile.GridSize) * Tile.GridSize,
            (int)Mathf.Floor(transform.position.z / Tile.GridSize) * Tile.GridSize
        );
    }

    /********************************************* ANIMATION FUNCTIONS *****************************************/

    private void StartHighlightAnimation()
    {

    }

    private void StopHighlightAnimation()
    {

    }

    public void UnitOutline(bool outlined)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach(var r in renderers)
        {
            if (outlined)
            {
                r.material.shader = this.outlineShader;
            }
            else
            {
                r.material.shader = this.standardShader;
            }
        }
    }
    /******************************************** TEST FUNCTIONS ********************************************/

    //for testing getactions
    public void DebugActions(List<Action> debug)
    {
        for (int i = 0; i < debug.Count; i++)
        {
            Debug.Log(i + " " + debug[i].Type + " " + debug[i].Power);
        }
    }

    //for testing
    public void DebugStatuses()
    {
        for (int i = 0; i < statuses.Count; i++)
        {
            Debug.Log(statuses[i].StatusName + " " + statuses[i].Duration);
        }
    }

}

//used to show type of unit
public enum UnitType
{
    fighter,
    frigate,
}
