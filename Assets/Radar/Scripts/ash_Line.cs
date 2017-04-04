using UnityEngine;
using System.Collections;

public class ash_Line : MonoBehaviour
{
    [HideInInspector]
    public Material blipMaterial; // reference to THIS line's blip

    Material rend; // the renderer material component

    // material color values
    int thisColorType = 0;
    int blipColorType = 0;

    // Use this for initialization
    public void Start()
    {
        // get the reference to the material on THIS object
        rend = GetComponent<Renderer>().material;
        thisColorType = ash_Radar.CheckMaterialColor(rend);
    }

    // Update is called once per frame
    void Update()
    {
        // only attempt to fade if this line is associated with a blip
        if (blipMaterial != null) Fade();
    }

    // on disable reset the blipmaterial
    void OnDisable()
    {
        blipMaterial = null;
    }

    // when a new contact is added to the list, this line must be initialized with the material of the related blip
    public void Initialize(GameObject b)
    {
        blipMaterial = b.GetComponent<Renderer>().material;
        blipColorType = ash_Radar.CheckMaterialColor(blipMaterial);
    }

    // fade out the material so the object vanishes
    void Fade()
    {
        // get the color of THIS line
        Color color = rend.GetColor(thisColorType);

        // set the alpha to the same as the blip so they fade together/pop back in together
        color.a = blipMaterial.GetColor(blipColorType).a;

        // set the altered material back into the renderer
        rend.SetColor(thisColorType, color);
    }
}
