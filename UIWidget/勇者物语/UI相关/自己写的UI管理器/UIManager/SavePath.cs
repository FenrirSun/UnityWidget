using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

public class SavePath 
{
    public static string prefabSavePath = "Assets/Resources/UIPrefab";
    public static string pathAssetName = "PrefabPathAsset";

    [MenuItem("CustomTool/Export Prefab Path")]
    public static void Execute()
    {
         if (!Directory.Exists(prefabSavePath))
         {
             Directory.CreateDirectory(prefabSavePath);
         }

        PrefabSaveModle pathAsset = ScriptableObject.CreateInstance<PrefabSaveModle>();
        pathAsset.windowPath = new List<PrefabSaveModle.PrefabPath>();
        UnityEngine.Object[] objs = Resources.LoadAll("UIPrefab");
        foreach (UnityEngine.Object obj in objs)
        {
            string assetPath = AssetDatabase.GetAssetOrScenePath(obj);
            if (!assetPath.Contains(".prefab"))
                continue;
 
            assetPath = assetPath.Substring(17, assetPath.Length - 24);
            PrefabSaveModle.PrefabPath path = new PrefabSaveModle.PrefabPath()
            {
                name = obj.name,
                path = assetPath
            };
            pathAsset.windowPath.Add(path);
        }
 
        string finalPath = prefabSavePath + "/" + pathAssetName + ".asset";
        AssetDatabase.CreateAsset(pathAsset, finalPath);
        AssetDatabase.Refresh();

    }
}
