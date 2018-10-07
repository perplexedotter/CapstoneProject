using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[SelectionBase]
public class TileEditor : MonoBehaviour {

    [SerializeField] [Range(1f, 20f)] float gridSize = 10f;

    TextMesh textMesh;

    private void Start()
    {
    }

    // Update is called once per frame
    void Update () {
        Vector3 snapPos;
        snapPos.x = Mathf.RoundToInt(transform.position.x / gridSize) * gridSize;
        snapPos.z = Mathf.RoundToInt(transform.position.z / gridSize) * gridSize;

        string tilePosition = Mathf.RoundToInt(snapPos.x / 10f) + "," + Mathf.RoundToInt(snapPos.z / 10f);

        textMesh = GetComponentInChildren<TextMesh>();
        //textMesh.text = tilePosition;

        gameObject.name = "Tile " + tilePosition;

        transform.position = new Vector3(snapPos.x, 0f, snapPos.z);
	}
}
