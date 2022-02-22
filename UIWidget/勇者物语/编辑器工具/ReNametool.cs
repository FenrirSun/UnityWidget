using UnityEngine;
using System.Collections;
using UnityEditor;
public class ReNametool : EditorWindow {
    private string replaceStr = string.Empty;
    private string newStr = string.Empty;
    [MenuItem("TNN/重命名")]
    public static void OpenReName()
    {
        GetWindow<ReNametool>();
    }
    public void OnGUI()
    {
        replaceStr = EditorGUILayout.TextField("替换的字符串", replaceStr);
        newStr = EditorGUILayout.TextField("新的字符串", newStr);
        if(GUILayout.Button("替换"))
        {
            if(string.IsNullOrEmpty(replaceStr))
            {
                //int index = 0;
                //foreach (var it in UnityEditor.Selection.objects)
                //{
                //    AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(it), AssetDatabase.GetAssetPath(it).Replace(it.name, newStr + "_" + index.ToString()));
                //    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(it));
                //    index++;
                //}
            }
            else
            {
                foreach (var it in UnityEditor.Selection.objects)
                {
                    //AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(it), AssetDatabase.GetAssetPath(it).Replace(replaceStr, newStr));

                    AssetDatabase.CopyAsset( AssetDatabase.GetAssetPath(it), AssetDatabase.GetAssetPath(it).Replace(replaceStr, newStr));
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(it));
                }
            }

        }
    }
}
