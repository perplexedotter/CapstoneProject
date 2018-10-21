using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[SelectionBase]
public class Unit : MonoBehaviour {

    //Needed to Detect Child Collision
    [SerializeField] Collider collider;

    [Header("Transform Properties")]
    //[SerializeField] protected Vector3 heightOffset;
    //[SerializeField] protected Vector3 moveDestination;
    [SerializeField] protected float moveSpeed = 10.0f;
    [SerializeField] protected float movementTolerance = 1f;
    [SerializeField] Tile currentTile;

    [Header("Unit Stats")]
    [Tooltip("Which player controls this unit")]
    [SerializeField] protected int playerNumber = 0;
    [SerializeField] protected UnitType type;
    [SerializeField] protected float hitPoints = 100;
    [SerializeField] protected float damageTaken = 0;
    [SerializeField] protected float attack = 0;
    [SerializeField] protected float speed = 100;
    [SerializeField] protected float mass = 400;
    [SerializeField] protected float shields = 0;
    [SerializeField] protected float hardPoints = 0;
    [SerializeField] protected int movementRange;

    [Header("Modules and Statuses")]
    [SerializeField] protected List<Module> modules;
    [SerializeField] protected List<StatusEffects> statuses;

    //Movement fields
    private bool isMoving = false;
    protected bool movementFinished = false;
    private List<Tile> path;
    private int pathIndex;

    void Awake()
    {

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

    //based on unit type provide this will give base stats to unit
    public void DefineUnit(UnitType Type)
    {
        modules = new List<Module>();
        statuses = new List<StatusEffects>();
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
        }
    }

    //Unit will take damage
    public float TakeDamage(float dmg)
    {
        damageTaken += dmg;
        return(GetHP() - damageTaken);
    }
    //returns list of actions
    //will combine actions into more powerful one
    public List<Action> getActions()
    {
        List<Action> actions = new List<Action>();
        List<Action> actionsSorted = new List<Action>();

        for (int i = 0; i < modules.Count; i++)
        {
            actions.Add(modules[i].action);
        }

        actionsSorted = actions.OrderBy(o => o.Type).ToList();
        for (int i = 0; i < actionsSorted.Count; i++)
        {
            if(i != actionsSorted.Count - 1)
            {
                if(actionsSorted[i].Type == actionsSorted[i + 1].Type)
                {
                    actionsSorted[i+1] = actionsSorted[i].Combine(actionsSorted[i + 1]);
                    actionsSorted.RemoveAt(i);
                    i -= 1;
                }
            }
        }
        debugActions(actionsSorted);
        return actionsSorted;
    }
    
    //for testing getactions
    public void debugActions(List<Action> debug)
    {
        for (int i = 0; i < debug.Count; i++)
        {
            Debug.Log(i + " " + debug[i].Type + " " + debug[i].Power);
        }
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
            if (statuses[i].duration == 1)
            {
                statuses.RemoveAt(i);
                i -= 1;
            }
        }
    }

    //for testing
    public void DebugStatuses()
    {
        for (int i = 0; i < statuses.Count; i++)
        {
            Debug.Log(statuses[i].statusName + " " + statuses[i].duration);
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
    public void RemoveModule(ModuleName module)
    {
        for(int i = 0; i < modules.Count; i++)
        {
            if(modules[i].ModuleName == module)
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

    public float GetDamage()
    {
        return damageTaken;
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
            if (statuses[i].statusName == statusType.hitPoints)
            {
                moddedStat += statuses[i].amount;
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
            if (statuses[i].statusName == statusType.mass)
            {
                moddedStat += statuses[i].amount;
            }
        }
        return moddedStat;
    }

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

    public virtual void TurnUpdate(){

	}


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
                if (Vector3.Distance(destination, transform.position) > movementTolerance) //May not need this if
                {
                    transform.position += (destination - transform.position).normalized * moveSpeed * Time.deltaTime;

                    if (Vector3.Distance(destination, transform.position) <= movementTolerance) //If the unit is close enough call it good
                    {
                        UpdateTile(path[pathIndex]);
                    }
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
            transform.position = currentTile.transform.position;
            movementFinished = true;
            isMoving = false;
        }
    }

    private void UpdateTile(Tile tile)
    {
        if(currentTile != null && currentTile.UnitOnTile == this) //Don't remove units other then yourself from the tile
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

    private void StartHighlightAnimation()
    {

    }

    private void StopHighlightAnimation()
    {

    }

    //DEPRECIATED Use TraversePath
    //This will make unit movement follow the actual NWSE movement they can perform
    //And prevent a unit from shortcutting across tiles they can't actually enter
    //public void MoveToTile(Tile tile)
    //{
    //    if (tile != null)
    //    {
    //        UpdateTile(tile);
    //        moveDestination = tile.transform.position;
    //        isMoving = true;
    //    }
    //}

    //DEPRECIATED
    //protected void MoveCharacter()
    //{
    //    if (Vector3.Distance(moveDestination, transform.position) > movementTolerance)
    //    {
    //        transform.position += (moveDestination - transform.position).normalized * moveSpeed * Time.deltaTime;

    //        if (Vector3.Distance(moveDestination, transform.position) <= movementTolerance)
    //        {
    //            movementFinished = true;
    //            isMoving = false;
    //            transform.position = moveDestination;
    //        }
    //    }
    //}
}

//used to show type of unit
public enum UnitType
{
    fighter,
}
