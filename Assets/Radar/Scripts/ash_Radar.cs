// Version 1.0

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[RequireComponent(typeof(AudioSource))]
public class ash_Radar : MonoBehaviour
{
    // shader color properties
    public static int _TintColor;
    public static int _Color;

    // radar contact class, holds all the info which represents the actual blips you see
    public class contactClass
    {
        public GameObject targetObject; // the actual object you want to track in 3d space
        public float targetDistance; // the distance from the player to the target
        public float targetVelocity; // the target's velocity
        public GameObject blip; // the blip you see on the radar, this should be a prefab
        public GameObject line; // height line for the blip
        public LineRenderer lineRenderer; // for the height line
        public float blipDistance; // keep track of the blip distance on the radar so it never displays "outside" the radar sphere image
        public int index; // the tag class index
        public Vector3 oldPosition; // used to calculate velocity
    }
    public List<contactClass> contacts = new List<contactClass>();

    // struct for holding the various radar shape components
    [System.Serializable]
    public struct radarObjectsStruct
    {
        [Tooltip("the actual game object that has the trigger for detecting targets")]
        public Transform triggerShape;

        [Tooltip("the size of the trigger area that actually detects the targets")]
        public Vector3 detectableArea;

        [HideInInspector]
        public GameObject plane;

        public radarObjectsStruct(Transform _triggerShape, Vector3 _detectableArea, Transform _plane)
        {
            triggerShape = null;
            detectableArea = Vector3.one;
            plane = null;
        }
    }

    // struct to hold the various view objects
    [System.Serializable]
    public struct radarViewObjectStruct
    {
        [Tooltip("the viewpoint object for the perspective view")]
        public Transform perspectiveView;

        [Tooltip("the viewpoint object for the top down view")]
        public Transform topDownView;
    }

    // objects class for keeping track of all the linked-in objects from the inspector
    [System.Serializable]
    public class objectsClass
    {
        [Tooltip("the player object")]
        public Transform player;

        [HideInInspector]
        public Transform container; // container object to hold all the active blips

        [Tooltip("the radar sphere, we use this to help position the blips inside it")]
        public Transform radarBackgroundSphere;

        [Tooltip("the radar camera")]
        public Camera radarCamera;

        [Tooltip("the various camera view points")]
        public radarViewObjectStruct viewPoints;

        //[Tooltip("the radar ring prefab object")]
        //public GameObject ringPrefab;

        [HideInInspector]
        public List<radarObjectsStruct> radarShapes = new List<radarObjectsStruct>(); // list that holds the different radar shape detectors

        [Tooltip("the object to use to show that a target has been selected")]
        public Transform selectedTarget;
    }
    public objectsClass objects = new objectsClass();

    // sound effect class
    [System.Serializable]
    public class soundClass
    {
        [Tooltip("the sound effect that the 'sweep' makes. Leave null if you do not wish to use a sound")]
        public AudioClip sweep;

        [Tooltip("the sound effect you hear when the radar detects a new contact. Leave null if you do not wish to use a sound")]
        public AudioClip newContact;
    }
    public soundClass sounds = new soundClass();

    // enum for radar styles option
    public enum radarStyleEnum
    {
        Perspective,
        TopDown,
    }

    // enum for tag option
    public enum tagMethodEnum
    {
        AllTheTags,
        Unity,
    }

    // enum for ping style
    public enum pingMethodEnum
    {
        Sweep,
        Constant,
    }

    // options class
    [System.Serializable]
    public class optionsClass
    {
        [Tooltip("your sensor strength, higher values means more likely to detect targets at distance, can never be less than zero")]
        public float sensorStrength = 100f;

        [Tooltip("check that the target is within 'sensor strength' range before adding as a contact. Disable to improve performance")]
        public bool checkTargetInRange = true;

        [Tooltip("check that the target is within Line Of Sight before adding as a contact. Disable to improve performance")]
        public bool checkTargetInLos = true;

        [Tooltip("code delay, it only runs once every <amount> of delay. Helps reduce cpu usage and speed up fps. Also controls the sweep effect speed. Lower values = worse performance but better quality")]
        [Range(0, 5)]
        public int delay = 1;

        [Tooltip("if you have third party tag systems installed then you can choose to use those tags or unity default")]
        public tagMethodEnum tagMethod = tagMethodEnum.AllTheTags;

        [Tooltip("enable to view debugging info")]
        public bool debug = false;

        [Tooltip("set to zero to disable fade completely. Otherwise value is how quickly the blips fade, lower value = slower rate. Setting this value to anything other than zero will decrease performance")]
        [Range(0f, 0.1f)]
        public float blipFadeRate = 0.001f;

        [Tooltip("how much to scale down the radar. Higher values = closer the blips are to the center of the radar. Generally, the larger your 'playing area' the higher this value should be")]
        public float scale = 100f;

