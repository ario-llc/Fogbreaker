using UnityEngine;
using System.Collections;

public class ash_Contact : MonoBehaviour
{
    [Tooltip("higher value = more likely to be detected at further distances")]
    public float signature = 100f;

    // Use this for initialization
    void Start()
    {
        // check that THIS object has a collider
        if (!CheckForComponents()) Debug.LogError(gameObject.name + " - requires a collider!");
    }

    // Update is called once per frame
    void Update()
    {
        // signature should never be less than zero
        if (signature < 0f) signature = 0f;
    }

    // method to check that at least some form of collider is on THIS object otherwise the radar won't work correctly
    bool CheckForComponents()
    {
        // temp variable
        bool check = false;

        // perform the checks, at least some kind needs to be present
        if (GetComponent<BoxCollider>() != null) check = true;
        if (GetComponent<SphereCollider>() != null) check = true;
        if (GetComponent<CapsuleCollider>() != null) check = true;
        if (GetComponent<MeshCollider>() != null) check = true;

        // return the result
        return check;
    }
}
