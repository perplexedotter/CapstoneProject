using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Unit {

	//// Use this for initialization
	//void Start () {
		
	//}
	
	//// Update is called once per frame
	//void Update () {
 //       //print("Update");
 //       if (IsMoving)
 //           //MoveCharacter();
 //       if (movementFinished)
 //       {
 //           movementFinished = false;
 //           GameManager.instance.FinishedMovement();
 //       }
 //   }

 //   //TODO Move this to base Character MoveToTile and remove GameManager.instance.nextTurn(); (should be handled in GameManager)
 //   public override void TurnUpdate () {
	//	if (Vector3.Distance(moveDestination, transform.position) > 0.1f) {
	//		transform.position += (moveDestination - transform.position).normalized * moveSpeed * Time.deltaTime;
			
	//		if (Vector3.Distance(moveDestination, transform.position) <= 0.1f) {
	//			transform.position = moveDestination;
	//			GameManager.instance.nextTurn();
	//		}
	//	}
		
	//	base.TurnUpdate ();
	//}
}