        [Tooltip("how should the blips be updated? Sweep is more efficient but targets further away get updated slower than those that are closer to the player")]
        public pingMethodEnum updateMethod = pingMethodEnum.Sweep;

        [Tooltip("the style of radar, top down or perspective")]
        public radarStyleEnum radarStyle = radarStyleEnum.Perspective;

        [Tooltip("pre-create this many blip objects (per tag), instead of instantiating/destroying at runtime. If more are required the system will instantiate them as needed")]
        public int blipPoolSize = 1;

        //[Tooltip("the detection range of the radar cone shape. Small ranges = shorter but wider detection area")]
        //public float radarConeRange = 1000f;
    }
    public optionsClass options = new optionsClass();

    // struct for the tracking class, this contains the individual properties for each target we wish to track
    [System.Serializable]
    public struct tagStruct
    {
        [Tooltip("objects with this tag will be assigned the Blip Prefab. Tag is case sensitive")]
        public string tag;

        [Tooltip("prefab for this blip")]
        public GameObject blipPrefab;

        [Tooltip("should this blip always face the camera. Useful for prefabs that are just a quad/plane")]
        public bool alwaysFaceCamera;

        [Tooltip("when 'Blips Face Camera' is enabled, use this vector to offset how the blip prefab faces the camera")]
        public Vector3 faceOffset;

        [Tooltip("enable to record the distance of contact to the player. Disable to improve performance")]
        public bool recordDistance;

        [Tooltip("enable to record the velocity of contact. Disable to improve performance")]
        public bool recordVelocity;

        // constructor so we can use the inspector to add more elements
        public tagStruct(GameObject _blipPrefab, string _tag, bool _faceCamera, bool _recordDistance, bool _recordVelocity)
        {
            blipPrefab = _blipPrefab;
            tag = _tag;
            alwaysFaceCamera = _faceCamera;
            faceOffset = Vector3.zero;
            recordDistance = _recordDistance;
            recordVelocity = _recordVelocity;
        }
    }

    [System.Serializable]
    public struct lineColorStruct
    {
        [Tooltip("the line color when the blip is above the radar plane")]
        public Color above;

        [Tooltip("the line color when the blip is below the radar plane")]
        public Color below;

        [Tooltip("the thickness of the height lines on the radar, increase this value if they are hard to see")]
        public float lineThickness;

        [Tooltip("the material (and shader) used to texture the lines")]
        public Material material;

        public lineColorStruct(Color _above, Color _below)
        {
            above = _above;
            below = _below;
            lineThickness = 0.004f;
            material = null;
        }
    }

    // class to define what the blips should look like for each tagged object
    [System.Serializable]
    public class tagClass
    {
        public tagStruct[] targets = new tagStruct[] { new tagStruct(null, string.Empty, false, true, true) };
        public lineColorStruct lineSettings = new lineColorStruct(Color.white, Color.yellow);
    }
    public tagClass radar = new tagClass();

    // blip struct for the blip pool
    public struct blipPoolStruct
    {
        public GameObject blip; // actual blip gameobject
        public int index; // index reference to which tag this blip uses
    }

    // blip pool variables/lists
    Transform pool;
    List<blipPoolStruct> blipPool = new List<blipPoolStruct>();
    List<GameObject> linePool = new List<GameObject>();

    // random variables
    int delayCounter = 0;
    float blipRange;
    //int sweepSoundDelay = 1000;
    //GameObject[] rings = new GameObject[6]; // number of rings to display on the radar
    //AudioSource audio;
    int currentRadarShapeIndex = -1;
    [HideInInspector]
    public GameObject defaultRadarShape = null;

    // the current target should be public because other methods outside the radar system may want to query it
    [HideInInspector]
    public contactClass currentTarget = null;
    int targetIndex = 0;
    int currentTagTargetIndex = 0;
    int timer = 0;
    const int timerIncrement = 100;

    void Awake()
    {
        // define the shader color properties
        _TintColor = Shader.PropertyToID("_TintColor");
        _Color = Shader.PropertyToID("_Color");
    }

    // check the material and find out what kind of color property it uses
    public static int CheckMaterialColor(Material mat)
    {
        if (mat.HasProperty(_Color)) return _Color;
        if (mat.HasProperty(_TintColor)) return _TintColor;

        Debug.LogError(mat.name + " - material does not have a _Color or _TintColor value! Suggest using a Shader like 'Particles/Additive'");
        return 0;
    }

