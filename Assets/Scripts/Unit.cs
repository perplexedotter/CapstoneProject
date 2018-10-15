using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Unit : MonoBehaviour {

    //Needed to Detect Child Collision
    [SerializeField] Collider collider;

    [Header("Transform Properties")]
    [SerializeField] protected Vector3 heightOffset;
    [SerializeField] protected Vector3 moveDestination;
	[SerializeField] protected float moveSpeed = 10.0f;
    [SerializeField] protected float movementTolerance = .25f;

    [Header("Unit Stats")]
    [Tooltip("Which player controls this unit")]
    [SerializeField] protected int playerNumber;
    [SerializeField] protected int hitPoints;

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

    //TODO replace with calculation based on speed
    public int GetMovementRange() {
        return 4;
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
}
