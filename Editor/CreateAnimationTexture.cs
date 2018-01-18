using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace VertexAnimater {

	public class CreateAnimationTexture {
        [System.Flags]
        public enum CreationModeFlags { NONE = 0, NEW_MESH = 1 << 0, COMBINED_MESH = 1 << 1 }

		public const float FPS = 5f;
		public const FilterMode ANIM_TEX_FILTER = FilterMode.Bilinear;

		public const float DT = 1f / FPS;
		public const float COLOR_DEPTH = 255f;

		public const string DIR_ASSETS = "Assets";
		public const string DIR_ROOT = "AnimationTex";

        [MenuItem("Custom/Create/VertexAnimation (Reuse Mesh)")]
		public static void CreateVertexTexture() {
            CreateVertexTexture (CreationModeFlags.NONE);
        }
        [MenuItem("Custom/Create/VeretxAnimation")]
        public static void CreateVertexTextureWithNewMesh() {
            CreateVertexTexture (CreationModeFlags.NEW_MESH);
        }
        [MenuItem("Custom/Create/VeretxAnimation (Combined)")]
        public static void CreateVertexTextureWithCombinedMesh() {
            CreateVertexTexture (CreationModeFlags.NEW_MESH | CreationModeFlags.COMBINED_MESH);
        }
        public static void CreateVertexTexture(CreationModeFlags flags) {
			GameObject selection = Selection.activeGameObject;
			if (selection == null) {
				Debug.Log("No Active GameObject");
				return;
			}
			if (!EditorApplication.isPlaying)
				EditorApplication.isPlaying = true;

            selection.AddComponent<Dummy>().StartCoroutine(CreateVertexTexture(selection, flags));
		}

        public static IEnumerator CreateVertexTexture(GameObject selection, CreationModeFlags flags) {
            var sampler = ContainsAllFlags(flags, CreationModeFlags.COMBINED_MESH)
                ? (IMeshSampler)new CombinedMeshSampler(selection) : (IMeshSampler)new SingleMeshSampler (selection);
            var vtex = new VertexTex (sampler);
		
            var folderPath = DIR_ASSETS + "/" + DIR_ROOT;
            if (!Directory.Exists (folderPath))
                AssetDatabase.CreateFolder (DIR_ASSETS, DIR_ROOT);
            var guid = AssetDatabase.CreateFolder (folderPath, selection.name);
            folderPath = AssetDatabase.GUIDToAssetPath (guid);
            AssetDatabase.SaveAssets ();
            AssetDatabase.Refresh ();
            yield return 0;

            var posPngPath = folderPath + "/" + selection.name + ".png";
            var normPngPath = folderPath + "/" + selection.name + "_normal.png";
            var posTex = Save (vtex.positionTex, posPngPath);
            var normTex = Save (vtex.normalTex, normPngPath);

            var renderer = selection.GetComponentInChildren<Renderer> ();
            Material mat = new Material (Shader.Find (ShaderConst.SHADER_NAME));
            if (renderer != null && renderer.sharedMaterial != null)
                mat.mainTexture = renderer.sharedMaterial.mainTexture;
            mat.SetTexture (ShaderConst.SHADER_ANIM_TEX, posTex);
            mat.SetVector (ShaderConst.SHADER_SCALE, vtex.scale);
            mat.SetVector (ShaderConst.SHADER_OFFSET, vtex.offset);
            mat.SetVector (ShaderConst.SHADER_ANIM_END, new Vector4 (sampler.Length, vtex.verticesList.Count - 1, 0f, 0f));
            mat.SetFloat (ShaderConst.SHADER_FPS, FPS);
            mat.SetTexture (ShaderConst.SHADER_NORM_TEX, normTex);

            AssetDatabase.CreateAsset (mat, folderPath + "/" + selection.name + "Mat.mat");
            AssetDatabase.SaveAssets ();
            AssetDatabase.Refresh ();

            var smr = selection.GetComponentInChildren<SkinnedMeshRenderer> ();
            var mesh = (smr != null ? smr.sharedMesh : null);

            if (ContainsAllFlags(flags, CreationModeFlags.NEW_MESH)) {
                mesh = sampler.Output;
                mesh.bounds = vtex.Bounds ();
                AssetDatabase.CreateAsset (mesh, folderPath + "/" + selection.name + ".asset");
                AssetDatabase.SaveAssets ();
                AssetDatabase.Refresh ();
            }

            GameObject go = new GameObject (selection.name);
            go.AddComponent<MeshRenderer> ().sharedMaterial = mat;
            go.AddComponent<MeshFilter> ().sharedMesh = mesh;
            PrefabUtility.CreatePrefab (folderPath + "/" + selection.name + ".prefab", go);
		}

		static Texture2D Save (Texture2D tex, string pngPath) {
            #if UNITY_5_5_OR_NEWER
            File.WriteAllBytes (pngPath, tex.EncodeToPNG ());
            AssetDatabase.ImportAsset (pngPath, ImportAssetOptions.ForceUpdate);
            var pngImporter = (TextureImporter)AssetImporter.GetAtPath (pngPath);
            var pngSettings = new TextureImporterSettings ();
            pngImporter.ReadTextureSettings (pngSettings);
            pngSettings.filterMode = ANIM_TEX_FILTER;
            pngSettings.mipmapEnabled = false;
            pngSettings.sRGBTexture = false;
            pngSettings.wrapMode = TextureWrapMode.Clamp;
            pngImporter.SetTextureSettings (pngSettings);
            var platformSettings = pngImporter.GetDefaultPlatformTextureSettings ();
            platformSettings.format = TextureImporterFormat.RGB24;
            platformSettings.maxTextureSize = Mathf.Max (platformSettings.maxTextureSize, Mathf.Max (tex.width, tex.height));
            platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
            pngImporter.SetPlatformTextureSettings (platformSettings);
            AssetDatabase.WriteImportSettingsIfDirty (pngPath);
            AssetDatabase.ImportAsset (pngPath, ImportAssetOptions.ForceUpdate);

            #else
            File.WriteAllBytes (pngPath, tex.EncodeToPNG ());
            AssetDatabase.ImportAsset (pngPath, ImportAssetOptions.ForceUpdate);
            var pngImporter = (TextureImporter)AssetImporter.GetAtPath (pngPath);
            var pngSettings = new TextureImporterSettings ();
            pngImporter.ReadTextureSettings (pngSettings);
            pngSettings.filterMode = ANIM_TEX_FILTER;
            pngSettings.mipmapEnabled = false;
            pngSettings.linearTexture = true;
            pngSettings.wrapMode = TextureWrapMode.Clamp;
            pngImporter.SetTextureSettings (pngSettings);
            pngImporter.textureFormat = TextureImporterFormat.RGB24;
            pngImporter.maxTextureSize = Mathf.Max (pngImporter.maxTextureSize, Mathf.Max (tex.width, tex.height));
            pngImporter.SaveAndReimport();
            //AssetDatabase.WriteImportSettingsIfDirty (pngPath);
            //AssetDatabase.ImportAsset (pngPath, ImportAssetOptions.ForceUpdate);
            #endif

			return (Texture2D)AssetDatabase.LoadAssetAtPath (pngPath, typeof(Texture2D));
		}
        static bool ContainsAllFlags(CreationModeFlags flags, CreationModeFlags contains) {
            return (flags & contains) == contains;
        }
	}
}