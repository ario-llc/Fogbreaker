using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class LookAtCamera : MonoBehaviour {

    public float maintainSize = 1;
    public bool maintainOffset = true;
    public Vector3 offset;

	private void OnWillRenderObject ()
    {
        var viewVec = Camera.current.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(-viewVec);

        float scale = maintainSize * viewVec.magnitude;
        transform.localScale = Vector3.one * scale;
        transform.localPosition = offset + offset.normalized * scale;
    }
}
