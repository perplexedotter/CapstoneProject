using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Tile : MonoBehaviour {

    public enum TileColors { standard, move, attack }

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
    [Header("Grid Posistion")]
    [SerializeField] int xCoord;
    [SerializeField] int yCoord;

    //[SerializeField] Material baseMaterial;
    //[SerializeField] Material mouseOverMaterial;

    [Header("Material Colors")]
    [SerializeField] Color baseColor;
    [SerializeField] Color moveRangeColor;
    [SerializeField] Color attackRangeColor;


    //TODO Possibly remove this. Tile may not need to care if unit is there and GameManager can handle that
    private Character characterOnTile;

    Renderer[] childrenRenderers;

    bool visted = false;

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

    //TODO Clean up coordianate systems
    private void Awake()
    {
        xCoord = coords.x = (int)Mathf.Floor(transform.position.x / 10);
        yCoord = coords.y = (int)Mathf.Floor(transform.position.z / 10);
    }

    // Use this for initialization
    void Start () {
        childrenRenderers = GetComponentsInChildren<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {
        xCoord = coords.x = (int)Mathf.Floor(transform.position.x / 10);
        yCoord = coords.y = (int)Mathf.Floor(transform.position.z / 10);

    }

    public void SetColor(Color color)
    {
        Queue<Color> colors = new Queue<Color>();
        foreach (Renderer r in childrenRenderers)
        {
            r.material.color = color;
            colors.Enqueue(color);
        }
        SendMessage("UpdateBaseColorQueue", colors);
    }

    //DEPRECIATED Use SetTileColor instead
    //Update all faces material
    //public void UpdateMaterial(Material material)
    //{
    //    foreach (Renderer renderer in childrenRenderers)
    //    {
    //        renderer.material = material;
    //    }
    //}

    void OnMouseDown(){
        GameManager.instance.TileClicked(this);
	}

    //DEPRECIATED Use ResetTileColor Instead
    //public void ResetTileMaterial()
    //{
    //    UpdateMaterial(BaseMaterial);
    //}

    public void ResetTileColor()
    {
        SetColor(baseColor);
    }



    public void SetTileColor(TileColors color)
    {
        switch (color)
        {
            case TileColors.standard:
                SetColor(baseColor);
                break;
            case TileColors.move:
                SetColor(moveRangeColor);
                break;
            case TileColors.attack:
                SetColor(attackRangeColor);
                break;
            default:
                SetColor(baseColor);
                break;
        }
    }

    private void StartHighlightAnimation()
    {

    }

    private void StopHighlightAnimation()
    {

    }
}
