using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    //Make avaliable in the editor
    [Header("Map Properties")]
    [SerializeField] int mapSize = 11;
    [SerializeField] float unitHeightOffset = 1.5f;
    [SerializeField] Map map;

    public static GameManager instance;
	[SerializeField] GameObject PlayerUnitPreFab;
    //public GameObject TilePreFab;
    //public GameObject NonPlayerUnitPreFab;

    private List<Tile> activeUnitPosMoves;

    //Create tile and char lists
	List<Unit> units = new List<Unit>();
    Unit activeUnit;
	int unitIndex = 0;


    public int MapSize
    {
        get
        {
            return mapSize;
        }
    }

    // Use this for initialization
    void Awake(){
		instance = this;		
	}
	void Start () {
		GenerateUnits();
        activeUnit = units[unitIndex];
	}
	
	// Update is called once per frame
    //TODO move this out of update
	void Update () {
		//units[unitIndex].TurnUpdate();
	}
	public void nextTurn()
    {
        map.ResetTileColors();
        UpdateActiveUnit();
        UpdateCurrentPossibleMoves();
        ShowCurrentPossibleMoves();
    }

    private void UpdateActiveUnit()
    {
        if (unitIndex + 1 < units.Count)
            unitIndex++;
        else
            unitIndex = 0;
        activeUnit = units[unitIndex];
    }

 //   public void MoveCurrentPlayer(Tile destinationTile) {
 //       activeUnit.MoveToTile(destinationTile);
	//}

	//this will create the units on the map for this level
	void GenerateUnits(){
		Unit unit;
        //NonPlayerUnit npc;
        GameObject obj = Instantiate(PlayerUnitPreFab, new Vector3(-100, -100, -100), Quaternion.Euler(new Vector3()));
        obj.transform.parent = transform;
        unit = obj.GetComponent<Unit>();
		//unit = ((GameObject)Instantiate(PlayerUnitPreFab, new Vector3(-100, -100, -100), Quaternion.Euler(new Vector3()))).GetComponent<Unit>();

        //makes the unit a fighter
        unit.DefineUnit(UnitType.fighter);

        unit.PlaceOnTile(map.GetTileByCoord(5, 1));
        units.Add(unit);
        nextTurn();

        //unit = ((GameObject)Instantiate(PlayerUnitPreFab, new Vector3((mapSize - 1) - Mathf.Floor(mapSize / 2), 1.5f, -(mapSize - 1) + Mathf.Floor(mapSize / 2)), Quaternion.Euler(new Vector3()))).GetComponent<PlayerUnit>();
        //unit.MoveToTile(map.GetTileByCoord(0, 0));
        //units.Add(unit);

        //Removed Extra units for clarity

        //unit = ((GameObject)Instantiate(PlayerUnitPreFab, new Vector3(4-Mathf.Floor(mapSize/2), 1.5f, -4+Mathf.Floor(mapSize/2)), Quaternion.Euler(new Vector3()))).GetComponent<PlayerUnit>();			
        //units.Add(unit);

        //npc = ((GameObject)Instantiate(NonPlayerUnitPreFab, new Vector3(12-Mathf.Floor(mapSize/2), 1.5f, -4+Mathf.Floor(mapSize/2)), Quaternion.Euler(new Vector3()))).GetComponent<NonPlayerUnit>();			
        //units.Add(npc);
    }

    

    //Update the active Units possible moves
    public void UpdateCurrentPossibleMoves()
    {
        activeUnitPosMoves = map.GetTilesInRange(activeUnit.CurrentTile, activeUnit.GetMovementRange());
    }

    //Display the active Units possible moves
    public void ShowCurrentPossibleMoves()
    {
        foreach (Tile tile in activeUnitPosMoves)
            tile.SetTileColor(Tile.TileColors.move);
    }

    //Respond to use clicking on a tile
    public void TileClicked(Tile tile)
    {
        //TODO Add logic for attacking and abilities
        if (activeUnitPosMoves != null && activeUnitPosMoves.Contains(tile) && !activeUnit.IsMoving && tile != activeUnit.CurrentTile)
        {
            activeUnit.TraversePath(map.GetPath(activeUnit.CurrentTile, tile));
            //activeUnit.MoveToTile(tile);
            //TODO Move this logic elsewhere
        }
    }

    public void FinishedMovement()
    {
        nextTurn();
    }

    //TODO add context dependent actions for unit clicks
    public void UnitClicked(Unit unit)
    {

    }

    //add short range module to active unit
    public void addShortRangeModule()
    {
        activeUnit.AddModule(new Module(ModuleName.shortRange));
        Debug.Log("HP:" + activeUnit.GetHP() + "  Mass:" + activeUnit.GetMass() + "  Attack:" + activeUnit.GetAttack());
        map.ResetTileColors();
        UpdateCurrentPossibleMoves();
        ShowCurrentPossibleMoves();
    }

    //remove short ranged module from active unit
    public void removeShortRangeModule()
    {
        activeUnit.RemoveModule(ModuleName.shortRange);
        map.ResetTileColors();
        UpdateCurrentPossibleMoves();
        ShowCurrentPossibleMoves();
    }

    //remove all modules from active unit
    public void removeAllModules()
    {
        activeUnit.RemoveAllModules();
        map.ResetTileColors();
        UpdateCurrentPossibleMoves();
        ShowCurrentPossibleMoves();
    }

    //DEPRICATED use Map.GetTilesInRange instead
    ////Takes a unit and finds all tiles they could possibly move to
    //public List<Tile> GetPossibleMoves(Unit unit)
    //{
    //    HashSet<Tile> possibleMoves = new HashSet<Tile>();
    //    Queue<Tile> tileQueue = new Queue<Tile>();
    //    int movementRange = unit.GetMovementRange();

    //    //Add starting tile and reset tiles to unvisited
    //    tileQueue.Enqueue(unit.CurrentTile);
    //    map.ResetVisited();

    //    //Loop while there are tiles to examine within range
    //    while (movementRange > 0 && tileQueue.Count > 0)
    //    {
    //        List<Tile> currentTiles = new List<Tile>();
    //        while (tileQueue.Count > 0)
    //        {
    //            Tile tileToExamine = tileQueue.Dequeue();
    //            tileToExamine.Visted = true;
    //            List<Tile> surroundTiles = map.GetSurroundingTiles(tileToExamine);
    //            foreach (Tile tile in surroundTiles)
    //            {
    //                //TODO adjust to account for terrain and for other units in path
    //                if (!possibleMoves.Contains(tile))
    //                {
    //                    possibleMoves.Add(tile);
    //                    if (!tile.Visted)
    //                        currentTiles.Add(tile);
    //                }
    //            }
    //        }
    //        //Add next set of tiles to queue and decrement range
    //        foreach (Tile tile in currentTiles)
    //            tileQueue.Enqueue(tile);
    //        movementRange--;
    //    }

    //    //Return tiles if there are any
    //    List<Tile> posMoves = new List<Tile>(possibleMoves);
    //    return posMoves.Count > 0 ? posMoves : null;
    //}

    //DEPRECIATED refactered mouse enters and exits to a seperate highlighter script
    //public void TileMouseExit(Tile tile)
    //{
    //    if (possibleMoves != null && possibleMoves.Contains(tile))
    //        tile.UpdateMaterial(tileMoveRangeMaterial);
    //    else
    //        tile.ResetTileMaterial();
    //}
}
