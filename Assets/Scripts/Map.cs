using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour {

    [SerializeField] List<Tile> mapTiles;

	// Use this for initialization
	void Start () {
        //mapTiles = GetComponents<Tile>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public Tile GetTileByCoord(int x, int y)
    {
        //TODO Optimize search
        foreach(Tile tile in mapTiles)
        {
            if (tile.GridPosition.x == x && tile.GridPosition.y == y)
                return tile;
        }
        return null;
    }

    public List<Tile> GetSurroundingTiles(Tile tile)
    {
        int x = (int) tile.GridPosition.x;
        int y = (int) tile.GridPosition.y;
        List<Tile> tiles = new List<Tile>();

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

    public void ResetVisited()
    {
        foreach(Tile tile in mapTiles)
        {
            tile.Visted = false;
        }
    }
}
