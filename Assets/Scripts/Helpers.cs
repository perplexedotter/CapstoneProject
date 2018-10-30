using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers {

	//normalizes y coords when moving camera
	public static Vector3 XZ(this Vector3 input) {
		return new Vector3(input.x, 0, input.z);
	}
}
