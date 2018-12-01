﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class BattleManager : MonoBehaviour {

    [SerializeField] Map map;
    [SerializeField] AIController ai;
    [SerializeField] float aiDelay = 1f;
    [SerializeField] float actionDelay = 1f;
    [SerializeField] float actionEffectsWait = 1f;
    [SerializeField] Text statusText;
    [SerializeField] Text roundsLeftText;

    public static BattleManager instance;
    private List<Tile> activeUnitPosMoves;
    private List<Tile> activeUnitPosMelee;
    private List<Tile> wormholeTileList;
    

    //To show tiles for player during attack
    private List<Tile> activeUnitPosShort;
    private List<Tile> activeUnitPosLong;

    private List<Action> activeUnitPosActions;

    //for checking victories
    
   	public enum VictoryType {waveSurvival, bossKill, destroyAll};
    [SerializeField] VictoryType victoryType;
    [SerializeField] int RoundsToSurvive;
    [SerializeField] int WaveRounds;
    public int ShipsLeft { get; protected set; }
    public int EnemiesLeft { get; protected set; }
    private bool bossDead;
    //Units in battle
    public List<Unit> units = new List<Unit>();
    public Unit activeUnit;
    string statusBarMods = "";
    int unitIndex = 0;
    bool unitTeleported = false;

    //adding waves
    public enum waveEnemyType {attackerAI, healerAI, longAI};
    [Header("Enemies for waves")]
    [SerializeField] GameObject meleeAIObj;
    [SerializeField] GameObject healAIObj;
    [SerializeField] GameObject longAIObj;
    [SerializeField] GameObject slowAIObj;

    [SerializeField] Tile enemyWarp;
    //Turn Order
    [Header("Round and Turn")]
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

    //Game State Flags
    private bool menuState = false;
    private bool actionState = false;
    private bool movingState = false;
    private bool inputPaused = false;

    //for handling battle menu
    [Header("Battle Menu Components")]
    [SerializeField] GameObject menuCanvas;
    [SerializeField] Button LongRangeButton;
    [SerializeField] Button ShortRangeButton;
    [SerializeField] Button HealButton;
    [SerializeField] Button SlowButton;
    [SerializeField] Button ActionButton;
    [SerializeField] Button MoveButton;
    [SerializeField] Button EndButton;
    [SerializeField] CanvasGroup BattleMenuCanvas;
    [SerializeField] CanvasGroup ActionMenuCanvas;
    [SerializeField] CanvasGroup GameOverMenuCanvas;
    [SerializeField] GameObject BattleMenu;
    [SerializeField] GameObject ActionMenu;
    [SerializeField] GameObject GameOverMenu;
    [SerializeField] Button NextGameButton;
    [SerializeField] Button TryAgainButton;
    [SerializeField] Text GameOverText;
    [SerializeField] Text WinLoseText;


    enum ActionChosen { longRange, shortRange, heal, slow};
    ActionChosen actionChosen;
    Action unitAction;

    //for Action Panel
    [SerializeField] GameObject actButton1;
    [SerializeField] GameObject actButton2;
    [SerializeField] GameObject actButton3;
    [SerializeField] GameObject actButton4;
    [SerializeField] ActionListControl makeAction;

    //for unit ui
    [SerializeField] public GameObject currentUnit;
    [SerializeField] public GameObject highlightedUnit;
    [SerializeField] public Text currentUnitText;
    [SerializeField] public Text currentUnitTextInfo;
    [SerializeField] public Text highlightedUnitText;
    [SerializeField] public Text highlightedUnitTextInfo;
    [SerializeField] public GameObject currentFighter;
    [SerializeField] public GameObject currentFighterEnemy;
    [SerializeField] public GameObject currentFrigate;
    [SerializeField] public GameObject currentFrigateEnemy;
    [SerializeField] public GameObject highlightedFighter;
    [SerializeField] public GameObject highlightedFighterEnemy;
    [SerializeField] public GameObject highlightedFrigate;
    [SerializeField] public GameObject highlightedFrigateEnemy;

    [Header("BattleFX")]
    [SerializeField] AudioSource explosionSound;
    [SerializeField] GameObject dmgFX;
    [SerializeField] GameObject healFX;
    [SerializeField] GameObject slowFX;
    //UNITY FUNCTIONS

    // Use this for initialization
    void Awake() {
        ShipsLeft = 0;
        EnemiesLeft = 0;
        instance = this;
        ToggleActionMenu(false);
        ToggleBattleMenu(false);
        bossDead = false;
        this.gameObject.SetActive(false);

    }
    void Start() {
        
        ToggleGameOverMenu(false);
        //TODO Only search children
        units = new List<Unit>(FindObjectsOfType<Unit>());
        GetUnitCounts(units);
        roundNumber = 0;
        wormholeTileList = map.GetWormholeTiles();
        roundTurnOrder = new List<Unit>(units);
        NextRound();
        activeUnit = roundTurnOrder[turnIndex];
        activeUnit.UnitOutline(true);
        statusBarMods = StringifyModList(activeUnit);
        activeUnitPosActions = activeUnit.GetActions();
        ResetToBattleMenu();
        //ProcessTurn();
        
    }

    // Update is called once per frame
    void Update()
    {
        //keep info by activeunit TODO: put in own function
        //Vector3 statusPos = Camera.main.WorldToScreenPoint(activeUnit.transform.position);
        //statusText.transform.position = statusPos;
        //statusText.text = "HP: " + activeUnit.DamageUnit(0) +  "\nType: " + activeUnit.GetShipType() + "\nMods: " + statusBarMods;

        //TODO Add logic to escape the battle menu to let player examine map/units
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //print("escape key was Clicked");
            ResetToBattleMenu();
        }
        //Highlight activeUnit
        //Highlighter h = activeUnit.CurrentTile.GetComponent<Highlighter>();
        //if(h && !h.Highlighted)
        //{
        //    h.Highlight();
        //}
    }

    private void AddUnitsFromMap()
    {
        List<Unit> unitsFromMap = map.GetAllUnits();
        foreach (var u in unitsFromMap)
            units.Add(u);
    }


    /*****************************************TURN / ROUND FUNCTIONS********************************/

    private void NextRound()
    {
        Debug.Log("Next round!");
        //TODO add other things assiciated with ending a round
        turnIndex = 0;
        unitTeleported = false;
        roundNumber++;
        if(victoryType == VictoryType.waveSurvival)
        {
            roundsLeftText.text = "Survive The Waves!\n     Rounds Left: " + (RoundsToSurvive - roundNumber).ToString();
        }
        if (victoryType == VictoryType.waveSurvival && roundNumber % WaveRounds==0)
        {
            GenerateWave();
        }
        UpdateTurnOrder(turnIndex); //Update the turn order for all units
        DisplayTurnOrder();
    }


    public void NextTurn()
    {
        ResetForNextTurn();
        activeUnit.DecrementStatuses(); //Decrement current units statues
        UpdateActiveUnit(); //Update the current unit
        statusBarMods = StringifyModList(activeUnit);
        Debug.Log("It is "+ activeUnit + "\'s turn!");

        if (!activeUnit.AIUnit) {
            ToggleBattleMenu(true);
            ToggleActionMenu(false);
        } else {
            ToggleBattleMenu(false);
            ToggleActionMenu(false);
        }

        if(CheckForVictory(units, ShipsLeft, EnemiesLeft, roundNumber, victoryType)) {
            Debug.Log("GAME IS OVER!!!!!!!");
            GameOver(true);
        }
        if(ShipsLeft <=0) {
            Debug.Log("GAME IS OVER!!!!!!!");
            GameOver(false);
        }
        ResetToBattleMenu();
        //ProcessTurn(); //Begin processing the next turn
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
        //TODO Convert this to a GetSpeed and Reverse the sort (Might keep mass)

        turnsToUpdate = turnsToUpdate.OrderBy(o => o.GetMass()).ToList();
        //turnsToUpdate.Sort((a, b) => a.GetMass().CompareTo(b.GetMass()));
        //Replace updated units
        for(int i = index; i < roundTurnOrder.Count; i++)
        {
            roundTurnOrder[i] = turnsToUpdate[i - index];
        }
    }

    private void UpdateActiveUnit()
    {

        if (turnIndex + 1 < roundTurnOrder.Count)
            turnIndex++;
        else
            NextRound();
        activeUnit.UnitOutline(false);
        //Get rid of the old active Units highlight
        //Highlighter h = activeUnit.CurrentTile.GetComponent<Highlighter>();
        //if (h && h.Highlighted)
        //{
        //    h.RemoveHighlight();
        //}
        activeUnit = roundTurnOrder[turnIndex];
        activeUnit.UnitOutline(true);
        activeUnitPosActions = activeUnit.GetActions();

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
        currentUnitUI();
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
                ShowActionRange(unitAction);
            }

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
                map.ResetTileColors();
                ShowCurrentPossibleMoves();
                MoveActiveUnitToTile(command.target);
                break;

            //TODO add processing for other actions
            case Command.CommandType.Action:
                map.ResetTileColors();
                ShowActionRange(command.action);
                //StartCoroutine(DelayActionCommand(command));
                StartCoroutine(ProcessAction(command.action, command.target));
                break;
        }
    }

    IEnumerator DelayActionCommand(Command command)
    {
        yield return new WaitForSeconds(aiDelay);
        ResolveAction(command.action, command.target);
        ProcessAITurn();
    }

    IEnumerator ProcessAction(Action action, Tile target)
    {
        inputPaused = true; // Stop input while action is being processed
        Unit unit = target.UnitOnTile;
        if(ResolveAction(action, target))
        {
            actionsTaken++;
            activeUnit.DisplayAction(action); //Animate Action
            yield return new WaitForSeconds(actionEffectsWait);
            DisplayActionEffect(action, target.UnitOnTile);
            yield return new WaitForSeconds(actionDelay); //Wait for animation
            //Check if unit is destroyed
            if (unit.Destroyed)
            {
                explosionSound.Play();
                DestroyUnit(unit);
            }
        }
        inputPaused = false; // resume input 
        ResetToBattleMenu();
    }

    private void DisplayActionEffect(Action action, Unit target)
    {
        GameObject fx = null;
        switch (action.Type)
        {
            case ActionType.LongAttack:
            case ActionType.MeleeAttack:
                fx = dmgFX;
                break;
            case ActionType.Heal:
                fx = healFX;
                break;
            case ActionType.Slow:
                fx = slowFX;
                break;
           
        }
        if (fx)
        {
            Vector3 trueTarget = target.transform.position + new Vector3(0, target.HeightOffset, 0);
            var effect = Instantiate(fx, trueTarget, Quaternion.Euler(-90, 0, 0));
            //effect.SetActive(true);
        }
       
    }

    /****************************************** UTILITY FUNCTIONS ******************************/

    private void ToggleGameOverMenu(bool on)
    {
        GameOverMenuCanvas.interactable = (on);
        GameOverMenu.SetActive(on);
        if (on)
        {
            //Remove the battlemanager and other UI
            //GameObject toDeactivate = GameObject.Find("WinCondition");
            //if (toDeactivate)
            //    toDeactivate.SetActive(false);
            //toDeactivate = GameObject.Find("CurrentUnit");
            //if (toDeactivate)
            //    toDeactivate.SetActive(false);
            ////GameObject.Find("CurrentUnit").SetActive(false);
            //toDeactivate = GameObject.Find("highlightedUnit");
            //if (toDeactivate)
            //    toDeactivate.SetActive(false);
            //if (victoryType == VictoryType.waveSurvival)
            //{
            //    GameObject.Find("StatusText").SetActive(false);
            //}
            menuCanvas.SetActive(false);
            gameObject.SetActive(false);
        }
    } 
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
        SetInteractableActions(on);
    }

    private void ResetToBattleMenu()
    {
        StartCoroutine(_ResetToBattleMenu());
    }

    private IEnumerator _ResetToBattleMenu()
    {
        inputPaused = true;
        yield return new WaitForSeconds(.1f);
        if (!activeUnit.AIUnit)
        {
            inputPaused = false;
            ToggleBattleMenu(true);            
        }
        else
        {
            ToggleBattleMenu(false);
        }
        ToggleActionMenu(false);
        movingState = false;
        actionState = false;
        menuState = true;
        ProcessTurn();
    }

    private void SetInteractableActions(bool on)
    {
        if (on)
        {
            HealButton.interactable = false;
            LongRangeButton.interactable = false;
            ShortRangeButton.interactable = false;
            SlowButton.interactable = false;
            foreach (var a in activeUnitPosActions)
            {
                switch (a.Type)
                {
                    case ActionType.Heal:
                        HealButton.interactable = true;
                        break;
                    case ActionType.LongAttack:
                        if(!unitMoved) //Long Range Modules can't be used after moving
                            LongRangeButton.interactable = true;
                        break;
                    case ActionType.MeleeAttack:
                        ShortRangeButton.interactable = true;
                        break;
                    case ActionType.Slow:
                        SlowButton.interactable = true;
                        break;
                }
            }
        }
    }

    private bool ResolveAction(Action action, Tile tile)
    {
        int playerNumber = activeUnit.PlayerNumber;
        Unit targetedUnit = tile.UnitOnTile;
        List<Tile> range = GetActionRange(action);
        bool resolved = false;
        if(range.Contains(tile) && targetedUnit != null)
        {
            //TODO update the ifs to follow the actions actual Target value
            switch (action.Type)
            {
                case ActionType.MeleeAttack:
                case ActionType.LongAttack:
                    if (targetedUnit.PlayerNumber != playerNumber)
                    {
                        activeUnit.FaceTile(tile);
                        resolved = true;
                        targetedUnit.DamageUnit(action.Power);
                    }
                    break;
                case ActionType.Slow:
                    if (targetedUnit.PlayerNumber != playerNumber)
                    {
                        activeUnit.FaceTile(tile);
                        targetedUnit.AddStatus(new StatusEffects(1, action.Power, statusType.mass));
                        UpdateTurnOrder(turnIndex + 1); //Update turn index for remaining units
                        resolved = true;
                    }
                    break;
                case ActionType.Heal:
                    if (targetedUnit.PlayerNumber == playerNumber)
                    {
                        activeUnit.FaceTile(tile);
                        targetedUnit.HealUnit(action.Power);
                        resolved = true;
                    }
                    break;
            }
        }
        return resolved;
    }

    private void DestroyUnit(Unit unit)
    {
        int unitIndex = roundTurnOrder.IndexOf(unit);
        if(unit.BossUnit) 
        {
            bossDead = true;
        }
        if(unitIndex > -1 && unitIndex < turnIndex)
        {
            turnIndex--;
        }
        roundTurnOrder.Remove(unit);
        unit.RemoveFromTile();
        //TODO have the unit it self handle the explosion and destruction
        
        Explode(unit.transform.position);
        Destroy(unit.gameObject);
        if(unit.AIUnit) {EnemiesLeft--;} else {ShipsLeft--;};
        Debug.Log("EnemiesLeft: " + EnemiesLeft + ", ShipsLeft: " + ShipsLeft);
    }

    private void MoveActiveUnitToTile(Tile tile)
    {
        activeUnit.TraversePath(map.GetMovementPath(activeUnit, tile));
    }

    //Update the active Units possible moves
    public void UpdateCurrentPossibleMoves()
    {
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
    //TODO make this take a tile so that it can be used for wormhole instead of list
    public void FinishedMovement()
    {
        unitMoved = true;
        //if unit is on wormhole
        //Debug.Log("wormholeTileList list:" + wormholeTileList.ToString());
        if(activeUnit.CurrentTile.Type == Tile.TileType.wormhole && !unitTeleported)
        {
            ActivateWormholeEvent(activeUnit.CurrentTile);
        }
        //for (int i = 0; i < wormholeTileList.Count; i++)
        //{
        //    if(wormholeTileList[i].UnitOnTile) {
        //        Debug.Log("unit on wormhole: " + wormholeTileList[i].UnitOnTile);
        //        if(!unitTeleported) {
        //           ActivateWormholeEvent(wormholeTileList[i]);
        //        }

        //    }
        //}
        //teleport to other wormhole
        ResetToBattleMenu();
    }


    public List<Tile> GetActionRange(Action action)
    {
        List<Tile> totalRange = map.GetTilesInRange(activeUnit.CurrentTile, action.Range);
        List<Tile> adjustedRange = new List<Tile>();
        Tile.TileColor color = action.Target == Target.Self || action.Target == Target.Ally ? Tile.TileColor.ally : Tile.TileColor.attack;
        foreach (var t in totalRange)
        {
            if (t.UnitOnTile == null)
                adjustedRange.Add(t);
            else if (t.UnitOnTile == activeUnit && (action.Target == Target.Self || action.Target == Target.Ally))
                adjustedRange.Add(t);
            else if (t.UnitOnTile.PlayerNumber == activeUnit.PlayerNumber && action.Target == Target.Ally)
                adjustedRange.Add(t);
            else if (t.UnitOnTile.PlayerNumber != activeUnit.PlayerNumber && action.Target == Target.Enemy)
                adjustedRange.Add(t);
            else if (action.Target == Target.Everyone)
                adjustedRange.Add(t);
        }
        return adjustedRange;
    }

    private Action GetActionOfType(ActionType type)
    {
        foreach(var a in activeUnitPosActions)
        {
            if (a.Type == type)
                return a;
        }
        return null;
    }

    public void ColorTiles(List<Tile> tiles, Tile.TileColor color)
    {
        foreach (var t in tiles)
        {
            t.SetTileColor(color);
        }
    }


    /************************************ DISPLAY FUNCTIONS ****************************/

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

    public void ShowActionRange(Action action)
    {
        List<Tile> inRange = GetActionRange(action);
        Tile.TileColor color = (action.Target == Target.Self || action.Target == Target.Ally) ? Tile.TileColor.ally : Tile.TileColor.attack;
        foreach(var t in inRange)
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

    /*************************************** MENU CLICK FUNCTIONS *********************************/

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
            //The player has chosen to end their turn while on a wormhole and has not already teleported
            if (activeUnit.CurrentTile.Type == Tile.TileType.wormhole && !unitTeleported)
            {
                ActivateWormholeEvent(activeUnit.CurrentTile);
            }
            NextTurn();
        }

    }
    public void LongButtonClicked()
    {
        menuState = false;
        actionChosen = (ActionChosen)0;
        actionState = true;
        //Should only be possible if action is avaliable
        unitAction = GetActionOfType(ActionType.LongAttack);
        ProcessTurn();
    }
    public void ShortButtonClicked()
    {
        menuState = false;
        actionChosen = (ActionChosen)1;
        actionState = true;
        //Should only be possible if action is avaliable
        unitAction = GetActionOfType(ActionType.MeleeAttack);
        ProcessTurn();
    }
    public void HealButtonClicked()
    {
        menuState = false;
        actionChosen = (ActionChosen)2;
        actionState = true;
        //Should only be possible if action is avaliable
        unitAction = GetActionOfType(ActionType.Heal);
        ProcessTurn();
    }
    public void SlowButtonClicked()
    {
        menuState = false;
        actionChosen = (ActionChosen)3;
        actionState = true;
        //Should only be possible if action is avaliable
        unitAction = GetActionOfType(ActionType.Slow);
        ProcessTurn();
    }

    /********************************** MAP CLICK FUNCTIONS *********************************/

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
                //ResolveAction(tile);
                //ResolveAction(unitAction, tile);
                //actionsTaken++;
                //Debug.Log("RESET TO BATTLEMENU AFTER ACTION");
                //ResetToBattleMenu();
                StartCoroutine(ProcessAction(unitAction, tile));
            }
        }

    }

    ////TODO Make this actually work (Add OnMouseOver to Unit that sends this message)
    //public void UnitClicked(Unit unit)
    //{
    //    TileClicked(unit.CurrentTile);
    //}

