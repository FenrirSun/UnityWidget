using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System.Text;
public class GetSceneDependencies : MonoBehaviour {

    [MenuItem("TNN/获得所有场景用到的资源贴图")]
    static void GetAllSceneDependencies()
    {
        int sceneIndex = 0;
        StringBuilder text = new StringBuilder();
        Dictionary<string, string> dependenciesTexs = new Dictionary<string, string>();
        Dictionary<string, string> dependenciesMat = new Dictionary<string, string>();
        //遍历所有游戏场景
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            sceneIndex++;
            if (EditorUtility.DisplayCancelableProgressBar("索引场景", string.Format("当前检索场景数：{0}/总场景数：{1}", sceneIndex, EditorBuildSettings.scenes.Length), (float)sceneIndex / (float)EditorBuildSettings.scenes.Length))
            {
                break;
            }
            else
            {

            }

            if (scene.enabled)
            {
                //打开场景
                EditorApplication.OpenScene(scene.path);

                Renderer[] renderers = (Renderer[])Resources.FindObjectsOfTypeAll(typeof(Renderer));
                int indexRen = 0;
                foreach (Renderer ren in renderers)
                {
                    indexRen++;
                    if (EditorUtility.DisplayCancelableProgressBar(string.Format("当前检索场景数：{0}/总场景数：{1}", sceneIndex, EditorBuildSettings.scenes.Length), string.Format("({0}/{1})", indexRen, renderers.Length), (float)indexRen / (float)renderers.Length))
                    {
                        break;
                    }
                    else
                    {
                        //foreach (var tex in EditorUtility.CollectDependencies(ren.materials))
                        //{
                        //    if (tex is Texture)
                        //    {
                        //        if (!dependenciesTexs.ContainsKey(AssetDatabase.GetAssetPath(tex)))
                        //        {
                        //            dependenciesTexs.Add(AssetDatabase.GetAssetPath(tex), AssetDatabase.GetAssetPath(tex));
                        //        }
                        //    }
                        //    else if (tex is Texture2D)
                        //    {
                        //        if (!dependenciesTexs.ContainsKey(AssetDatabase.GetAssetPath(tex)))
                        //        {
                        //            dependenciesTexs.Add(AssetDatabase.GetAssetPath(tex), AssetDatabase.GetAssetPath(tex));
                        //        }
                        //    }
                        //    else if (tex is Texture3D)
                        //    {
                        //        if (!dependenciesTexs.ContainsKey(AssetDatabase.GetAssetPath(tex)))
                        //        {
                        //            dependenciesTexs.Add(AssetDatabase.GetAssetPath(tex), AssetDatabase.GetAssetPath(tex));
                        //        }
                        //    }
                        //}

                        foreach (var tex in EditorUtility.CollectDependencies(ren.sharedMaterials))
                        {
                            if (tex is Texture)
                            {
                                if (!dependenciesTexs.ContainsKey(AssetDatabase.GetAssetPath(tex)))
                                {
                                    dependenciesTexs.Add(AssetDatabase.GetAssetPath(tex), AssetDatabase.GetAssetPath(tex));
                                }
                            }
                            else if (tex is Texture2D)
                            {
                                if (!dependenciesTexs.ContainsKey(AssetDatabase.GetAssetPath(tex)))
                                {
                                    dependenciesTexs.Add(AssetDatabase.GetAssetPath(tex), AssetDatabase.GetAssetPath(tex));
                                }
                            }
                            else if (tex is Texture3D)
                            {
                                if (!dependenciesTexs.ContainsKey(AssetDatabase.GetAssetPath(tex)))
                                {
                                    dependenciesTexs.Add(AssetDatabase.GetAssetPath(tex), AssetDatabase.GetAssetPath(tex));
                                }
                            }
                        }

                        foreach (var it in ren.sharedMaterials)
                        {
                            if (!dependenciesMat.ContainsKey(AssetDatabase.GetAssetPath(it)))
                            {
                                dependenciesMat.Add(AssetDatabase.GetAssetPath(it), AssetDatabase.GetAssetPath(it));
                            }
                        }

                    }
                }
            }
        }
        EditorUtility.ClearProgressBar();

        foreach (var it in dependenciesTexs) 
        {
            text.Append(it.Key);
            text.Append("\n");
        }
        WriteToTxt(text.ToString(),Application.dataPath + "/Editor/DeleAll/SceneTexture.txt");

