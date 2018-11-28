using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ModuleTips : MonoBehaviour,  IPointerEnterHandler, IPointerExitHandler{

    [SerializeField] Module module;
    [SerializeField] GameObject moduleTipBox;
    [SerializeField] Transform parent;
    [SerializeField] List<string> tipText = new List<string>();

    GameObject moduleTip;

    bool toolTipVisible = false;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {


    }

    private string BuildModuleText()
    {
        string modText = "";

        if(module is ActiveModule)
        {
            ActiveModule mod = (ActiveModule)module;
            modText += "PWR: " + mod.Power +" RNG: " + mod.Range + "\n\n";
        }
        List<string> modInfo = module.ModuleInfo;
        foreach(var s in modInfo)
        {
            modText += s + "\n";
        }
        return modText;

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Text text = moduleTipBox.GetComponentInChildren<Text>();

        Debug.Log("Mouse Enter");
        if (!toolTipVisible)
        {
            //text.text = "";
            //foreach (var s in tipText)
            //    text.text += (s + "\n");
            text.text = BuildModuleText();
            moduleTip = Instantiate(moduleTipBox, parent, false);
            moduleTip.transform.position = new Vector3(transform.position.x -40, transform.position.y, transform.position.z - 1);
            toolTipVisible = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (toolTipVisible)
        {
            Destroy(moduleTip);
            toolTipVisible = false;
        }
    }
}
