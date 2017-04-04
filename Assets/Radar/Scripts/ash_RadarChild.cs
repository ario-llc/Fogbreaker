//#define AllTheTags // <-- comment out this line if you DO NOT have "AllTheTags!" installed. Uncomment this line if you have "AllTheTags!" installed.

using UnityEngine;
using System.Collections;

#if AllTheTags
using PigeonCoopToolkit.AllTheTags;
#endif

public class ash_RadarChild : MonoBehaviour
{
    ash_Radar radarSystem; // the radar system

    [SerializeField]
    [Tooltip("the actual textured plane prefab")]
    GameObject planePrefab;

    [HideInInspector]
    public GameObject plane;

    [SerializeField]
    bool defaultShape = false;

    bool showSweepEffect = true;

    // random variables
    float delayCounter = 0f;
    RaycastHit hit;
    bool useUnityTags = false;

    // Use this for initialization
    void Awake()
    {
        // find the radar system
        radarSystem = FindObjectOfType<ash_Radar>();
        if (radarSystem == null) Debug.Log("You must include a Radar System prefab in the scene!");

        // set THIS object to the same layer as the radar to avoid interrupting the LOS ray casts
        gameObject.layer = radarSystem.gameObject.layer;

        // create the textured horizontal radar plane (the thing you see the blips on) and add it as a child of the radar background sphere
        plane = Instantiate(planePrefab) as GameObject;
        //plane.transform.position = Vector3.zero; // reset its position data just in case (not needed I think, or it gets it from the prefab?)
        plane.transform.parent = radarSystem.objects.radarBackgroundSphere;
        plane.layer = radarSystem.gameObject.layer;
        plane.GetComponent<ash_Plane>().sweepEffect.layer = radarSystem.gameObject.layer;

        // add this shape to the radar system
        radarSystem.AddShape(gameObject);

        // set this shape to default if enabled
        if (defaultShape) radarSystem.defaultRadarShape = gameObject;
    }

