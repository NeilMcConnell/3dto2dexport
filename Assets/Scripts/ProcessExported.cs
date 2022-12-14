using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public static class ProcessExported 
{
    [MenuItem("Neil/Set Clamping", false, 100)]
    public static void SetClamping()
    {
        var guids = AssetDatabase.FindAssets("t:Texture2d _R315", new string[] {@"Assets/Exported" });
        //List<string> paths = new List<string>();
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            //paths.Add(path);
            Debug.Log(path);

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.filterMode = FilterMode.Point;
            importer.textureType = TextureImporterType.Sprite;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.spriteImportMode = SpriteImportMode.Single;

            if (!path.Contains("_NormalMap"))
            {
                var normalMapPath = path.Replace(".tga", "_NormalMap.tga");
                Texture2D normalMapTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(normalMapPath);
                Assert.IsNotNull(normalMapTexture);
                importer.secondarySpriteTextures = new SecondarySpriteTexture[] {
                    new SecondarySpriteTexture() {
                         name = "_NormalMap",
                         texture = normalMapTexture
                    }
                };
            }

            importer.SaveAndReimport();
            //EditorUtility.SetDirty
       
            //var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            //Debug.Log(texture.name);
            //texture.cl
            //EditorUtility.SetDirty(texture);

        }

        //foreach (var path in paths)
        //{
        //    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        //}
    }


}
