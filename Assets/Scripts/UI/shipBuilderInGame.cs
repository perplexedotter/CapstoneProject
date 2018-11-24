using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class shipBuilderInGame : MonoBehaviour
{
    [SerializeField] public BattleManager BM;
    [SerializeField] public Map map;
    [SerializeField] public Map customMap;
    [SerializeField] public GameObject gameOverMenu;
    [SerializeField] public GameObject battleMenu;
    [SerializeField] public GameObject actionMenu;
    [SerializeField] public GameObject defaultButton;
    [SerializeField] public GameObject customButton;
    [SerializeField] public UnitSave saves;
    [SerializeField] public GameObject loadUnitsUI;
    private UnitData saveData;
    [SerializeField] public List<GameObject> loadbuttons;
    [SerializeField] public Text unitCount;
    [SerializeField] public List<Tile> mapTiles = new List<Tile>();
    [SerializeField] public List<Unit> unitPlayer = new List<Unit>();
    [SerializeField] public List<GameObject> playerObjects = new List<GameObject>();
    [SerializeField] public List<Unit> units = new List<Unit>();
    [SerializeField] public List<UnitData> unitDatas = new List<UnitData>();
    [SerializeField] public int count;
    [SerializeField] public int addedUnits;
    [SerializeField] public string type;
    // Use this for initialization
    void Start()
    {
        BM.gameObject.SetActive(true);

        var tiles = FindObjectsOfType<Tile>();
        mapTiles = new List<Tile>(tiles);

        var Units = FindObjectsOfType<Unit>();
        unitPlayer = new List<Unit>(Units);
        foreach (Unit t in Units)
        {
            if (t.PlayerNumber == 1)
            {
                unitPlayer.Remove(t);
            }
        }

        unitDatas.Add(new UnitData("Default 1", UnitType.fighter, ModuleType.shortRange, ModuleType.shortRange));
        unitDatas.Add(new UnitData("Default 2", UnitType.fighter, ModuleType.longRange, ModuleType.longRange));
        unitDatas.Add(new UnitData("Default 3", UnitType.frigate, ModuleType.heal, ModuleType.heal, ModuleType.heal));
        for (int i = 0; i < 9; i++)
        {
            unitDatas.Add(saves.load(i));
        }

        for (int i = 0; i < 12; i++)
        {
            if (unitDatas[i].type == UnitType.fighter)
            {
                loadbuttons[i].GetComponentInChildren<Text>().text = unitDatas[i].unitName + ": " + unitDatas[i].type + "\n" + "Module 1: " + unitDatas[i].module1 + "   Module 2: " + unitDatas[i].module2;
            }
            else
            {
                loadbuttons[i].GetComponentInChildren<Text>().text = unitDatas[i].unitName + ": " + unitDatas[i].type + "\n" + "Module 1: " + unitDatas[i].module1 + "   Module 2: " + unitDatas[i].module2 + "   Module 3: " + unitDatas[i].module3;
            }
        }

        count = unitPlayer.Count;
        updateCount();
        BM.gameObject.SetActive(false);
        map.gameObject.SetActive(true);
    }

    public void updateCount()
    {
        unitCount.text = "Count: " + addedUnits + "/" + count;
        if (addedUnits >= count)
        {
            loadUnitsUI.gameObject.SetActive(false);
            runDefault();
            //BM.CustomStart();
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
        }
    }

    public void runCustom()
    {
        BM.gameObject.SetActive(true);
        var tiles = FindObjectsOfType<Tile>();
        foreach (Tile t in tiles)
        {
            t.UnitOnTile = null;
        }
        BM.gameObject.SetActive(false);
        defaultButton.SetActive(false);
        customButton.SetActive(false);
        foreach (Unit t in unitPlayer)
        {
            Destroy(t.gameObject);
        }
        unitPlayer = new List<Unit>();
        loadUnitsUI.gameObject.SetActive(true);
        //BM.gameObject.SetActive(true);
    }

    public void runDefault()
    {
        print("Default Map is Running");
        battleMenu.SetActive(true);
        BM.gameObject.SetActive(true);
        defaultButton.SetActive(false);
        customButton.SetActive(false);
        foreach(GameObject player in playerObjects)
        {
            Unit unit = player.GetComponent<Unit>();
            unit.CustomUpdate();
        }
    }

    public void loadButton(int loadValue)
    {
        customMap.gameObject.SetActive(true);
        loadUnitsUI.gameObject.SetActive(false);
        saveData = unitDatas[loadValue];
        if (saveData.type == UnitType.fighter)
        {
            type = "Fighter2HP/Fighter2HPPlayer0";
        }
        else
        {
            type = "Frigate3HP/Frigate3HPBase";
        }
    }

    public void addUnit(Vector3 position)
    {
        GameObject unit = GameObject.Instantiate(Resources.Load("Prefabs/Units/" + type), BM.transform) as GameObject;
        BM.gameObject.SetActive(true);
        unit.gameObject.transform.position = position;

        foreach (Tile t in mapTiles)
        {
            if (t.gameObject.transform.position == position)
            {
                Unit Unit = unit.GetComponent<Unit>();
                Unit.enabled = enabled;
                Unit.CurrentTile = t;
                t.UnitOnTile = Unit;
            }
        }

        BM.gameObject.SetActive(false);
        playerObjects.Add(unit);
        List<ModuleType> modulesT = new List<ModuleType>();
        if (saveData.type == UnitType.fighter)
        {
            modulesT.Add(saveData.module1);
            modulesT.Add(saveData.module2);
        }
        else
        {
            modulesT.Add(saveData.module1);
            modulesT.Add(saveData.module2);
            modulesT.Add(saveData.module3);
        }
        foreach (ModuleType t in modulesT)
        {
            customMap.gameObject.SetActive(false);
            BM.gameObject.SetActive(true);
            if (t == ModuleType.shortRange)
            {
                GameObject mod = GameObject.Instantiate(Resources.Load("Prefabs/Modules/ActiveModules/ShortRangeModule/ShortAttackModule"), unit.transform) as GameObject;
            }
            else if (t == ModuleType.longRange)
            {
                GameObject mod = GameObject.Instantiate(Resources.Load("Prefabs/Modules/ActiveModules/LongRangeModule/LongAttackModule"), unit.transform) as GameObject;
            }
            else if (t == ModuleType.heal)
            {
                GameObject mod = GameObject.Instantiate(Resources.Load("Prefabs/Modules/ActiveModules/HealModule/HealModule"), unit.transform) as GameObject;
            }
            else if (t == ModuleType.slow)
            {
                GameObject mod = GameObject.Instantiate(Resources.Load("Prefabs/Modules/ActiveModules/SlowModule/SlowModule"), unit.transform) as GameObject;
            }
            BM.gameObject.SetActive(false);
            customMap.gameObject.SetActive(true);
        }
        Unit customUnit = unit.GetComponent<Unit>();
        BM.gameObject.SetActive(true);
        customUnit.customUnit();
        BM.gameObject.SetActive(false);
        //GameObject mod = GameObject.Instantiate(Resources.Load("Prefabs/Modules/MeleeModule"), GameObject.Find("Unit").transform) as GameObject;
        //GameObject unit = GameObject.Instantiate(Resources.Load("Prefabs/Units/FighterBase"), BM.transform) as GameObject;
        StartCoroutine(DelayMod());
    }


    IEnumerator DelayMod()
    {
        yield return new WaitForSeconds(.02f);
        addedUnits++;
        customMap.gameObject.SetActive(false);
        loadUnitsUI.gameObject.SetActive(true);
        updateCount();
    }
}
