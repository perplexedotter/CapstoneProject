using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Map : MonoBehaviour {

    //[SerializeField] List<Tile> mapTiles;

    Dictionary<Vector2Int, Tile> mapDict = new Dictionary<Vector2Int, Tile>();

	// Use this for initialization
    //TODO have script automatically get tiles.
	void Awake () {
        //Find all the tiles and add them to the dictionary. Ignoring duplicates
        var tiles = FindObjectsOfType<Tile>();
        foreach(Tile t in tiles)
        {
            if (!mapDict.ContainsKey(t.GetGridPos()))
                mapDict.Add(t.GetGridPos(), t);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //If the 
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
        Tile adjacentTile = GetTileByCoord(x, y+1);
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

    //Resets all tiles visited status for BFS algo
    public void ResetVisited()
    {
        foreach(var tile in mapDict.Values)
            tile.Visted = false;
    }

    //Resets all tiles in map to base color
    public void ResetTileColors()
    {
        foreach (var tile in mapDict.Values)
            tile.ResetTileColor();
    }

    //DEPRECIATED Use ResetTileColors Instead
    //public void ResetTileMaterials()
    //{
    //    foreach (Tile tile in mapTiles)
    //        tile.ResetTileMaterial();
    //}
}
