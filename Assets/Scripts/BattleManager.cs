using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour {

    //[Header("Map Properties")]
    [SerializeField] Map map;
    [SerializeField] AIController ai;

    public static BattleManager instance;

    private List<Tile> activeUnitPosMoves;

    //Units in battle
    List<Unit> units = new List<Unit>();
    Unit activeUnit;
    int unitIndex = 0;

    //Turn Order
    [SerializeField] List<Unit> roundTurnOrder;
    [SerializeField] int turnIndex;
    [SerializeField] int roundNumber = 0;

    //AI Commands
    List<Command> commands;
    int commandIndex = 0;
    bool unitSlowed;

    //Events this turn
    bool unitMoved;
    int actionsTaken;

    //Menu statuses used to keep track of which menus are open
    //This will determine what clicking on units and tiles does
    bool baseMenuOpen;
    bool moveMenuOpen;
    bool attackMenuOpen;
    bool specialMenuOpen;
    bool specialSelected;
    
    //for Action Panel
    [SerializeField] GameObject actButton1;
    [SerializeField] GameObject actButton2;
    [SerializeField] GameObject actButton3;
    [SerializeField] GameObject actButton4;
    [SerializeField] ActionListControl makeAction;

    //[SerializeField] int mapSize = 11;
    //[SerializeField] float unitHeightOffset = 1.5f;

    //[SerializeField] GameObject PlayerUnitPreFab;
    //[SerializeField] GameObject Player2UnitPreFab;

    //public GameObject TilePreFab;
    //public GameObject NonPlayerUnitPreFab;

    // Use this for initialization
    void Awake() {

        instance = this;


        //this is how to add modules to units via children
        //just know that the units use void awake for building module list -- which means these will get added after they have called awake
        //the units have a small wait time to call functions in awake, which provides enough time to add components
        /*
        GameObject mod = GameObject.Instantiate(Resources.Load("Prefabs/Modules/MeleeModule"), GameObject.Find("Unit").transform) as GameObject;
        GameObject mod2 = GameObject.Instantiate(Resources.Load("Prefabs/Modules/RangeAttackModule"), GameObject.Find("Unit").transform) as GameObject;
        GameObject mod3 = GameObject.Instantiate(Resources.Load("Prefabs/Modules/HealModule"), GameObject.Find("Unit").transform) as GameObject;
        GameObject mod4 = GameObject.Instantiate(Resources.Load("Prefabs/Modules/SlowModule"), GameObject.Find("Unit").transform) as GameObject;
        */



    }
    void Start() {
        //GenerateUnits();
        AddUnitsFromMap();
        NextRound();
        activeUnit = roundTurnOrder[turnIndex];
        ProcessTurn();
    }

    private void AddUnitsFromMap()
    {
        List<Unit> unitsFromMap = map.GetAllUnits();
        foreach (var u in unitsFromMap)
            units.Add(u);
    }


    private void NextRound()
    {
        //TODO add other things assiciated with ending a round
        turnIndex = 0;
        roundNumber++;
        unitSlowed = false;
        roundTurnOrder = new List<Unit>(units); //TODO Maybe make this the previous turnOrder as seed
        UpdateTurnOrder(turnIndex); //Update the turn order for all units
        DisplayTurnOrder();
    }


    // Update is called once per frame
    void Update() {

    }

    public void NextTurn()
    {
        ResetForNextTurn();
        activeUnit.DecrementStatuses(); //Decrement current units statues
        UpdateActiveUnit(); //Update the current unit
        ProcessTurn(); //Begin processing the next turn
    }

    private void ResetForNextTurn()
    {
        //Reset Turn Variables
        commands = null;
        commandIndex = 0;
        unitMoved = false;
        actionsTaken = 0;

        //Reset Menus
        baseMenuOpen = false;
        moveMenuOpen = false;
        attackMenuOpen = false;
        specialMenuOpen = false;
        specialSelected = false;

        //Reset Map
        map.ResetTileColors();
    }

    //Updates the turn order for all units starting at the index given
    //This allows for this function be used after a unt is slowed to imediatily 
    //Update and display the turn order and for the turn order to be updated each round
    private void UpdateTurnOrder(int index)
    {
        //Get units to update
        List<Unit> turnsToUpdate = new List<Unit>();
        for(int i = index; i < roundTurnOrder.Count; i++)
        {
            turnsToUpdate.Add(roundTurnOrder[i]);
        }
        //TODO Convert this to a GetSpeed and Reverse the sort
        turnsToUpdate.Sort((a, b) => a.GetMass().CompareTo(b.GetMass()));
        //Replace updated units
        for(int i = index; i < roundTurnOrder.Count; i++)
        {
            roundTurnOrder[i] = turnsToUpdate[i - index];
        }
    }

    private void UpdateActiveUnit()
    {
        //destroys active unit if destoyed bool = true 
        if (activeUnit.Destroyed())
        {
            roundTurnOrder.RemoveAt(turnIndex);
            units.Remove(activeUnit);
            Explode(activeUnit.transform.position);
            Destroy(activeUnit.gameObject);
            turnIndex -= 1;
        }

        //if (unitIndex + 1 < units.Count)
        //    unitIndex++;
        //else
        //    unitIndex = 0;
        //activeUnit = units[unitIndex];
        if (turnIndex + 1 < roundTurnOrder.Count)
            turnIndex++;
        else
            NextRound();
        activeUnit = roundTurnOrder[turnIndex];

    }

    public void ProcessTurn()
    {
        if (activeUnit.AIUnit)
        {
            ProcessAITurn();
        }
        else
        {
            ProcessPlayerTurn();
        }
    }

    private void ProcessPlayerTurn()
    {
        //TODO Fillout this function to actually process a players turn
        if (unitMoved)
        {
            NextTurn();
        }
        else
        {
            UpdateCurrentPossibleMoves();
            ShowCurrentPossibleMoves();
        }
    }

    private void ProcessAITurn()
    {
        if (commands == null)
            commands = ai.GetAICommands(activeUnit);
        if (commandIndex == commands.Count)
            NextTurn();
        else
        {
            ProcessCommand(commands[commandIndex++]);
        }
    }

    private void ProcessCommand(Command command)
    {
        switch (command.commandType)
        {
            //TODO add checking to make sure AI is making valid moves
            case Command.CommandType.Move:
                UpdateCurrentPossibleMoves();
                ShowCurrentPossibleMoves();
                MoveActiveUnitToTile(command.target);
                break;

            //TODO add processing for other actions
            case Command.CommandType.Action:
                ProcessAITurn();
                break;
        }
    }

    private void MoveActiveUnitToTile(Tile tile)
    {
        activeUnit.TraversePath(map.GetMovementPath(activeUnit, tile));
    }

    //Update the active Units possible moves
    public void UpdateCurrentPossibleMoves()
    {
        //activeUnitPosMoves = map.GetTilesInRange(activeUnit.CurrentTile, activeUnit.GetMovementRange());
        activeUnitPosMoves = map.GetMovementRange(activeUnit);
    }

    //Called by a unit when it has finished moving so that BattleManager can resume control
    public void FinishedMovement()
    {
        unitMoved = true;
        ProcessTurn();
    }


    //DISPLAY FUNCTIONS

    //Display the active Units possible moves
    public void ShowCurrentPossibleMoves()
    {
        //map.ResetTileColors();
        foreach (Tile tile in activeUnitPosMoves)
            tile.SetTileColor(Tile.TileColor.move);
    }

    public void ColorTiles(List<Tile> tiles, Tile.TileColor color)
    {
        foreach (var t in tiles)
        {
            t.SetTileColor(color);
        }
    }

    public void DisplayMovementRange()
    {
        ShowCurrentPossibleMoves();
    }

    public void DisplayMeleeRange()
    {
        map.ResetTileColors();
        List<Tile> meleeRange = map.GetMeleeRange(activeUnit);
        ColorTiles(meleeRange, Tile.TileColor.attack);
        //ShowCurrentPossibleMoves();

        List<Unit> allysInRange = map.GetAllyUnitsInMeleeRange(activeUnit);
        List<Unit> enemiesInRange = map.GetAllyUnitsInMeleeRange(activeUnit);
        foreach (var u in allysInRange)
        {
            u.CurrentTile.SetTileColor(Tile.TileColor.ally);
        }

    }

    //TODO Implement this
    private void DisplayTurnOrder()
    {

    }

    //CLICK FUNCTIONS

    //Respond to use clicking on a tile
    public void TileClicked(Tile tile)
    {
        //TODO Add logic for attacking and abilities
        if (activeUnitPosMoves != null && activeUnitPosMoves.Contains(tile)
            && !activeUnit.IsMoving && tile != activeUnit.CurrentTile
            && tile.UnitOnTile == null && !activeUnit.AIUnit)
        {
            MoveActiveUnitToTile(tile);
        }
    }

    //TODO posible link unit and tile clicks together
    public void UnitClicked(Unit unit)
    {

    }

    //TEST FUNCTIONS

    
    //make explosion at location passed to function
    public void Explode(Vector3 Pos)
    {
        Destroy(GameObject.Instantiate(Resources.Load("Prefabs/Explode"), Pos, Quaternion.identity) as GameObject, 5);

    }

    //adds damage dealt to active unit's running total
    public void AddDamage(float damage)
    {
        activeUnit.AddDamage(damage);
    }

    //returns damage of active unit
    public float GetDamageDealt()
    {
        return activeUnit.GetDamage();
    }
    // make active unit a fighter
    public void DefineFighter()
    {
        activeUnit.DefineUnit(UnitType.fighter);
    }
    //add short range module to active unit
    public void AddShortRangeModule()
    {
        activeUnit.AddModule(new MeleeAttackModule());
        Debug.Log("HP:" + activeUnit.GetHP() + "  Mass:" + activeUnit.GetMass());
        map.ResetTileColors();
        UpdateCurrentPossibleMoves();
        ShowCurrentPossibleMoves();
    }

    //add long range module to active unit
    public void AddLongRangeModule()
    {
        activeUnit.AddModule(new RangeAttackModule());
        Debug.Log("HP:" + activeUnit.GetHP() + "  Mass:" + activeUnit.GetMass());
        map.ResetTileColors();
        UpdateCurrentPossibleMoves();
        ShowCurrentPossibleMoves();
    }
    //add Healing module to active unit
    public void AddHealModule()
    {
        activeUnit.AddModule(new HealModule());
        Debug.Log("HP:" + activeUnit.GetHP() + "  Mass:" + activeUnit.GetMass());
        map.ResetTileColors();
        UpdateCurrentPossibleMoves();
        ShowCurrentPossibleMoves();
    }

    //add Slowing module to active unit
    public void AddSlowModule()
    {
        activeUnit.AddModule(new SlowModule());
        Debug.Log("HP:" + activeUnit.GetHP() + "  Mass:" + activeUnit.GetMass());
        map.ResetTileColors();
        UpdateCurrentPossibleMoves();
        ShowCurrentPossibleMoves();
    }
    //remove short ranged module from active unit
    public void RemoveShortRangeModule()
    {
        activeUnit.RemoveModule(ModuleType.shortRange);
        map.ResetTileColors();
        UpdateCurrentPossibleMoves();
        ShowCurrentPossibleMoves();
    }

    //remove short ranged module from active unit
    public void RemoveLongRangeModule()
    {
        activeUnit.RemoveModule(ModuleType.longRange);
        map.ResetTileColors();
        UpdateCurrentPossibleMoves();
        ShowCurrentPossibleMoves();
    }
    //remove short ranged module from active unit
    public void RemoveHealModule()
    {
        activeUnit.RemoveModule(ModuleType.heal);
        map.ResetTileColors();
        UpdateCurrentPossibleMoves();
        ShowCurrentPossibleMoves();
    }
    //remove short ranged module from active unit
    public void RemoveSlowModule()
    {
        activeUnit.RemoveModule(ModuleType.slow);
        map.ResetTileColors();
        UpdateCurrentPossibleMoves();
        ShowCurrentPossibleMoves();
    }
    //remove all modules from active unit
    public void RemoveAllModules()
    {
        activeUnit.RemoveAllModules();
        map.ResetTileColors();
        UpdateCurrentPossibleMoves();
        ShowCurrentPossibleMoves();
    }

    //Get threat from unit
    public void getThreat()
    {
        Debug.Log(activeUnit.GetThreat());
    }

    //Example of how to add status effect 
    public void AddStatusEffect()
    {
        //for testing
        activeUnit.AddStatus(new StatusEffects(5, 100, statusType.mass));
        map.ResetTileColors();
        UpdateCurrentPossibleMoves();
        ShowCurrentPossibleMoves();
    }

    //for testing getAction of activeUnit
    public void GetActions()
    {
        actButton1.SetActive(false);
        actButton2.SetActive(false);
        actButton3.SetActive(false);
        actButton4.SetActive(false);
        List<Action> action = activeUnit.GetActions();
        makeAction.MakeActionList(action);
    }

    //for testing taking damage
    public void TakeDamage()
    {
        Debug.Log(activeUnit.GetHP() - activeUnit.GetDamage());
        activeUnit.TakeDamage(50);
        Debug.Log(activeUnit.GetHP() - activeUnit.GetDamage());
    }

    

    public void ActivateAction(int i)
    {
        if (i == 1)
        {
            actButton1.SetActive(true);
        }
        if (i == 2)
        {
            actButton2.SetActive(true);
        }
        if (i == 3)
        {
            actButton3.SetActive(true);
        }
        if (i == 4)
        {
            actButton4.SetActive(true);
        }

    }

    //public int MapSize
    //{
    //    get
    //    {
    //        return mapSize;
    //    }
    //}

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

    ////this will create the units on the map for this level
    //void GenerateUnits(){
    //	Unit unit;
    //       //Unit 0
    //       GameObject obj = Instantiate(PlayerUnitPreFab, new Vector3(-100, -100, -100), Quaternion.Euler(new Vector3()));
    //       obj.transform.parent = transform;
    //       unit = obj.GetComponent<Unit>();
    //       unit.PlayerNumber = 0;


    //       //makes the unit a fighter
    //       unit.DefineUnit(UnitType.fighter);

    //       unit.PlaceOnTile(map.GetTileByCoord(5, 1));
    //       units.Add(unit);

    //       //Unit 1
    //       obj = Instantiate(PlayerUnitPreFab, new Vector3(-100, -100, -100), Quaternion.Euler(new Vector3()));
    //       obj.transform.parent = transform;
    //       unit = obj.GetComponent<Unit>();
    //       unit.PlayerNumber = 0;

    //       //makes the unit a fighter
    //       unit.DefineUnit(UnitType.fighter);

    //       unit.PlaceOnTile(map.GetTileByCoord(2, 2));
    //       units.Add(unit);

    //       //Unit 2
    //       obj = Instantiate(Player2UnitPreFab, new Vector3(-100, -100, -100), Quaternion.Euler(new Vector3()));
    //       obj.transform.parent = transform;
    //       unit = obj.GetComponent<Unit>();

    //       //makes the unit a fighter
    //       unit.DefineUnit(UnitType.fighter);
    //       unit.PlayerNumber = 1;

    //       unit.PlaceOnTile(map.GetTileByCoord(1, 1));
    //       units.Add(unit);
    //       NextTurn();
    //   }

}
