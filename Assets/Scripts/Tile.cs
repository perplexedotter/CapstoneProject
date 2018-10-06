using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
	
	public Vector2 gridPosition = Vector2.zero;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnMouseEnter() {
		transform.GetComponent<Renderer>().material.color = Color.blue;
		//Debug.Log("Mouse Position ("+ gridPosition.x + ","+gridPosition.y+")");
	}
	void OnMouseExit() {
		transform.GetComponent<Renderer>().material.color = Color.white;
		//Debug.Log("Mouse Position ("+ gridPosition.x + ","+gridPosition.y+")");
	}

	void OnMouseDown(){
		GameManager.instance.moveCurrentPlayer(this);
	}
}
