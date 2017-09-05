using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace VertexAnimater {

	public class PngFile {
        public const string EXTENSION = ".png";
        public const FilterMode ANIM_TEX_FILTER = FilterMode.Bilinear;

        public static string AppendExtension(string name) {
            return name + EXTENSION;
        }
        public static Texture2D Save (Texture2D tex, string pngPath) {
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
	}
}