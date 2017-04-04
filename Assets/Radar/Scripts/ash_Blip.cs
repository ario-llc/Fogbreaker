using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))] // just force a box collider to be used since they are supposed to be the quickest
public class ash_Blip : MonoBehaviour
{
    ash_Radar radarSystem; // the radar system object

    [HideInInspector]
    public ash_FadeOverTime fadeComponent;

    // Use this for initialization
    void Start()
    {
        // find the radar system
        radarSystem = FindObjectOfType<ash_Radar>();
        if (radarSystem == null) Debug.Log("You must include a Radar System prefab in the scene!");

        // check if the blip has a collider
        if (!radarSystem.GetComponent<ash_Radar>().CheckForCollider(transform)) Debug.LogError(gameObject.name + " - must have a collider!");

        // the blip must not have a rigidbody
        if (GetComponent<Rigidbody>() != null) Debug.LogError(gameObject.name + " - must not have a RigidBody!");
    }

    // Update is called once per frame
    void Update()
    {

    }

    // when the radar sweep object triggers the blip, the blip then calls the radar system to update its position .ect
    void OnTriggerEnter(Collider col) // OnTriggerStay() doesn't always work
    {
        if (col.GetComponent<ash_RadarSweepEffect>() != null)
        {
            // reset the blips opacity if we are using fading
            if (radarSystem.options.blipFadeRate != 0f && fadeComponent != null) fadeComponent.ResetFade();

            // update its position
            radarSystem.UpdateRadarContact(transform);
        }
    }
}
