using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class ash_RadarSweepEffect : MonoBehaviour
{
    ash_Radar radarSystem;

    [SerializeField]
    [Tooltip("the rotation direction of the sweep effect, max values are -1 to 1")]
    Vector3 rotateDirection = Vector3.zero;

    enum SweepStyleEnum
    {
        Sweep,
        BackAndForth,
    }

    [SerializeField]
    SweepStyleEnum sweepStyle = SweepStyleEnum.Sweep;

    // Use this for initialization
    void Start()
    {
        // find the radar system
        radarSystem = FindObjectOfType<ash_Radar>();
        if (radarSystem == null) Debug.Log("You must include a Radar System prefab in the scene!");

        // set limits
        if (rotateDirection.x > 0f) rotateDirection.x = 1f;
        if (rotateDirection.x < 0f) rotateDirection.x = -1f;
        if (rotateDirection.y > 0f) rotateDirection.y = 1f;
        if (rotateDirection.y < 0f) rotateDirection.y = -1f;
        if (rotateDirection.z > 0f) rotateDirection.z = 1f;
        if (rotateDirection.z < 0f) rotateDirection.z = -1f;

        // set rigid body
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().isKinematic = true;

        // set layer to that of the radar system
        gameObject.layer = radarSystem.gameObject.layer;

        // check that THIS object has a collider
        if (!radarSystem.GetComponent<ash_Radar>().CheckForCollider(transform)) Debug.LogError(transform.root.name + " - requires a collider!");

        // set triggers
        radarSystem.GetComponent<ash_Radar>().SetTrigger(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        // perform the effect
        if (sweepStyle == SweepStyleEnum.Sweep) Sweep();
        if (sweepStyle == SweepStyleEnum.BackAndForth) BackAndForth();
    }

    // perform the sweep effect. This effect rotates the sweep entirely in a 360 degree rotation
    void Sweep()
    {
        // temp variable, we use math.abs and subtract more than the original variable to get a value for the rotation of the sweep
        int r = Mathf.Abs(radarSystem.options.delay - 6);

        // rotate the sweep effect
        transform.Rotate(rotateDirection * r);
    }

    // perform the effect
    void BackAndForth()
    {
        // temp variable, we use math.abs and subtract more than the original variable to get a value for the rotation of the sweep
        int r = Mathf.Abs(radarSystem.options.delay - 6);

        // get the angle of the rotation
        float angleX = transform.localEulerAngles.x;

        float limit = 60f; // max angle degree to "turn" in either direction

        // set limits so the effect sweeps back and forth - may need to adjust this or the sweep game objecs scale if the sweep isn't "hitting" all the blips
        if (angleX > limit && angleX < 200f) rotateDirection.x = -1f;
        if (angleX < (360f - limit) && angleX > 200f) rotateDirection.x = 1f;

        // rotate the sweep effect
        transform.Rotate(rotateDirection * r);
    }
}
