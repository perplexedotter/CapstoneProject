using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Test: MonoBehaviour {
    public GameObject Object;
    public Module Object2;

	// Use this for initialization
	void Start () {
        SceneManager.LoadScene("Scenes/map1");
        Object = GameObject.Instantiate(Resources.Load("Prefabs/Modules/MeleeModule"), GameObject.Find("Unit").transform.parent) as GameObject;
        //Object = GameObject.Find("Unit");

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

