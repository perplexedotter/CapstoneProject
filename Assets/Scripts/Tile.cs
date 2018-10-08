using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Tile : MonoBehaviour {

    [SerializeField] Vector2 gridPosition = Vector2.zero;

    [SerializeField] Material baseMaterial;
    [SerializeField] Material mouseOverMaterial;

    private Character characterOnTile;

    Renderer[] childrenRenderers;

    bool visted = false;

    public Vector2 GridPosition
    {
        get
        {
            return gridPosition;
        }
    }

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

    // Use this for initialization
    void Start () {
        childrenRenderers = GetComponentsInChildren<Renderer>();
        //Convert transform position to x and y
        gridPosition = new Vector2(Mathf.Floor(transform.position.x / 10), Mathf.Floor(transform.position.z / 10));
        UpdateMaterial(BaseMaterial);
	}
	
	// Update is called once per frame
	void Update () {
		
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