/**************************************** TEST FUNCTIONS *********************************/
    public void RemoveHover()
    {
        highlightedUnit.SetActive(false);
    }

    //show hovered Unit UI
    public void HoveredUI(Unit unit)
    {
        if (unit.PlayerNumber == 0)
        {
            highlightedUnit.GetComponent<Image>().color = new Color32(74, 81, 161, 141);
        }
        else
        {
            highlightedUnit.GetComponent<Image>().color = new Color32(144, 42, 42, 141);
        }
        if (highlightedUnit.activeSelf == false) {
            highlightedUnit.SetActive(true);
            highlightedUnitText.text = "";
            highlightedUnitTextInfo.text = "";
            float currentHP = unit.GetMaxHP() - unit.GetDamage();
            highlightedUnitTextInfo.text = "  Current HP: " + currentHP;
            List<ModuleType> list = unit.GetModuleTypes();

            if (unit.getUnitType() == UnitType.fighter)
            {
                highlightedFrigate.gameObject.SetActive(false);
                highlightedFrigateEnemy.gameObject.SetActive(false);

                highlightedUnitText.text = "Fighter";
                if (unit.PlayerNumber == 0)
                {
                    highlightedFighter.gameObject.SetActive(true);
                    highlightedFighterEnemy.gameObject.SetActive(false);
                }
                else
                {
                    highlightedFighter.gameObject.SetActive(false);
                    highlightedFighterEnemy.gameObject.SetActive(true);
                }
            }
            else if (unit.getUnitType() == UnitType.frigate)
            {
                highlightedFighter.gameObject.SetActive(false);
                highlightedFighterEnemy.gameObject.SetActive(false);

                highlightedUnitText.text = "Frigate";
                if (unit.PlayerNumber == 0)
                {
                    highlightedFrigate.gameObject.SetActive(true);
                    highlightedFrigateEnemy.gameObject.SetActive(false);
                }
                else
                {
                    highlightedFrigate.gameObject.SetActive(false);
                    highlightedFrigateEnemy.gameObject.SetActive(true);
                }
            }
            if (unit.BossUnit == true)
            {
                highlightedUnitText.text = "BOSS";
            }
            List<Action> actionsList = unit.GetActions();

            List<ModuleType> modList = unit.GetModuleTypes();
            string modstr = "";
            foreach (ModuleType mod in modList)
            {
                if (mod == ModuleType.shortRange)
                {
                    modstr += "Short Range";
                    modstr += "   ";
                }
                else if (mod == ModuleType.longRange)
                {
                    modstr += "Long Range";
                    modstr += "   ";
                }
                else if (mod == ModuleType.heal)
                {
                    modstr += "Heal";
                    modstr += "   ";
                }
                else if (mod == ModuleType.slow)
                {
                    modstr += "Slow";
                    modstr += "   ";
                }
            }

            highlightedUnitTextInfo.text += "     Type: " + unit.GetShipType() + "\n  Mods: " + modstr;

            if (actionState)
            {
                List<Action> actions = activeUnit.GetActions();
                Action action = null;
                ActionType chosenAction = ActionType.Slow;
                if (actionChosen == 0)
                {
                    chosenAction = ActionType.LongAttack;
                }
                else if (actionChosen == (ActionChosen)1)
                {
                    chosenAction = ActionType.MeleeAttack;
                }
                else if (actionChosen == (ActionChosen)2)
                {
                    chosenAction = ActionType.Heal;
                }
                int power = 0;
                foreach (Action a in actions)
                {
                    if (a.Type == chosenAction)
                    {
                        power = a.Power;
                        action = a;
                        break;
                    }
                }

                List<Tile> inRange = GetActionRange(action);
                Tile.TileColor color = (action.Target == Target.Self || action.Target == Target.Ally) ? Tile.TileColor.ally : Tile.TileColor.attack;
                bool testUnit = false;
                foreach (var t in inRange)
                {
                    if (t.gameObject.name == unit.CurrentTile.gameObject.name)
                    {
                        testUnit = true;
                        break;
                    }
                }

                if (testUnit)
                {
                    float expectedHealthHeal = currentHP + power;
                    float expectedHealthDmg = currentHP - power;
                    if(expectedHealthHeal > unit.GetMaxHP())
                    {
                        expectedHealthHeal = unit.GetMaxHP();
                    }
                    if(expectedHealthDmg < 0)
                    {
                        expectedHealthDmg = 0;
                    }
                    if (actionChosen == ActionChosen.heal)
                    {
                        highlightedUnitTextInfo.text = "  Action = Heal\n  New HP: " + expectedHealthHeal + " (" + currentHP + " + " + power + ")\n  Max HP: " + unit.GetMaxHP();
                    }
                    else if (actionChosen == ActionChosen.slow)
                    {
                        highlightedUnitTextInfo.text = "  Action = Slow\n  Target's move distance will be decreased";
                    }
                    else if (actionChosen == ActionChosen.shortRange)
                    {
                        highlightedUnitTextInfo.text = "  Action = Short Range\n  New HP: " + expectedHealthDmg + " (" + currentHP + " - " + power + ")";
                    }
                    else if (actionChosen == ActionChosen.longRange)
                    {
                        highlightedUnitTextInfo.text = "  Action = Long Range\n  New HP: " + expectedHealthDmg + " (" + currentHP + " - " + power + ")";
                    }
                }

            }
        }
        else
        {
            highlightedUnit.SetActive(false);
        }
    }

    //show current Unit UI (stats + modules)
    public void currentUnitUI()
    {
        //current unit UI box
        if (currentUnit.activeSelf == false)
        {
            currentUnit.gameObject.SetActive(true);
        }
        
        //Update color scheme red vs blue
        if(activeUnit.PlayerNumber == 0)
        {
            currentUnit.GetComponent<Image>().color = new Color32(74, 81, 161, 141);
        }
        else
        {
            currentUnit.GetComponent<Image>().color = new Color32(144, 42, 42, 141);
        }

        //clear text boxes
        currentUnit.gameObject.SetActive(true);
        currentUnitText.text = "";
        currentUnitTextInfo.text = "";

        float currentHP = activeUnit.GetMaxHP() - activeUnit.GetDamage();
        currentUnitTextInfo.text = "  Current HP: " + currentHP;
        List<ModuleType> list = activeUnit.GetModuleTypes();

        //add correct image based on unit type and player number
        if (activeUnit.getUnitType() == UnitType.fighter)
        {
            currentFrigate.gameObject.SetActive(false);
            currentFrigateEnemy.gameObject.SetActive(false);

            currentUnitText.text = "Fighter";
            if (activeUnit.PlayerNumber == 0)
            {
                currentFighter.gameObject.SetActive(true);
                currentFighterEnemy.gameObject.SetActive(false);
            }
            else
            {
                currentFighter.gameObject.SetActive(false);
                currentFighterEnemy.gameObject.SetActive(true);
            }
        }
        else if (activeUnit.getUnitType() == UnitType.frigate)
        {
            currentFighter.gameObject.SetActive(false);
            currentFighterEnemy.gameObject.SetActive(false);

            currentUnitText.text = "Frigate";
            if (activeUnit.PlayerNumber == 0)
            {
                currentFrigate.gameObject.SetActive(true);
                currentFrigateEnemy.gameObject.SetActive(false);
            }
            else
            {
                currentFrigate.gameObject.SetActive(false);
                currentFrigateEnemy.gameObject.SetActive(true);
            }
        }

        if (activeUnit.BossUnit == true)
        {
            currentUnitText.text = "BOSS";
        }
        List<Action> actionsList = activeUnit.GetActions();

        //Add list of unit's mods to UI
        List<ModuleType> modList = activeUnit.GetModuleTypes();
        string modstr = "";
        foreach(ModuleType mod in modList)
        {
            if(mod == ModuleType.shortRange)
            {
                modstr += "Short Range";
                modstr += "   ";
            }
            else if (mod == ModuleType.longRange)
            {
                modstr += "Long Range";
                modstr += "   ";
            }
            else if (mod == ModuleType.heal)
            {
                modstr += "Heal";
                modstr += "   ";
            }
            else if (mod == ModuleType.slow)
            {
                modstr += "Slow";
                modstr += "   ";
            }
        }
        currentUnitTextInfo.text += "     Type: " + activeUnit.GetShipType() + "\n  Mods: " + modstr;
        
    }

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

    private string StringifyModList(Unit unit) {
        string modStr = "";
        List<ModuleType> modList = unit.GetModuleTypes();
        for(int i = 0; i < modList.Count; i++) {
            modStr += modList[i].ToString();
            modStr += "   ";
        }
        return modStr;
    }
    //triggered when a unit lands on a wormhole tile. Thi swill teleport the user to the empty wormhole
    private void ActivateWormholeEvent(Tile wormhole){
        Debug.Log("Wormhole event activated!");
       
        if (wormhole.WormholeDestination == null || wormhole.UnitOnTile == null)
        {
            Debug.Log("Error no destination or unit to transport");
            return;
        }
        if(wormhole.WormholeDestination.UnitOnTile != null)
        {
            Debug.Log("Wormhole is blocked by another ship!");
        }
        else
        {
            AudioSource audioSource = GameObject.Find("BGMusic").GetComponent<AudioSource>();
            audioSource.PlayOneShot(Resources.Load("Sounds/teleport", typeof(AudioClip)) as AudioClip);

            wormhole.UnitOnTile.PlaceOnTile(wormhole.WormholeDestination);
            unitTeleported = true;
        }
    }


    private void GetUnitCounts(List<Unit> units) {
        foreach (Unit unit in units)
        {
            if(unit.AIUnit) {
                EnemiesLeft++;
            } else {
                ShipsLeft++;
            }
        }
    }
	public bool CheckForVictory(List<Unit> units, int ShipsLeft, int EnemiesLeft, int roundNumber, VictoryType vt){
        Debug.Log("checking victory state" +vt);
		switch (vt)
		{
			case VictoryType.waveSurvival:
				if(roundNumber > RoundsToSurvive){ return true;} else {return false;}
			case VictoryType.bossKill:
                //boss still in unit list = game keeps going
                return bossDead;
			case VictoryType.destroyAll:
				if(EnemiesLeft <= 0){ return true;} else {return false;}
			
            default:
                return false;
		}
	}

    //initiates end of level
    private void GameOver(bool won) {
        ToggleBattleMenu(false);
        ToggleActionMenu(false);
        if(won) {
            WinLoseText.text = "YOU WON!!";
        } else {
            WinLoseText.text = "YOU LOSE";
            NextGameButton.interactable = false;

                      
        }
        ToggleGameOverMenu(true);
    }
    //IN PROGRESS
    private void GenerateWave() {
        Unit waveUnit;
        GameObject spawnObject = healAIObj;
        int spawnIndex = UnityEngine.Random.Range(0,3);
        switch (spawnIndex)
        {
            case 0:
                spawnObject = healAIObj;
                break;
            case 1:
                spawnObject = meleeAIObj;
                break;
            case 2:
                spawnObject = longAIObj;
                break;
            default:
                spawnObject = slowAIObj;
                break;
        }
        GameObject go = Instantiate(spawnObject, enemyWarp.transform.position, Quaternion.identity) as GameObject; 
        go.transform.parent = GameObject.Find("BattleManager").transform;
        waveUnit = go.GetComponent<Unit>();
        roundTurnOrder.Add(waveUnit);
    }

}
