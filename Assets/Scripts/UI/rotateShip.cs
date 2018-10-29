using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotateShip : MonoBehaviour {

    float rotSpeed = 20;

    private void OnMouseDrag()
    {
        float rotX = Input.GetAxis("Mouse X") * rotSpeed * Mathf.Deg2Rad;
        float rotY = Input.GetAxis("Mouse Y") * rotSpeed * Mathf.Deg2Rad;

        this.gameObject.transform.RotateAround(Vector3.up, -rotX);
        this.gameObject.transform.RotateAround(Vector3.right, rotY);
        Debug.Log(this);
        Debug.Log(this.gameObject);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

    }

}
