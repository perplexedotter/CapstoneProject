using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[SelectionBase]
public class Unit : MonoBehaviour {

    //Needed to Detect Child Collision
    [SerializeField] Collider collider;
    [SerializeField] Shader outlineShader;
    [SerializeField] Shader standardShader;

    [Header("Transform Properties")]
    [SerializeField] protected float moveSpeed = 75.0f;
    [SerializeField] protected float heightOffset = 2f;
    [SerializeField] Tile currentTile;


    [Header("Unit Stats")]
    [Tooltip("Which player controls this unit")]
    [SerializeField] protected int playerNumber = 0;
    [SerializeField] private bool aIUnit = false;
    [SerializeField] private bool bossUnit = false;
    [SerializeField] protected UnitType type;
    [SerializeField] protected float hitPoints = 100;
    [SerializeField] protected float damageTaken = 0;
    [SerializeField] protected float damageDealt = 0;
    [SerializeField] protected float attack = 0;
    [SerializeField] protected float speed = 100;
    [SerializeField] protected float mass = 400;
    [SerializeField] protected float shields = 0;
    [SerializeField] protected float hardPoints = 0;
    [SerializeField] protected int threat;
    [SerializeField] protected int value;
    [SerializeField] protected int movementRange;
    [SerializeField] protected bool destroy = false;

    [Header("Modules and Statuses")]
    [SerializeField] protected List<Module> modules = new List<Module>();
    [SerializeField] protected List<Vector3> modulePositions = new List<Vector3>();
    [SerializeField] protected List<StatusEffects> statuses = new List<StatusEffects>();


    //Movement fields
    private bool isMoving = false;
    protected bool movementFinished = false;
    public List<Tile> path;
    private int pathIndex;

    //Flags
    private bool meleeCapable = false;
    private bool longRangeCapable = false;
    private int longRangeCapability = -1;

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
        //modules = new List<Module>(GetComponentsInChildren<Module>());
        SetAttackCapabilityFlags();
        InstantiateModules();

    }

    public void InstantiateModules()
    {
        int i = 0, j = 0;
        while (i < modules.Count && j < modulePositions.Count)
        {
            if (modules[i] != null)
            {
                GameObject module = Instantiate(modules[i].gameObject, gameObject.transform, false) as GameObject;
                module.transform.localPosition = modulePositions[j];
            }
            i++;
            j++;
        }
        modules = new List<Module>(GetComponentsInChildren<Module>());

    }

    private void SetAttackCapabilityFlags()
    {
        List<Action> actions = GetActions();
        foreach(var a in actions)
        {
            if(a.Type == ActionType.MeleeAttack)
            {
                meleeCapable = true;
            }
            else if(a.Type == ActionType.LongAttack)
            {
                longRangeCapable = true;
                longRangeCapability = Math.Max(longRangeCapability, a.Range);
            }
        }
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

    //Link mouse clicks to current tile
    void OnMouseDown()
    {
        if (currentTile != null && !currentTile.ShipBuilderTile)
        {
            Debug.Log("clicked");
            BattleManager.instance.TileClicked(currentTile);
        }
    }

    /*********************************************** SETUP FUNCTIONS **********************************/

    //Get list of modules and update their location
    public void customUnit()
    {
        modules.Clear();
        modules = new List<Module>(GetComponentsInChildren<Module>());
        StartCoroutine(customUnitWait());
    }

    public void CustomUpdate()
    {
        int i = 0, j = 0;
        while (i < modules.Count && j < modulePositions.Count)
        {
            if (modules[i] != null)
            {
                GameObject module = modules[i].gameObject;
                module.transform.localPosition = modulePositions[j];
            }
            i++;
            j++;
        }
    }
    IEnumerator customUnitWait()
    {
        yield return new WaitForSeconds(.01f);
        SetAttackCapabilityFlags();

        CustomUpdate();
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

    public bool BossUnit
    {
        get
        {
            return bossUnit;
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

    public bool MeleeCapable
    {
        get
        {
            return meleeCapable;
        }
    }

    public int LongRangeCapability
    {
        get
        {
            return longRangeCapability;
        }
    }

    public float HeightOffset
    {
        get
        {
            return heightOffset;
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

    public int GetValue()
    {
        int tempValue = value;
        foreach(var m in modules)
        {
            tempValue += m.Value;
        }
        return tempValue;
    }

    //calculate threat
    public int GetThreat()
    {
        int tempThreat = threat;
        foreach(var m in modules)
        {
            tempThreat += m.Threat;
        }
        return tempThreat;
    }

    public List<Action> GetActions()
    {
        Dictionary<ActionType, Action> actions = new Dictionary<ActionType, Action>();
        foreach(var m in modules)
        {
            Action a;
            if(m == null)
            {
                break;
            }
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
    public UnitType getUnitType()
    {
        return type;
    }
    public List<ModuleType> GetModuleTypes()
    {
        HashSet<ModuleType> moduleTypes = new HashSet<ModuleType>();
        foreach (var m in modules)
        {
            moduleTypes.Add(m.Type);
        }
        return new List<ModuleType>(moduleTypes);
    }
    public UnitType GetShipType()
    {
        return type;
    }
    //returns base hp + bonus from modules
    public float GetHP()
    {
        float moddedStat = hitPoints;

        //for (int i = 0; i < modules.Count; i++)
        //{
        //    moddedStat += modules[i].HitPoints;
        //}

        for (int i = 0; i < statuses.Count; i++)
        {
            if (statuses[i].Type == statusType.hitPoints)
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
            if (statuses[i].Type == statusType.mass)
            {
                moddedStat += statuses[i].Amount;
            }
        }
        return moddedStat;
    }

    public bool HasStatus(statusType type)
    {
        foreach(var s in statuses)
            if (s.Type == type)
                return true;
        return false;
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
        foreach (var s in statuses)
            s.DecrementDuration();
        statuses.RemoveAll(o => o.Duration <= 0);
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
            if(modules[i].Type == module)
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
            Debug.Log("transforming unit on tile " + tile.ToString());
            UpdateTile(tile);
        }
    }

    public void RemoveFromTile()
    {
        if(currentTile != null)
        {
            currentTile.UnitOnTile = null;
        }
        currentTile = null;
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
        Renderer renderer = GetComponentInChildren<UnitBody>().GetComponent<Renderer>();
        if (outlined)
        {
            renderer.material.shader = this.outlineShader;
        }
        else
        {
            renderer.material.shader = this.standardShader;
        }
        //Renderer[] renderers = GetComponentsInChildren<Renderer>();
        //foreach(var r in renderers)
        //{
        //    if (outlined)
        //    {
        //        r.material.shader = this.outlineShader;
        //    }
        //    else
        //    {
        //        r.material.shader = this.standardShader;
        //    }
        //}

        return;
    }

    /******************************************** FX METHODS ***********************************************/
    public void DisplayAction(Action action)
    {

        foreach (var m in modules)
        {
            //Offset for because units float above tiles
            var parent = m.transform.parent;
            m.DisplayAction(action);
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
            Debug.Log(statuses[i].Type + " " + statuses[i].Duration);
        }
    }

}

//used to show type of unit
public enum UnitType
{
    fighter,
    frigate,
    none,
}
