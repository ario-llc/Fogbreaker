using UnityEngine;
using System.Collections;

public class ash_FadeOverTime : MonoBehaviour
{
    ash_Radar radarSystem; // the radar system object

    // random variables
    Color original;
    Color altered;
    Material rend; // the renderer material component

    // material color values
    int thisColorType = 0;

    // Use this for initialization
    public void Start()
    {
        // find the radar system
        radarSystem = FindObjectOfType<ash_Radar>();
        if (radarSystem == null) Debug.Log("You must include a Radar System prefab in the scene!");

        // get a reference to the material
        rend = GetComponent<Renderer>().material;
        thisColorType = ash_Radar.CheckMaterialColor(rend);

        // save the original starting material color
        original = rend.GetColor(thisColorType);
    }

    // Update is called once per frame
    void Update()
    {
        // fade the blip if set in the options, but only if sweeping method is used
        if (radarSystem != null)
        {
            if (radarSystem.options.blipFadeRate != 0f && radarSystem.options.updateMethod == ash_Radar.pingMethodEnum.Sweep) Fade(radarSystem.options.blipFadeRate);
        }
    }

    public void ResetFade()
    {
        // reset the blips opacity
        rend.SetColor(thisColorType, original);
    }

    // fade out the material so the object vanishes
    void Fade(float value)
    {
        // get the material color from the renderer
        altered = rend.GetColor(thisColorType);

        // reduce the alpha
        altered.a -= value;

        // ensure limits
        altered.a = Mathf.Clamp(altered.a, 0f, 1f);

        // set the altered material back into the renderer
        rend.SetColor(thisColorType, altered);
    }
}
