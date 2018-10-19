using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour {

    public static AIController instance;
    [SerializeField] Map map;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //TODO expand this AI to other possiblities
    //TODO make this return a list of actions instead of directly manipulating unit
    public void GetAIActions(Unit unit)
    {

    }
}
