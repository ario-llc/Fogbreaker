using UnityEngine;
using System.Collections;

public class MagnifyReticle : MonoBehaviour
{

    public Renderer reticleRenderer;

    public void ZoomOut()
    {
        var initialValue = reticleRenderer.material.GetFloat("_Zoom");
        StartCoroutine(SetZoomCoroutine(initialValue, 1, 0.25f));
    }

    public void ZoomIn()
    {
        var initialValue = reticleRenderer.material.GetFloat("_Zoom");
        StartCoroutine(SetZoomCoroutine(initialValue, 2, 0.25f));
    }

    private IEnumerator SetZoomCoroutine(float startValue, float endValue, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            reticleRenderer.material.SetFloat("_Zoom", Mathf.Lerp(startValue, endValue, time / duration));
            yield return new WaitForEndOfFrame();
        }
    }

}
