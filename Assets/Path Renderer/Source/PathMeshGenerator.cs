using UnityEngine;

namespace Ario.PathRenderer {

public abstract class PathMeshGenerator : ScriptableObject {
	public abstract void Generate ( PathData data, ref Mesh mesh );
}

}