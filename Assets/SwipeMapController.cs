using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HedgehogTeam.EasyTouch;
public class SwipeMapController : MonoBehaviour {

	public bool isSwipeEnabled = true; 
	public GameObject Controller; 

	public GameObject[] cameraObjectsOn;
	public GameObject[] cameraObjectOff;

	public GameObject [] mapObjectsOn;
	public GameObject [] mapObjectOff;



	void OnEnable(){
		EasyTouch.On_SwipeEnd += On_SwipeEnd;
	}

	void OnDestroy(){
		EasyTouch.On_SwipeEnd -= On_SwipeEnd;
	}



	void On_SwipeEnd (Gesture gesture){



		if (isSwipeEnabled){
				switch (gesture.swipe){
			case EasyTouch.SwipeDirection.DownLeft:
			case EasyTouch.SwipeDirection.UpLeft:
			case EasyTouch.SwipeDirection.Left:
					//Actions to turn off map
				Controller.GetComponent <disableAfterTimeStart>().setMapOff();
				foreach (GameObject mapon in cameraObjectsOn) {

					mapon.SetActive (true);


				}
				foreach (GameObject mapoff in cameraObjectOff) {

					mapoff.SetActive (false);

				}



					break;
				case EasyTouch.SwipeDirection.DownRight:
				case EasyTouch.SwipeDirection.UpRight:
				case EasyTouch.SwipeDirection.Right:
					//Actions to turn on map
				Controller.GetComponent <disableAfterTimeStart>().setMapOn();
				foreach (GameObject mapon in mapObjectsOn) {

					mapon.SetActive (true);



				}
				foreach (GameObject mapoff in mapObjectOff) {

					mapoff.SetActive (false);

				}


					break;

				}
			}







	}

	public void setMapOFF()
	{
		Controller.GetComponent <disableAfterTimeStart>().setMapOff();
		foreach (GameObject mapon in cameraObjectsOn) {

			mapon.SetActive (true);


		}
		foreach (GameObject mapoff in cameraObjectOff) {

			mapoff.SetActive (false);

		}


	}


	public void setMapOn()
	{

		Controller.GetComponent <disableAfterTimeStart>().setMapOn();
		foreach (GameObject mapon in mapObjectsOn) {

			mapon.SetActive (true);



		}
		foreach (GameObject mapoff in mapObjectOff) {

			mapoff.SetActive (false);

		}


	}


}
