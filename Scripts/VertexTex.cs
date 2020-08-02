using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using nobnak.Gist.ObjectExt;
using nobnak.Gist.Extensions.Texture2DExt;

namespace VertexAnimater {

    public class VertexTex : System.IDisposable {
        public const float FPS = 5f;
        public const FilterMode ANIM_TEX_FILTER = FilterMode.Bilinear;

        public const float DT = 1f / FPS;
        public const float COLOR_DEPTH = 255f;
        public const float COLOR_DEPTH_INV = 1f / COLOR_DEPTH;

        public readonly Vector2[] uv2;
        public readonly Vector4 scale;
        public readonly Vector4 offset;
        public readonly Texture2D positionTex;
		public readonly Texture2D normalTex;
        public readonly float frameEnd;
        public readonly List<Vector3[]> verticesList;
		public readonly List<Vector3[]> normalsList;

        Vector3[] _cacheVertices;

        public VertexTex(IMeshSampler sample) {
            verticesList = new List<Vector3[]> ();
			normalsList = new List<Vector3[]> ();
            for (float t = 0; t < (sample.Length + DT); t += DT) {
                Matrix4x4 mpos, mnorm;
                var combinedMesh = sample.Sample (t, out mpos, out mnorm);
                var vertices = combinedMesh.vertices;
                var normals = combinedMesh.normals;
                for (var i = 0; i < vertices.Length; i++) {
                    vertices [i] = mpos.MultiplyPoint3x4 (vertices [i]);
                    normals [i] = mnorm.MultiplyVector (normals [i]);
                }
                verticesList.Add (vertices);
                normalsList.Add (normals);
            }

            var firstVertices = verticesList[0];
            var firstVertex = firstVertices[0];
            var vertexCount = firstVertices.Length;
            frameEnd = vertexCount - 1;

            var minX = firstVertex.x;
            var minY = firstVertex.y;
            var minZ = firstVertex.z;
            var maxX = firstVertex.x;
            var maxY = firstVertex.y;
            var maxZ = firstVertex.z;
            foreach (var vertices in verticesList) {
                for (var i = 0; i < vertices.Length; i++) {
                    var v = vertices [i];
                    minX = Mathf.Min (minX, v.x);
                    minY = Mathf.Min (minY, v.y);
                    minZ = Mathf.Min (minZ, v.z);
                    maxX = Mathf.Max (maxX, v.x);
                    maxY = Mathf.Max (maxY, v.y);
                    maxZ = Mathf.Max (maxZ, v.z);
                }
            }
            scale = new Vector4 (maxX - minX, maxY - minY, maxZ - minZ, 1f);
            offset = new Vector4 (minX, minY, minZ, 1f);
            Debug.LogFormat("Scale={0} Offset={1}", scale, offset);

            var texWidth = LargerInPow2 (vertexCount);
            var texHeight = LargerInPow2 (verticesList.Count * 2);
            Debug.Log (string.Format ("tex({0}x{1}), nVertices={2} nFrames={3}", texWidth, texHeight, vertexCount, verticesList.Count));

			positionTex = positionTex.Create(texWidth, texHeight, TextureFormat.RGB24, false, true);
            positionTex.filterMode = ANIM_TEX_FILTER;
            positionTex.wrapMode = TextureWrapMode.Clamp;

			normalTex = normalTex.Create(texWidth, texHeight, TextureFormat.RGB24, false, true);
			normalTex.filterMode = ANIM_TEX_FILTER;
			normalTex.wrapMode = TextureWrapMode.Clamp;

            uv2 = new Vector2[vertexCount];
            var texSize = new Vector2 (1f / texWidth, 1f / texHeight);
            var halfTexOffset = 0.5f * texSize;
            for (int i = 0; i < uv2.Length; i++)
                uv2 [i] = new Vector2 ((float)i * texSize.x, 0f) + halfTexOffset;
            for (int y = 0; y < verticesList.Count; y++) {
                Vector3[] vertices = verticesList [y];
				Vector3[] normals = normalsList [y];
                for (int x = 0; x < vertices.Length; x++) {
					var pos = Normalize (vertices [x], offset, scale);
					Color c0, c1;
					Encode (pos, out c0, out c1);
                    positionTex.SetPixel (x, y, c0);
					positionTex.SetPixel (x, y + (texHeight >> 1), c1);

                    var normal = 0.5f * (normals [x].normalized + Vector3.one);
					Encode (normal, out c0, out c1);
					normalTex.SetPixel (x, y, c0);
					normalTex.SetPixel (x, y + (texHeight >> 1), c1);
                }
            }
            positionTex.Apply ();
			normalTex.Apply ();
        }

        public Vector3 Position(int vid, float frame) {
            frame = Mathf.Clamp (frame, 0f, frameEnd);
            var uv = uv2 [vid];
            uv.y += frame * positionTex.texelSize.y;
            var pos1 = positionTex.GetPixelBilinear (uv.x, uv.y);
            var pos2 = positionTex.GetPixelBilinear (uv.x, uv.y + 0.5f);
            return new Vector3 (
                (pos1.r + pos2.r / COLOR_DEPTH) * scale.x + offset.x,
                (pos1.g + pos2.g / COLOR_DEPTH) * scale.y + offset.y,
                (pos1.b + pos2.b / COLOR_DEPTH) * scale.z + offset.z);
        }
        public Bounds Bounds() { return new Bounds ((Vector3)(0.5f * scale + offset), (Vector3)scale); }
        public Vector3[] Vertices(float frame) {
            frame = Mathf.Clamp (frame, 0f, frameEnd);
            var index = Mathf.Clamp ((int)frame, 0, verticesList.Count - 1);
            var vertices = verticesList [index];
            return vertices;
        }

		public static Vector3 Normalize(Vector3 pos, Vector3 offset, Vector3 scale) {
			return new Vector3 (
				(pos.x - offset.x) / scale.x,
				(pos.y - offset.y) / scale.y,
				(pos.z - offset.z) / scale.z);
		}
		public static void Encode(float v01, out float c0, out float c1) {
			c0 = Mathf.Clamp01(Mathf.Floor(v01 * COLOR_DEPTH) * COLOR_DEPTH_INV);
			c1 = Mathf.Clamp01 (Mathf.Round((v01 - c0) * COLOR_DEPTH * COLOR_DEPTH) * COLOR_DEPTH_INV);
		}
		public static void Encode(Vector3 v01, out Color c0, out Color c1) {
			float c0x, c0y, c0z, c1x, c1y, c1z;
			Encode (v01.x, out c0x, out c1x);
			Encode (v01.y, out c0y, out c1y);
			Encode (v01.z, out c0z, out c1z);
			c0 = new Color (c0x, c0y, c0z, 1f);
			c1 = new Color (c1x, c1y, c1z, 1f);
		}
        public static int LargerInPow2(int width) {
            width--;
            var digits = 0;
            while (width > 0) {
                width >>= 1;
                digits++;
            }
            return 1 << digits;
        }

        #region IDisposable implementation
        public void Dispose () {
			positionTex.Destroy ();
			normalTex.Destroy ();
        }
        #endregion
    }
}
