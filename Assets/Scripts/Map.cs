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

    Dictionary<Vector2Int, Tile> mapDict = new Dictionary<Vector2Int, Tile>();

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
	
    //SEARCH FUNCTIONS

    //Specialized BFS for unit movement
    //private Dictionary<Tile, TileSearchField> MovementBFS(Unit unit)
    //{

    //}

    private Dictionary<Tile, TileSearchField> RangeLimitedSearch(Tile startTile, int range)
    {
        return RangeLimitedSearch(startTile, range, false, true, -1);
    }

    //TODO Possibly Change to dijkstra
    private Dictionary<Tile, TileSearchField> RangeLimitedSearch(Tile startTile, int range, bool movement, bool traverseAsteroid, int playerNumber){
        if (range <= 0 || startTile == null)
            return null;

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
        while (range > 0 && tileQueue.Count > 0)
        {
            //Create a list of tiles to be added at this range increment
            List<TileSearchField> currentTiles = new List<TileSearchField>();
            while (tileQueue.Count > 0)
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
                        }

                    }
                    /*If this is a movement search and there is an enemy on the tile add it to the tilesInRange
                     *But mark it as visited so that it won't be counted for movement but will be accounted for
                     *this will be useful for determining melee range. The function that gets movement will remove this tile before
                     *returning the list it creates
                     */
                    else if (!tsf.visited && movement && tsf.Tile.UnitOnTile != null && tsf.Tile.UnitOnTile.PlayerNumber != playerNumber)
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


    //RANGE FUNCTIONS (MOVEMENT BASED)

    public List<Tile> GetMovementRange(Unit unit)
    {
        Dictionary<Tile, TileSearchField> tileDict = RangeLimitedSearch(unit.CurrentTile, unit.GetMovementRange(), true, false, unit.PlayerNumber);
        if (tileDict == null)
            return null;
        List<Tile> tilesInRange = new List<Tile>();
        foreach(var t in tileDict.Keys)
        {
            if (t.UnitOnTile == null)
                tilesInRange.Add(t);
        }
        return tilesInRange;
    }

    //TODO possibly make this a call to GetPath and possibly take a unit instead of playerNumber
    public List<Tile> GetMovementPath(Unit unit, Tile end)
    {
        return GetMovementPath(unit.CurrentTile, end, unit.PlayerNumber);
    }

    public List<Tile> GetMovementPath(Tile start, Tile end, int playerNumber)
    {
        if (start == end)//Can't get a path from a tile to itself
            return null;
        //Use the size to ensure that range is unlimited regardless of the size of the map
        Dictionary<Tile, TileSearchField> tileDict = RangeLimitedSearch(start, mapDict.Count, true, false, playerNumber);
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

    
    //MELEE RANGE FUNCTIONS (MOVEMENT BASED)

    public List<Tile> GetMeleeRange(Unit unit)
    {
        Dictionary<Tile, TileSearchField> tileDict = RangeLimitedSearch(unit.CurrentTile, unit.GetMovementRange() + 1, true, false, unit.PlayerNumber);
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

    
    //RANGE FUNCTIONS (NOT MOVEMENT BASED)


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


    //UTILITY FUNCTIONS
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

    //Resets all tiles in map to base color
    public void ResetTileColors()
    {
        foreach (var tile in mapDict.Values)
            tile.ResetTileColor();
    }

    //DEPRECIATED
    ////Resets all tiles visited status for BFS algo
    //public void ResetVisited()
    //{
    //    foreach (var tile in mapDict.Values)
    //        tile.Visted = false;
    //}

    //DEPRECIATED Use ResetTileColors Instead
    //public void ResetTileMaterials()
    //{
    //    foreach (Tile tile in mapTiles)
    //        tile.ResetTileMaterial();
    //}
}
