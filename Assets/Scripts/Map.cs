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

    //TODO Possibly Change to dijkstra
    private Dictionary<Tile, TileSearchField> RangeLimitedSearch(Tile startTile, int range){
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

        //While there are tiles to search and range hasn't been exceed loop
        while (range >= 0 && tileQueue.Count > 0)
        {
            //Create a list of tiles to be added at this range increment
            List<TileSearchField> currentTiles = new List<TileSearchField>();
            while (tileQueue.Count > 0)
            {
                TileSearchField tileToExamine = tileQueue.Dequeue();
                tileToExamine.visited = true;
                tilesInRange.Add(tileToExamine);

                //Get appropriate TileSearchFields
                foreach (var t in GetSurroundingTiles(tileToExamine.Tile))
                {
                    //TODO Add accounting for Terrain may need to change algo
                    TileSearchField tsf = toSearch[t.GetGridPos()];
                    if (!tsf.visited)
                    {
                        tsf.exploredFrom = tileToExamine.Tile;
                        currentTiles.Add(tsf);
                    }
                }
            }
            //Add tiles at this range increment to queue
            foreach (var tile in currentTiles)
                tileQueue.Enqueue(tile);
            range--;
        }
        if (tilesInRange.Count <= 0)
            return null;
        else
        {
            //Convert HashSet to dictionary for use in other functions
            Dictionary<Tile, TileSearchField> result = new Dictionary<Tile, TileSearchField>();
            foreach(var t in tilesInRange)
            {
                result.Add(t.Tile, t);
            }
            return result;
        }
    }

    //Returns a list of all units in range passed including on the start tile
    public List<Unit> GetUnitsInRange(Tile startTile, int range)
    {
        Dictionary<Tile, TileSearchField> tileDict = RangeLimitedSearch(startTile, range);
        List<Unit> units = new List<Unit>();
        List<Tile> tiles = null;
        if (tileDict != null)
            tiles = new List<Tile>(tileDict.Keys);
        foreach(var t in tiles)
        {
            if (t.UnitOnTile != null)
                units.Add(t.UnitOnTile);
        }
        return units.Count > 0 ? units : null;
    }

    //Returns a list of tiles that are within a given range
    public List<Tile> GetTilesInRange(Tile startTile, int range)
    {
        Dictionary<Tile, TileSearchField> tileDict = RangeLimitedSearch(startTile, range);
        return tileDict != null ? new List<Tile>(tileDict.Keys) : null;
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
        while(nextTile != null && !startFound) //Walk back from end adding tiles to path
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

        //If there are tiles return them, else return null
        return tiles.Count > 0 ? tiles : null;
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
