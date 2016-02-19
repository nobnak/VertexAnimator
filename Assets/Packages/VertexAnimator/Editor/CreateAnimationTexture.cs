using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace VertexAnimater {

	public class CreateAnimationTexture {
		public const float FPS = 5f;
		public const FilterMode ANIM_TEX_FILTER = FilterMode.Bilinear;

		public const float DT = 1f / FPS;
		public const float COLOR_DEPTH = 255f;

		public const string SHADER_NAME = "VertexAnim/OneTime";
		public const string SHADER_ANIM_TEX = "_AnimTex";
		public const string SHADER_SCALE = "_AnimTex_Scale";
		public const string SHADER_OFFSET = "_AnimTex_Offset";
		public const string SHADER_ANIM_END = "_AnimTex_AnimEnd";
		public const string SHADER_FPS = "_AnimTex_FPS";

		public const string DIR_ASSETS = "Assets";
		public const string DIR_ROOT = "AnimationTex";

		[MenuItem("Custom/Create/VertexAnimation")]
		public static void CreateMaterial() {
			GameObject selection = Selection.activeGameObject;
			if (selection == null) {
				Debug.Log("No Active GameObject");
				return;
			}
			if (!EditorApplication.isPlaying)
				EditorApplication.isPlaying = true;

			selection.AddComponent<MonoBehaviour>().StartCoroutine(CreateMaterial(selection));
		}

		public static IEnumerator CreateMaterial(GameObject selection) {
            var sampler = new CombinedMeshSampler (selection);

            List<Vector3[]> verticesList = new List<Vector3[]> ();

            for (float time = 0; time < (sampler.Length + DT); time += DT) {
                var combinedMesh = sampler.Sample (time);
                verticesList.Add (combinedMesh.vertices);
            }

            Vector2[] uv2;
            Vector4 scale;
            Vector4 offset;
            var tex2d = NewMethod (verticesList, out scale, out offset, out uv2);

            var mesh = sampler.CombinedMesh;
            mesh.uv2 = uv2;
            mesh.bounds = new Bounds ((Vector3)(0.5f * scale + offset), (Vector3)scale);
			
			var folderPath = DIR_ASSETS + "/" + DIR_ROOT;
			if (!Directory.Exists(folderPath))
				AssetDatabase.CreateFolder(DIR_ASSETS, DIR_ROOT);
			var guid = AssetDatabase.CreateFolder(folderPath, selection.name);
			folderPath = AssetDatabase.GUIDToAssetPath(guid);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			yield return 0;

			var pngPath = folderPath + "/" + selection.name + ".png";
			File.WriteAllBytes(pngPath, tex2d.EncodeToPNG());
			AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);
			var pngImporter = (TextureImporter) AssetImporter.GetAtPath(pngPath);
			var pngSettings = new TextureImporterSettings();
			pngImporter.ReadTextureSettings(pngSettings);
			pngSettings.filterMode = ANIM_TEX_FILTER;
			pngSettings.mipmapEnabled = false;
			pngSettings.linearTexture = true;
            pngSettings.maxTextureSize = Mathf.Max(tex2d.width, tex2d.height);
			pngSettings.wrapMode = TextureWrapMode.Clamp;
			pngSettings.textureFormat = TextureImporterFormat.RGB24;
			pngImporter.SetTextureSettings(pngSettings);
			AssetDatabase.WriteImportSettingsIfDirty(pngPath);
			AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);

			Material mat = new Material(Shader.Find(SHADER_NAME));
			//mat.mainTexture = skinnedMesh.sharedMaterial.mainTexture;
			mat.SetTexture (SHADER_ANIM_TEX, (Texture2D)AssetDatabase.LoadAssetAtPath (pngPath, typeof(Texture2D)));
			mat.SetVector (SHADER_SCALE, scale);
			mat.SetVector (SHADER_OFFSET, offset);
            mat.SetVector (SHADER_ANIM_END, new Vector4 (sampler.Length, verticesList.Count - 1, 0f, 0f));
			mat.SetFloat (SHADER_FPS, FPS);

			AssetDatabase.CreateAsset(mat, folderPath + "/" + selection.name + "Mat.mat");
			AssetDatabase.CreateAsset(mesh, folderPath + "/" + selection.name + "Mesh.asset");
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			
			GameObject go = new GameObject(selection.name);
			go.AddComponent<MeshRenderer>().sharedMaterial = mat;
			go.AddComponent<MeshFilter>().sharedMesh = mesh;
			PrefabUtility.CreatePrefab(folderPath + "/" + selection.name + ".prefab", go);
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

        static Texture2D NewMethod (List<Vector3[]> verticesList, out Vector4 scale, out Vector4 offset, out Vector2[] uv2) {
            var firstVertices = verticesList[0];
            var firstVertex = firstVertices[0];
            var vertexCount = firstVertices.Length;

            float minX = firstVertex.x, minY = firstVertex.y, minZ = firstVertex.z, maxX = firstVertex.x, maxY = firstVertex.y, maxZ = firstVertex.z;
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
            var texWidth = LargerInPow2 (vertexCount);
            var texHeight = LargerInPow2 (verticesList.Count * 2);
            Debug.Log (string.Format ("tex({0}x{1}), nVertices={2} nFrames={3}", texWidth, texHeight, vertexCount, verticesList.Count));
            Texture2D tex2d = new Texture2D (texWidth, texHeight, TextureFormat.RGB24, false);
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
                    float d = 1f / COLOR_DEPTH;
                    var c1 = new Color (Mathf.Floor (posX * COLOR_DEPTH) * d, Mathf.Floor (posY * COLOR_DEPTH) * d, Mathf.Floor (posZ * COLOR_DEPTH) * d);
                    tex2d.SetPixel (x, y, c1);
                    var c2 = new Color ((posX - c1.r) * COLOR_DEPTH, (posY - c1.g) * COLOR_DEPTH, (posZ - c1.b) * COLOR_DEPTH);
                    tex2d.SetPixel (x, y + (texHeight >> 1), c2);
                }
            }
            tex2d.Apply ();
            return tex2d;
        }
	}
}