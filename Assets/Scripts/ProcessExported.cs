using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;



public class AssetEdittingScope : IDisposable
{
    public AssetEdittingScope() => AssetDatabase.StartAssetEditing();

    public void Dispose()
    {
        AssetDatabase.StopAssetEditing();
    }
}


public static class ProcessExported 
{
    [MenuItem("Neil/Set Clamping", false, 100)]
    public static void SetClamping()
    {

        using var holdImportsScope = new AssetEdittingScope();

        var guids = AssetDatabase.FindAssets("t:Texture2d _R315", new string[] {@"Assets/Exported" });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
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
        }
    }


}
