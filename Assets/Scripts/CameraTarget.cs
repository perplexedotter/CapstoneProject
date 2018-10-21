using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour {
	[SerializeField] public Vector3 cameraTarget;
	[SerializeField] public float zoomSpeed;
	[SerializeField] public float minZoom;
	[SerializeField] public float maxZoom;
	// Use this for initialization
	void Start () {
		//starts at center of map
		transform.LookAt(cameraTarget);
		
	}
	
	// Update is called once per frame
	void Update () {
		HandleZoom();
	}

	public void LookAtMe(Vector3 v) {
	}

	private void HandleZoom() {
		float scrollValue = Input.mouseScrollDelta.y;

		if (scrollValue != 0.0) {
			if (Input.GetKey(KeyCode.LeftControl)) {
				//rotate
				Vector3 center = cameraTarget;
				Vector3 distToCenter = transform.position - cameraTarget;
				Vector3 angles =new Vector3(0,scrollValue);
				Quaternion rotation = Quaternion.Euler(angles);
				Vector3 direction = rotation * distToCenter;
				transform.position = cameraTarget + direction;
			}

			//zoom
			float newSize = Camera.main.orthographicSize = scrollValue;
			Camera.main.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
		}
	}
}
