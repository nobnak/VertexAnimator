using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VertexAnimater {
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class BakedAnimation : MonoBehaviour {
		public float time = 0f;
        public Material mat;

        Mesh _mesh;
		MeshFilter _filter;

		void Awake() {
			_filter = GetComponent<MeshFilter> ();
            _mesh = _filter.mesh;
		}
		void OnDestroy() {
			if (_mesh != null) {
				Destroy (_mesh);
				_mesh = null;
			}
		}

		void Update() {
            var tex = mat.GetTexture (ShaderConst.SHADER_ANIM_TEX) as Texture2D;
            var scale = mat.GetVector (ShaderConst.SHADER_SCALE);
            var offset = mat.GetVector (ShaderConst.SHADER_OFFSET);
            var fps = mat.GetFloat (ShaderConst.SHADER_FPS);

            var frame = time * fps;
            var vertices = _mesh.vertices;
            for (var i = 0; i < vertices.Length; i++) {
                var c1 = tex.GetPixel (i, (int)frame);
                var c2 = tex.GetPixel (i, (int)frame + (tex.height >> 1));
                vertices [i] = new Vector3 (
                    (c1.r + c2.r * VertexTex.COLOR_DEPTH_INV) * scale.x + offset.x,
                    (c1.g + c2.g * VertexTex.COLOR_DEPTH_INV) * scale.y + offset.y,
                    (c1.b + c2.b * VertexTex.COLOR_DEPTH_INV) * scale.z + offset.z);
            }
            _mesh.vertices = vertices;
		}
	}
}