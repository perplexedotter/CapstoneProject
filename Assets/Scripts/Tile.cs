using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Tile : MonoBehaviour {
	
	public Vector2 gridPosition = Vector2.zero;

    [SerializeField] Material baseMaterial;
    [SerializeField] Material mouseOverMaterial;

    private Character characterOnTile;

    Renderer[] childrenRenderers;

    // Use this for initialization
    void Start () {
        childrenRenderers = GetComponentsInChildren<Renderer>();
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

	void OnMouseEnter() {
        UpdateMaterial(mouseOverMaterial);
    }
	void OnMouseExit() {
        //Update all faces material
        UpdateMaterial(baseMaterial);
    }

    void OnMouseDown(){
        print("Click");
		GameManager.instance.moveCurrentPlayer(this);
	}
}