    void Start()
    {
        // check that THIS object has a collider
        if (!radarSystem.GetComponent<ash_Radar>().CheckForCollider(transform)) Debug.LogError(transform.root.name + " - requires a collider!");

        // set triggers
        radarSystem.GetComponent<ash_Radar>().SetTrigger(gameObject);

        // the parent object MUST have a rigidbody attached otherwise the this collider doesn't work?
        if (transform.root.GetComponent<Rigidbody>() == null) Debug.LogError(transform.root.name + " - requires a RigidBody script to be attached for the radar to work!");
        // the parent object MUST have a collider
        if (!radarSystem.GetComponent<ash_Radar>().CheckForCollider(transform.root)) Debug.LogError(transform.root.name + " - requires a collider!");

        // scale the object (and trigger) to match the radar scale
        //transform.localScale = new Vector3(radarSystem.options.scale, radarSystem.options.scale, radarSystem.options.scale); // not sure i want this

        // check that THIS OBJECTS' PARENT/ROOT object (should be the player) has a rigidbody
        if (transform.root.GetComponent<Rigidbody>() == null) Debug.LogError(transform.root.name + " - requires a rigidbody!");

        // set the size of the radar triggers. Those are the things that actually "detect" targets
        if ((transform.localScale.x + transform.localScale.y + transform.localScale.z) < 1f)
        {
            Debug.LogError(gameObject.name + " - scale is too small! x,y,z should all be larger than zero. You may experience inaccurate readings with sizers smaller than 1,1,1 (this is a Unity issue)");
        }

        // set the collider sizes so the user must change the transform scale to change the size of the detectable area
        if (GetComponent<SphereCollider>() != null) GetComponent<SphereCollider>().radius = 0.5f;
        if (GetComponent<BoxCollider>() != null) GetComponent<BoxCollider>().size = Vector3.one;
        if (GetComponent<CapsuleCollider>() != null)
        {
            GetComponent<CapsuleCollider>().radius = 0.5f;
            GetComponent<CapsuleCollider>().height = 1f;
        }

        // if update method is set to constant then disable the sweep effect
        if (radarSystem.options.updateMethod == ash_Radar.pingMethodEnum.Constant) showSweepEffect = false;

        // enable/disable the sweep effect
        if (showSweepEffect)
        {
            plane.GetComponent<ash_Plane>().sweepEffect.SetActive(true);
        }
        else
        {
            plane.GetComponent<ash_Plane>().sweepEffect.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
    
    }

    // constantly check all objects inside the trigger area for new contacts and out-of-range/missing existing contacts.
    void OnTriggerEnter(Collider col) // OnTriggerStay() doesn't always work
    {
        // only check once in a while (save cpu)
        if (delayCounter >= (radarSystem.options.delay + Random.Range(-0.01f, 0.01f))) // for some reason the trigger sometimes fails to detect unless you add the Random.Range to "shake it up" a bit - not sure what causes this yet
        //if (delayCounter >= radarSystem.options.delay) // this is buggy even though it should work?
        {
            // we want to check the tracking target list for all the tags we want to find
            for (int i = 0; i < radarSystem.radar.targets.Length; i++)
            {
                // AllTheTags!
                // use all the tags if we have defined it
#if AllTheTags
                if (radarSystem.options.tagMethod == ash_Radar.tagMethodEnum.AllTheTags)
                {
                    if (Tags.HasTag(col.gameObject, radarSystem.radar.targets[i].tag))
                    {
                        if (col.GetComponent<ash_Contact>() != null) CheckForContact(col.gameObject, i);
                    }
                }
#elif !AllTheTags
                useUnityTags = true; // otherwise use unity default
#endif

                // Unity
                if (radarSystem.options.tagMethod == ash_Radar.tagMethodEnum.Unity || useUnityTags)
                {
                    if (col.tag == radarSystem.radar.targets[i].tag)
                    {
                        if (col.GetComponent<ash_Contact>() != null) CheckForContact(col.gameObject, i);
                    }
                }
            }

            delayCounter = 0f;
        }

        delayCounter++;
    }

    // if something leaves the radar trigger then remove it (it automatically checks if the contact exists before removing)
    void OnTriggerExit(Collider col)
    {
        radarSystem.RemoveContact(col.gameObject);
    }

    // once we have found an object with an acceptable tag, we need to check if its within range and Line Of Sight
    void CheckForContact(GameObject col, int index)
    {
        //check if object is in range
        if (ContactInRange(col.gameObject))
        {
            //check if in LoS
            if (ContactHasLOS(col.gameObject))
            {
                //try and add the new contact
                radarSystem.AddContact(col.gameObject, index);
            }
            else
            {
                //otherwise try and remove the contact (if its existing)
                RemoveContact(col);
            }
        }
        else
        {
            //otherwise try and remove the contact (if its existing)
            RemoveContact(col);
        }
    }

    // try and remove the contact from the list. The method on the parent will check if it already exists for us
    void RemoveContact(GameObject contact)
    {
        radarSystem.RemoveContact(contact);
    }

    // method to check if the contact is within range.
    bool ContactInRange(GameObject contact)
    {
        // check if the contact has the correct script attached. 
        if (contact.GetComponent<ash_Contact>() == null)
        {
            // if not then return false so it doesn't get added to the contact list
            Debug.LogError(contact.name + " - is trying to be added as a contact but does not contain an 'ash_Contact.cs' script!");
            return false;
        }

        // only check if target is in range if enabled in the options
        if (radarSystem.options.checkTargetInRange)
        {
            // get the distance to the target from the player and work out based on both parties' signatures if they are within range
            float distance = Vector3.Distance(contact.transform.position, radarSystem.objects.player.position);
            float sensorCheck = distance / contact.GetComponent<ash_Contact>().signature;

            // don't add target if beyond sensor range/strength to detect
            if (sensorCheck > radarSystem.options.sensorStrength) return false;
        }

        return true;
    }

    // check if target is within Line Of Sight
    bool ContactHasLOS(GameObject contact)
    {
        // only check for LOS if enabled in the options
        if (radarSystem.options.checkTargetInLos)
        {
            // make a layerMask of the radar systems layer
            int layerMask = 1 << radarSystem.gameObject.layer;

            // we want to check against all targets that may be blocking LOS that doesn't have the same layer as the radar system
            layerMask = ~layerMask;

            // do a line cast and see what we hit
            if (Physics.Linecast(radarSystem.objects.player.position, contact.transform.position, out hit, layerMask))
            {
                // if the object we hit is the same contact we are trying to add then success
                if (hit.transform.root == contact.transform)
                {
                    if (radarSystem.options.debug) Debug.DrawRay(radarSystem.objects.player.position, hit.transform.root.position - radarSystem.objects.player.position, Color.green);
                    return true;
                }
                else
                {
                    // otherwise there must be something blocking the line
                    if (radarSystem.options.debug) Debug.DrawRay(radarSystem.objects.player.position, hit.transform.root.position - radarSystem.objects.player.position, Color.red);
                    return false;
                }
            }

            return false;
        }

        return true;
    }
}
