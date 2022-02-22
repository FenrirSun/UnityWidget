using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;

public class MakeImagePrefab : MonoBehaviour 
{
    [MenuItem("CustomTool/Make Image Prefab")]
    static void MakeAtlas()
    {
        string targetDir = Application.dataPath + "/Resources/UI";

        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        DirectoryInfo fromDirInfo = new DirectoryInfo(Application.dataPath + "/Image");

        foreach (DirectoryInfo dirInfo in fromDirInfo.GetDirectories())
        {
            MakeOnePrefab(dirInfo, fromDirInfo, targetDir);
        }
    }

    static void MakeOnePrefab(DirectoryInfo dirInfo, DirectoryInfo fromDirInfo, string targetDir)
    {
        string fieldName = dirInfo.Name;
        FileInfo[] allPngFiles = null;
        bool hasFindDynSpriteField = false;

        // 遍历子文件夹找到Dynamic文件夹,只有文件夹内的sprite才会被保存
        foreach (DirectoryInfo childDir in dirInfo.GetDirectories())
        {
            if (childDir.Name == "Dynamic")
            {
                allPngFiles = childDir.GetFiles("*.png", SearchOption.AllDirectories);
                hasFindDynSpriteField = true;
                break;
            }
        }

        if (hasFindDynSpriteField)
        {
            if (allPngFiles.Length <= 0)
            {
                string shortPath = fromDirInfo.FullName.Substring(fromDirInfo.FullName.IndexOf("Assets"));
                Debug.LogWarning(string.Format("There is no sprite where path is \"{0}/{1}/Dynamic\".Do you forget to add needed sprite there?", shortPath, fieldName));
            }
            else
            {
                GameObject go = new GameObject(fieldName);
                AtlasMap am = go.AddComponent<AtlasMap>();

                // 如果“Resources/UI”下没有和“Image对应的文件夹”则创建一个//
                string prefabParentFieldPath = string.Format("{0}/{1}", targetDir, fieldName);
                if (!Directory.Exists(prefabParentFieldPath))
                {
                    Directory.CreateDirectory(prefabParentFieldPath);
                }

                // 将Sprite存入AtlasMap脚本中//
                foreach (FileInfo pngFile in allPngFiles)
                {
                    string assetPath = pngFile.FullName.Substring(pngFile.FullName.IndexOf("Assets"));
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                    if (sprite == null)
                    {
                        Debug.LogWarning(string.Format("It's not a sprite which path is \"{0}\", and don't move it to DynSprite field.", assetPath));
                        continue;
                    }
                    am.AddSprite(sprite);
                }

                // 在对应文件夹上生成预设//
                string prefabAllPath = string.Format("{0}/{1}DynSprite.prefab", prefabParentFieldPath, fieldName);
                string prefabPath = prefabAllPath.Substring(prefabAllPath.IndexOf("Assets"));
                PrefabUtility.CreatePrefab(prefabPath, go);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // 销毁go//
                GameObject.DestroyImmediate(go);
            }
        }
        else
        {
            string shortPath = fromDirInfo.FullName.Substring(fromDirInfo.FullName.IndexOf("Assets"));
            Debug.Log(string.Format("There is no DynSprite field where path is \"{0}/{1}\".Are you sure this UI needn't DynSprite?", shortPath, fieldName));
        }
    }
}

