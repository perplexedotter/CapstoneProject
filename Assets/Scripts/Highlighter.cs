using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlighter : MonoBehaviour {

    [SerializeField] Color highlightColor = Color.yellow;
    Renderer[] childRenderers;
    Queue<Color> childColors;
    UnityEngine.EventSystems.EventSystem eventSystem;

    private bool highlighted;

    public bool Highlighted
    {
        get
        {
            return highlighted;
        }

        set
        {
            highlighted = value;
        }
    }


    // Use this for initialization
    void Start () {
        childRenderers = GetComponentsInChildren<Renderer>();
        eventSystem = GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnMouseEnter()
    {
        Highlight();
        highlighted = true;
        UnitTileHighlight();
    }

    private void OnMouseOver()
    {
        if (!highlighted)
        {
            Highlight();
            highlighted = true;
            UnitTileHighlight();
        }
    }

    private void OnMouseExit()
    {
        RemoveHighlight();
        highlighted = false;
        RemoveUnitTileHighlight();
    }

    public void Highlight()
    {
        if (childRenderers != null && !eventSystem.IsPointerOverGameObject())
        {
            childColors = new Queue<Color>();
            foreach (Renderer r in childRenderers)
            {
                if (r.material.HasProperty("_Color")){
                    childColors.Enqueue(r.material.color);
                    r.material.color = highlightColor;
                }
            }
        }
        //Send a message to other scripts on object to begin animation

        SendMessage("StartHighlightAnimation");
    }
    public void RemoveHighlight()
    {
        if (childRenderers != null)
        {
            foreach (Renderer r in childRenderers)
            {
                if(childColors != null && childColors.Count > 0 && r.material.HasProperty("_Color"))
                    r.material.color = childColors.Dequeue();
            }
        }
        //Send a message to other scripts on object to stop animation

        SendMessage("StopHighlightAnimation");
    }

    //Links the highlighting of the unit and tile together
    private void UnitTileHighlight()
    {
        Unit unit = GetComponent<Unit>();
        Tile tile = GetComponent<Tile>();
        if (unit != null && unit.CurrentTile != null)
        {
            GameObject unitTile = unit.CurrentTile.gameObject;
            unitTile.GetComponent<Highlighter>().Highlight();
        }
        else if (tile != null && tile.UnitOnTile != null)
        {
            GameObject tileUnit = tile.UnitOnTile.gameObject;
            tileUnit.GetComponent<Highlighter>().Highlight();
        }
    }

    private void RemoveUnitTileHighlight()
    {
        Unit unit = GetComponent<Unit>();
        Tile tile = GetComponent<Tile>();
        if (unit != null && unit.CurrentTile != null)
        {
            GameObject unitTile = unit.CurrentTile.gameObject;
            unitTile.GetComponent<Highlighter>().RemoveHighlight();
        }
        else if (tile != null && tile.UnitOnTile != null)
        {
            GameObject tileUnit = tile.UnitOnTile.gameObject;
            tileUnit.GetComponent<Highlighter>().RemoveHighlight();
        }
    }    //Allows other scripts to send a message indicating the base colors of the children has changed
    private void UpdateBaseColorQueue(Queue<Color> colors)
    {
        //print("Colors Updated");
        childColors = colors;
    }

}
