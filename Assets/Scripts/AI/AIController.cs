using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour {
    enum Team {Enemy, Ally};

    Unit aiUnit;
    ModuleType moduleType;

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

    //TODO expand this AI to other possiblities
    //TODO make this return a list of actions instead of directly manipulating unit
    public List<Command> GetAICommands(Unit unit)
    {
        this.aiUnit = unit;
        commands = new List<Command>();
        GetAIUnitType();
        switch (moduleType)
        {
            case ModuleType.shortRange:
                commands = ProcessMeleeUnitTurn();
                break;
        }
        return commands;
    }

    //AI TURN FUNCTIONS

    private List<Command> ProcessMeleeUnitTurn()
    {
        //TODO Add logic to determine if unit should move then attack or attack then move
        //GetTileClosestToEnemy();
        //TODO Replace This with a calculation based on threat enemies present
        Action meleeAction = GetMeleeAction();
        List<Command> commands = new List<Command>();
        
        //Determine if highest threat in melee range is adjacent
        //TODO make this actually threat based
        List<Unit> adjEnemies = GetAdjacentUnits(aiUnit.CurrentTile, Team.Enemy);

        //TODO make this actually threat based current (replace with commented out code)
        Unit highestThreatUnit = adjEnemies.Count > 0 ? adjEnemies[0] : GetHighestThreat(map.GetEnemyUnitsInMeleeRange(aiUnit));
        //Unit highestThreatUnit = GetHighestThreat(map.GetEnemyUnitsInMeleeRange(aiUnit));

        //If the highest threat unit is in range attack it
        if (highestThreatUnit != null && adjEnemies.Count > 0 && adjEnemies.Contains(highestThreatUnit))
        {
            commands.Add(new Command(highestThreatUnit.CurrentTile, Command.CommandType.Action, meleeAction));
        }
        else
        {
            //Unit closestEnemy = GetClosestUnit(aiUnit.CurrentTile, true);
            //If there is not unit in melee range find the closest enemy and move to engage
            if (highestThreatUnit == null)
            {
                highestThreatUnit = GetClosestUnit(aiUnit.CurrentTile, Team.Enemy);
            }
            //Move to the enemy with the highest threat
            Tile closestTileToEnemy = GetClosestTile(map.GetMovementRange(aiUnit), highestThreatUnit.CurrentTile);
            commands.Add(new Command(closestTileToEnemy, Command.CommandType.Move, null));
        }
        /*Check if the commands include a movement. If it doesn't unit has attacked without moving
         * If this is the case move away to a lower threat location
         */
        if (!ContainsMove(commands))
        {
            //Move to a lower threat area
        }
        /* The unit has moved without attacking. Attack if possible*/
        if (!ContainsAction(commands) && ContainsMove(commands))
        {
            //Get Enemies Adjacent to move
            adjEnemies = GetAdjacentUnits(GetFirstMoveTile(commands), Team.Enemy);
            Unit target = GetHighestThreat(adjEnemies);
            if (target != null)
                commands.Add(new Command(target.CurrentTile, Command.CommandType.Action, meleeAction));
        }

        return commands;
    }


    //UTILITY FUNCTIONS

    //Returns the unit with highest threat from a list of units
    private Unit GetHighestThreat(List<Unit> units)
    {
        Unit highestThreat = null;
        //TODO Convert this to find highest threat unit
        highestThreat = units.Count > 0 ? units[0] : null;
        return highestThreat;
    }

    private Action GetMeleeAction()
    {
        List<Action> actions = aiUnit.GetActions();
        Action bestMeleeAction = null;
        foreach(var a in actions)
        {
            if(a.Type == ActionType.ShortAttack)
            {
                //There should be only one melee action but this will get the best if there is more then one
                if(bestMeleeAction == null || bestMeleeAction.Power < a.Power)
                    bestMeleeAction = a;
            }
        }
        return bestMeleeAction;
    }

    private bool ContainsMove(List<Command> commands)
    {
        foreach (var c in commands)
            if (c.commandType == Command.CommandType.Move)
                return true;
        return false;
    }

    private bool ContainsAction(List<Command> commands)
    {
        foreach (var c in commands)
            if (c.commandType == Command.CommandType.Action)
                return true;
        return false;
    }

    private Tile GetFirstMoveTile(List<Command> commands)
    {
        foreach (var c in commands)
            if (c.commandType == Command.CommandType.Move)
                return c.target;
        return null;
    }

    //A ai unit without modules with break this which is fine because that unit wouldn't make sense in the current design
    private void GetAIUnitType()
    {
        moduleType = aiUnit.GetModuleTypes()[0];
    }

    //Gets and returns the Unit that is closest to the specified Tile
    //If the enemy flag is set it will return the closest enemy
    //If the flag is not set it will return the closest ally
    //It will never return the current unit
    private Unit GetClosestUnit(Tile tile, Team team)
    {
        int aiPlayerNumber = aiUnit.PlayerNumber;
        List<Unit> units = map.GetAllUnits();
        float closestDist = float.MaxValue;
        Unit closestUnit = null;
        foreach (var u in units)
        {
            if (u != aiUnit && ((team == Team.Enemy && u.PlayerNumber != aiPlayerNumber) || (team == Team.Ally && u.PlayerNumber == aiPlayerNumber)))
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

    List<Unit> GetAdjacentUnits(Tile tile, Team team)
    {
        List<Unit> units = map.GetAdjacentUnits(tile);
        List<Unit> adjacentUnits = new List<Unit>();
        foreach(var u in units)
            if ((team == Team.Enemy && u.PlayerNumber != aiUnit.PlayerNumber) || (team == Team.Ally && u.PlayerNumber == aiUnit.PlayerNumber))
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
    private Unit GetHighestThreatInMeleeRange(Team team)
    {

        return null;
    }
}
