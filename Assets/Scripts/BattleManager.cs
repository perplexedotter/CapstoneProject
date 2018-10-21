using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour {

    //Make avaliable in the editor
    [Header("Map Properties")]
    [SerializeField] int mapSize = 11;
    [SerializeField] float unitHeightOffset = 1.5f;
    [SerializeField] Map map;
    [SerializeField] AIController ai;

    public static BattleManager instance;
	[SerializeField] GameObject PlayerUnitPreFab;
	[SerializeField] GameObject Player2UnitPreFab;

    //public GameObject TilePreFab;
    //public GameObject NonPlayerUnitPreFab;

    private List<Tile> activeUnitPosMoves;

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
        //GenerateUnits();
        AddUnitsFromMap();
        NextTurn();
        activeUnit = units[unitIndex];
	}

    private void AddUnitsFromMap()
    {
        List<Unit> unitsFromMap = map.GetAllUnits();
        foreach (var u in unitsFromMap)
            units.Add(u);
    }

    // Update is called once per frame
    //TODO move this out of update
    void Update () {
		//units[unitIndex].TurnUpdate();
	}
	public void NextTurn()
    {
        map.ResetTileColors();
        UpdateActiveUnit();
        UpdateCurrentPossibleMoves();
        ShowCurrentPossibleMoves();
        if (activeUnit.AIUnit)
        {
            ai.GetAIActions(activeUnit);
        }
        //decrements that status of active unit
        activeUnit.DecrementStatuses();
    }

    private void UpdateActiveUnit()
    {
        if (unitIndex + 1 < units.Count)
            unitIndex++;
        else
            unitIndex = 0;
        activeUnit = units[unitIndex];
    }



	//this will create the units on the map for this level
	void GenerateUnits(){
		Unit unit;
        //Unit 0
        GameObject obj = Instantiate(PlayerUnitPreFab, new Vector3(-100, -100, -100), Quaternion.Euler(new Vector3()));
        obj.transform.parent = transform;
        unit = obj.GetComponent<Unit>();
        unit.PlayerNumber = 0;


        //makes the unit a fighter
        unit.DefineUnit(UnitType.fighter);

        unit.PlaceOnTile(map.GetTileByCoord(5, 1));
        units.Add(unit);

        //Unit 1
        obj = Instantiate(PlayerUnitPreFab, new Vector3(-100, -100, -100), Quaternion.Euler(new Vector3()));
        obj.transform.parent = transform;
        unit = obj.GetComponent<Unit>();
        unit.PlayerNumber = 0;

        //makes the unit a fighter
        unit.DefineUnit(UnitType.fighter);

        unit.PlaceOnTile(map.GetTileByCoord(2, 2));
        units.Add(unit);

        //Unit 2
        obj = Instantiate(Player2UnitPreFab, new Vector3(-100, -100, -100), Quaternion.Euler(new Vector3()));
        obj.transform.parent = transform;
        unit = obj.GetComponent<Unit>();

        //makes the unit a fighter
        unit.DefineUnit(UnitType.fighter);
        unit.PlayerNumber = 1;

        unit.PlaceOnTile(map.GetTileByCoord(1, 1));
        units.Add(unit);
        NextTurn();
    }

    

    //Update the active Units possible moves
    public void UpdateCurrentPossibleMoves()
    {
        //activeUnitPosMoves = map.GetTilesInRange(activeUnit.CurrentTile, activeUnit.GetMovementRange());
        activeUnitPosMoves = map.GetMovementRange(activeUnit);
    }

    //Display the active Units possible moves
    public void ShowCurrentPossibleMoves()
    {
        //map.ResetTileColors();
        foreach (Tile tile in activeUnitPosMoves)
            tile.SetTileColor(Tile.TileColor.move);
    }

    //Respond to use clicking on a tile
    public void TileClicked(Tile tile)
    {
        //TODO Add logic for attacking and abilities
        if (activeUnitPosMoves != null && activeUnitPosMoves.Contains(tile) 
            && !activeUnit.IsMoving && tile != activeUnit.CurrentTile
            && tile.UnitOnTile == null && !activeUnit.AIUnit)
        {
            //TODO Move this logic elsewhere
            activeUnit.TraversePath(map.GetMovementPath(activeUnit, tile));
        }
    }

    //TODO Have Battle Manager record this and then determine what to do
    public void FinishedMovement()
    {
        //TODO AI won't move unit directly this is just a test
        //if(ai.FinishedMovement())
        //if (activeUnit.AIUnit)
        //    ai.FinishedMovement();
        NextTurn();
    }

    //TODO add context dependent actions for unit clicks
    public void UnitClicked(Unit unit)
    {

    }

    //add short range module to active unit
    public void addShortRangeModule()
    {
        activeUnit.AddModule(new MeleeAttackModule());
        Debug.Log("HP:" + activeUnit.GetHP() + "  Mass:" + activeUnit.GetMass());
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

    //Example of how to add status effect 
    public void addStatusEffect()
    {
        //for testing
        activeUnit.AddStatus(new StatusEffects(5, 100, statusType.mass));
        map.ResetTileColors();
        UpdateCurrentPossibleMoves();
        ShowCurrentPossibleMoves();
    }

    //for testing getAction of activeUnit
    public void getActions()
    {
        List<Action> action = activeUnit.getActions();
    }

    //for testing taking damage
    public void takeDamage()
    {
        Debug.Log(activeUnit.GetHP() - activeUnit.GetDamage());
        activeUnit.TakeDamage(50);
        Debug.Log(activeUnit.GetHP() - activeUnit.GetDamage());
    }


    //TEST FUNCTIONS

    public void DisplayMeleeRange()
    {
        map.ResetTileColors();
        List<Tile> meleeRange = map.GetMeleeRange(activeUnit);
        ColorTiles(meleeRange, Tile.TileColor.attack);
        //ShowCurrentPossibleMoves();

        List<Unit> allysInRange = map.GetAllyUnitsInMeleeRange(activeUnit);
        List<Unit> enemiesInRange = map.GetAllyUnitsInMeleeRange(activeUnit);
        foreach(var u in allysInRange)
        {
            u.CurrentTile.SetTileColor(Tile.TileColor.ally);
        }
    }

    public void ColorTiles(List<Tile> tiles, Tile.TileColor color)
    {
        foreach(var t in tiles)
        {
            t.SetTileColor(color);
        }
    }

    public void DisplayMovementRange()
    {
        ShowCurrentPossibleMoves();
    }

    //   public void MoveCurrentPlayer(Tile destinationTile) {
    //       activeUnit.MoveToTile(destinationTile);
    //}

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