using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[SelectionBase]
[RequireComponent(typeof(Tile))]
public class TileEditor : MonoBehaviour {
#if UNITY_EDITOR
    Tile tile;

    private void Awake()
    {
        tile = GetComponent<Tile>();
    }

    // Update is called once per frame
    void Update () {
        gameObject.name = "Tile " + tile.GetGridPos().x + "," + tile.GetGridPos().y;

        transform.position = new Vector3(tile.GetCoords().x, 0, tile.GetCoords().y);
	}
#endif
}
