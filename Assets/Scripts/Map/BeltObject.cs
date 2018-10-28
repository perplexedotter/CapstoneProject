using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeltObject : MonoBehaviour {
	[SerializeField] private float orbitSpeed;
	[SerializeField] private GameObject parent;
	[SerializeField] private bool clockwise;
	[SerializeField] private float rotationSpeed;
	[SerializeField] private Vector3 rotationDirection;
	
	public void SetupBeltObject(float _speed, float _rotationSpeed, GameObject _parent, bool _clockwise) {
		orbitSpeed = _speed;
		rotationSpeed = _rotationSpeed;
		parent = _parent;
		clockwise = _clockwise;
		rotationDirection = new Vector3(Random.Range(0,360),Random.Range(0,360),Random.Range(0,360));
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(clockwise) {
			transform.RotateAround(parent.transform.position, parent.transform.up, orbitSpeed * Time.deltaTime); 
		} else {
			transform.RotateAround(parent.transform.position, -parent.transform.up, orbitSpeed * Time.deltaTime); 
			
		}
		transform.Rotate(rotationDirection, rotationSpeed * Time.deltaTime);
	}
}
