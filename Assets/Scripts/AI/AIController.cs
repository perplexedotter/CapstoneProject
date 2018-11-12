using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIController : MonoBehaviour {

    /***************************************************CLASSES AND ENUMS *************************************/



   

    /********************************************** FIELDS *****************************************/

    Unit unit;
    ModuleType moduleType;

    List<Command> commands;

    public static AIController instance;
    [SerializeField] Map map;

    /********************************************* UNITY METHODS ***************************************/

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    /********************************************************** COMMAND FUNCTIONS ******************************/

    //TODO possibly add logic for units to have multiple diff modules
    public List<Command> GetAICommands(Unit unit)
    {
        this.unit = unit;
        commands = new List<Command>();
        GetAIUnitType();
        switch (moduleType)
        {
            case ModuleType.shortRange:
                commands = GetMeleeCommands();
                break;
            case ModuleType.heal:
                commands = GetHealCommands();
                break;
        }
        return commands;
    }

    //TODO Add logic to find enemies that can be destroyed in single attack and target them first
    private List<Command> GetMeleeCommands()
    {
        Action meleeAction = GetAction(ActionType.ShortAttack);
        List<Command> commands = new List<Command>();
        
        //Determine if highest value in melee range is adjacent
        List<Unit> adjEnemies = GetAdjacentUnits(unit.CurrentTile, Team.Enemy);
        List<Tile> meleeRange = map.GetMovementRangeExtended(unit, 1);
        Unit highestValueUnit = GetHighestValue(map.GetUnits(meleeRange, unit.PlayerNumber, Team.Enemy));


        //If the highest value unit is in range attack it
        if (highestValueUnit != null && adjEnemies.Count > 0 && adjEnemies.Contains(highestValueUnit))
        {
            commands.Add(new Command(highestValueUnit.CurrentTile, Command.CommandType.Action, meleeAction));
        }
        else
        {
            //If there is not unit in melee range find the closest enemy and move to engage
            if (highestValueUnit == null)
            {
                highestValueUnit = GetClosestUnit(unit.CurrentTile, Team.Enemy);
            }
            //Move to the enemy with the highest value
            Tile closestTileToEnemy = GetClosestTile(map.GetMovementRange(unit), highestValueUnit.CurrentTile);
            commands.Add(new Command(closestTileToEnemy, Command.CommandType.Move, null));
        }
        /*TODO Implement
         * Check if the commands include a movement. If it doesn't unit has attacked without moving
         * If this is the case move away to a lower threat location
         * Possibly not staying put should block enemies from healers and such
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
            Unit target = GetHighestValue(adjEnemies);
            if (target != null)
                commands.Add(new Command(target.CurrentTile, Command.CommandType.Action, meleeAction));
        }

        return commands;
    }


    private List<Command> GetHealCommands()
    {
        Action action = GetAction(ActionType.Heal);
        List<Command> commands = new List<Command>();

        //Find most damaged highest value unit in potential range
        List<Unit> damagedAllies = map.GetAllAllies(unit)
            .Where(unit => unit.PlayerNumber == unit.PlayerNumber && unit.GetDamage() > 0)
            .OrderBy(o=>o.GetValue())
            .OrderByDescending(o=>o.GetHP())
            .ToList();

        List<Tile> healRange = map.GetMovementRangeExtended(unit, action.Range);
        HashSet<Unit> unitsInRange = new HashSet<Unit>(map.GetUnits(healRange));

        Unit healTarget = null;
        foreach(var u in damagedAllies)
        {
            if (unitsInRange.Contains(u))
            {
                healTarget = u;
                break;
            }
        }
        //Get information about threat and enemies and allies in range
        HashSet<Tile> movementRange = new HashSet<Tile>(map.GetMovementRange(unit));
        Dictionary<Tile, Map.TileData> tileData = map.GetTileData(map.GetMovementRange(unit), unit, action.Range);
        //Sort the tiles by least enemies in range -> lowest threat -> most allies in range 
        List<Map.TileData> safestUsefulPos = tileData.Values
            .OrderByDescending(o => o.DistToNearestEnemy)
            .OrderBy(o => o.Threat)
            .OrderByDescending(o => o.AlliesInUnitRange)
            .ToList();
        //There is a damaged unit to heal
        if (healTarget != null)
        {
            //If target is not in heal range move nearer to it first
            if(!map.GetUnitsInRange(unit.CurrentTile, action.Range).Contains(healTarget))
            {
                //Get the tiles that are within movement range and healing range of target
                HashSet<Tile> tilesInRange = new HashSet<Tile>(map.GetTilesInRange(unit.CurrentTile, action.Range));
                tilesInRange.UnionWith(movementRange);
                //Find the safest tile of these options based on previous sort
                foreach(var t in safestUsefulPos)
                {
                    if (tilesInRange.Contains(t.Tile))
                    {
                        commands.Add(new Command(t.Tile, Command.CommandType.Move, null));
                        break;
                    }
                }
            }
            //Heal target
            commands.Add(new Command(healTarget.CurrentTile, Command.CommandType.Action, action));
        }
        //If no movement has occured move to the safest useful position
        if (!ContainsMove(commands))
        {
            if(safestUsefulPos.Count > 0)
                commands.Add(new Command(safestUsefulPos[0].Tile, Command.CommandType.Move, null));
        }
        return commands;
    }

    private List<Command> GetRangeCommands()
    {
        //If unit has firing opportunites fire

        //Move to a better location

        return null;
    }

    private List<Command> GetSlowCommands()
    {
        //Get allies current experienced threat levels.
        
        //Target enemy unit the reduces allies experienced threat the most

        return null;
    }

    /************************************************ UTILITY FUNCTIONS *****************************************/

    //Returns the unit with highest value from a list of units
    private Unit GetHighestValue(List<Unit> units)
    {
        Unit highestValue = null;
        foreach(var u in units)
        {
            if((highestValue == null || highestValue.GetValue() < u.GetValue()))
            {
                highestValue = u;
            }
        }
        return highestValue;
    }

    private Action GetAction(ActionType type)
    {
        List<Action> actions = unit.GetActions();
        Action bestAction = null;
        foreach(var a in actions)
        {
            if(a.Type == type)
            {
                //There should be only one melee action but this will get the best if there is more then one
                if(bestAction == null || bestAction.Power < a.Power)
                    bestAction = a;
            }
        }
        return bestAction;
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
        moduleType = unit.GetModuleTypes()[0];
    }

    //Gets and returns the Unit that is closest to the specified Tile
    //If the enemy flag is set it will return the closest enemy
    //If the flag is not set it will return the closest ally
    //It will never return the current unit
    private Unit GetClosestUnit(Tile tile, Team team)
    {
        int aiPlayerNumber = unit.PlayerNumber;
        List<Unit> units = map.GetAllUnits();
        float closestDist = float.MaxValue;
        Unit closestUnit = null;
        foreach (var u in units)
        {
            if (u != unit && ((team == Team.Enemy && u.PlayerNumber != aiPlayerNumber) || (team == Team.Ally && u.PlayerNumber == aiPlayerNumber)))
            {
                float distToE = Vector2Int.Distance(u.CurrentTile.GetCoords(), unit.CurrentTile.GetCoords());
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
            if ((team == Team.Enemy && u.PlayerNumber != unit.PlayerNumber) || (team == Team.Ally && u.PlayerNumber == unit.PlayerNumber))
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
}
