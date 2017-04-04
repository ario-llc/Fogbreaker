using UnityEngine;
using System.Collections;

public class ash_Wanderer : MonoBehaviour
{
    [HideInInspector]
    public float wanderAmount = 1f;

    int counter = 0;
    int dir = 0;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Wander();
    }

    void Wander()
    {
        if (counter > 100)
        {
            dir++;
            counter = 0;
        }
        else
        {
            counter++;
        }

        if (dir > 6) dir = 0;

        switch (dir)
        {
            case 0:
                transform.Translate(Vector3.forward * wanderAmount);
                break;

            case 1:
                transform.Translate(Vector3.right * wanderAmount);
                break;

            case 2:
                transform.Translate(Vector3.up * wanderAmount);
                break;

            case 3:
                transform.Translate(Vector3.left * wanderAmount);
                break;

            case 4:
                transform.Translate(Vector3.back * wanderAmount);
                break;

            case 5:
                transform.Translate(Vector3.down * wanderAmount);
                break;
        }
    }
}
