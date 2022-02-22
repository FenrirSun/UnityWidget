using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using Code.External.Editor;
using Code.External.Engine;
using System.Text;
public static class AssetUtility
{
    public static void CreateAsset<T>() where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (string.IsNullOrEmpty(path))
        {
            path = "Assets";
        }
        else if (!string.IsNullOrEmpty(Path.GetExtension(path)))
        {
            path = path.Replace(Path.GetFileName(path), "");
        }

        string name = typeof(T).Name.ToString();
        if (name.EndsWith("Asset"))
            name = name.Substring(0, name.Length - 5);

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + name + ".asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }

    [MenuItem("Assets/Copy Resources Path")]
    public static void CopyResourcesPath()
    {
        var obj = Selection.activeObject;
        string assetPath = AssetDatabase.GetAssetOrScenePath(obj);
        string resourcesPath = assetPath.Substring("assets/resources/".Length);

        string fileName = resourcesPath.LastSubstringStartsOfAny('/', '\\');
        if (!string.IsNullOrEmpty(fileName) && fileName.LastIndexOf('.') >= 0)
        {
            fileName = fileName.Substring(0, fileName.LastIndexOf('.'));

            resourcesPath = fileName;
        }

        EditorGUIUtility.systemCopyBuffer = resourcesPath;
    }

    [MenuItem("Assets/Copy Resources Path", validate = true)]
    public static bool CopyResourcesPathValidate()
    {
        var obj = Selection.activeObject;
        if (obj == null)
            return false;

        string assetPath = AssetDatabase.GetAssetOrScenePath(obj);
        if (string.IsNullOrEmpty(assetPath))
            return false;
        if (!assetPath.StartsWith("assets/resources/", StringComparison.InvariantCultureIgnoreCase))
            return false;
        return true;
    }

    [MenuItem("Assets/Copy Select Path")]
    public static void CopySelectPath()
    {
        EditorGUIUtility.systemCopyBuffer = string.Empty;
        StringBuilder resourcesPath = new StringBuilder();

        foreach (UnityEngine.Object it in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets))
        {
            resourcesPath.Append(AssetDatabase.GetAssetPath(it));
            resourcesPath.Append("\n");
        }


        EditorGUIUtility.systemCopyBuffer = resourcesPath.ToString();
    }
}