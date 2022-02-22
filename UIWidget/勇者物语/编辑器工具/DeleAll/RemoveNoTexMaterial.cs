using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
public class RemoveNoTexMaterial : MonoBehaviour {
    [MenuItem("TNN/删除无贴图材质球")]
    static void RemoveNoTex()
    {
        Dictionary<string, string> remove = new Dictionary<string, string>();
        int index = 0;
        foreach(var it in Selection.objects)
        {
            index++;
            EditorUtility.DisplayProgressBar("提取所选资源中无贴图的材质球", string.Format("{0}/{1}", index, Selection.objects.Length), (float)index / (float)Selection.objects.Length);
            if(it is Material)
            {
                //if ((((Material)it).mainTexture == null && !((Material)it).shader.name.Contains("MLand2")) || ((Material)it).shader == null)
                //{
                //    if (!remove.ContainsKey(AssetDatabase.GetAssetPath(it)))
                //    {
                //        remove.Add(AssetDatabase.GetAssetPath(it), AssetDatabase.GetAssetPath(it));
                //    }
                //}

                if (((Material)it).shader == null)
                {
                    if (!remove.ContainsKey(AssetDatabase.GetAssetPath(it)))
                    {
                        remove.Add(AssetDatabase.GetAssetPath(it), AssetDatabase.GetAssetPath(it));
                    }
                }

                if(((Material)it).mainTexture != null)
                {
                    continue;
                }

                bool is_has_texture = false;
                foreach(var tex in EditorUtility.CollectDependencies(new Object[]{it}))
                {
                    if(tex is Texture)
                    {
                        is_has_texture = true;
                        break;
                    }
                    else if (tex is Texture2D)
                    {
                        is_has_texture = true;
                        break;
                    }
                    else if (tex is Texture3D)
                    {
                        is_has_texture = true;
                        break;
                    }
                }
                if(!is_has_texture)
                {
                    if (!remove.ContainsKey(AssetDatabase.GetAssetPath(it))) 
                    {
                        remove.Add(AssetDatabase.GetAssetPath(it), AssetDatabase.GetAssetPath(it));
                    }
                }
            }
        }
        index = 0;
        foreach (var it in remove)
        {
            index++;
            EditorUtility.DisplayProgressBar("提取所选资源中无贴图的材质球", string.Format("{0}/{1}", index, remove.Count), (float)index / (float)remove.Count);

            System.IO.File.Delete(it.Value);

            Debug.LogError(it.Value);
        }

        EditorUtility.ClearProgressBar();
    }
}
