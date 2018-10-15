using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Unit : MonoBehaviour {

    [Header("Transform Properties")]
    [SerializeField] protected Vector3 heightOffset;
    [SerializeField] protected Vector3 moveDestination;
	[SerializeField] protected float moveSpeed = 10.0f;
    [SerializeField] protected float movementTolerance = .25f;

    [Header("Unit Stats")]
    [Tooltip("Which player controls this unit")]
    [SerializeField] protected int playerNumber;

    //Contains Base Stats
    [SerializeField] public baseCharStat DamageTaken { get; protected set; }
    [SerializeField] public baseCharStat Hardpoints { get; protected set; }
    [SerializeField] public baseCharStat Health { get; protected set; }
    [SerializeField] public baseCharStat Attack { get; protected set; }
    [SerializeField] public baseCharStat Speed { get; protected set; }
    [SerializeField] public baseCharStat Mass { get; protected set; }

    //Contains Stats with modifiers  
    [SerializeField] protected float ModdedHealth;
    [SerializeField] protected float ModdedAttack;
    [SerializeField] protected float ModdedSpeed;
    [SerializeField] protected float ModdedMass;
    
    //Two short range modules
    public Module shortRange1 { get; protected set; }
    public Module shortRange2 { get; protected set; }

    [SerializeField] private int movementRange = 4;
    private bool isMoving = false;
    protected bool movementFinished = false;
    Tile currentTile;

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

    void Awake () {
		moveDestination = transform.position;

        //TODO Move elsewhere
        //Base Stats for fighter
        Health = new baseCharStat(100);
        Mass = new baseCharStat(400);
        ModdedMass = Mass.baseStat;
        Hardpoints = new baseCharStat(2);
        Speed = new baseCharStat(100);
        DamageTaken = new baseCharStat(0);
        Attack = new baseCharStat(20);
        shortRange1 = new Module();
        shortRange2 = new Module();
        GetFinalStat();
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        ProcessMovement();
    }

    //Get the Final stats (base + modifiers)
    public void GetFinalStat()
    {
        ModdedHealth = Health.getModdedValue();
        ModdedAttack = Attack.getModdedValue();
        ModdedSpeed = Speed.getModdedValue();
        ModdedMass = Mass.getModdedValue();

    }

    private void ProcessMovement()
    {
        if (IsMoving)
            MoveCharacter();
        if (movementFinished)
        {
            movementFinished = false;
            GameManager.instance.FinishedMovement();
        }
    }

    public virtual void TurnUpdate(){

	}

    // (1000 - Mass) / 100 = movement
    public int GetMovementRange()
    {
        movementRange = (int)((1000 - ModdedMass) / 100);
        return movementRange;
    }


    //TODO Make unit get a path of tiles and follow that path
    //This will make unit movement follow the actual NWSE movement they can perform
    //And prevent a unit from shortcutting across tiles they can't actually enter
    public void MoveToTile(Tile tile)
    {
        if (tile != null)
        {
            moveDestination = tile.transform.position + heightOffset;
            currentTile = tile;
            isMoving = true;
        }
    }

    protected void MoveCharacter()
    {
        if (Vector3.Distance(moveDestination, transform.position) > movementTolerance)
        {
            transform.position += (moveDestination - transform.position).normalized * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(moveDestination, transform.position) <= movementTolerance)
            {
                movementFinished = true;
                isMoving = false;
                transform.position = moveDestination;
            }
        }
    }

    //Places the unit on a tile instantly (No Visual Movment)
    public void PlaceOnTile(Tile tile)
    {
        if(tile != null)
        {
            moveDestination = transform.position = tile.transform.position + heightOffset;
            currentTile = tile;
        }
    }

    private void StartHighlightAnimation()
    {

    }

    private void StopHighlightAnimation()
    {

    }

    public bool addModule(ModuleName module, Unit c)
    {
        if (module == ModuleName.shortRange)
        {
            if (shortRange1.equipped == 0)
            {
                shortRange1.EquipShortRange(c);
                return false;
            }
            else if (shortRange2.equipped == 0)
            {
                shortRange2.EquipShortRange(c);
                return false;
            }
        }
        return true;
    }

    public void removeAllModules(Unit c)
    {
        if(shortRange2.equipped >= 1)
        {
            shortRange1.UnequipAll(c);
            shortRange2.UnequipAll(c);
        }
        if (shortRange1.equipped >= 1)
        {
            shortRange1.UnequipAll(c);
        }
    }
}
