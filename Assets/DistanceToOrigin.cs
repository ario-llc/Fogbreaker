using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DistanceToOrigin : MonoBehaviour {

	//public GameObject origin;
	public Camera cam;
	public int scale = 1;

	private float distance;
	private float cameraDistance;
	private float height;

	void Start () {
		if (cam == null)
			cam = Camera.main;
	}

	void Update () {
		// figure out distance from camera to object on a 2D plane, ignoring the camera height
		var camera2dPosition = new Vector2(cam.transform.position.x, cam.transform.position.z);
		var object2dposition = new Vector2 (this.transform.position.x, this.transform.position.z);
		distance = Vector2.Distance( camera2dPosition, object2dposition);
		Debug.Log ("Distance is  " + distance);
		// update text with distance
		GetComponent<TextMeshPro> ().SetText( transform.parent.tag + " Distance: " + Mathf.Round(distance * 1.3f) + "m");

		// update font size with respect to distance between objects, taking height into account
		cameraDistance = Vector3.Distance (cam.transform.position, this.transform.position);
		if (GetComponent<TextMeshPro> ().fontSize > 12) {
			GetComponent<TextMeshPro> ().fontSize = (int)cameraDistance * scale / 10;
		}
//		height = (int)cameraDistance * scale / 10;
//		transform.position = new Vector3( 0, height, 0);
//		this.transform.position = Vector3(height;
	}

}
