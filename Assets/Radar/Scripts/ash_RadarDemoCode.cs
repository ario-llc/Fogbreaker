using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ash_RadarDemoCode : MonoBehaviour
{
    public List<GameObject> objects = new List<GameObject>();

    ash_Radar radarSystem;

    [SerializeField]
    int amountToSpawn = 10;

    [SerializeField]
    float area = 500f;

    [SerializeField]
    float wanderAmount = 1f;

    [SerializeField]
    bool showUI = true;

    // Use this for initialization
    void Start()
    {
        // find the radar system
        radarSystem = FindObjectOfType<ash_Radar>();
        if (radarSystem == null) Debug.Log("You must include a Radar System prefab in the scene!");

        // spawn our demo objects to track
        for (int i = 0; i < amountToSpawn; i++)
        {
            GameObject go = Instantiate(objects[Random.Range(0, objects.Count)]);
            go.transform.position = new Vector3(Random.Range(-area, area), Random.Range(-area, area), Random.Range(-area, area));
            go.AddComponent<ash_Wanderer>();
            go.GetComponent<ash_Wanderer>().wanderAmount = wanderAmount;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    // show us our contacts
    void OnGUI()
    {
        if (showUI)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Radar Scale: " + radarSystem.options.scale.ToString("0.00"));
            radarSystem.options.scale = GUILayout.HorizontalSlider(radarSystem.options.scale, 10f, 3000f, GUILayout.Width(400f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Change Radar Shape")) radarSystem.ChangeRadarShape();


            GUILayout.FlexibleSpace();

            GUILayout.Label("View: ");
            if (radarSystem.options.radarStyle == ash_Radar.radarStyleEnum.Perspective)
            {
                if (GUILayout.Button("Top Down"))
                {
                    radarSystem.options.radarStyle = ash_Radar.radarStyleEnum.TopDown;
                    radarSystem.SetRadarView();
                }
            }
            else
            {
                if (GUILayout.Button("Perspective"))
                {
                    radarSystem.options.radarStyle = ash_Radar.radarStyleEnum.Perspective;
                    radarSystem.SetRadarView();
                }
            }
            GUILayout.EndHorizontal();

            // buttons to demonstrate the various public functions for finding and locking targets
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cycle Target")) radarSystem.CycleTargets();
            if (GUILayout.Button("Select Closest Target")) radarSystem.SelectNearestTarget();
            if (GUILayout.Button("Select Closest Hostile")) radarSystem.SelectNearestTarget("Hostile"); // just include the tag your looking for
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (radarSystem.currentTarget != null) GUILayout.Label("Current Target: " + radarSystem.currentTarget.targetObject.name);

            GUILayout.Label("# of contacts: " + radarSystem.contacts.Count.ToString());

            // but only show if we actually have any (save cpu)
            if (radarSystem.contacts.Count > 0)
            {
                // this shows us one of the ways we can use the contact list for our own purposes. You simply need to reference the contacts list and then you can access all the data about the individual targets
                foreach (ash_Radar.contactClass c in radarSystem.contacts)
                {
                    GUILayout.BeginHorizontal();
                    if (radarSystem.currentTarget != null)
                    {
                        if (radarSystem.currentTarget.targetObject == c.targetObject) GUILayout.Label(">");
                    }
                    GUILayout.Label("Target: '" + c.targetObject.name + "'     Distance: " + c.targetDistance.ToString("0.00") + "     Velocity: " + c.targetVelocity.ToString("0.00"));
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Label("No contacts.");
            }
        }
    }
}
