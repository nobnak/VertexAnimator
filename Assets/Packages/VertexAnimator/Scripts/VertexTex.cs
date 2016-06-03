using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        public readonly Texture2D tex2d;
        public readonly float frameEnd;
        public readonly List<Vector3[]> verticesList;

        Vector3[] _cacheVertices;

        public VertexTex(CombinedMeshSampler sample) { 
            verticesList = new List<Vector3[]> ();
            for (float t = 0; t < (sample.Length + DT); t += DT) {
                var combinedMesh = sample.Sample (t);
                verticesList.Add (combinedMesh.vertices);
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

            tex2d = new Texture2D (texWidth, texHeight, TextureFormat.RGB24, false, true);
            tex2d.filterMode = ANIM_TEX_FILTER;
            tex2d.wrapMode = TextureWrapMode.Clamp;
            uv2 = new Vector2[vertexCount];
            var texSize = new Vector2 (1f / texWidth, 1f / texHeight);
            var halfTexOffset = 0.5f * texSize;
            for (int i = 0; i < uv2.Length; i++)
                uv2 [i] = new Vector2 ((float)i * texSize.x, 0f) + halfTexOffset;
            for (int y = 0; y < verticesList.Count; y++) {
                Vector3[] vertices = verticesList [y];
                for (int x = 0; x < vertices.Length; x++) {
                    float posX = (vertices [x].x - offset.x) / scale.x;
                    float posY = (vertices [x].y - offset.y) / scale.y;
                    float posZ = (vertices [x].z - offset.z) / scale.z;

                    var c1x = Mathf.Clamp01(Mathf.Floor(posX * COLOR_DEPTH) * COLOR_DEPTH_INV);
                    var c1y = Mathf.Clamp01(Mathf.Floor(posY * COLOR_DEPTH) * COLOR_DEPTH_INV);
                    var c1z = Mathf.Clamp01(Mathf.Floor(posZ * COLOR_DEPTH) * COLOR_DEPTH_INV);
                    tex2d.SetPixel (x, y, new Color(c1x, c1y, c1z, 1));

                    var c2x = Mathf.Clamp01 (Mathf.Round((posX - c1x) * COLOR_DEPTH * COLOR_DEPTH) * COLOR_DEPTH_INV);
                    var c2y = Mathf.Clamp01 (Mathf.Round((posY - c1y) * COLOR_DEPTH * COLOR_DEPTH) * COLOR_DEPTH_INV);
                    var c2z = Mathf.Clamp01 (Mathf.Round((posZ - c1z) * COLOR_DEPTH * COLOR_DEPTH) * COLOR_DEPTH_INV);
                    tex2d.SetPixel (x, y + (texHeight >> 1), new Color(c2x, c2y, c2z, 1));
                }
            }
            tex2d.Apply ();
        }

        public Vector3 Position(int vid, float frame) {
            frame = Mathf.Clamp (frame, 0f, frameEnd);
            var uv = uv2 [vid];
            uv.y += frame * tex2d.texelSize.y;
            var pos1 = tex2d.GetPixelBilinear (uv.x, uv.y);
            var pos2 = tex2d.GetPixelBilinear (uv.x, uv.y + 0.5f);
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
            GameObject.Destroy (tex2d);
        }
        #endregion
    }
}
