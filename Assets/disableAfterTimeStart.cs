using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class disableAfterTimeStart : MonoBehaviour {

	private GameObject map;





	void Start()
	{
		map = GameObject.Find("worldRoot");
		StartCoroutine ("disableAfterTime");

	}

	IEnumerator disableAfterTime ()
	{


		yield return new WaitForSeconds(.5f);
		map.SetActive (false); 




	}

	public void setMapOn()
	{
		map.SetActive (true);


	}

	public void setMapOff()
	{

		map.SetActive (false); 

	}


}
