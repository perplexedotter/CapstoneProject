using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Testing Scrolling Panel UI output for a list of actions
//May repurpose to do something else (like ship builder)
public class ActionListControl : MonoBehaviour
{
    [SerializeField] public BattleManager BM;
    [SerializeField] GameObject buttonTemplate;
    List<Action> action;
    static public List<GameObject> buttons;
    private bool test;
    void Start()
    {
        buttons = new List<GameObject>();
        test = false;
    }

    //destroy actions/buttons in Panel
    public void clearActionList()
    {
        if (buttons.Count > 0)
        {
            foreach (GameObject button in buttons)
            {
                Destroy(button.gameObject);
            }
            buttons.Clear();
            test = false;
        }
    }

    //Generates a list of clickable actions in Scroll Panel
    public void MakeActionList(List<Action> action)
    {
        GameObject.Find("ActionScrollList").transform.position = new Vector3(500f, 66f, 0.0f);
        buttonTemplate = GameObject.Find("ActionButton");
        clearActionList();

        for (int i = 0; i < action.Count; i++)
        {
            GameObject button = Instantiate(buttonTemplate) as GameObject;
            button.SetActive(true);

            string actionType = "";

            if(action[i].Type == ActionType.ShortAttack)
            {
                actionType = "Close Attack";
            }
            else if (action[i].Type == ActionType.LongAttack)
            {
                actionType = "Ranged Attack";
            }
            else if (action[i].Type == ActionType.Heal)
            {
                actionType = "Heal";
            }
            else if (action[i].Type == ActionType.Slow)
            {
                actionType = "Slow";
            }
            if(i == 0)
            {
                GameObject.Find("ActionButton").GetComponent<ActionListButton>().SetText(actionType, action[i].Power, action[i].Range);
            }
            else
            {
                //Debug.Log(buttons.Count);
                Debug.Log(button);

                button.GetComponent<ActionListButton>().SetText(actionType, action[i].Power, action[i].Range);
                button.transform.SetParent(GameObject.Find("ActionButton").transform.parent, false);
                buttons.Add(button.gameObject);
                Destroy(GameObject.Find("ActionButton(Clone)"));
            }
        }
    }
    
	
	// Update is called once per frame
	void Update () {
		
	}
}