    // Use this for initialization
    void Start()
    {
        // make sure the radar sphere is equally scaled
        if (!System.Array.TrueForAll<float>(new float[] { objects.radarBackgroundSphere.localScale.x, objects.radarBackgroundSphere.localScale.y, objects.radarBackgroundSphere.localScale.z }, val => (objects.radarBackgroundSphere.localScale.x == val)))
        {
            Debug.LogError("The " + objects.radarBackgroundSphere.name + " must be scaled evenly!");
        }

        // is there a player object assigned?
        if (objects.player == null) Debug.LogError("You must assign a player object!");

        #region SET ALL CAMERAS TO NOT RENDER RADAR LAYER
        // set EVERY camera in the scene other than the radar camera to not view the radar layer. Users will have to comment out this code if they want manual control over it
        Camera[] cameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in cameras)
        {
            // switch off the radar layer on each cam, but not the radar camera
            if (cam != objects.radarCamera) cam.cullingMask &= ~(1 << gameObject.layer);
        }
        #endregion

        // set the radar camera culling mask to only show the radar layer
        objects.radarCamera.cullingMask = 1 << gameObject.layer;

        // create the blip container object, set it as a child and to the same layer as the radar system
        objects.container = new GameObject("Active Blip Objects").transform;
        objects.container.parent = transform;
        objects.container.gameObject.layer = gameObject.layer;

        // now we find the radius of the radar sphere from center to edge, this defines the area that the blips must stay within
        blipRange = objects.radarBackgroundSphere.localScale.x / 2f;

        // set the view style for the radar
        SetRadarView();

        // create the radar rings that represent ranges
        //CreateRings();

        // get components
        //audio = GetComponent<AudioSource>();

        // create the object pools
        CreatePool();

        // check prefab layers
        objects.selectedTarget.gameObject.layer = gameObject.layer;

        // make sure we are only using the one radar system
        ash_Radar[] radars = FindObjectsOfType<ash_Radar>();
        if (radars.Length > 1) Debug.LogError("There can only be one " + gameObject.name + " per scene!");

        // we must have at least one radar shape in order to detect targets
        ash_RadarShapes[] shapes = FindObjectsOfType<ash_RadarShapes>();
        if (shapes.Length > 1) Debug.LogError("There can only be one Radar Shape prefab per scene!");
        if (shapes.Length == 0) Debug.LogError("You must have one Radar Shape prefab in the scene!");

        // check that the layer exists
        string checkLayer = LayerMask.LayerToName(gameObject.layer);
        if (checkLayer == string.Empty) Debug.LogError("You must assign a layer to " + gameObject.name + " and it's children.");

