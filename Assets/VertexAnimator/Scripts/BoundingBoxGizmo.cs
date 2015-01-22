using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class BoundingBoxGizmo : MonoBehaviour {

	void OnDrawGizmos() {
		var bounds = renderer.bounds;
		Gizmos.DrawWireCube(bounds.center, bounds.size);
	}
}
