using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RadarSystem
{
    private static RadarSystem mInstance;
    public static RadarSystem instance
    {
        get
        {
            if (mInstance == null )
            {
                mInstance = new RadarSystem();
            }
            return mInstance;
        } 
    }

    public HashSet<RadarTarget> targets;

    private RadarSystem ()
    {
        targets = new HashSet<RadarTarget>();
    }

    public void RegisterTarget ( RadarTarget target )
    {
        targets.Add(target);
    }
    public void UnregisterTarget ( RadarTarget target )
    {
        targets.Remove(target);
    }
}

public class RadarTarget : MonoBehaviour {

    public Color blipColor;

    private void OnEnable ()
    {
        RadarSystem.instance.RegisterTarget(this);
    }

    private void OnDisable ()
    {
        RadarSystem.instance.UnregisterTarget(this);
    }

}
