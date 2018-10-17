using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Unit : MonoBehaviour {

    //Needed to Detect Child Collision
    [SerializeField] Collider collider;

    [Header("Transform Properties")]
    //[SerializeField] protected Vector3 heightOffset;
    //[SerializeField] protected Vector3 moveDestination;
    [SerializeField] protected float moveSpeed = 10.0f;
    [SerializeField] protected float movementTolerance = 1f;

    [Header("Unit Stats")]
    [Tooltip("Which player controls this unit")]
    [SerializeField] protected float playerNumber;
    [SerializeField] protected float hitPoints;
    [SerializeField] protected float damageTaken;
    [SerializeField] protected float attack;
    [SerializeField] protected float speed;
    [SerializeField] protected float mass;
    [SerializeField] protected float shields;
    [SerializeField] protected float hardPoints;

    protected UnitType type;
    protected List<Module> modules;

    //[SerializeField] private int movementRange = 4;
    private bool isMoving = false;
    protected bool movementFinished = false;
    Tile currentTile;
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

    //returns base hp + bonus from modules
    public float GetHP()
    {
        float moddedStat = hitPoints;

        for (int i = 0; i < modules.Count; i++)
        {
            moddedStat += modules[i].HitPoints;
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

        return moddedStat;
    }

    //returns base attack + bonus from modules
    public float GetAttack()
    {
        float moddedStat = attack;

        for (int i = 0; i < modules.Count; i++)
        {
            moddedStat += modules[i].Attack;
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

    public virtual void TurnUpdate(){

	}

    //returns movement after calculation based on unit mass
    public int GetMovementRange() {
        float moddedMass = GetMass();
        int movementRange = (int)((1000 - moddedMass) / 100);
        return movementRange;
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
            GameManager.instance.FinishedMovement();
        }
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
        else
        {
            transform.position = currentTile.transform.position;
            movementFinished = true;
            isMoving = false;
        }
    }

    private void UpdateTile(Tile tile)
    {
        if(currentTile != null)
            currentTile.UnitOnTile = null;
        currentTile = tile;
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
