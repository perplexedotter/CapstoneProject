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
    protected unitType type;

    //Contains modifications to stats
    protected modCharStat healthMod;
    protected modCharStat attackMod;
    protected modCharStat massMod;

    //Contains Stats with modifiers  
    [SerializeField] protected float ModdedHealth;
    [SerializeField] protected float ModdedAttack;
    [SerializeField] protected float ModdedSpeed;
    [SerializeField] protected float ModdedMass;
    
    //Two short range modules
    public Module shortRange { get; protected set; }
    protected int totalEquipped;

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
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        ProcessMovement();
    }
    
    //called to define base stats of unit
    //allows for future unit types to be called
    public void defineUnit(unitType Type)
    {
        if (unitType.fighter == Type)
        {
            //Base Stats for fighter
            Health = new baseCharStat(100);
            Mass = new baseCharStat(400);
            ModdedMass = Mass.baseStat;
            Hardpoints = new baseCharStat(2);
            Speed = new baseCharStat(100);
            DamageTaken = new baseCharStat(0);
            Attack = new baseCharStat(20);
            type = Type;
            shortRange = new Module(ModuleName.shortRange);
            GetFinalStat();
        }
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

    //equips short range module
    private void addShort(Unit c)
    {
        shortRange.equip();
        totalEquipped += 1;
        healthMod = new modCharStat(0.25f, StatIdentifier.Addition_Percent, 200, this);
        c.Health.PlusModStat(healthMod);

        attackMod = new modCharStat(15, StatIdentifier.basic, 100, this);
        c.Attack.PlusModStat(attackMod);

        massMod = new modCharStat(100, StatIdentifier.basic, 100, this);
        c.Mass.PlusModStat(massMod);
    }

    //will handle adding modules to units
    //in the future we can add other units besides fighter
    public bool addModule(ModuleName module, Unit c)
    {
        if (c.type == unitType.fighter)
        {
            if (module == ModuleName.shortRange)
            {
                if (shortRange.equipped == 0 && totalEquipped < 2)
                {
                    addShort(c);
                    return false;
                }
                else if (shortRange.equipped == 1 && totalEquipped < 2)
                {
                    addShort(c);
                    return false;
                }
                else
                {
                    Debug.Log("You can't equip more than 2 short range modules to fighter");
                }
            }
        }
        return true;
    }

    //unequips everything
    public void removeAllModules(Unit c)
    {
        shortRange.unequip();
        totalEquipped = 0;
        c.Health.DeleteAllMods(this);
        c.Attack.DeleteAllMods(this);
        c.Mass.DeleteAllMods(this);
    }
}

//used to show type of unit
public enum unitType
{
    fighter,
}
