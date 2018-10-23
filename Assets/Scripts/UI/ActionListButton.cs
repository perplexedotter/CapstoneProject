using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Customize action panel button
//attach onclick function
public class ActionListButton : MonoBehaviour
{
    [SerializeField] private Text ActionText;
    [SerializeField] public BattleManager BM;

    public void SetText(string ActionText)
    {
        this.gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
        this.ActionText.text = ActionText;
    }
    public void OnClick()
    {

        if (this.ActionText.text == "Close Attack")
        {
            BM.AddShortRangeModule();
        }
        else if(this.ActionText.text == "Range Attack")
        {
            BM.AddLongRangeModule();
        }
        else if (this.ActionText.text == "Heal")
        {
            BM.AddHealModule();
        }
        else if (this.ActionText.text == "Slow")
        {
            BM.AddSlowModule();
        }
    }
}