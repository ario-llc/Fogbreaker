using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine .UI; 

public class setColor : MonoBehaviour {

	public Image eye1;
	public Image eye2; 
	public Image map1;
	public Image map2;
	public Selectable selectable;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void setRed()
	{

		eye1.color = new Color (255f, 40f, 81f, 100f);
		eye2.color = new Color (255f, 40f, 81f, 100f);
		map1.color = new Color (255f, 40f, 81f, 100f);
		map2.color = new Color (255f, 40f, 81f, 100f);

	}

	public void setWhite()
	{

		eye1.color = new Color (255f, 255f, 255f, 100f);
		eye2.color = new Color (255f, 255f, 255f, 100f);
		map1.color = new Color (255f, 255f, 255f, 100f);
		map2.color = new Color (255f, 255f, 255f, 100f);


	}
}
