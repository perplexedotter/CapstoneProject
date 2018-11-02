using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//
public class ShipDesigner : MonoBehaviour {
    [SerializeField] public GameObject Fighter;
    [SerializeField] public GameObject Frigate;
    [SerializeField] public GameObject ScrollList;
    [SerializeField] public GameObject ContinueOn;
    [SerializeField] public GameObject ClearModules;
    [SerializeField] public Text ModuleInfo;
    [SerializeField] public Text ModuleCount;

    [SerializeField] protected List<ModuleType> modules = new List<ModuleType>();
    [SerializeField] protected UnitType type;

    public void ShowFighter()
    {
        if (GameObject.Find("FighterShipBuilder") == null)
        {
            Frigate.gameObject.SetActive(false);
            Fighter.gameObject.SetActive(true);
            type = UnitType.fighter;
            ScrollList.gameObject.SetActive(true);
            modules = new List<ModuleType>();
            ModuleInfo.text = "Modules Chosen:\n";
            moduleCount();
            
        }
        else
        {
            Fighter.gameObject.SetActive(false);
            ScrollList.gameObject.SetActive(false);
            type = UnitType.none;
            clearText();
        }

    }

    public void ShowFrigate()
    {
        if (GameObject.Find("FrigateShipBuilder") == null)
        {
            Fighter.gameObject.SetActive(false);
            Frigate.gameObject.SetActive(true);
            type = UnitType.frigate;
            ScrollList.gameObject.SetActive(true);
            modules = new List<ModuleType>();
            ModuleInfo.text = "Modules Chosen:\n";
            moduleCount();
        }

        else
        {
            Frigate.gameObject.SetActive(false);
            ScrollList.gameObject.SetActive(false);
            type = UnitType.none;
            clearText();
        }
    }

    private bool checkUnit()
    {
        if (type == UnitType.fighter && modules.Count >= 2)
        {
            return false;
        }
        else if (type == UnitType.frigate && modules.Count >= 3)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void moduleCount()
    {
        int hardPoints;
        if(type == UnitType.fighter)
        {
            hardPoints = 2;
        }
        else
        {
            hardPoints = 3;
        }
        ModuleCount.text = modules.Count + "/" + hardPoints;
    }
    public void addCloseRange()
    {
        if (checkUnit() && type != UnitType.none)
        {
            modules.Add(ModuleType.shortRange);
            ModuleInfo.text += "Close Range Attack\n";
            moduleCount();
        }

    }

    public void addLongRange()
    {
        if (checkUnit() && type != UnitType.none)
        {
            modules.Add(ModuleType.longRange);
            ModuleInfo.text += "Long Range Attack\n";
            moduleCount();
        }

    }

    public void addHeal()
    {
        if (checkUnit() && type != UnitType.none)
        {
            modules.Add(ModuleType.heal);
            ModuleInfo.text += "Heal\n";
            moduleCount();
        }
    }

    public void addSlow()
    {
        if (checkUnit() && type != UnitType.none)
        {
            modules.Add(ModuleType.slow);
            ModuleInfo.text += "Slow\n";
            moduleCount();
        }
    }

    public void clearText()
    {
        ModuleInfo.text = "";
        ModuleCount.text = "";
    }

    public void continueOn()
    {
        Debug.Log("Unit Made");
    }
    
    public void clearModules()
    {
        clearText();
        modules = new List<ModuleType>();
    }
    // Use this for initialization
    void Start () {
        type = UnitType.none;
        clearText();
	}
	
    
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Alpha1) && !checkUnit())
        {
            Debug.Log("Unit Made");
        }
    }
}
