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
            var vtex = new VertexTex (sampler);

            var mesh = sampler.CombinedMesh;
            //mesh.uv2 = vtex.uv2;
            mesh.bounds = vtex.Bounds ();
			
			var folderPath = DIR_ASSETS + "/" + DIR_ROOT;
			if (!Directory.Exists(folderPath))
				AssetDatabase.CreateFolder(DIR_ASSETS, DIR_ROOT);
			var guid = AssetDatabase.CreateFolder(folderPath, selection.name);
			folderPath = AssetDatabase.GUIDToAssetPath(guid);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			yield return 0;

			var posPngPath = folderPath + "/" + selection.name + ".png";
			var normPngPath = folderPath + "/" + selection.name + "_normal.png";
			var posTex = Save (vtex.positionTex, posPngPath);
			var normTex = Save (vtex.normalTex, normPngPath);

			var renderer = selection.GetComponentInChildren<Renderer> ();
			Material mat = new Material(Shader.Find(ShaderConst.SHADER_NAME));
			if (renderer != null && renderer.sharedMaterial != null)
				mat.mainTexture = renderer.sharedMaterial.mainTexture;
			mat.SetTexture (ShaderConst.SHADER_ANIM_TEX, posTex);
            mat.SetVector (ShaderConst.SHADER_SCALE, vtex.scale);
            mat.SetVector (ShaderConst.SHADER_OFFSET, vtex.offset);
            mat.SetVector (ShaderConst.SHADER_ANIM_END, new Vector4 (sampler.Length, vtex.verticesList.Count - 1, 0f, 0f));
            mat.SetFloat (ShaderConst.SHADER_FPS, FPS);
			mat.SetTexture (ShaderConst.SHADER_NORM_TEX, normTex);

			AssetDatabase.CreateAsset(mat, folderPath + "/" + selection.name + "Mat.mat");
			AssetDatabase.CreateAsset(mesh, folderPath + "/" + selection.name + "Mesh.asset");
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			
			GameObject go = new GameObject(selection.name);
			go.AddComponent<MeshRenderer>().sharedMaterial = mat;
			go.AddComponent<MeshFilter>().sharedMesh = mesh;
			PrefabUtility.CreatePrefab(folderPath + "/" + selection.name + ".prefab", go);
		}

		static Texture2D Save (Texture2D tex, string pngPath) {
			File.WriteAllBytes (pngPath, tex.EncodeToPNG ());
			AssetDatabase.ImportAsset (pngPath, ImportAssetOptions.ForceUpdate);
			var pngImporter = (TextureImporter)AssetImporter.GetAtPath (pngPath);
			var pngSettings = new TextureImporterSettings ();
			pngImporter.ReadTextureSettings (pngSettings);
			pngSettings.filterMode = ANIM_TEX_FILTER;
			pngSettings.mipmapEnabled = false;
			pngSettings.linearTexture = true;
			pngSettings.maxTextureSize = Mathf.Max (tex.width, tex.height);
			pngSettings.wrapMode = TextureWrapMode.Clamp;
			pngSettings.textureFormat = TextureImporterFormat.RGB24;
			pngImporter.SetTextureSettings (pngSettings);
			AssetDatabase.WriteImportSettingsIfDirty (pngPath);
			AssetDatabase.ImportAsset (pngPath, ImportAssetOptions.ForceUpdate);
			return (Texture2D)AssetDatabase.LoadAssetAtPath (pngPath, typeof(Texture2D));
		}
	}
}