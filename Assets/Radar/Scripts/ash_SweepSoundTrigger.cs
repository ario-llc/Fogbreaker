using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class ash_SweepSoundTrigger : MonoBehaviour
{
    ash_Radar radarSystem;

    // Use this for initialization
    void Start()
    {
        // find the radar system
        radarSystem = FindObjectOfType<ash_Radar>();
        if (radarSystem == null) Debug.Log("You must include a Radar System prefab in the scene!");
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider col)
    {
        if (col.GetComponent<ash_RadarSweepEffect>() != null) radarSystem.PlaySweepSound();
    }
}
