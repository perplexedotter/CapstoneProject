using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mapShipBuilder : MonoBehaviour {

    [SerializeField] public bool active = false;
    [SerializeField] Color chooseColor;
    [SerializeField] Color baseColor;
    [SerializeField] Color pickedColor;
    Tile tile;
    Vector3 position;
    // Use this for initialization
    void Start () {
        tile = this.GetComponent<Tile>();
        position = this.transform.position;
        if (active)
        {
            tile.SetColor(chooseColor);
        }
    }

    private void OnMouseUp()
    {
        if (active)
        {
            tile.SetColor(pickedColor);
            active = false;
            GameObject ship = GameObject.Find("ShipBuilder");
            shipBuilderInGame shipB = ship.GetComponent<shipBuilderInGame>();
            shipB.addUnit(position);
        }
    }



    // Update is called once per frame
    void Update()
    {

       

    }

}
