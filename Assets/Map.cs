using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour {

    

	// Use this for initialization
	void Start () {
        LogChildren();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void LogChildren()
    {
        foreach(Transform row in transform)
        {
            foreach(Transform tile in row)
            {
                print(tile.GetComponent<Material>());
            }
        }
    }
}
