using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ario.PathRenderer {

[CreateAssetMenu(menuName="Path Renderer/Mesh Generators/Planar Generator")]
public class PlanarPathMeshGenerator : PathMeshGenerator {
	public Vector3 normal = new Vector3(0,1,0);
	//public float width = 1;
	public AnimationCurve width = AnimationCurve.Linear(0,1,1,1);
	
	[Range(0.1f,10)]
	public float vertexDistance = 1;

	public override void Generate ( PathData data, ref Mesh mesh ) {
		mesh.Clear();

		var vertices = new List<Vector3>();
		var normals = new List<Vector3>();
		var uv = new List<Vector2>();
		var triangles = new List<int>();

		var spanCount = Mathf.CeilToInt(data.Length / vertexDistance) + 1;
		var spanLength = data.Length / (spanCount-1);

		for ( int span = 0; span < spanCount; span ++ ) {
			var pathPoint = data.Evaluate( span * spanLength );
			var pathForward = data.EvaluateTangent( span * spanLength );
			var pathRight = Vector3.Cross( pathForward, normal ).normalized; 
			
			var spanWidth = width.Evaluate( (float)span / spanCount ) * 0.5f;

			// add the vertices for this path segment.
			vertices.Add( pathPoint - pathRight * spanWidth );
			vertices.Add( pathPoint + pathRight * spanWidth );

			normals.Add( normal );
			normals.Add( normal );

			uv.Add( new Vector2( 0, span * spanLength ) );
			uv.Add( new Vector2( 1, span * spanLength ) );
		}

		for ( int span = 0; span < spanCount-1; span ++ ) {
			var v0 = span * 2 + 0;
			var v1 = span * 2 + 1;
			var v2 = (span+1) * 2 + 0;
			var v3 = (span+1) * 2 + 1;

			triangles.Add( v0 );
			triangles.Add( v1 );
			triangles.Add( v3 );
			
			triangles.Add( v3 );
			triangles.Add( v2 );
			triangles.Add( v0 );
		}

		mesh.SetVertices( vertices );
		mesh.SetNormals( normals );
		mesh.SetUVs( 0, uv );
		mesh.SetTriangles( triangles, 0 );
		mesh.RecalculateBounds();
	}
}

}