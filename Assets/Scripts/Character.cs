using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

    [SerializeField] protected Vector3 heightOffset;
    [SerializeField] protected Vector3 moveDestination;
	[SerializeField] protected float moveSpeed = 10.0f;

    //TODO replace with calculation based on speed
    private int movementRange = 4;

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
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public virtual void TurnUpdate(){

	}

    //TODO replace with calculation based on speed
    public int GetMovementRange() { return movementRange; }

    public void MoveToTile(Tile tile)
    {
        if (tile != null)
        {
            moveDestination = tile.transform.position + heightOffset;
            currentTile = tile;
        }
    }
}
