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

    public Tile GetMapTileByCoord(int x, int y)
    {
        //TODO Optimize search
        foreach(Tile tile in mapTiles)
        {
            if (tile.GridPosition.x == x && tile.GridPosition.y == y)
                return tile;
        }
        return null;
    }

}
