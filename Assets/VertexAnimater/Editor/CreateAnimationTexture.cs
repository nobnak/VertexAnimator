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
		public const float COLOR_DEPTH = 256f;

		public const string SHADER_ANIM_TEX = "_AnimTex";
		public const string SHADER_SCALE = "_Scale";
		public const string SHADER_OFFSET = "_Offset";
		public const string SHADER_ANIM_END = "_AnimEnd";
		public const string SHADER_FPS = "_FPS";

	    [MenuItem("Custom/Create/VertexAnimation")]
	    public static void CreateMaterial() {
	        GameObject selection = Selection.activeGameObject;
	        if (selection == null) {
				Debug.Log("No Active GameObject");
				return;
			}
	        Animation animation = selection.animation;
	        if (animation == null) {
				Debug.Log("No Animation");
				return;
			}
	        AnimationState state = animation[animation.clip.name];
	        if (state == null) {
				Debug.Log("No AnimationState");
				return;
			}
	        SkinnedMeshRenderer skinnedMesh = selection.GetComponentInChildren<SkinnedMeshRenderer>();
	        if (skinnedMesh == null) {
				Debug.Log("No SkinnedMeshRenderer");
				return;
			}
	        if (!EditorApplication.isPlaying)
	            EditorApplication.isPlaying = true;

	        selection.AddComponent<MonoBehaviour>().StartCoroutine(CreateMaterial(selection, animation, state, skinnedMesh));
	    }

	    public static IEnumerator CreateMaterial(GameObject selection, Animation animation, AnimationState state, SkinnedMeshRenderer skinnedMesh) {
	        Mesh mesh = new Mesh();
	        state.time = 0;
	        state.speed = 0;
			yield return 0;
	        animation.Play(state.name);
	        skinnedMesh.BakeMesh(mesh);

	        float
	            minX = mesh.vertices[0].x,
	            minY = mesh.vertices[0].y,
	            minZ = mesh.vertices[0].z,
	            maxX = mesh.vertices[0].x,
	            maxY = mesh.vertices[0].y,
	            maxZ = mesh.vertices[0].z;

	        Mesh tmpMesh = new Mesh();

	        List<Vector3[]> verticesList = new List<Vector3[]>();

			var trSelected = selection.transform;
			var trSkin = skinnedMesh.transform;
			for (float time = 0; time < (state.length + DT); time += DT) {
	            state.time = time;
	            yield return 0;
	            skinnedMesh.BakeMesh(tmpMesh);
	            Vector3[] vertices = tmpMesh.vertices;
				for (var i = 0; i < vertices.Length; i++) {
					vertices[i] = trSelected.InverseTransformPoint(trSkin.TransformPoint(vertices[i]));
					var v = vertices[i];

					minX = Mathf.Min(minX, v.x);
					minY = Mathf.Min(minY, v.y);
					minZ = Mathf.Min(minZ, v.z);

					maxX = Mathf.Max(maxX, v.x);
					maxY = Mathf.Max(maxY, v.y);
					maxZ = Mathf.Max(maxZ, v.z);
	            }
	            verticesList.Add(vertices);
	        }

			var scale = new Vector4(maxX - minX, maxY - minY, maxZ - minZ, 1f);
			var offset = new Vector4(minX, minY, minZ, 1f);

			mesh.vertices = new Vector3[mesh.vertexCount];
			mesh.bounds = skinnedMesh.localBounds;

			var texWidth = LargerInPow2(mesh.vertexCount);
			var texHeight = LargerInPow2(verticesList.Count * 2);
			Debug.Log(string.Format("tex({0}x{1}), nVertices={2} nFrames={3}", texWidth, texHeight, mesh.vertexCount, verticesList.Count));
			Texture2D tex2d = new Texture2D(texWidth, texHeight, TextureFormat.RGB24, false);
	        tex2d.filterMode = ANIM_TEX_FILTER;
	        tex2d.wrapMode = TextureWrapMode.Clamp;
	        Vector2[] uv2 = new Vector2[mesh.vertexCount];

			var texSize = new Vector2(1f / texWidth, 1f / texHeight);
			var halfTexOffset = 0.5f * texSize;
	        for (int i = 0; i < uv2.Length; i++)
				uv2[i] = new Vector2((float)i * texSize.x, 0f) + halfTexOffset;
	        mesh.uv2 = uv2;
	        for (int y = 0; y < verticesList.Count; y++) {
	            Vector3[] vertices = verticesList[y];
	            for (int x = 0; x < vertices.Length; x++) {
	                float posX = (vertices[x].x - offset.x) / scale.x;
	                float posY = (vertices[x].y - offset.y) / scale.y;
	                float posZ = (vertices[x].z - offset.z) / scale.z;

					float d = 1f / COLOR_DEPTH;
					var c1 = new Color(
						Mathf.Floor(posX * COLOR_DEPTH) * d, 
						Mathf.Floor(posY * COLOR_DEPTH) * d, 
						Mathf.Floor(posZ * COLOR_DEPTH) * d);
	                tex2d.SetPixel(x, y, c1);

					var c2 = new Color((posX - c1.r) * COLOR_DEPTH, (posY - c1.g) * COLOR_DEPTH, (posZ - c1.b) * COLOR_DEPTH);
					tex2d.SetPixel(x, y + (texHeight >> 1), c2);
	            }
	        }
	        tex2d.Apply();

	        AssetDatabase.CreateFolder("Assets", "AnimationTex_" + selection.name);
	        AssetDatabase.SaveAssets();
	        AssetDatabase.Refresh();

	        yield return 0;

	        string folderPath = "Assets/" + "AnimationTex_" + selection.name;

	        //AssetDatabase.CreateAsset(tex2d, folderPath + "/" + selection.name + "Tex.asset");
			var pngPath = folderPath + "/" + selection.name + ".png";
			File.WriteAllBytes(pngPath, tex2d.EncodeToPNG());
			AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);
			var pngImporter = (TextureImporter) AssetImporter.GetAtPath(pngPath);
			var pngSettings = new TextureImporterSettings();
			pngImporter.ReadTextureSettings(pngSettings);
			pngSettings.filterMode = ANIM_TEX_FILTER;
			pngSettings.mipmapEnabled = false;
			pngSettings.linearTexture = true;
			pngSettings.maxTextureSize = Mathf.Max(texWidth, texHeight);
			pngSettings.wrapMode = TextureWrapMode.Clamp;
			pngSettings.textureFormat = TextureImporterFormat.RGB24;
			pngImporter.SetTextureSettings(pngSettings);
			AssetDatabase.WriteImportSettingsIfDirty(pngPath);
			AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);

	        Material mat = new Material(Shader.Find("VertexAnim/oneshot"));
			mat.mainTexture = skinnedMesh.sharedMaterial.mainTexture;
			//mat.SetTexture (SHADER_ANIM_TEX, tex2d);
			mat.SetTexture("_AnimTex", (Texture2D) AssetDatabase.LoadAssetAtPath(pngPath, typeof(Texture2D)));
			mat.SetVector (SHADER_SCALE, scale);
			mat.SetVector (SHADER_OFFSET, offset);
			mat.SetVector (SHADER_ANIM_END, new Vector4 (state.length, verticesList.Count - 1, 0f, 0f));
			mat.SetFloat (SHADER_FPS, FPS);

	        AssetDatabase.CreateAsset(mat, folderPath + "/" + selection.name + "Mat.mat");
	        AssetDatabase.CreateAsset(mesh, folderPath + "/" + selection.name + "Mesh.asset");
	        AssetDatabase.SaveAssets();
	        AssetDatabase.Refresh();
			
			GameObject go = new GameObject(selection.name);
			//go.transform.rotation = skinnedMesh.transform.rotation;
			//go.transform.position = skinnedMesh.transform.position;
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
	}
}