using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CompassHeadingText : MonoBehaviour {

    public Text heading;
    public Transform userCamera;

    void Start()
    {
        heading = GetComponent<Text>();
    }
	
	void Update () {
        Vector3 cameraDirection = userCamera.transform.forward;
        float cameraHeading = Mathf.Atan2(cameraDirection.z, cameraDirection.x);
        heading.text = Mathf.Round((cameraHeading * Mathf.Rad2Deg) + 180).ToString();
    }
}
