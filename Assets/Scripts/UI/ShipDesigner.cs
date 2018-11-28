using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ShipDesigner : MonoBehaviour {
    [SerializeField] public GameObject Fighter;
    [SerializeField] public GameObject Frigate;
    [SerializeField] public Unit unitFigher;
    [SerializeField] public Unit unitFrigate;
    [SerializeField] public GameObject ScrollList;
    [SerializeField] public GameObject ContinueOn;
    [SerializeField] public GameObject ClearModules;
    [SerializeField] public Text ModuleInfo;
    [SerializeField] public Text ModuleCount;
    [SerializeField] public UnitSave unitSave;
    [SerializeField] public Dropdown dropDown;
    [SerializeField] public int dropDownValue;
    public UnitData unitData;
    public GameObject activeUnit;
    [SerializeField] protected List<ModuleType> modules = new List<ModuleType>();
    [SerializeField] protected UnitType type;

    //Displaying the model of the fighter or hiding it 
    public void ShowFighter()
    {
        ContinueOn.gameObject.SetActive(false);

        if (GameObject.Find("Fighter2HPPlayer0") == null)
        {
            Frigate.gameObject.SetActive(false);
            Fighter.gameObject.SetActive(true);
            activeUnit = Fighter;
            ClearModules.gameObject.SetActive(true);
            type = UnitType.fighter;
            ScrollList.gameObject.SetActive(true);
            modules = new List<ModuleType>();
            ModuleInfo.text = "Modules Chosen:\n";
            moduleCount();
            AddModulesToActiveUnit();
        }
        else
        {
            clearModules();
            Fighter.gameObject.SetActive(false);
            ScrollList.gameObject.SetActive(false);
            ClearModules.gameObject.SetActive(false);
            type = UnitType.none;
            clearText();
            activeUnit = null;
        }
    }
 

    //Displaying the model of the frigate or hiding it 
    public void ShowFrigate()
    {
        ContinueOn.gameObject.SetActive(false);

        if (GameObject.Find("Frigate3HPBase") == null)
        {
            Fighter.gameObject.SetActive(false);
            Frigate.gameObject.SetActive(true);
            activeUnit = Frigate;
            ClearModules.gameObject.SetActive(true);
            type = UnitType.frigate;
            ScrollList.gameObject.SetActive(true);
            modules = new List<ModuleType>();
            ModuleInfo.text = "Modules Chosen:\n";
            moduleCount();
            AddModulesToActiveUnit();
        }

        else
        {
            clearModules();
            Frigate.gameObject.SetActive(false);
            ScrollList.gameObject.SetActive(false);
            ClearModules.gameObject.SetActive(false);
            type = UnitType.none;
            clearText();
            activeUnit = null;
        }
    }

    //check if unit is completed (type + full modules)
    private bool checkUnit()
    {
        if (type == UnitType.fighter && modules.Count >= 2)
        {
            ContinueOn.gameObject.SetActive(true);
            return false;
        }
        else if (type == UnitType.frigate && modules.Count >= 3)
        {
            ContinueOn.gameObject.SetActive(true);
            return false;
        }
        else
        {
            return true;
        }
    }

    //Display/Update the current number of modules equipped
    public void moduleCount()
    {
        int hardPoints;
        if (type == UnitType.fighter)
        {
            hardPoints = 2;
        }
        else
        {
            hardPoints = 3;
        }
        ModuleCount.text = modules.Count + "/" + hardPoints;
        AddModulesToActiveUnit();
    }

    //equip close range module
    public void addCloseRange()
    {
        if (checkUnit() && type != UnitType.none)
        {
            modules.Add(ModuleType.shortRange);
            ModuleInfo.text += "Close Range Attack\n";
            moduleCount();
            checkUnit();
        }

    }

    //equip long range module
    public void addLongRange()
    {
        if (checkUnit() && type != UnitType.none)
        {
            modules.Add(ModuleType.longRange);
            ModuleInfo.text += "Long Range Attack\n";
            moduleCount();
            checkUnit();
        }

    }

    //equip heal module
    public void addHeal()
    {
        if (checkUnit() && type != UnitType.none)
        {
            modules.Add(ModuleType.heal);
            ModuleInfo.text += "Heal\n";
            moduleCount();
            checkUnit();
        }
    }

    //equip slow module
    public void addSlow()
    {
        if (checkUnit() && type != UnitType.none)
        {
            modules.Add(ModuleType.slow);
            ModuleInfo.text += "Slow\n";
            moduleCount();
            checkUnit();
        }
    }

    //clear the text for module count and content
    public void clearText()
    {
        ModuleInfo.text = "";
        ModuleCount.text = "";
    }

    //this will save the current build
    //only can be called if full modules and type selected
    public void continueOn()
    {
        DropDownUpdate();
        if (type == UnitType.fighter)
        {
            string name = "Unit" + dropDownValue;
            unitData = new UnitData(name, UnitType.fighter, modules[0], modules[1]);
            unitSave.save(unitData);
            Debug.Log(unitSave.load(dropDownValue - 1).unitName + " " + unitSave.load(dropDownValue - 1).type);
        }
        else if (type == UnitType.frigate)
        {
            string name = "Unit" + dropDownValue;
            unitData = new UnitData(name, UnitType.frigate, modules[0], modules[1], modules[2]);
            unitSave.save(unitData);
            Debug.Log(unitSave.load(dropDownValue - 1).unitName + " " + unitSave.load(dropDownValue - 1).type);
        }
    }

    //clear module list and empty the module display text
    public void clearModules()
    {
        ContinueOn.gameObject.SetActive(false);
        ModuleInfo.text = "Modules Chosen:\n";
        modules = new List<ModuleType>();
        moduleCount();
    }

    //update the current dropdown value
    public void DropDownUpdate()
    {
        dropDownValue = dropDown.value + 1;
    }

    // Use this for initialization
    // add a listener to call function when dropdown is changed
    void Start() {
        type = UnitType.none;
        clearText();
        dropDown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(dropDown);
        });
        DropdownValueChanged(dropDown);
    }


    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddModulesToActiveUnit();
        }
    }

    //this is called to update current unit on display
    void DropdownValueChanged(Dropdown change)
    {
        DropDownUpdate();
        unitData = new UnitData();
        unitData = unitSave.load(dropDownValue - 1);
        if (unitData.type == UnitType.fighter)
        {
            Fighter.gameObject.SetActive(true);
            Frigate.gameObject.SetActive(false);
            activeUnit = Fighter;
            ClearModules.gameObject.SetActive(true);
            type = UnitType.fighter;
            ScrollList.gameObject.SetActive(true);
            clearModules();
            addModules(unitData.module1);
            addModules(unitData.module2);
            moduleCount();
        }
        else if (unitData.type == UnitType.frigate)
        {
            Fighter.gameObject.SetActive(false);
            Frigate.gameObject.SetActive(true);
            activeUnit = Frigate;
            ClearModules.gameObject.SetActive(true);
            type = UnitType.frigate;
            ScrollList.gameObject.SetActive(true);

            clearModules();
            addModules(unitData.module1);
            addModules(unitData.module2);
            addModules(unitData.module3);
            moduleCount();
        }
        else
        {

        }
        StartCoroutine(DelayMod());
    }

        
    IEnumerator DelayMod()
    {
        yield return new WaitForSeconds(.01f);
        AddModulesToActiveUnit();
    }

    //This will add module to list
    public void addModules(ModuleType type)
    {
        if (type == ModuleType.shortRange)
        {
            addCloseRange();
        }
        else if (type == ModuleType.longRange)
        {
            addLongRange();
        }
        else if (type == ModuleType.heal)
        {
            addHeal();
        }
        else if (type == ModuleType.slow)
        {
            addSlow();
        }
    }

    private void AddModulesToActiveUnit()
    {
        List<Module> Mods = new List<Module>(activeUnit.GetComponentsInChildren<Module>());
        foreach(Module mod in Mods)
        {
            Destroy(mod.gameObject);
        }

        StartCoroutine(DelayMod2());
    }
    IEnumerator DelayMod2()
    {
        yield return new WaitForSeconds(.01f);

        if (activeUnit != null)
        {
            foreach (ModuleType t in modules)
            {
                if (t == ModuleType.shortRange)
                {
                    GameObject mod = GameObject.Instantiate(Resources.Load("Prefabs/Modules/ShortRangeModule/ShortAttackModule"), activeUnit.transform) as GameObject;
                }
                else if (t == ModuleType.longRange)
                {
                    GameObject mod = GameObject.Instantiate(Resources.Load("Prefabs/Modules/LongRangeModule/LongAttackModule"), activeUnit.transform) as GameObject;
                }
                else if (t == ModuleType.heal)
                {
                    GameObject mod = GameObject.Instantiate(Resources.Load("Prefabs/Modules/HealModule/HealModule"), activeUnit.transform) as GameObject;
                }
                else if (t == ModuleType.slow)
                {
                    GameObject mod = GameObject.Instantiate(Resources.Load("Prefabs/Modules/SlowModule/SlowModule"), activeUnit.transform) as GameObject;
                }
            }
            Unit unit = activeUnit.GetComponentInChildren<Unit>();
            unit.customUnit();
        }
        
    }
}
