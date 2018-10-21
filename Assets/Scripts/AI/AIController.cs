using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour {

    Unit aiUnit;
    Unit closestEnemy;

    public static AIController instance;
    [SerializeField] Map map;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //TODO expand this AI to other possiblities
    //TODO make this return a list of actions instead of directly manipulating unit
    public void GetAIActions(Unit unit)
    {
        this.aiUnit = unit;
        GetTileClosestToEnemy();
    }

    private void GetTileClosestToEnemy()
    {
        List<Unit> enemies = map.GetAllUnits();
        float closestDist = float.MaxValue;
        foreach(var e in enemies)
        {
            if(e.PlayerNumber != aiUnit.PlayerNumber)
            {
                float distToE = Vector2Int.Distance(e.CurrentTile.GetCoords(), aiUnit.CurrentTile.GetCoords());
                if (distToE < closestDist)
                {
                    closestDist = distToE;
                    closestEnemy = e;
                }
            }

        }

        closestDist = float.MaxValue; //Reset distance
        Tile closestTile = null;
        List<Tile> movementRange = map.GetMovementRange(aiUnit);
        foreach (var t in movementRange)
        {
            float distToEnemy = Vector2Int.Distance(closestEnemy.CurrentTile.GetCoords(), t.GetCoords());
            if(distToEnemy < closestDist)
            {
                closestDist = distToEnemy;
                closestTile = t;
            }
        }
        List<Tile> path = map.GetMovementPath(aiUnit, closestTile);
        aiUnit.TraversePath(path);
    }

    //TODO remove this function AI will not handle movement
    public void FinishedMovement()
    {

    }
}
