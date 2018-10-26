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
    private List<Tile> activeUnitPosMelee;

    //To show tiles for player during attack
    private List<Tile> activeUnitPosShort;
    private List<Tile> activeUnitPosLong;

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
    
    //for handling battle menu
    [SerializeField] Button LongRangeButton;
    [SerializeField] Button ShortTangeButton;
    [SerializeField] Button HealButton;
    [SerializeField] Button SlowButton;
    [SerializeField] Button ActionButton;
    [SerializeField] Button MoveButton;
    [SerializeField] Button EndButton;
    [SerializeField] CanvasGroup BattleMenuCanvas;
    [SerializeField] CanvasGroup ActionMenuCanvas;
    [SerializeField] GameObject BattleMenu;
    [SerializeField] GameObject ActionMenu;
    private bool menuState = false;
    private bool actionState = false;
    private bool movingState = false;
    private bool inputPaused = false;
    enum ActionChosen { longRange, shortRange, heal, slow};
    ActionChosen actionChosen;

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
        ToggleActionMenu(false);

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
        Debug.Log("Next round!");
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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            print("escape key was Clicked");
            if(!activeUnit.AIUnit){
                ResetToBattleMenu();
            }   
        }
    }

    public void NextTurn()
    {
        ResetForNextTurn();
        activeUnit.DecrementStatuses(); //Decrement current units statues
        UpdateActiveUnit(); //Update the current unit
        Debug.Log("It is "+ activeUnit + "\'s turn!");

        if (!activeUnit.AIUnit) {
            ToggleBattleMenu(true);
            ToggleActionMenu(false);
        } else {
            ToggleBattleMenu(false);
            ToggleActionMenu(false);
        }
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
        
        if (unitMoved && actionsTaken > 0)
        {   
            Debug.Log("next bc of processplayerturn and unitmoved is" + unitMoved);
            NextTurn();
        }
        else
        {
            //handles movement tile colorings and clicks
            if (movingState) {
                ToggleBattleMenu(false);
                ToggleActionMenu(false);
                //safety sanity check to catch fast-click bugs
                if(unitMoved)
                {
                    ResetToBattleMenu();
                }
                UpdateCurrentPossibleMoves();
                ShowCurrentPossibleMoves();
            }
            //sets menu for use during turn
            if (menuState) {
                
                if (unitMoved)
                {
                    MoveButton.interactable = false;
                } else {
                    MoveButton.interactable = true;
                }
                if (actionsTaken > 0)
                {
                    ActionButton.interactable = false;
                } else {
                    ActionButton.interactable = true;
                }
                map.ResetTileColors();
            }
            if (actionState) {
                if(actionsTaken > 0)
                {
                    ResetToBattleMenu();
                }
                ToggleBattleMenu(false);
                ToggleActionMenu(false);
                //do actions based on selection
                switch (actionChosen)
                {
                    case ActionChosen.longRange:
                        UpdateCurrentPossibleLongAttack();
                        ShowCurrentPossibleLongRange();
                        break;
                    case ActionChosen.shortRange:
                        UpdateCurrentPossibleShortAttack();
                        ShowCurrentPossibleShortRange();
                        break;
                    case ActionChosen.heal:
                        UpdateCurrentPossibleShortAttack();
                        ShowCurrentPossibleHealRange();
                        break;
                    case ActionChosen.slow:
                        UpdateCurrentPossibleLongAttack();
                        ShowCurrentPossibleSlowRange();
                        break;
                    default:
                        Debug.Log("Entered action state with no action chosen");
                        ResetToBattleMenu();
                        break;

                }
                //do action stuff
            }

        }
    }
    //enables or disables the clickability and the physical button object
    private void ToggleBattleMenu(bool on)
    {
        BattleMenuCanvas.interactable = (on);
        BattleMenu.SetActive(on);
        EndButton.interactable = on;

    } 

    //enables or disables the clickability and the physical button object
    private void ToggleActionMenu(bool on)
    {
        ActionMenuCanvas.interactable = on;
        ActionMenu.SetActive(on);
        if(on) {EndButton.interactable = !on;}
        
    }
 
    private void ResetToBattleMenu()
    {
        StartCoroutine(_ResetToBattleMenu());
    }

    private IEnumerator _ResetToBattleMenu()
    {
        inputPaused = true;
        yield return new WaitForSeconds(.1f);
        inputPaused = false;
        ToggleBattleMenu(true);
        ToggleActionMenu(false);

        movingState = false;
        actionState = false;
        menuState = true;
        ProcessTurn();
    }

    public void ActionButtonClicked()
    {   
        ToggleBattleMenu(false);
        ToggleActionMenu(true);
        
    }
    public void MoveButtonClicked()
    {
        if (!actionState && !movingState)
        {
            menuState = false;
            ToggleBattleMenu(false);
            ToggleActionMenu(false);
            movingState = true;
            ProcessTurn();
        }
    }
    public void EndButtonClicked()
    {
        //makes sure end isnt clicked in error during other work states
        if (!actionState && !movingState)
        {
            NextTurn();
        }   
        
    }
    public void LongButtonClicked()
    {
        menuState = false;
        actionChosen = (ActionChosen)0;
        actionState = true;
        ProcessTurn();
    }
    public void ShortButtonClicked()
    {
        menuState = false;
        actionChosen = (ActionChosen)1;
        actionState = true;
        ProcessTurn();
    }
    public void HealButtonClicked()
    {
        menuState = false;
        actionChosen = (ActionChosen)2;
        actionState = true;
        ProcessTurn();
    }
    public void SlowButtonClicked()
    {
        menuState = false;
        actionChosen = (ActionChosen)3;
        actionState = true;
        ProcessTurn();
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
    //takes the current tile clicked after action and resolves sadi action on unit on tile
    private bool ResolveAction(Tile tile) 
    {
        switch (actionChosen)
        {
            case ActionChosen.longRange:
                if(activeUnitPosLong.Contains(tile) && tile.UnitOnTile && tile.UnitOnTile.AIUnit)
                {
                    Debug.Log("Long Attack Hits" + tile.UnitOnTile.name);
                    tile.UnitOnTile.TakeDamage(50);
                    return true;
                }
                break;
            case ActionChosen.shortRange:
                if(activeUnitPosShort.Contains(tile) && tile.UnitOnTile && tile.UnitOnTile.AIUnit)
                {
                    Debug.Log("Short Attack Hits" + tile.UnitOnTile.name);
                    tile.UnitOnTile.TakeDamage(100);
                    return true;
                }
                break;
            case ActionChosen.heal:
                if(activeUnitPosShort.Contains(tile) && tile.UnitOnTile && !tile.UnitOnTile.AIUnit)
                {
                    Debug.Log("Heal Attack Hits" + tile.UnitOnTile.name);
                    tile.UnitOnTile.TakeDamage(-100);
                    return true;
                }
                break;
            case ActionChosen.slow:
                if(activeUnitPosLong.Contains(tile) && tile.UnitOnTile && tile.UnitOnTile.AIUnit)
                {
                    Debug.Log("Slow Attack Hits" + tile.UnitOnTile.name);
                    tile.UnitOnTile.AddStatus(new StatusEffects(5, 100, statusType.mass));
                    return true;
                }
                break;
            default:
                Debug.Log("Entered action state with no action chosen");
                ResetToBattleMenu();
                break;
        }
        return false;
    }
    //Update the active Units possible moves
    public void UpdateCurrentPossibleMoves()
    {
        //activeUnitPosMoves = map.GetTilesInRange(activeUnit.CurrentTile, activeUnit.GetMovementRange());
        activeUnitPosMoves = map.GetMovementRange(activeUnit);
    }
    public void UpdateCurrentPossibleMelee()
    {
        activeUnitPosMelee = map.GetMeleeRange(activeUnit);
    }
    public void UpdateCurrentPossibleShortAttack()
    {
        activeUnitPosShort = map.GetShortAttackRange(activeUnit);
    }
    public void UpdateCurrentPossibleLongAttack()
    {
        activeUnitPosLong = map.GetLongAttackRange(activeUnit);
    }

    //Called by a unit when it has finished moving so that BattleManager can resume control
    public void FinishedMovement()
    {
        unitMoved = true;
        ResetToBattleMenu();
    }


    //DISPLAY FUNCTIONS

    //Display the active Units possible moves
    public void ShowCurrentPossibleMoves()
    {
        //map.ResetTileColors();
        foreach (Tile tile in activeUnitPosMoves)
            tile.SetTileColor(Tile.TileColor.move);
    }
    public void ShowCurrentPossibleLongRange()
    {
        //map.ResetTileColors();
        foreach (Tile tile in activeUnitPosLong)
            tile.SetTileColor(Tile.TileColor.attack);
    }
    public void ShowCurrentPossibleShortRange()
    {
        //map.ResetTileColors();
        foreach (Tile tile in activeUnitPosShort)
            tile.SetTileColor(Tile.TileColor.attack);
    }
    public void ShowCurrentPossibleHealRange()
    {
        //map.ResetTileColors();
        foreach (Tile tile in activeUnitPosShort)
            tile.SetTileColor(Tile.TileColor.ally);
    }
    public void ShowCurrentPossibleSlowRange()
    {
        //map.ResetTileColors();
        foreach (Tile tile in activeUnitPosLong)
            tile.SetTileColor(Tile.TileColor.attack);
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
        if (!inputPaused)
        {
            //TODO Add logic for attacking and abilities
            if (activeUnitPosMoves != null && activeUnitPosMoves.Contains(tile)
                && !activeUnit.IsMoving && tile != activeUnit.CurrentTile
                && tile.UnitOnTile == null && !activeUnit.AIUnit && movingState)
            {
                MoveActiveUnitToTile(tile);
            }
            //logic for actions taken
            if (actionState && !activeUnit.AIUnit && tile.UnitOnTile)
            {
                ResolveAction(tile);
                actionsTaken++;
                Debug.Log("RESET TO BATTLEMENU AFTER ACTION");
                ResetToBattleMenu();

            }
        }

    }

    //TODO posible link unit and tile clicks together
    public void UnitClicked(Unit unit)
    {
        TileClicked(unit.CurrentTile);
    }

    //TEST FUNCTIONS

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
