using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleCameraPerspective : MonoBehaviour {

	public Camera cam;

	private bool perspective;

	// Use this for initialization
	void Start () {

		perspective = true;

		if (cam == null)
			cam = Camera.main; 

	}
	
	// Update is called once per frame
	public void ChangePerspective () {

		if (!perspective) {
			cam.transform.Translate (Vector3.up * 160, Space.World);
			cam.transform.Rotate ( Vector3.right, 45f, Space.Self );
			perspective = false;
		} else {
			cam.transform.Rotate ( Vector3.right, -45f, Space.Self );
			cam.transform.Translate (Vector3.up * -160, Space.World);
			perspective = true;
		}

	}
}
