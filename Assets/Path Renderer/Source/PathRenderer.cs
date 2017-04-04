using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ario.PathRenderer {

[ExecuteInEditMode]
public class PathRenderer : MonoBehaviour {

	public PathData data;

	public bool useWorldSpace = true;

	public PathMeshGenerator[] meshGenerators;

	public Material[] materials;

	private Mesh[] m_pathMeshes;
	
	private void Update () {
		if (meshGenerators != null)
			DrawMeshes();
	}

	private void DrawMeshes () {
		if ( m_pathMeshes == null || m_pathMeshes.Length != meshGenerators.Length )
			m_pathMeshes = new Mesh[ meshGenerators.Length ];

		for ( int i = 0; i < meshGenerators.Length; i ++ ) {
			if (m_pathMeshes[i] == null)
				m_pathMeshes[i] = new Mesh();

			meshGenerators[i].Generate( data, ref m_pathMeshes[i] );

			Graphics.DrawMesh( 
				m_pathMeshes[i],
				useWorldSpace ? Vector3.zero : transform.position, 
				useWorldSpace ? Quaternion.identity : transform.rotation, 
				(materials.Length >= meshGenerators.Length) ? materials[i] : null, 
				0 
			);
		}
	}

}

}