        // set the starting radar shape
        ChangeRadarShape();
    }

    // when the game runs, each radar shape "checks in" with this system and adds itself
    public void AddShape(GameObject go)
    {
        // create a new shape object
        radarObjectsStruct r = new radarObjectsStruct();

        // define the properties
        r.triggerShape = go.transform;
        r.detectableArea = go.transform.localScale;
        r.plane = go.GetComponent<ash_RadarChild>().plane;

        // set layer of object and all children
        //go.layer = gameObject.layer;
        foreach (Transform child in go.transform)
        {
            child.gameObject.layer = gameObject.layer;
        }

        // add it to the shapes list
        objects.radarShapes.Add(r);
    }

    // change the viewing style of the radar
    public void SetRadarView()
    {
        // set the radar view style, perspective or top down by parenting the radar camera to the respective view point object
        if (options.radarStyle == radarStyleEnum.Perspective) objects.radarCamera.transform.parent = objects.viewPoints.perspectiveView;
        if (options.radarStyle == radarStyleEnum.TopDown) objects.radarCamera.transform.parent = objects.viewPoints.topDownView;

        // reset the radar cameras local position and rotation so it matches that of the view point object its parented to. This "should" not be needed but I like to make sure anyway
        objects.radarCamera.transform.localPosition = Vector3.zero;
        objects.radarCamera.transform.localRotation = Quaternion.identity;

        // clear the current target
        currentTarget = null;
    }

    /*
    // create the radar rings that represent ranges
    void CreateRings()
    {
        // create the radar "rings" which help to show the various range/distance of the target to the player. Note the "range" will change depending on the options.radarScale. For example,
        // the inner ring might mark the target at 50 units away, but if you change the scale it might mark the target at 250 units away. This is a visual guide in-game only.
        //float scaledDown = objects.radarBackgroundSphere.localScale.x / options.scale;
        float scaledDown = objects.radarBackgroundSphere.localScale.x / rings.Length;
        for (int i = 0; i < rings.Length; i++)
        {
            rings[i] = Instantiate(objects.ringPrefab) as GameObject;
            rings[i].transform.parent = objects.radarBackgroundSphere;
            rings[i].transform.localPosition = Vector3.zero;
            rings[i].transform.localRotation = Quaternion.identity;

            // rings[i].transform.Rotate(180f, 0f, 0f); // flip the plane over so we can see it (3ds export issue)

            // we scale each ring so its slightly bigger/smaller than the last
            //Vector3 ringScale = new Vector3(scaledDown + (0.1f * i), scaledDown + (0.1f * i), 1f);
            Vector3 ringScale = new Vector3(scaledDown, scaledDown, 1f);

            rings[i].transform.localScale = ringScale;

            scaledDown += objects.radarBackgroundSphere.localScale.x / rings.Length;

            // if the rings are the same size as the radar sphere/plane then disable it because we don't need to show it as we already have an edge
            if (rings[i].transform.localScale.x >= 1f) rings[i].SetActive(false);
        }
    }
    */

    // method for creating all the blips and lines so we don't have to use instantiate or destroy
    void CreatePool()
    {
        // create a container object to hold all these pool objects
        pool = new GameObject("Pool").transform;

        // set the pool as a child of the radar system
        pool.parent = transform;

        // set the pool layer to the same as the radar system
        pool.gameObject.layer = gameObject.layer;

        // keep track of how many blips we create so we can then create an equal amount of lines
        int numberOfObjects = 0;

        // go through the list of tags and create the predefined amount of blips for EACH tag
        for (int t = 0; t < radar.targets.Length; t++)
        {
            for (int i = 0; i < options.blipPoolSize; i++)
            {
                // create the blip object
                GameObject go = CreateBlip(t);

                // now that the blip is in the list we can disable it and enable it later easily
                go.SetActive(false);

                // keep track of how many blips we create
                numberOfObjects++;
            }
        }

        // now we need an equal amount of line objects for all the blips
        for (int l = 0; l < numberOfObjects; l++)
        {
            // create the line
            GameObject line = CreateLine();

            // now that the line is in the list we can disable it and enable it later easily
            line.SetActive(false);
        }
    }

    // creates the actual radar blip height line
    GameObject CreateLine()
    {
        // create the line
        GameObject line = new GameObject("Line");

        // add the line renderer component and set the lines layer to match the radar system, add the line to the pool object to keep the hierarchy clean
        line.AddComponent<LineRenderer>();
        line.layer = gameObject.layer;
        line.transform.parent = pool.transform;

        // define the line component
        LineRenderer lr = line.GetComponent<LineRenderer>();
        lr.material = radar.lineSettings.material;
        //lr.material = new Material(Shader.Find("Particles/Additive")); // won't work in webplayer
        //lr.lineRenderer.material.SetFloat("_Mode", 2f); // sets the standard shader to "Fade" mode
        lr.SetWidth(radar.lineSettings.lineThickness, radar.lineSettings.lineThickness);
        lr.SetVertexCount(2);
        lr.useWorldSpace = false;
        lr.receiveShadows = false;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.useLightProbes = false;

        // only add the fade script if we are actually going to use fading
        if (options.blipFadeRate != 0f) line.AddComponent<ash_Line>(); // add the fade script

        // add the line to the line pool list
        linePool.Add(line);

        // return the line game object
        return line;
    }

    // creates the actual radar blips
    GameObject CreateBlip(int t)
    {
        // create the blip
        GameObject go = Instantiate(radar.targets[t].blipPrefab);

        // add the blip as a child of the container object to keep things clean, rename it so we can identify it and set its layer to match the radar system
        go.transform.parent = pool.transform;
        go.name = radar.targets[t].tag + " Blip";
        go.layer = gameObject.layer;

        // if fadeOverTime is enabled then add the fade component script
        if (options.blipFadeRate != 0f)
        {
            go.AddComponent<ash_FadeOverTime>();
            go.GetComponent<ash_Blip>().fadeComponent = go.GetComponent<ash_FadeOverTime>();
        }

        // now we need to create the new blip struct
        blipPoolStruct newBlip = new blipPoolStruct();

        // fill its' properties
        newBlip.blip = go; // the actual blip gameobject
        newBlip.index = t; // the index which tells us which tag it belongs to

        // add the blip to the pool list so we can use it later
        blipPool.Add(newBlip);

        // return the blip game object
        return go;
    }

    // Update is called once per frame
    void Update()
    {
        // debugging
        if (options.debug) DebugLOS();

        // sensor strength must never be less than zero
        if (options.sensorStrength < 0f) options.sensorStrength = 0f;

        // display the radar blips for constant mode
        if (options.updateMethod == pingMethodEnum.Constant) ConstantRadar();

        // get the velocity of each target when using sweep mode, otherwise the method is called from the ConstantRadar()
        if (options.updateMethod == pingMethodEnum.Sweep) GetTargetVelocity();

        // show the currently selected target
        ShowCurrentTarget();

        // timer
        Timer();
    }

    // show the selected target prefab around the currently selected target
    void ShowCurrentTarget()
    {
        // show and hide the selected target object if there is a current target
        if (currentTarget == null)
        {
            if (objects.selectedTarget.gameObject.activeSelf) objects.selectedTarget.gameObject.SetActive(false);
        }
        else
        {
            if (currentTarget.targetObject.gameObject.activeSelf) // maybe change this to the blip object?
            {
                if (!objects.selectedTarget.gameObject.activeSelf) objects.selectedTarget.gameObject.SetActive(true);

                // set the selected object to the same position and rotation of the current target blip (so it matches the "always face camera" option
                objects.selectedTarget.localPosition = currentTarget.blip.transform.localPosition;
                objects.selectedTarget.rotation = currentTarget.blip.transform.rotation;

                // set the scale to slightly bigger than the blip
                objects.selectedTarget.localScale = currentTarget.blip.transform.localScale * 1.5f;
            }
            else
            {
                if (objects.selectedTarget.gameObject.activeSelf) objects.selectedTarget.gameObject.SetActive(false);
            }
        }
    }

    // method to cycle through ALL available targets
    public void CycleTargets()
    {
        // only check if we have something in the list
        if (contacts.Count > 0)
        {
            if (targetIndex < contacts.Count)
            {
                currentTarget = contacts[targetIndex];
            }
            else
            {
                currentTarget = null;
            }

            targetIndex++;
        }

        // reset so we can deselect the target
        if (targetIndex > contacts.Count) targetIndex = 0;

        // reset the tag target index because we just want to cycle through all targets, then when we hit SelectNearestTarget() we want to target the very closest tag target again instead of
        // continuing on half way through the list
        currentTagTargetIndex = 0;
    }

    // simple timer method
    void Timer()
    {
        if (timer > 0) timer--;

        // timer must never be less than zero
        if (timer < 0) timer = 0;
    }

    // this method will select the nearest target that matches the tag parameter.
    // the tag must be exactly the same as the ones entered into the targets list in the inspector (case sensitive)
    // if you continue to use this method it will cycle through ALL of the tagged contacts, from nearest to furthest
    public void SelectNearestTarget(string tag)
    {
        // if timer is zero, reset tag target index
        if (timer == 0) currentTagTargetIndex = 0;

        // increment timer variable every time we use this method. So as the user spams the key to cycle through the targets the timer increases to give them time to select a target
        timer = timerIncrement;

        // ensure the tag target index never exceeds the size of the list, reset it back to zero so we can cycle from the start again
        if (currentTagTargetIndex >= contacts.Count) currentTagTargetIndex = 0;

        // only check if we have something in the list
        if (contacts.Count > 0)
        {
            int k = 0; // temp variable

            // sort the list based on distance but only on the first "cycle".
            // the sort by distance causes any objects that are the same distance to not be selected properly when sort is called every time. So only call it on the start of the cycle
            if (currentTagTargetIndex == 0) contacts.Sort((x, y) => x.targetDistance.CompareTo(y.targetDistance));

            // if we have supplied the tag parameter
            if (tag != string.Empty)
            {
                // search through the radar.targets list and find the entry that matches the supplied tag
                for (int i = 0; i < radar.targets.Length; i++)
                {
                    if (radar.targets[i].tag == tag)
                    {
                        // now go through the contacts list and find the next contact that matches the tag from the radar target list. But we want to start the search AFTER the last target we selected.
                        for (int c = currentTagTargetIndex; c < contacts.Count; c++)
                        {
                            if (contacts[c].index == i) // if the contact matches the tag we're looking for
                            {
                                // increment the tag target index, so when we use this method again it starts the search AFTER this contact
                                currentTagTargetIndex = (c + 1);

                                // set the current target
                                currentTarget = contacts[c];
                                k = c; // let us know which index we have used

                                //goto resumeHere; // skip the rest of the for loops since we have the right target

                                if (targetIndex == k) targetIndex++; // increment the target index variable so we don't reselect the same contact when using cycle target
                                return;
                            }
                        }

                        // we went through the whole contact list looking for next closest tag target but we couldn't find any, so reset the index and we can start fresh
                        currentTagTargetIndex = 0;
                    }
                }

                // if the code got this far then you must have supplied the wrong tag (check spelling) OR you have simply run out of tagged targets that you are searching for.
                if (currentTagTargetIndex != 0) Debug.Log("No such tag available or tag spelling incorrect.");
            }

        //resumeHere:
            //if (targetIndex == k) targetIndex++; // increment the target index variable so we don't reselect the same contact when using cycle target
        }
    }

    // selects the nearest anything.
    // if you continue to use this method it will cycle through ALL of the contacts, from nearest to furthest
    public void SelectNearestTarget()
    {
        // if timer is zero, reset tag target index
        if (timer == 0) currentTagTargetIndex = 0;

        // increment timer variable every time we use this method. So as the user spams the key to cycle through the targets the timer increases to give them time to select a target
        timer = timerIncrement;

        // ensure the tag target index never exceeds the size of the list, reset it back to zero so we can cycle from the start again
        if (currentTagTargetIndex >= contacts.Count) currentTagTargetIndex = 0;

        // only check if we have something in the list
        if (contacts.Count > 0)
        {
            int k = 0; // temp variable

            // sort the list based on distance but only on the first "cycle".
            // the sort by distance causes any objects that are the same distance to not be selected properly when sort is called every time. So only call it on the start of the cycle
            if (currentTagTargetIndex == 0) contacts.Sort((x, y) => x.targetDistance.CompareTo(y.targetDistance));

            // if no tag parameter is supplied then just select the nearest target
            currentTarget = contacts[currentTagTargetIndex]; // closest contact after sorting should always be [0]
            k = 0; // let us know which index we have used

            currentTagTargetIndex++;

            if (targetIndex == k) targetIndex++; // increment the target index variable so we don't reselect the same contact when using cycle target
        }
    }

    // handle changing the radar shape and size
    public void ChangeRadarShape()
    {
        // change the radar shape
        currentRadarShapeIndex++;

        if (currentRadarShapeIndex == objects.radarShapes.Count) currentRadarShapeIndex = 0;

        // go through all the radar shapes and enable/disable them one at a time
        for (int i = 0; i < objects.radarShapes.Count; i++)
        {
            // if we have a default radar shape set during start(), then we should display it first
            if (defaultRadarShape != null)
            {
                if (objects.radarShapes[i].triggerShape == defaultRadarShape.transform)
                {
                    objects.radarShapes[i].triggerShape.gameObject.SetActive(true);
                    objects.radarShapes[i].plane.gameObject.SetActive(true);
                }
                else
                {
                    objects.radarShapes[i].triggerShape.gameObject.SetActive(false);
                    objects.radarShapes[i].plane.gameObject.SetActive(false);
                }
            }
            else
            {
                if (i == currentRadarShapeIndex)
                {
                    objects.radarShapes[i].triggerShape.gameObject.SetActive(true);
                    objects.radarShapes[i].plane.gameObject.SetActive(true);
                }
                else
                {
                    objects.radarShapes[i].triggerShape.gameObject.SetActive(false);
                    objects.radarShapes[i].plane.gameObject.SetActive(false);
                }
            }
        }

        // reset default radar shape variable (we only want to check it on start)
        defaultRadarShape = null;

        // reset current target
        currentTarget = null;

        /*
        // change the radar shape size/scale based on its detection range. It should be thinner the further out it goes
        float tip = 1000f / (options.radarConeRange * 0.001f);
        tip = Mathf.Clamp(tip, 1f, 1000f); // hard limit
        objects.radarCone.triggerShape.localScale = new Vector3(tip, tip, options.radarConeRange);
        */

        // clear the current contact list because we are changing the shape and size of the detection area we need to renew contacts
        ClearContactList();
    }

    // method to check that at least some form of collider is on THIS OBJECTS' PARENT/ROOT object otherwise the radar won't work correctly
    public bool CheckForCollider(Transform go)
    {
        // temp variable
        bool check = false;

        // perform the checks, at least some kind needs to be present
        if (go.GetComponent<BoxCollider>() != null) check = true;
        if (go.GetComponent<SphereCollider>() != null) check = true;
        if (go.GetComponent<CapsuleCollider>() != null) check = true;
        if (go.GetComponent<MeshCollider>() != null) check = true;

        // return the result
        return check;
    }

    // method to set triggers on the object
    public void SetTrigger(GameObject go)
    {
        // the collider should always be a trigger, we only want to check whats inside it, not actually stop things from entering/exiting
        if (go.GetComponent<SphereCollider>() != null) go.GetComponent<SphereCollider>().isTrigger = true;
        if (go.GetComponent<BoxCollider>() != null) go.GetComponent<BoxCollider>().isTrigger = true;
        if (go.GetComponent<MeshCollider>() != null)
        {
            go.GetComponent<MeshCollider>().convex = true;
            go.GetComponent<MeshCollider>().isTrigger = true;
        }
        if (go.GetComponent<CapsuleCollider>() != null) go.GetComponent<CapsuleCollider>().isTrigger = true;
    }

    // method to clear the contact list and thus remove the blips and lines
    void ClearContactList()
    {
        int safety = 0; // safety variable to ensure we dont have infinite loops

        // we only want to remove stuff if the list has something in it
        if (contacts.Count > 0)
        {
            while (contacts.Count > 0 || safety < 1000)
            {
                // return if we don't have any more contacts. The code gets to this point but it shouldn't?
                if (contacts.Count == 0) return;

                // as we remove contacts from the list, the list gets smaller, so we remove the first contact, the second one then becomes the first as the list resizes down. So we only need to keep removing index zero
                RemoveContact(contacts[0].targetObject);

                safety++;
            }
        }
    }

    // just play the sound fx
    public void PlaySweepSound()
    {
        // play it at the players position so the origin doesn't matter
        AudioSource.PlayClipAtPoint(sounds.sweep, objects.player.position);
    }

    // constantly update the blips based on delay
    void ConstantRadar()
    {
        // only run if we actually have a contact (save cpu)
        if (contacts.Count > 0)
        {
            // ge the velocity of each target every frame
            GetTargetVelocity();

            // only check once in a while (save cpu)
            if (delayCounter >= options.delay)
            {
                // go through all the current contacts
                for (int i = 0; i < contacts.Count; i++)
                {
                    // update the blip
                    UpdateBlip(i);
                }

                delayCounter = 0;
            }

            delayCounter++;
        }
    }

    // method to get the velocity of each target in the contact list. This has to be called every frame otherwise the velocity value will not be accurate
    void GetTargetVelocity()
    {
        // only perform the method if there is a contact
        if (contacts.Count > 0)
        {
            // go through the contact list and get the velocity for each target
            foreach (contactClass a in contacts)
            {
                // if 'Record Velocity' is enabled, get the velocity of the target contact based on which tag it belongs to
                if (radar.targets[a.index].recordVelocity)
                {
                    // could add a lerp here to make the returned velocity value smoother, or the user could implement it themself in their gui code
                    a.targetVelocity = ((a.targetObject.transform.position - a.oldPosition).magnitude) / Time.deltaTime;
                    a.oldPosition = a.targetObject.transform.position;
                }
            }
        }
    }

    // update radar blips, called from each blip when the sweep effect triggers them
    public void UpdateRadarContact(Transform c)
    {
        int a = -1; // define a temp variable

        // go through the contacts list and see if the blip transform we pass in matches any in the list
        for (int t = 0; t < contacts.Count; t++)
        {
            // if the blip is a contact in the list then break out of this loop
            if (c == contacts[t].blip.transform)
            {
                a = t; // set the contact
                break;
            }
        }

        // if the contact transform we passed in doesn't match one in the contacts list then return
        if (a == -1) return;

        // update the blip
        UpdateBlip(a);
    }

    // this is the method that does all the work, it displays the radar blips and line heights
    void UpdateBlip(int a)
    {
        // get the position of the target in 3d space relative to the players position and scale it down so it fits within the radar sphere
        Vector3 pos = (contacts[a].targetObject.transform.position - objects.player.position) / options.scale;

        // position the blip in relation to the players rotation
        //contacts[a].blip.transform.position = objects.player.InverseTransformPoint(pos); // old code, obsolete. Its InverseTransformDIrection and not inversetransformpoint apparently
        contacts[a].blip.transform.localPosition = objects.player.InverseTransformDirection(pos);

        // create a temp vector so we can offset the blip and line positions later
        Vector3 blipPosition;

        // if the radar style is top down then we need to keep the blip pinned to the radar plane (same y position as the radar sphere)
        if (options.radarStyle == radarStyleEnum.TopDown)
        {
            blipPosition = contacts[a].blip.transform.position;
            blipPosition.y = objects.radarBackgroundSphere.transform.position.y;
            contacts[a].blip.transform.position = blipPosition;
        }

        // get the distance of the blip to the center of the radar sphere
        contacts[a].blipDistance = Vector3.Distance(objects.radarBackgroundSphere.position, contacts[a].blip.transform.position);

        // do not allow the blip to move "outside" the radar sphere
        if (contacts[a].blipDistance > blipRange)
        {
            pos = objects.radarBackgroundSphere.position - contacts[a].blip.transform.position;
            pos = pos.normalized;
            pos *= (contacts[a].blipDistance - blipRange);
            contacts[a].blip.transform.localPosition += pos;
        }

        // if this blip is set to always face the camera
        if (radar.targets[contacts[a].index].alwaysFaceCamera)
        {
            // look at the camera
            contacts[a].blip.transform.LookAt(objects.radarCamera.transform.position);

            // offset the rotation of the blip, this is because when some models are exported from a modeling application like 3d studio max, if the rotation axis are not set
            // correctly then when the above line faces the blip at the camera, it may not always be facing in the right direction. This offset fixes that.
            contacts[a].blip.transform.rotation = new Quaternion(contacts[a].blip.transform.rotation.x + radar.targets[contacts[a].index].faceOffset.x,
                                                                 contacts[a].blip.transform.rotation.y + radar.targets[contacts[a].index].faceOffset.y,
                                                                 contacts[a].blip.transform.rotation.z + radar.targets[contacts[a].index].faceOffset.z,
                                                                 contacts[a].blip.transform.rotation.w);
        }

        // if 'Record Distance' is enabled, get the distance from the player to the target contact
        if (radar.targets[contacts[a].index].recordDistance) contacts[a].targetDistance = Vector3.Distance(contacts[a].targetObject.transform.position, objects.player.position); // move this to the individual tag section, so each tag can choose whether to record or not

        // if 'Record Velocity' is enabled, get the velocity of the target contact
        //if (radar.targets[contacts[a].index].recordVelocity)
        //{
        //    contacts[a].targetVelocity = ((contacts[a].targetObject.transform.position - contacts[a].oldPosition).magnitude) / Time.deltaTime;
        //    contacts[a].oldPosition = contacts[a].targetObject.transform.position;
        //}

        // set the line renderer start position to that of the blip
        contacts[a].lineRenderer.SetPosition(0, contacts[a].blip.transform.localPosition);

        // set the end position to the same location as the blip but have its Y axis set to that of the radar sphere (so that it always protudes above or below the radar plane)
        blipPosition = contacts[a].blip.transform.localPosition;
        blipPosition.y = 0f;// objects.radarBackgroundSphere.transform.position.y; // <-- used when the radar is not using local coords
        contacts[a].lineRenderer.SetPosition(1, blipPosition);

        // default color of the line will be black so it doesnt get rendered
        Color lineColor = Color.black;

        // color the line based on whether the blip is above or below, this helps to differentiate in-game
        if (contacts[a].blip.transform.localPosition.y > 0f)
        {
            lineColor = radar.lineSettings.above;
        }

        if (contacts[a].blip.transform.localPosition.y < 0f)
        {
            lineColor = radar.lineSettings.below;
        }

        // color the line
        contacts[a].lineRenderer.SetColors(lineColor, lineColor);
    }

    // debugging Line Of Sight method. Just draws a ray from the player to each contact
    void DebugLOS()
    {
        if (contacts.Count > 0)
        {
            foreach (contactClass c in contacts)
            {
                Debug.DrawRay(objects.player.position, c.targetObject.transform.position - objects.player.position, Color.green);
            }
        }
    }

    // actual method for adding new contacts
    public void AddContact(GameObject contact, int i)
    {
        bool isNewContact = true; // temp variable

        // first we need to check if the contact we are trying to add is already in the list
        foreach (contactClass c in contacts)
        {
            // if it already exists, then we don't want to add it again so set temp variable to false
            if (c.targetObject == contact) isNewContact = false;
        }

        // if after checking the current contacts in the list, if the temp variable is still true then it must be a new contact so add it
        if (isNewContact)
        {
            // if its a new contact then lets create the object
            contactClass newContact = new contactClass();

            // define the properties
            newContact.targetObject = contact;

            // get the next available object from the pools
            newContact.blip = GetBlipFromPool(i); // make sure we return the appropriate blip for this tagged enemy
            newContact.line = GetLineFromPool();

            // set the parents
            newContact.blip.transform.parent = objects.container;
            newContact.line.transform.parent = objects.container;

            // set the public variables
            newContact.lineRenderer = newContact.line.GetComponent<LineRenderer>();

            // initialize the component
            if (newContact.line.GetComponent<ash_Line>() != null) newContact.line.GetComponent<ash_Line>().Initialize(newContact.blip);

            newContact.index = i; // this is what we use to reference options in the classes that affect a wide range of contacts

            // play new contact sounds fx
            if (sounds.newContact != null) AudioSource.PlayClipAtPoint(sounds.newContact, objects.player.position);

            // add the new contact to the list
            contacts.Add(newContact);
        }
    }

    // method to retrieve the next available object in the pool. Go through the list and find the next object that is inactive, enable and return it
    GameObject GetBlipFromPool(int index)
    {
        foreach (blipPoolStruct b in blipPool)
        {
            // we only want to check for inactive blips that match the targets tag (the index)
            if (b.index == index)
            {
                if (!b.blip.activeSelf)
                {
                    b.blip.SetActive(true);
                    return b.blip;
                }
            }
        }

        // if all the pool objects are currently in use then we should create another
        if (options.debug) Debug.Log("Creating additional blip");
        return CreateBlip(index);

        //Debug.LogError("Returning null object from blip pool. Suggest increasing the blip pool size");
        //return null;
    }

    // method to return the unused objects back to the pool
    void ReturnToPool(GameObject go)
    {
        go.transform.parent = pool.transform;
        go.SetActive(false);
    }

    // method to retrieve the next available object in the pool. Go through the list and find the next object that is inactive, enable and return it
    GameObject GetLineFromPool()
    {
        foreach (GameObject go in linePool)
        {
            if (!go.activeSelf)
            {
                go.SetActive(true);
                return go;
            }
        }

        // if all the pool objects are currently in use then we should create another
        Debug.Log("Creating additional line");
        return CreateLine();

        //Debug.LogError("Returning null object from line pool. Suggest increasing the blip pool size");
        //return null;
    }

    // method for removing a contact from the list
    public void RemoveContact(GameObject contact)
    {
        // go through the list and only remove the contact if it exists
        foreach (contactClass c in contacts)
        {
            if (c.targetObject == contact)
            {
                // return the unused objects back to the pool
                ReturnToPool(c.blip);
                ReturnToPool(c.line);

                // remove the contact
                contacts.Remove(c);

                // we don't need to keep searching through the contact list because there should only be just this one that matches so return
                return;
            }
        }
    }
}
