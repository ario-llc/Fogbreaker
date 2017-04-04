using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Ario.PathRenderer;

[ExecuteInEditMode]
public class WaypointSet : MonoBehaviour {

	[SerializeField]
	private PathRenderer m_pathRenderer;

	public Transform[] waypointTransforms;	
	
	private void Awake () {
		if (m_pathRenderer == null)
			m_pathRenderer = gameObject.GetComponentInChildren<PathRenderer>();
	}

	private void Update () {
		if (m_pathRenderer != null) {				
			if (m_pathRenderer.data.points == null || m_pathRenderer.data.points.Length != waypointTransforms.Length) 
				m_pathRenderer.data.points = new Vector3[waypointTransforms.Length];

			for (int i = 0; i < waypointTransforms.Length; i ++ ) {
				m_pathRenderer.data.points[i] = waypointTransforms[i].position;
			}
		}
	}

}
