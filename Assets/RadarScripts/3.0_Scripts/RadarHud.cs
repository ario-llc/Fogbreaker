using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RadarHud : MonoBehaviour {

    private Dictionary<RadarTarget, GameObject> mBlips = new Dictionary<RadarTarget,GameObject>();

    [SerializeField]
    private GameObject mBlipPrefab;

    public Transform originTransform;

    [SerializeField, HideInInspector]
    private GameObject mBlipParent;

    void Awake ()
    {
        mBlipParent = new GameObject("Blips");
        mBlipParent.transform.SetParent(transform, false);
        mBlipParent.hideFlags = HideFlags.DontSave;
    }

    void Update () {
        // update the positions of radar blips. For each radar target
        foreach ( var target in RadarSystem.instance.targets )
        {
            // check if we've got a blip child for this target. If there isn't one yet,
            // instantiate one!
            if ( !mBlips.ContainsKey(target) )
            {
                var newBlip = (GameObject)GameObject.Instantiate(mBlipPrefab, Vector3.zero, Quaternion.identity);
                // make the blip a child of this radar object.
                newBlip.transform.SetParent(mBlipParent.transform, false);
                // set the blip's color to be the color specified by the target.
                newBlip.GetComponentInChildren<Renderer>().material.SetColor("_Color", target.blipColor);
                // and add it to the blip dictionary
                mBlips.Add(target, newBlip);
            }

            // now, get the blip for this target
            var blip = mBlips[target];
            // calculate the target's offset direction from this radar object
            var offsetVec = (target.transform.position - originTransform.position);
            offsetVec = originTransform.InverseTransformDirection(offsetVec);

            // clamp the offset vec to be planar, and normalize it.
            //offsetVec -= originTransform.up * Vector3.Dot(offsetVec, originTransform.up);
            //offsetVec = offsetVec.normalized * 0.5f;
            offsetVec = new Vector3(offsetVec.x, 0, offsetVec.z).normalized * 0.5f;

            // and set the blip position to be that unit-length vector.
            blip.transform.localPosition = offsetVec;

            //Debug.Log("Offset angle: "+ originTransform.rotation.y);
            //blip.transform.Rotate(Vector3.up, originTransform.rotation.y, Space.World);
            //Debug.Log("Blip transform rotation is : "+blip.transform.rotation.y);
        }
    }

}
