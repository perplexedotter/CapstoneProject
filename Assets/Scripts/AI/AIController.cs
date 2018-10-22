using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour {

    public class Command
    {
        public enum CommandType {Move, Action};

        public Tile target;
        public CommandType commandType;
        public Action action;
    }

    Unit aiUnit;
    Unit closestEnemy;
    ModuleType moduleType;
    //TODO replace with AIAction class
    int actionsTaken;
    bool moved;

    List<Command> commands;

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
        actionsTaken = 0;
        moved = false;
        commands = new List<Command>();
        GetAIUnitType();
        if(moduleType == ModuleType.shortRange)
        {
            ProcessMeleeUnitTurn();
        }
    }



    private void ProcessMeleeUnitTurn()
    {
        //TODO Add logic to determine if unit should move then attack or attack then move
        //GetTileClosestToEnemy();
        //TODO Replace This with a calculation based on threat enemies present

        List<Unit> adjEnemies = GetAdjacentUnits(aiUnit.CurrentTile, true);
        //If enemies are near by attack adjacent enemy then move (Will be changed with threat calc
        if(adjEnemies.Count > 0)
        {
            //Attack closest enemy for now
        }
        else
        {
            //Move to closest enemy
        }
        Unit closestEnemy = GetClosestUnit(aiUnit.CurrentTile, true);
        Tile closestTileToEnemy = GetClosestTile(map.GetMovementRange(aiUnit), closestEnemy.CurrentTile);
        //TODO Replace this with the AI returning a move request
        List<Tile> path = map.GetMovementPath(aiUnit, closestTileToEnemy);
        aiUnit.TraversePath(path);

    }

    //TODO remove this function AI will not handle movement
    public void FinishedMovement()
    {

    }

    //UTILITY FUNCTIONS

    //A ai unit without modules with break this which is fine because that unit wouldn't make sense in the current design
    private void GetAIUnitType()
    {
        moduleType = aiUnit.GetModuleTypes()[0];
    }


    //Gets and returns the Unit that is closest to the specified Tile
    //If the enemy flag is set it will return the closest enemy
    //If the flag is not set it will return the closest ally
    //It will never return the current unit
    private Unit GetClosestUnit(Tile tile, bool enemy)
    {
        int aiPlayerNumber = aiUnit.PlayerNumber;
        List<Unit> units = map.GetAllUnits();
        float closestDist = float.MaxValue;
        Unit closestUnit = null;
        foreach (var u in units)
        {
            if (u != aiUnit && ((enemy && u.PlayerNumber != aiPlayerNumber) || (!enemy && u.PlayerNumber == aiPlayerNumber)))
            {
                float distToE = Vector2Int.Distance(u.CurrentTile.GetCoords(), aiUnit.CurrentTile.GetCoords());
                if (distToE < closestDist)
                {
                    closestDist = distToE;
                    closestUnit = u;
                }
            }
        }
        return closestUnit;
    }

    List<Unit> GetAdjacentUnits(Tile tile, bool enemy)
    {
        List<Unit> units = map.GetAdjacentUnits(tile);
        List<Unit> adjacentUnits = new List<Unit>();
        foreach(var u in units)
            if ((enemy && u.PlayerNumber != aiUnit.PlayerNumber) || (!enemy && u.PlayerNumber == aiUnit.PlayerNumber))
                adjacentUnits.Add(u);
        return adjacentUnits;
    }

    private Tile GetClosestTile(List<Tile>avaliableTiles, Tile target)
    {
        Tile closestTile = null;
        float closestDist = float.MaxValue; //Reset distance
        foreach (var t in avaliableTiles)
        {
            float distToTile = Vector2Int.Distance(target.GetCoords(), t.GetCoords());
            if (distToTile < closestDist)
            {
                closestDist = distToTile;
                closestTile = t;
            }
        }
        return closestTile;
    }

    /*This function will look at all units in melee range
     *If the enemy flag is set the enemy with the highest threat will be returned
     * If the flag is not set the ally with the highest threat will be retruned
     * If no units are in range that meet the selected critera null will be returned
     */
    private Unit GetHighestThreatInMeleeRange(bool enemy)
    {

        return null;
    }
}
