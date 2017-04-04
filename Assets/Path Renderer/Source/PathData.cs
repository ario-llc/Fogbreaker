using UnityEngine;

namespace Ario.PathRenderer {

public enum PathInterpolationMode {
	Linear,
	CatmullRom
}

[System.Serializable]
public class PathData {
	public Vector3[] points;
	public PathInterpolationMode interpolationMode;

	// Todo: If this is called often, we may want to implement caching.
	public float Length {
		get {
			if (points.Length < 2)
				return 0;

			float length = 0;
			for ( int i = 0; i < points.Length-1; i ++ ) {
				length += Vector3.Distance( points[i], points[i+1] );
			}
			return length;
		}
	}

	public Vector3 GetPoint ( int index ) {
		if ( points.Length == 0 )
			return Vector3.zero;

		return points[ Mathf.Clamp(index, 0, points.Length-1) ];
	}

	public Vector3 GetTangent ( int index ) {
		return (GetPoint(index+1) - GetPoint(index-1)) * 0.5F;
	}

	public Vector3 Evaluate ( float distance ) {
		var i0 = 0;
		var i1 = 0;
		float interp = 0;

		for ( int i = 0; i < points.Length-1; i ++ ) {
			float segmentLength = Vector3.Distance( GetPoint(i), GetPoint(i+1) );
			i0 = i;
			i1 = i+1;
			interp = distance/segmentLength;
		
			if (distance < segmentLength)
				break;
				
			distance -= segmentLength;
		}

		var p0 = GetPoint(i0);
		var p1 = GetPoint(i1);

		if (interpolationMode == PathInterpolationMode.Linear) {
			return LinearInterpolation( p0, p1, interp );
		}else if (interpolationMode == PathInterpolationMode.CatmullRom) {
			var t0 = GetTangent( i0 );
			var t1 = GetTangent( i1 );
			return CatmullRomInterpolation( p0, p1, t0, t1, interp );
		}

		return Vector3.zero;
	}

	public Vector3 EvaluateTangent ( float distance ) {
		return (Evaluate(distance + 0.1f) - Evaluate(distance - 0.1f)).normalized;
	}

	private Vector3 LinearInterpolation ( Vector3 P1, Vector3 P2, float t ) {
		return Vector3.Lerp( P1, P2, t );
	}

	private Vector3 CatmullRomInterpolation ( Vector3 P1, Vector3 P2, Vector3 T1, Vector3 T2, float t ) {
		float t2 = t*t;
		float t3 = t2*t;

		float h1 =  2*t3 - 3*t2 + 1; 	// calculate basis function 1
		float h2 = -2*t3 + 3*t2; 		// calculate basis function 2
		float h3 =    t3 - 2*t2 + t; 	// calculate basis function 3
		float h4 =    t3 -   t2; 		// calculate basis function 4
		
		Vector3 p = h1*P1 + 			// multiply and sum all funtions
					h2*P2 + 			// together to build the interpolated
					h3*T1 + 			// point along the curve.
					h4*T2;

		return p;
	}

}

}