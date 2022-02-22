using UnityEngine;
using System.Collections;
using UnityEditor;
public class CopyAtlas : EditorWindow {
    [MenuItem("TNN/Copy Atlas")]
    static void OpenWin()
    {
        GetWindow<CopyAtlas>();
    }

    public O_UIAtlas o_Atlas;

    void OnGUI() 
    {
        o_Atlas = EditorGUILayout.ObjectField(o_Atlas, typeof(O_UIAtlas)) as O_UIAtlas;
        if(GUILayout.Button("COPY"))
        {
            string path = AssetDatabase.GetAssetPath(o_Atlas);

            if (path.Contains(".prefab"))
            {
                path.Replace(".prefab", "_New.prefab");
            }

            GameObject go = new GameObject("New");
            UIAtlas atlas = go.AddComponent<UIAtlas>();
            atlas.spriteMaterial = o_Atlas.spriteMaterial;
            foreach(var it in o_Atlas.spriteList)
            {
                UISpriteData sprite = new UISpriteData();
                sprite.name = it.name;

                sprite.paddingLeft = (int)it.paddingLeft;
                sprite.paddingRight = (int)it.paddingRight;
                sprite.paddingTop = (int)it.paddingTop;
                sprite.paddingBottom = (int)it.paddingBottom;

                sprite.x = (int)it.outer.x;
                sprite.y = (int)it.outer.y;
                sprite.height = (int)it.outer.height;
                sprite.width = (int)it.outer.width;

                atlas.spriteList.Add(sprite);
            }

            GameObject atlasObj = PrefabUtility.CreatePrefab(GetNewName(path), go) as GameObject;

            DestroyImmediate(go);



            return;

            //path = GetNewName(path);

            //if (!string.IsNullOrEmpty(path))
            //{
            //    GameObject go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            //    string matPath = AssetDatabase.GetAssetPath(o_Atlas.spriteMaterial);

            //    // Try to load the material
            //    Material mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;

            //    // If the material doesn't exist, create it
            //    if (mat == null)
            //    {
            //        Shader shader = Shader.Find(NGUISettings.atlasPMA ? "Unlit/Premultiplied Colored" : "Unlit/Transparent Colored");
            //        mat = new Material(shader);

            //        // Save the material
            //        AssetDatabase.CreateAsset(mat, matPath);
            //        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            //        // Load the material so it's usable
            //        mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;
            //    }

            //    // Create a new prefab for the atlas
            //    Object prefab = (go != null) ? go : PrefabUtility.CreateEmptyPrefab(path);

            //    // Create a new game object for the atlas
            //    string atlasName = path.Replace(".prefab", "");
            //    atlasName = atlasName.Substring(path.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
            //    go = new GameObject(atlasName);
            //    go.AddComponent<UIAtlas>().spriteMaterial = mat;

            //    // Update the prefab
            //    PrefabUtility.ReplacePrefab(go, prefab);
            //    DestroyImmediate(go);
            //    AssetDatabase.SaveAssets();
            //    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            //}

        }
    }

    string GetNewName(string path)
    {
        int index = path.LastIndexOf('/');
        string name = path.Substring(index + 1);
        return path.Substring(0,index + 1) + "New_" + name;
    }
}
