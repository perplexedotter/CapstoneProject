using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

    [Header("Transfom Properties")]
    [SerializeField] protected Vector3 heightOffset;
    [SerializeField] protected Vector3 moveDestination;
	[SerializeField] protected float moveSpeed = 10.0f;

    [Header("Unit Stats")]
    [Tooltip("Which player controls this unit")]
    [SerializeField] protected int playerNumber;

    //Contains Base Stats
    [SerializeField] public baseCharStat Health;
    [SerializeField] public baseCharStat Attack;
    [SerializeField] public baseCharStat Speed;
    [SerializeField] public baseCharStat Mass;
    [SerializeField] public baseCharStat DamageTaken;
    [SerializeField] public baseCharStat Hardpoints;

    //Contains Stats with modifiers  
    [SerializeField] public float ModdedHealth;
    [SerializeField] public float ModdedAttack;
    [SerializeField] public float ModdedSpeed;
    [SerializeField] public float ModdedMass;

    [SerializeField] private int movementRange = 4;
    

    private Tile currentTile;

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
      
    }
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        
    }
	public virtual void TurnUpdate(){

	}

    //Get the Final stats (base + modifiers)
    public void GetFinalStat()
    {
        ModdedHealth = Health.getModdedValue();
        ModdedAttack = Attack.getModdedValue();
        ModdedSpeed = Speed.getModdedValue();
        ModdedMass = Mass.getModdedValue();
        
    }

    // (1000 - Mass) / 100 = movement
    public int GetMovementRange()
    {
        movementRange = (int)((1000 - ModdedMass) / 100);
        return movementRange;
    }

    public void MoveToTile(Tile tile)
    {
        if (tile != null)
        {
            moveDestination = tile.transform.position + heightOffset;
            currentTile = tile;
        }
    }

    //Places the unit on a tile instantly (No Visual Movment)
    public void PlaceOnTile(Tile tile)
    {
        if(tile != null)
        {
            transform.position = tile.transform.position + heightOffset;
        }
    }

    private void StartHighlightAnimation()
    {

    }

    private void StopHighlightAnimation()
    {

    }
}
