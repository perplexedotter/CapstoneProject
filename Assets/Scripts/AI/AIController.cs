using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIController : MonoBehaviour {

    /***************************************************CLASSES AND ENUMS *************************************/



   

    /********************************************** FIELDS *****************************************/

    Unit unit;
    List<ModuleType> moduleTypes;

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
        UpdateModuleTypes();
        //It is assummed the AIUnit has at least on module equipped
        ModuleType moduleType = moduleTypes[0];
        switch (moduleType)
        {
            case ModuleType.shortRange:
                commands = GetMeleeAttackCommands();
                break;
            case ModuleType.heal:
                commands = GetHealCommands();
                break;
            case ModuleType.longRange:
                commands = GetLongAttackCommands();
                break;
            case ModuleType.slow:
                commands = GetSlowCommands();
                break;
        }
        return commands;
    }

    //TODO Add logic to find enemies that can be destroyed in single attack and target them first
    private List<Command> GetMeleeAttackCommands()
    {
        Action meleeAction = GetAction(ActionType.MeleeAttack);
        List<Command> commands = new List<Command>();
        
        //Determine if highest value in melee range is adjacent
        List<Unit> adjEnemies = GetAdjacentUnits(unit.CurrentTile, Team.Enemy);
        List<Tile> meleeRange = map.GetMovementRangeExtended(unit, meleeAction.Range);
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

            List<Tile> movementRange = map.GetMovementRange(unit);
            
            //Check if the highestValueUnit is in range because it is adjacent to a wormhole destination
            List<Tile> tilesAroundHVU = map.GetTilesInRange(highestValueUnit.CurrentTile, meleeAction.Range);
            Tile closestTileToEnemy = null;
            foreach (var t in tilesAroundHVU)
            {
                //If the tile within the HVU is a wormhole and that wormholes destination is within the AI units movement range
                //The unit should move to the wormhole tile within its range to warp to the enemy
                if (t.WormholeDestination != null && movementRange.Contains(t.WormholeDestination))
                    closestTileToEnemy = t.WormholeDestination;
            }

            //If there is no optimal wormhole move, move normally
            if (!closestTileToEnemy)
                closestTileToEnemy = GetClosestTile(movementRange, highestValueUnit.CurrentTile);
            //Check if a wormhole destination is closer then the chosen tile

                //TODO Implement


            //if (highestValueUnit.CurrentTile)
            //    GetClosestTile(map.GetMovementRange(unit), highestValueUnit.CurrentTile);
            //Move to the enemy with the highest value
            commands.Add(new Command(closestTileToEnemy, Command.CommandType.Move, null));
        }
        /* The unit has moved without attacking. Attack if possible*/
        if (!ContainsAction(commands) && ContainsMove(commands))
        {
            //Get Enemies Adjacent to move
            Tile moveTile = GetFirstMoveTile(commands);
            adjEnemies = GetAdjacentUnits(moveTile, Team.Enemy);
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
            .Where(u => u.PlayerNumber == unit.PlayerNumber && u.GetDamage() > 0)
            .OrderBy(u=>u.GetValue())
            .OrderByDescending(u=>u.GetHP())
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
        //Dictionary<Tile, Map.TileData> tileData = map.GetTileData(map.GetMovementRange(unit), unit, action.Range);
        ////Sort the tiles by least enemies in range -> lowest threat -> most allies in range 
        //List<Map.TileData> safestUsefulPositions = tileData.Values
        //    .OrderByDescending(o => o.DistToNearestEnemy)
        //    .OrderBy(o => o.Threat)
        //    .OrderByDescending(o => o.AlliesInUnitRange)
        //    .ToList();

        List<Tile> bestPossibleMoves = SafestPositionsWithUnitsInRange(unit, action.Range, Team.Ally);
        //There is a damaged unit to heal
        if (healTarget != null)
        {
            //If target is not in heal range move nearer to it first
            if(!map.GetUnitsInRange(unit.CurrentTile, action.Range).Contains(healTarget))
            {
                //Get the tiles that are within movement range and healing range of target
                HashSet<Tile> tilesInRange = new HashSet<Tile>(map.GetTilesInRange(healTarget.CurrentTile, action.Range));
                tilesInRange.UnionWith(movementRange);
                //Find the safest tile of these options based on previous sort
                foreach(var t in bestPossibleMoves)
                {
                    if (tilesInRange.Contains(t))
                    {
                        commands.Add(new Command(t, Command.CommandType.Move, null));
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
            if(bestPossibleMoves.Count > 0)
                commands.Add(new Command(bestPossibleMoves[0], Command.CommandType.Move, null));
        }
        return commands;
    }

    private List<Command> GetLongAttackCommands()
    {
        Action action = GetAction(ActionType.LongAttack);
        List<Command> commands = new List<Command>();

        //Attack if possible
        //Get enemies within range and order by value
        List<Unit> enemiesInRange = map.GetUnitsInRange(unit.CurrentTile, action.Range)
            .Where(u => u.PlayerNumber != unit.PlayerNumber)
            .OrderByDescending(u => u.GetValue())
            .ToList();
        //Attack most valueable unit in range
        if(enemiesInRange.Count > 0)
        {
            commands.Add(new Command(enemiesInRange[0].CurrentTile, Command.CommandType.Action, action));
        }

        //Handle Movement
        //Get best tiles for movement
        List<Tile> bestPossibleMoves = SafestPositionsWithUnitsInRange(unit, action.Range, Team.Enemy);

        //Find the most valueable unit in extended range
        List<Tile> extendedRange = map.GetMovementRangeExtended(unit, action.Range);
        List<Unit> enemies = map.GetUnits(extendedRange)
            .Where(u => u.PlayerNumber != unit.PlayerNumber)
            .ToList();
        Unit highestValueEnemy = GetHighestValue(enemies);
        

        if (highestValueEnemy)
        {
            Tile bestMove = FindTileInRange(bestPossibleMoves, highestValueEnemy.CurrentTile, action.Range);
            if (bestMove)
                commands.Add(new Command(bestMove, Command.CommandType.Move, null));
        }
        //If there has been no move then no enemies are within range
        //Move closer to enemies
        if (!ContainsMove(commands))
        {
            List<Map.TileData> tileData = map.GetTileData(map.GetMovementRange(unit), unit, action.Range)
                .Values
                .OrderBy(o => o.DistToNearestEnemy)
                .OrderBy(o => o.Threat)
                .ToList();
            if(tileData.Count > 0)
            {
                Tile nearestToAllies = tileData[0].Tile;
                commands.Add(new Command(nearestToAllies, Command.CommandType.Move, null));
            }
        }
        return commands;
    }

    private List<Command> GetSlowCommands()
    {
        //TODO possibly alter to this
        //Get allies current experienced threat levels.
        //Target enemy unit the reduces allies experienced threat the most

        Action action = GetAction(ActionType.Slow);
        List<Command> commands = new List<Command>();

        List<Tile> extendedRange = map.GetMovementRangeExtended(unit, action.Range);
        List<Tile> movementRange = map.GetMovementRange(unit);
        //Get highest value enemy in extended range 
        Unit HVU = GetHighestValue(map.GetUnits(extendedRange, unit.PlayerNumber, Team.Enemy));

        if (HVU)
        {
            //If enemy is not in range move in range

            //If enemy is in range slow

        }
        //If there is no enemy in range get closest enemy and move near
        else
        {

        }

        if (!ContainsMove(commands))
        {
            //Else if no move has been made move to safestUseful position

        }

        return commands;
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
        Tile moveTile = null;
        foreach (var c in commands)
            if (c.commandType == Command.CommandType.Move)
            {
                moveTile = c.target;
                break;
            }
        //If the Tile is a wormhole and useable return the destination
        moveTile = CheckForWormhole(moveTile);
        return moveTile;
    }

    private void UpdateModuleTypes()
    {
        moduleTypes = new List<ModuleType>();
        
        foreach(var m in unit.GetModuleTypes())
        {
            if (!moduleTypes.Contains(m))
                moduleTypes.Add(m);
        }
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
            Tile tile = CheckForWormhole(t);
            float distToTile = Vector2Int.Distance(target.GetCoords(), tile.GetCoords());
            if (distToTile < closestDist)
            {
                closestDist = distToTile;
                closestTile = t;
            }
        }
        return closestTile;
    }

    private Tile CheckForWormhole(Tile t)
    {
        //Checks if a tile is a wormhole and if it is and the destination is avaliable returns the destination
        return t.WormholeDestination != null && t.WormholeDestination.UnitOnTile == null 
            ? t.WormholeDestination : t;
    }

    private Tile FindTileInRange(List<Tile> tilesToSearch, Tile target, int range)
    {
        Tile tileInRange = null;
        HashSet<Tile> inRangeOfTarget = new HashSet<Tile>(map.GetTilesInRange(target, range));
        foreach(var t in tilesToSearch)
        {
            //If the tile is a wormhole and its destination is not blocked replace with destination
            Tile tile = CheckForWormhole(t);
            //Test is the tile is in range
            if (inRangeOfTarget.Contains(tile))
            {
                //If it is add the original tile (not wormhole destination)
                tileInRange = t;
                break;
            }
        }


        return tileInRange;
    }

    private List<Tile> SafestPositionsWithUnitsInRange(Unit unit, int range, Team team)
    {
        //Get tiledata for the unit and range passed
        Dictionary<Tile, Map.TileData> tileData = map.GetTileData(map.GetMovementRange(unit), unit, range);
        //Sort the tiles by least enemies in range -> lowest threat
        List<Map.TileData> safestUsefulPositions = tileData.Values
            .OrderBy(o => o.DistToNearestAlly)
            .OrderByDescending(o => o.DistToNearestEnemy)
            .OrderBy(o => o.Threat)
            .ToList();


        bool unitsInRange = false;

        //Sort based on allies or enemies in range based on team passed
        if (team == Team.Ally)
        {
            //Check if there are actually units in range
            foreach (var td in safestUsefulPositions)
            {
                if(td.AlliesInUnitRange > 0)
                {
                    unitsInRange = true;
                    break;
                }
            }
            if (unitsInRange)
            {
                safestUsefulPositions = safestUsefulPositions.OrderByDescending(o => o.AlliesInUnitRange).ToList();
            }
            //If there are no allies in range sort by the least threatened position in the direction allies
            else
            {
                safestUsefulPositions = safestUsefulPositions
                    .OrderBy(o => o.DistToNearestAlly)
                    .OrderBy(o => o.Threat)
                    .ToList();
            }
        }
        else if(team == Team.Enemy)
        {
            //Check if there are actually units in range
            foreach (var td in safestUsefulPositions)
            {
                if (td.EnemiesInUnitRange > 0)
                {
                    unitsInRange = true;
                    break;
                }
            }
            if (unitsInRange)
            {
                safestUsefulPositions = safestUsefulPositions.OrderByDescending(o => o.EnemiesInUnitRange).ToList();
            }
            //If there are no enemies in range sort by the least threatened position in the direction enemies
            else
            {
                safestUsefulPositions = safestUsefulPositions
                    .OrderBy(o => o.DistToNearestEnemy)
                    .OrderBy(o => o.Threat)
                    .ToList();
            }
        }

        //Return the actual tiles
        return safestUsefulPositions.Select(o=>o.Tile).ToList();
    }


}
