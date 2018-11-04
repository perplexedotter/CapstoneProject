using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Map : MonoBehaviour {

    //To be used to remove search fields from Tile itself.
    //This should help make the search thread safe 
    //which may be important for optimizing AI
    private class TileSearchField
    {
        Tile tile;
        public Tile exploredFrom;
        public bool visited;

        public Tile Tile
        {
            get
            {
                return tile;
            }
        }

        public TileSearchField(Tile tile)
        {
            this.tile = tile;
            exploredFrom = null;
            visited = false;
        }
    }

    public class TileData
    {
        Tile tile;
        //The total threat this unit would experince in this location.
        int enemyThreat = 0;
        //TODO determine if this is just that action's range or movement + action range
        //These are the units that this units action's could target from this tile
        int distToNearestEnemy;
        int enemiesInUnitRange = 0;
        int alliesInUnitRange = 0;

        public TileData(Tile tile)
        {
            Tile = tile;
        }

        public int Threat
        {
            get
            {
                return enemyThreat;
            }

            set
            {
                enemyThreat = value;
            }
        }

        public int EnemiesInUnitRange
        {
            get
            {
                return enemiesInUnitRange;
            }

            set
            {
                enemiesInUnitRange = value;
            }
        }

        public int AlliesInUnitRange
        {
            get
            {
                return alliesInUnitRange;
            }

            set
            {
                alliesInUnitRange = value;
            }
        }

        public Tile Tile
        {
            get
            {
                return tile;
            }

            set
            {
                tile = value;
            }
        }

        public int DistToNearestEnemy
        {
            get
            {
                return distToNearestEnemy;
            }

            set
            {
                distToNearestEnemy = value;
            }
        }
    }

    Dictionary<Vector2Int, Tile> mapDict = new Dictionary<Vector2Int, Tile>();

    /********************************************* UNITY FUNCTIONS ******************************************/

	// Use this for initialization
	void Awake () {
        //Find all the tiles and add them to the dictionary. Ignoring duplicates
        var tiles = FindObjectsOfType<Tile>();
        foreach(Tile t in tiles)
        {
            if (!mapDict.ContainsKey(t.GetGridPos()))
                mapDict.Add(t.GetGridPos(), t);
        }
	}
	
    /********************************************* SEARCH FUNCTIONS ***************************************/

    private Dictionary<Tile, TileSearchField> RangeLimitedSearch(Tile startTile, int range)
    {
        return RangeLimitedSearch(startTile, null, range, false, true, -1);
    }

    //TODO Possibly Change to dijkstra
    private Dictionary<Tile, TileSearchField> RangeLimitedSearch(Tile startTile, Tile endTile, int range, bool movement, bool traverseAsteroid, int playerNumber){
        if (range <= 0 || startTile == null)
            return null;
        bool endFound = false;
        //Create a dictionary contian the TileSearchFields to be used in the search
        Dictionary<Vector2Int, TileSearchField> toSearch = new Dictionary<Vector2Int, TileSearchField>();
        foreach(var t in mapDict.Values)
        {
            toSearch.Add(t.GetGridPos(), new TileSearchField(t));
        }

        //Set up the Queue and Set of tilesInRange
        HashSet<TileSearchField> tilesInRange = new HashSet<TileSearchField>();
        Queue<TileSearchField> tileQueue = new Queue<TileSearchField>();
        tileQueue.Enqueue(toSearch[startTile.GetGridPos()]);
        tilesInRange.Add(toSearch[startTile.GetGridPos()]);

        //While there are tiles to search and range hasn't been exceed loop
        while (range > 0 && tileQueue.Count > 0 && !endFound)
        {
            //Create a list of tiles to be added at this range increment
            List<TileSearchField> currentTiles = new List<TileSearchField>();
            while (tileQueue.Count > 0 && !endFound)
            {
                TileSearchField tileToExamine = tileQueue.Dequeue();
                tileToExamine.visited = true;
                //tilesInRange.Add(tileToExamine);

                //Get appropriate TileSearchFields
                foreach (var t in GetSurroundingTiles(tileToExamine.Tile))
                {
                    //TODO Add accounting for Terrain may need to change algo
                    TileSearchField tsf = toSearch[t.GetGridPos()];
                    //Only add a Tile if the tile has not been visited
                    //If this is a movement search do not allow the search to pass through units the player doesn't own
                    if (!tsf.visited && (!movement || tsf.Tile.UnitOnTile == null || tsf.Tile.UnitOnTile.PlayerNumber == playerNumber))
                    {
                        //If the search can traverse Asteroids or the tile is not an Asteroid add to range
                        //TODO expand to better account for asteroids and other tiles if needed
                        if(traverseAsteroid || tsf.Tile.Type != Tile.TileType.asteroid)
                        {
                            tsf.exploredFrom = tileToExamine.Tile;
                            tilesInRange.Add(tsf);
                            currentTiles.Add(tsf);
                            if (tsf.Tile == endTile)
                                endFound = true;   
                        }

                    }
                    /*If this is a movement search and there is an enemy on the tile add it to the tilesInRange
                     *But mark it as visited so that it won't be counted for movement but will be accounted for
                     *this will be useful for determining melee range. The function that gets movement will remove this tile before
                     *returning the list it creates
                     */
                    if (!tsf.visited && ((movement && tsf.Tile.UnitOnTile != null && tsf.Tile.UnitOnTile.PlayerNumber != playerNumber)
                        || (!traverseAsteroid && tsf.Tile.Type == Tile.TileType.asteroid)))
                    {
                        tsf.visited = true;
                        tilesInRange.Add(tsf);
                    }
                }
            }
            //Add tiles at this range increment to queue
            foreach (var tile in currentTiles)
                tileQueue.Enqueue(tile);
            range--;
        }

        //Convert HashSet to dictionary for use in other functions
        Dictionary<Tile, TileSearchField> result = new Dictionary<Tile, TileSearchField>();
        foreach(var t in tilesInRange)
        {
            result.Add(t.Tile, t);
        }
        return result;

    }


    /******************************************** RANGE FUNCTIONS (MOVEMENT BASED) ***********************************/

    public List<Tile> GetMovementRange(Unit unit)
    {
        bool traverseAsteroid = false;
        Dictionary<Tile, TileSearchField> tileDict = RangeLimitedSearch(unit.CurrentTile, null, unit.GetMovementRange(), true, traverseAsteroid, unit.PlayerNumber);
        if (tileDict == null)
            return null;
        List<Tile> tilesInRange = new List<Tile>();
        foreach(var t in tileDict.Keys)
        {
            if (t.UnitOnTile == null && (traverseAsteroid || t.Type != Tile.TileType.asteroid))
                tilesInRange.Add(t);
        }
        return tilesInRange;
    }

    public List<Tile> GetMovementPath(Unit unit, Tile end)
    {
        return GetMovementPath(unit.CurrentTile, end, unit.PlayerNumber);
    }

    public List<Tile> GetMovementPath(Tile start, Tile end, int playerNumber)
    {
        if (start == end)//Can't get a path from a tile to itself
            return null;
        //Use the size to ensure that range is unlimited regardless of the size of the map
        Dictionary<Tile, TileSearchField> tileDict = RangeLimitedSearch(start, end, mapDict.Count, true, false, playerNumber);
        if (tileDict == null)
            return null;

        Tile nextTile = tileDict[end].exploredFrom;
        List<Tile> path = new List<Tile>();
        path.Add(end);
        bool startFound = false;
        while (nextTile != null && !startFound) //Walk back from end adding tiles to path
        {
            path.Add(nextTile);
            if (nextTile == start)
                startFound = true;
            else
                nextTile = tileDict[nextTile].exploredFrom;
        }

        if (startFound) //If path is valid reverse path and return
        {
            path.Reverse();
            return path;
        }
        else
            return null;
    }

    
    /************************************ MELEE RANGE FUNCTIONS (MOVEMENT BASED) ************************************/

    public List<Tile> GetMeleeRange(Unit unit)
    {
        Dictionary<Tile, TileSearchField> tileDict = RangeLimitedSearch(unit.CurrentTile, null, unit.GetMovementRange() + 1, true, false, unit.PlayerNumber);
        return tileDict != null ? new List<Tile>(tileDict.Keys) : new List<Tile>();
    }
    public List<Tile> GetShortAttackRange(Unit unit)
    {
        Dictionary<Tile, TileSearchField> tileDict = RangeLimitedSearch(unit.CurrentTile, null, 1, true, false, unit.PlayerNumber);
        return tileDict != null ? new List<Tile>(tileDict.Keys) : new List<Tile>();
    }
    public List<Tile> GetLongAttackRange(Unit unit)
    {
        Dictionary<Tile, TileSearchField> tileDict = RangeLimitedSearch(unit.CurrentTile, null, 4, true, false, unit.PlayerNumber);
        return tileDict != null ? new List<Tile>(tileDict.Keys) : new List<Tile>();
    }

    public List<Unit> GetUnitsInMeleeRange(Unit unit)
    {
        List<Tile> tilesInMeleeRange = GetMeleeRange(unit);
        if (tilesInMeleeRange == null)
            return null;
        List<Unit> unitsInMeleeRange = new List<Unit>();
        foreach(var t in tilesInMeleeRange)
        {
            if (t.UnitOnTile != null)
                unitsInMeleeRange.Add(t.UnitOnTile);
        }

        return unitsInMeleeRange;
    }

    public List<Unit> GetEnemyUnitsInMeleeRange(Unit unit)
    {
        List<Unit> unitsInRange = GetUnitsInMeleeRange(unit);
        List<Unit> enemiesInRange = new List<Unit>();
        foreach(var u in unitsInRange)
        {
            if (u.PlayerNumber != unit.PlayerNumber)
                enemiesInRange.Add(u);
        }
        return enemiesInRange;
    }

    public List<Unit> GetAllyUnitsInMeleeRange(Unit unit)
    {
        List<Unit> unitsInRange = GetUnitsInMeleeRange(unit);
        List<Unit> enemiesInRange = new List<Unit>();
        foreach (var u in unitsInRange)
        {
            if (u.PlayerNumber == unit.PlayerNumber)
                enemiesInRange.Add(u);
        }
        return enemiesInRange;
    }

    
    /************************************ RANGE FUNCTIONS (NOT MOVEMENT BASED) ********************************/


    //Returns a list of all units in range passed including on the start tile
    public List<Unit> GetUnitsInRange(Tile startTile, int range)
    {
        Dictionary<Tile, TileSearchField> tileDict = RangeLimitedSearch(startTile, range);
        List<Unit> units = new List<Unit>();
        List<Tile> tiles = null;
        if (tileDict != null)
            tiles = new List<Tile>(tileDict.Keys);
        foreach (var t in tiles)
        {
            if (t.UnitOnTile != null)
                units.Add(t.UnitOnTile);
        }
        return units;
    }

    //Returns a list of tiles that are within a given range
    public List<Tile> GetTilesInRange(Tile startTile, int range)
    {
        Dictionary<Tile, TileSearchField> tileDict = RangeLimitedSearch(startTile, range);
        return tileDict != null ? new List<Tile>(tileDict.Keys) : new List<Tile>();
    }

    //Returns a list of tiles that represent a path from the start tile to the end tile
    public List<Tile> GetPath(Tile start, Tile end)
    {
        if (start == end)//Can't get a path from a tile to itself
            return null;
        //Use the size to ensure that range is unlimited regardless of the size of the map
        Dictionary<Tile, TileSearchField> tileDict = RangeLimitedSearch(start, mapDict.Count);
        if (tileDict == null)
            return null;

        Tile nextTile = tileDict[end].exploredFrom;
        List<Tile> path = new List<Tile>();
        path.Add(end);
        bool startFound = false;
        while (nextTile != null && !startFound) //Walk back from end adding tiles to path
        {
            path.Add(nextTile);
            if (nextTile == start)
                startFound = true;
            else
                nextTile = tileDict[nextTile].exploredFrom;
        }

        if (startFound) //If path is valid reverse path and return
        {
            path.Reverse();
            return path;
        }
        else
            return null;
    }

    /*********************************** SPECIAL RANGE FUNCTIONS ************************************/

    /// <summary>
    /// This method will take a unit and range extension and return all tiles the unit can move to
    /// then for the outer edge of the tiles will add tiles that are with in the rangeExtension not based
    /// on movement. This is to allow the AI and UI to know how far from a units current position it can
    /// use an ability (like healing)
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="rangeExtension"></param>
    /// <returns>A list of tiles that are in the units extended range</returns>
    public List<Tile> GetMovementRangeExtended(Unit unit, int rangeExtension)
    {
        int moveRange = unit.GetMovementRange();
        int pNumber = unit.PlayerNumber;
        Tile start = unit.CurrentTile;

        HashSet<Tile> range = new HashSet<Tile>(RangeLimitedSearch(start, null, moveRange, true, false, pNumber).Keys);
        HashSet<Tile> outerEdge = new HashSet<Tile>(range);
        outerEdge.ExceptWith(RangeLimitedSearch(start, null, moveRange - 1, true, false, pNumber).Keys);
        foreach(var t in outerEdge)
        {
            range.UnionWith(RangeLimitedSearch(t, null, rangeExtension, false, true, pNumber).Keys);
        }
        return new List<Tile>(range);
    }

    /********************************* TILE DATA FUNCTIONS **************************************/

    public Dictionary<Tile, TileData> GetTileData(Unit unit, int range)
    {
        return GetTileData(new List<Tile>(mapDict.Values), unit, range);
    }

    public Dictionary<Tile, TileData> GetTileData(List<Tile> tiles, Unit unit, int range)
    {
        int playerNumber = unit.PlayerNumber;
        List<Unit> units = GetAllUnits();
        Dictionary<Tile, int> threatValues = GetThreatValues(playerNumber);
        Dictionary<Tile, TileData> dataDict = new Dictionary<Tile, TileData>();
        foreach (var t in tiles)
        {
            TileData td = new TileData(t);
            td.Threat = threatValues[t];
            //For each unit determine if they are within range
            foreach (var u in units)
            {
                Tile unitTile = u.CurrentTile;
                if (u != unit && unitTile != null)
                {
                    //If the tiles coords are with range of the current tile increment apropriate count
                    int distanceToUnit = GetTileDistance(unitTile, t);
                    if (distanceToUnit <= range)
                    {
                        if (u.PlayerNumber == playerNumber)
                            td.AlliesInUnitRange++;
                        else
                            td.EnemiesInUnitRange++;
                    }

                }
            }
            td.DistToNearestEnemy = DistanceToNearestEnemy(t, playerNumber);
            dataDict.Add(t, td);
        }

        return dataDict;
    }

    public Dictionary<Tile, int> GetThreatValues(int playerNumber)
    {
        List<Unit> enemies = GetAllEnemies(playerNumber);
        List<HashSet<Tile>> enemyMeleeRanges = new List<HashSet<Tile>>();
        Dictionary<Tile, int> threatValues = new Dictionary<Tile, int>();
        foreach (var t in mapDict.Values)
        {
            threatValues.Add(t, 0);
        }
        foreach (var e in enemies)
        {
            if (e.MeleeCapable)
            {
                List<Tile> meleeRange = GetMeleeRange(e);
                foreach(var t in meleeRange)
                {
                    int threat = e.GetThreat() - GetTileDistance(t, e.CurrentTile);
                    threatValues[t] += threat;
                }
            }
            else if (e.LongRangeCapability > 0)
            {
                List<Tile> longRange = GetTilesInRange(e.CurrentTile, e.LongRangeCapability);
                foreach(var t in longRange)
                {
                    int threat = e.GetThreat() - GetTileDistance(t, e.CurrentTile);
                    threatValues[t] += threat;
                }
            }
        }
        return threatValues;
    }

    /********************************* UTILITY FUNCTIONS **************************************/

    //Returns a list of all units that are on the map
    /// <summary>
    /// Gets all the units currently on the map
    /// </summary>
    /// <returns>A list of units</returns>
    public List<Unit> GetAllUnits()
    {
        List<Unit> units = new List<Unit>();
        foreach (var t in mapDict.Values)
            if (t.UnitOnTile != null)
                units.Add(t.UnitOnTile);
        return units;
    }

    public List<Unit> GetAllEnemies(Unit unit)
    {
        return GetAllEnemies(unit.PlayerNumber);
    }

    private List<Unit> GetAllEnemies(int playerNumber)
    {
        List<Unit> units = GetAllUnits();
        List<Unit> enemies = new List<Unit>();
        foreach (var u in units)
            if (playerNumber != u.PlayerNumber)
                enemies.Add(u);
        return enemies;
    }
    public List<Unit> GetAllAllies(Unit unit)
    {
        List<Unit> units = GetAllUnits();
        List<Unit> allies = new List<Unit>();
        foreach (var u in units)
            if (unit.PlayerNumber == u.PlayerNumber)
                allies.Add(u);
        return allies;
    }

    public List<Unit> GetUnits(List<Tile> tiles)
    {
        List<Unit> units = new List<Unit>();
        foreach (var t in tiles)
            if (t.UnitOnTile != null)
                units.Add(t.UnitOnTile);
        return units;
    }


    public List<Tile> GetWormholeTiles()
    {
        List<Tile> tiles = new List<Tile>();
        foreach (var t in mapDict.Values)
            if (t.Type == Tile.TileType.wormhole)
                tiles.Add(t);
        return tiles;
    }
    
    public Tile GetTileByCoord(int x, int y)
    {
        return GetTileByVector(new Vector2Int(x, y));
    }

    public Tile GetTileByVector(Vector2Int vector)
    {
        Tile tile;
        if (mapDict.TryGetValue(vector, out tile))
            return tile;
        return null;
    }

    public List<Tile> GetSurroundingTiles(Tile tile)
    {
        List<Tile> tiles = new List<Tile>();
        int x = tile.GetGridPos().x;
        int y = tile.GetGridPos().y;

        //Check each position aroud the tile and if there is a tile add it to the list
        Tile adjacentTile = GetTileByCoord(x, y + 1);
        if (adjacentTile)
            tiles.Add(adjacentTile);
        adjacentTile = GetTileByCoord(x + 1, y);
        if (adjacentTile)
            tiles.Add(adjacentTile);
        adjacentTile = GetTileByCoord(x, y - 1);
        if (adjacentTile)
            tiles.Add(adjacentTile);
        adjacentTile = GetTileByCoord(x - 1, y);
        if (adjacentTile)
            tiles.Add(adjacentTile);

        return tiles;
    }

    public int DistanceToNearestEnemy(Tile tile, int playerNumber)
    {
        int distance = int.MaxValue;
        List<Unit> enemies = GetAllEnemies(playerNumber);
        foreach(var e in enemies)
        {
            if(e.CurrentTile != null)
                distance = Math.Min(distance, GetTileDistance(tile, e.CurrentTile));
        }
        return distance;
    }

    public int GetTileDistance(Tile a, Tile b)
    {
        return (Math.Abs(a.GetGridPos().x - b.GetGridPos().x) + Math.Abs(a.GetGridPos().y - b.GetGridPos().y));
    }

    //Checks if units are adjacent to each other
    public bool UnitsAreAdjacent(Unit u1, Unit u2)
    {
        if (u1 == null || u2 == null || u1.CurrentTile == null || u2.CurrentTile == null)
            return false;
        bool adjacent = false;
        List<Tile> tilesAroundU1 = GetSurroundingTiles(u1.CurrentTile);
        foreach (var t in tilesAroundU1)
        {
            if (u2.CurrentTile == t)
                adjacent = true;
        }
        return adjacent;
    }

    public List<Unit> GetAdjacentUnits(Unit unit)
    {
        return GetAdjacentUnits(unit.CurrentTile);
    }

    public List<Unit> GetAdjacentUnits(Tile tile)
    {
        List<Tile> tilesAroundUnit = GetSurroundingTiles(tile);
        List<Unit> units = new List<Unit>();
        foreach (var t in tilesAroundUnit)
        {
            if (t.UnitOnTile != null)
                units.Add(t.UnitOnTile);
        }
        return units;
    }

    public void ColorTiles(List<Tile> tiles, Tile.TileColor tileColor)
    {
        foreach( var t in tiles)
        {
            t.SetTileColor(tileColor);
        }
    }

    //Resets all tiles in map to base color
    public void ResetTileColors()
    {
        foreach (var tile in mapDict.Values)
            tile.ResetTileColor();
    }

}
