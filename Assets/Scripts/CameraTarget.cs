using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour {
	[SerializeField] public Vector3 cameraTarget;
	[SerializeField] public float zoomSpeed;
	[SerializeField] public float panSpeed;
	[SerializeField] public float minZoom;
	[SerializeField] public float maxZoom;

	private Plane _Plane;
	// Use this for initialization
	void Start () {
		_Plane = new Plane(Vector3.up, Vector3.zero);
		//starts at center of map
		transform.LookAt(cameraTarget);
		
	}
	
	// Update is called once per frame
	void Update () {
		HandleZoom();
		HandlePan();
	}

	private void HandlePan() {
		Vector2 mousePos = Input.mousePosition;
		Vector3 dRight = transform.right.XZ();
		Vector3 dUp = transform.up.XZ();
		//Debug.Log(mousePos);
		if (Input.GetKey(KeyCode.A)) {
			transform.position -= dRight * Time.deltaTime *  panSpeed;
		}
		if (Input.GetKey(KeyCode.D)) {
			transform.position += dRight * Time.deltaTime *  panSpeed;
		}
		if (Input.GetKey(KeyCode.W)) {
			transform.position += dUp * Time.deltaTime *  panSpeed;
		}
		if (Input.GetKey(KeyCode.S)) {
			transform.position -= dUp * Time.deltaTime *  panSpeed;
		}
	}

	private void HandleZoom() {
		float scrollValue = Input.mouseScrollDelta.y;

		if (scrollValue != 0.0) {
			if (Input.GetKey(KeyCode.LeftControl)) {
				//rotate
				Vector3 center = GetPanTarget();
				Vector3 distToCenter = transform.position - center;
				Vector3 angles =new Vector3(0,scrollValue, 0);
				Quaternion rotation = Quaternion.Euler(angles);
				Vector3 direction = rotation * distToCenter;
				transform.position = center + direction;
				transform.LookAt(center);

			} else {
 
			//zoom
				float newSize = Camera.main.orthographicSize - (scrollValue * zoomSpeed);
				Camera.main.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
			}			
		}
	}
	//find y intercept of camera view to find new rotation center
	private Vector3 GetPanTarget() {
		Ray r = new Ray(transform.position, transform.forward);
		float distance = 0.0f;

		if (_Plane.Raycast(r, out distance)) {
			return r.GetPoint(distance);
		}
		return Vector3.zero;
	}
}