        StringBuilder matText = new StringBuilder();
        foreach (var it in dependenciesMat)
        {
            matText.Append(it.Key);
            matText.Append("\n");
        }
        WriteToTxt(matText.ToString(), Application.dataPath + "/Editor/DeleAll/SceneMaterial.txt");

        AssetDatabase.Refresh();
    }

    static void WriteToTxt(string text, string filePath)
    {

        

        //if (!File.Exists(filePath))
        //{
        //    File.Create(filePath);
        //}
        //else
        //{
        //    File.Delete(filePath);
        //    File.Create(filePath);
        //}
        FileStream fs = new FileStream(filePath, FileMode.Create);

        StreamWriter write = new StreamWriter(fs);

        write.Write(text);
        write.Flush();
        write.Close();
        fs.Close();
    }
    [MenuItem("TNN/获得文件夹下的资源贴图")]
    static void GetFolderTextures()
    {
        StringBuilder text = new StringBuilder();
        Dictionary<string, string> dependenciesTexs = new Dictionary<string, string>();
        Dictionary<string, string> dependenciesMat = new Dictionary<string, string>();

        int indexCount = 0;
        EditorUtility.DisplayProgressBar("搜索中", "正在获取，请稍等！！！", (float)indexCount / (float)10000);

        Object[] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        foreach (UnityEngine.Object Obj in objs)
        {
            indexCount++;
            //if (EditorUtility.DisplayCancelableProgressBar("搜索中",string.Format("({0}/{1})", indexCount, objs.Length),(float)indexCount / (float)objs.Length))
            //{
            //    break;
            //}
            //else
            {
                EditorUtility.DisplayProgressBar("搜索中", string.Format("({0}/{1})", indexCount, objs.Length), (float)indexCount / (float)objs.Length);
                if (Obj is Texture)
                {
                    if (!dependenciesTexs.ContainsKey(AssetDatabase.GetAssetPath(Obj)))
                    {
                        dependenciesTexs.Add(AssetDatabase.GetAssetPath(Obj), AssetDatabase.GetAssetPath(Obj));
                    }
                }
                else if (Obj is Texture2D)
                {
                    if (!dependenciesTexs.ContainsKey(AssetDatabase.GetAssetPath(Obj)))
                    {
                        dependenciesTexs.Add(AssetDatabase.GetAssetPath(Obj), AssetDatabase.GetAssetPath(Obj));
                    }
                }
                else if (Obj is Texture3D)
                {
                    if (!dependenciesTexs.ContainsKey(AssetDatabase.GetAssetPath(Obj)))
                    {
                        dependenciesTexs.Add(AssetDatabase.GetAssetPath(Obj), AssetDatabase.GetAssetPath(Obj));
                    }
                }
                else if(Obj is Material)
                {
                    if (!dependenciesMat.ContainsKey(AssetDatabase.GetAssetPath(Obj)))
                    {
                        dependenciesMat.Add(AssetDatabase.GetAssetPath(Obj), AssetDatabase.GetAssetPath(Obj));
                    }
                }
            }
        }

        
        foreach (var it in dependenciesTexs)
        {
            text.Append(it.Key);
            text.Append("\n");
        }
        EditorUtility.ClearProgressBar();

        StringBuilder matText = new StringBuilder();
        foreach (var it in dependenciesMat)
        {
            matText.Append(it.Key);
            matText.Append("\n");
        }

        WriteToTxt(text.ToString(), Application.dataPath + "/Editor/DeleAll/FolderTexture.txt");
        WriteToTxt(matText.ToString(), Application.dataPath + "/Editor/DeleAll/FolderMaterial.txt");

        AssetDatabase.Refresh();
    }
    [MenuItem("TNN/获得文件夹下的Prefab使用的贴图和材质球")]
    static void GetFolderPrefabDependencies()
    {
        Dictionary<string, string> dependenciesTexs = new Dictionary<string, string>();
        Dictionary<string, string> dependenciesMat = new Dictionary<string, string>();
        Dictionary<string, string> dependenciesAll = new Dictionary<string, string>();

        int indexCount = 0;
        EditorUtility.DisplayProgressBar("搜索中", "正在获取，请稍等！！！", (float)indexCount / (float)10000);

        Object[] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);

        foreach(var obj in objs)
        {
            indexCount++;
            EditorUtility.DisplayProgressBar("搜索中", string.Format("({0}/{1})", indexCount, objs.Length), (float)indexCount / (float)objs.Length);

            if (AssetDatabase.GetAssetPath(obj).EndsWith(".prefab"))
            {
                foreach (var dep in EditorUtility.CollectDependencies(new Object[] { obj }))
                {
                    if (dep != null)
                    {
                        if (!dependenciesAll.ContainsKey(AssetDatabase.GetAssetPath(dep))
                            //&& !AssetDatabase.GetAssetPath(dep).StartsWith("Assets/Resources")
                            && !AssetDatabase.GetAssetPath(dep).EndsWith(".cs"))
                        {
                            dependenciesAll.Add(AssetDatabase.GetAssetPath(dep), AssetDatabase.GetAssetPath(dep));
                        }
                    }
                    if (dep is Texture) 
                    {
                        if (!dependenciesTexs.ContainsKey(AssetDatabase.GetAssetPath(dep)))
                        {
                            dependenciesTexs.Add(AssetDatabase.GetAssetPath(dep), AssetDatabase.GetAssetPath(dep));
                        }
                    }
                    else if (dep is Texture2D)
                    {
                        if (!dependenciesTexs.ContainsKey(AssetDatabase.GetAssetPath(dep)))
                        {
                            dependenciesTexs.Add(AssetDatabase.GetAssetPath(dep), AssetDatabase.GetAssetPath(dep));
                        }
                    }
                    else if (dep is Texture3D)
                    {
                        if (!dependenciesTexs.ContainsKey(AssetDatabase.GetAssetPath(dep)))
                        {
                            dependenciesTexs.Add(AssetDatabase.GetAssetPath(dep), AssetDatabase.GetAssetPath(dep));
                        }
                    }
                    else if (dep is Material)
                    {
                        if (!dependenciesMat.ContainsKey(AssetDatabase.GetAssetPath(dep)))
                        {
                            dependenciesMat.Add(AssetDatabase.GetAssetPath(dep), AssetDatabase.GetAssetPath(dep));
                        }
                    }
                }
            }
            else
            {
                continue;
            }
        }

        StringBuilder texText = new StringBuilder();
        StringBuilder matext = new StringBuilder();
        StringBuilder allext = new StringBuilder();
        EditorUtility.ClearProgressBar();

        foreach (var it in dependenciesTexs)
        {
            texText.Append(it.Key);
            texText.Append("\n");
        }
        foreach (var it in dependenciesMat)
        {
            matext.Append(it.Key);
            matext.Append("\n");
        }
        foreach (var it in dependenciesAll)
        {
            allext.Append(it.Key);
            allext.Append("\n");
        }
        WriteToTxt(texText.ToString(), Application.dataPath + "/Editor/DeleAll/FolderPrefabDepTexture.txt");

        WriteToTxt(matext.ToString(), Application.dataPath + "/Editor/DeleAll/FolderPrefabDepMaterial.txt");

        WriteToTxt(allext.ToString(), Application.dataPath + "/Editor/DeleAll/FolderPrefabDepAll.txt");
    }

    [MenuItem("TNN/未在Prefab中使用的材质球")]
    static void GetDependenciesNull()
    {
        int index = 0;
        foreach(var obj in Selection.objects)
        {
            index++;
            EditorUtility.DisplayProgressBar("搜索中", string.Format("({0}/{1})", index, Selection.objects.Length), (float)index / (float)Selection.objects.Length);

            bool isRef = false;
            foreach (var dep in EditorUtility.CollectDependencies(new Object[] { obj }))
            {
                
                if (AssetDatabase.GetAssetPath(dep).EndsWith(".prefab") || AssetDatabase.GetAssetPath(dep).EndsWith(".fbx"))
                {
                    isRef = true;
                    break;
                }
            }
            if (!isRef) 
            {
                Debug.LogError(AssetDatabase.GetAssetPath(obj));
            }
        }

        EditorUtility.ClearProgressBar();
    }
    [MenuItem("TNN/获得旧UIPrefab资源")]
    static void GetOldUI()
    {
        Dictionary<string, string> dependenciesTexs = new Dictionary<string, string>();
        Dictionary<string, string> dependenciesMat = new Dictionary<string, string>();
        Dictionary<string, string> dependenciesAll = new Dictionary<string, string>();

        int indexCount = 0;
        EditorUtility.DisplayProgressBar("搜索中", "正在获取，请稍等！！！", (float)indexCount / (float)10000);

        Object[] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);

        foreach (var obj in objs)
        {
            indexCount++;
            EditorUtility.DisplayProgressBar("搜索中", string.Format("({0}/{1})", indexCount, objs.Length), (float)indexCount / (float)objs.Length);

            if (AssetDatabase.GetAssetPath(obj).EndsWith(".prefab"))
            {
                foreach (var dep in EditorUtility.CollectDependencies(new Object[] { obj }))
                {
                    if (dep != null)
                    {
                        if (!dependenciesAll.ContainsKey(AssetDatabase.GetAssetPath(dep))
                            //&& !AssetDatabase.GetAssetPath(dep).StartsWith("Assets/Resources")
                            && !AssetDatabase.GetAssetPath(dep).EndsWith(".cs")
                            && AssetDatabase.GetAssetPath(dep).StartsWith("Assets/Resources/UI/"))
                        {
                            dependenciesAll.Add(AssetDatabase.GetAssetPath(dep), AssetDatabase.GetAssetPath(dep));
                        }
                    }
                    if (dep is Texture)
                    {
                        if (!dependenciesTexs.ContainsKey(AssetDatabase.GetAssetPath(dep)))
                        {
                            dependenciesTexs.Add(AssetDatabase.GetAssetPath(dep), AssetDatabase.GetAssetPath(dep));
                        }
                    }
                    else if (dep is Texture2D)
                    {
                        if (!dependenciesTexs.ContainsKey(AssetDatabase.GetAssetPath(dep)))
                        {
                            dependenciesTexs.Add(AssetDatabase.GetAssetPath(dep), AssetDatabase.GetAssetPath(dep));
                        }
                    }
                    else if (dep is Texture3D)
                    {
                        if (!dependenciesTexs.ContainsKey(AssetDatabase.GetAssetPath(dep)))
                        {
                            dependenciesTexs.Add(AssetDatabase.GetAssetPath(dep), AssetDatabase.GetAssetPath(dep));
                        }
                    }
                    else if (dep is Material)
                    {
                        if (!dependenciesMat.ContainsKey(AssetDatabase.GetAssetPath(dep)))
                        {
                            dependenciesMat.Add(AssetDatabase.GetAssetPath(dep), AssetDatabase.GetAssetPath(dep));
                        }
                    }
                }
            }
            else
            {
                continue;
            }
        }

        StringBuilder texText = new StringBuilder();
        StringBuilder matext = new StringBuilder();
        StringBuilder allext = new StringBuilder();
        EditorUtility.ClearProgressBar();

        foreach (var it in dependenciesTexs)
        {
            texText.Append(it.Key);
            texText.Append("\n");
        }
        foreach (var it in dependenciesMat)
        {
            matext.Append(it.Key);
            matext.Append("\n");
        }
        foreach (var it in dependenciesAll)
        {
            allext.Append(it.Key);
            allext.Append("\n");
        }
        WriteToTxt(texText.ToString(), Application.dataPath + "/Editor/DeleAll/FolderPrefabDepTexture.txt");

        WriteToTxt(matext.ToString(), Application.dataPath + "/Editor/DeleAll/FolderPrefabDepMaterial.txt");

        WriteToTxt(allext.ToString(), Application.dataPath + "/Editor/DeleAll/FolderPrefabDepAll.txt");
    }

    [MenuItem("TNN/获得Prefab资源")]
    static void GetPrefabAModel()
    {
        Dictionary<string, string> dependenciesAll = new Dictionary<string, string>();

        int indexCount = 0;
        EditorUtility.DisplayProgressBar("搜索中", "正在获取，请稍等！！！", (float)indexCount / (float)10000);

        Object[] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);

        foreach (var obj in objs)
        {
            indexCount++;
            EditorUtility.DisplayProgressBar("搜索中", string.Format("({0}/{1})", indexCount, objs.Length), (float)indexCount / (float)objs.Length);

            if (AssetDatabase.GetAssetPath(obj).EndsWith(".prefab") 
                || AssetDatabase.GetAssetPath(obj).EndsWith(".fbx")
                || AssetDatabase.GetAssetPath(obj).EndsWith(".FBX"))
            {
                if (!dependenciesAll.ContainsKey(AssetDatabase.GetAssetPath(obj)))
                {
                    dependenciesAll.Add(AssetDatabase.GetAssetPath(obj), AssetDatabase.GetAssetPath(obj));
                }
            }
        }

        EditorUtility.ClearProgressBar();
        StringBuilder text = new StringBuilder();
        foreach (var it in dependenciesAll)
        {
            text.Append(it.Key);
            text.Append("\n");
        }
        WriteToTxt(text.ToString(), Application.dataPath + "/Editor/DeleAll/FolderPrefabAll.txt");

    }
}
