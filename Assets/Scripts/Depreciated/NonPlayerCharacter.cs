using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPlayerCharacter : Unit {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public override void TurnUpdate () {
		if (Vector3.Distance(moveDestination, transform.position) > 0.1f) {
			transform.position += (moveDestination - transform.position).normalized * moveSpeed * Time.deltaTime;
			
			if (Vector3.Distance(moveDestination, transform.position) <= 0.1f) {
				transform.position = moveDestination;
				GameManager.instance.nextTurn();
			}
		} else {
			moveDestination = new Vector3(0 - Mathf.Floor(Random.Range(0, GameManager.instance.MapSize/2)), 1.5f, Mathf.Floor(Random.Range(0-GameManager.instance.MapSize/2, 0)));
		}
		
		base.TurnUpdate();
	}
	
}
