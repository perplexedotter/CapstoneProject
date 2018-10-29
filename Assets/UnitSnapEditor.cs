using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Unit))]
public class UnitSnapEditor : MonoBehaviour {

    Unit unit;
    Dictionary<Vector2Int, Tile> tileDict;

	// Use this for initialization
	void Start () {
        unit = GetComponent<Unit>();
        tileDict = new Dictionary<Vector2Int, Tile>();
        Tile[] tiles = FindObjectsOfType<Tile>();
        foreach(var t in tiles)
        {
            tileDict.Add(t.GetCoords(), t);
        }
        Tile tile = null;
        if(tileDict.TryGetValue(unit.GetCoords(), out tile)){
            unit.CurrentTile = tile;
            tile.UnitOnTile = unit;
        }
    }
#if UNITY_EDITOR
    // Update is called once per frame
    void Update () {
        if (!Application.isPlaying)
        {
            transform.position = new Vector3(unit.GetCoords().x, 0, unit.GetCoords().y);
            Tile tile = null;
            if (tileDict.TryGetValue(unit.GetCoords(), out tile))
            {
                unit.CurrentTile = tile;
                tile.UnitOnTile = unit;
            }
        }
    }
#endif
}
