using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Tile : MonoBehaviour {

    public struct Coords
    {
        public int x, y;

        public Coords(int p1, int p2)
        {
            x = p1;
            y = p2;
        }
    }

    public Coords coords;

    //[SerializeField] Vector2 gridPosition = Vector2.zero;
    [SerializeField] int xCoord;
    [SerializeField] int yCoord;

    [SerializeField] Material baseMaterial;
    [SerializeField] Material mouseOverMaterial;


    //TODO Possibly remove this. Tile may not need to care if unit is there and GameManager can handle that
    private Character characterOnTile;

    Renderer[] childrenRenderers;

    bool visted = false;

    //public Vector2 GridPosition
    //{
    //    get
    //    {
    //        return gridPosition;
    //    }
    //}

    public bool Visted
    {
        get
        {
            return visted;
        }

        set
        {
            visted = value;
        }
    }

    public Material BaseMaterial
    {
        get
        {
            return baseMaterial;
        }

        set
        {
            baseMaterial = value;
        }
    }

    //TODO Clean up coordianate systems
    private void Awake()
    {
        xCoord = coords.x = (int)Mathf.Floor(transform.position.x / 10);
        yCoord = coords.y = (int)Mathf.Floor(transform.position.z / 10);
    }

    // Use this for initialization
    void Start () {
        childrenRenderers = GetComponentsInChildren<Renderer>();
        //Convert transform position to x and y
        //CMathf.Floor(transform.position.x / 10), Mathf.Floor(transform.position.z / 10));

        UpdateMaterial(BaseMaterial);
	}
	
	// Update is called once per frame
	void Update () {
        xCoord = coords.x = (int)Mathf.Floor(transform.position.x / 10);
        yCoord = coords.y = (int)Mathf.Floor(transform.position.z / 10);

    }

    //Update all faces material
    public void UpdateMaterial(Material material)
    {
        foreach (Renderer renderer in childrenRenderers)
        {
            renderer.material = material;
        }
    }

    //TODO maybe let gameManager handle this
	void OnMouseEnter() {
        UpdateMaterial(mouseOverMaterial);
    }
	void OnMouseExit() {
        GameManager.instance.TileMouseExit(this);
    }

    void OnMouseDown(){
        GameManager.instance.TileClicked(this);
	}

    public void ResetTileMaterial()
    {
        UpdateMaterial(BaseMaterial);
    }
}
