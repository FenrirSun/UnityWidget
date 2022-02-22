using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
public class DeleteResources : MonoBehaviour {

    static List<string> paths = new List<string>();
    [MenuItem("TNN/删除资源")]
    static void DeletePath()
    {
        paths.Clear();



        ReadPath();

        //if (EditorUtility.DisplayDialog("注意", "为了以防万一误删资源，该功能先屏蔽！！！，如果要删除，注释掉DeleteResources.cs文件", "确定"))
        //{
        //    return;
        //}
        //return;

        if (EditorUtility.DisplayDialog("注意", "为了以防万一误删资源，确定要删除么！！！", "确定","取消"))
        {
            int index = 1;
            foreach (var it in paths)
            {
                if (System.IO.Directory.Exists(Application.dataPath + it.Substring(6, it.Length - 6)))
                {
                    System.IO.Directory.Delete(Application.dataPath + it.Substring(6, it.Length - 6), true);
                    EditorUtility.DisplayProgressBar("删除资源中", string.Format("{0}/{1}", index, paths.Count), (float)index / (float)paths.Count);
                }
                if (File.Exists(Application.dataPath + it.Substring(6, it.Length - 6)))
                {
                    File.Delete(it);
                    EditorUtility.DisplayProgressBar("删除资源中", string.Format("{0}/{1}", index, paths.Count), (float)index / (float)paths.Count);
                }
                index++;
                Debug.LogError(Application.dataPath + it.Substring(6, it.Length - 6));

            }
            EditorUtility.ClearProgressBar();
            paths.Clear();
            AssetDatabase.Refresh();
        }

        
    }

    public static void ReadPath()
    {
        string filePath = Application.dataPath + "/Editor/DeleAll/Delete.txt";
        using (var stream = new StreamReader(filePath))
        {
            while (!stream.EndOfStream)
            {
                string tempPath = stream.ReadLine();
                if (!string.IsNullOrEmpty(tempPath) && !tempPath.Contains("/*"))
                {
                    paths.Add(tempPath);
                }
            }
        }
    }
}
