using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 粒子系统优化管理器
/// </summary>
public class ParticleSystemTool : Editor
{
    [MenuItem("CheckAssets/Particle/限制粒子最大数量为10")]
    static void FormatModel()
    {
        if (Selection.objects.Length < 1)
        {
            Debug.LogError("请选一个/多个文件夹！");
            return;
        }

        string existPath = AssetDatabase.GetAssetPath(Selection.objects[0]);
        if (!Directory.Exists(existPath))
        {
            Debug.LogError("选中的不是文件夹或文件夹不存在");
            return;
        }

        AssetDatabase.StartAssetEditing();
        string[] filePath = TextureFormatTool.GetFilePath(Selection.objects);
        var assetsPath = AssetDatabase.FindAssets("t:GameObject", filePath);
        for (int i = 0; i < assetsPath.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(assetsPath[i]);
            GameObject prefabObj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            EditorUtility.DisplayProgressBar("format Model", path, (float)(i + 1) / assetsPath.Length);
            ParticleSystem[] particles = prefabObj.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem particle in particles)
            {
                if (particle.maxParticles > 10)
                    particle.maxParticles = 10;
            }

            //PrefabUtility.ConnectGameObjectToPrefab();
            Debug.Log("prefabObj : " + prefabObj.name);
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.StopAssetEditing();
        AssetDatabase.Refresh();
    }

    [MenuItem("CheckAssets/Model/模型检查")]
    static void FormatModel()
    {
        if (Selection.objects.Length < 1)
        {
            Debug.LogError("选一个/多个文件夹！");
            return;
        }

        string existPath = AssetDatabase.GetAssetPath(Selection.objects[0]);
        if (!Directory.Exists(existPath))
        {
            Debug.LogError("选一个 / 多个文件夹！");
            return;
        }

        AssetDatabase.StartAssetEditing();
        List<string> pathList = new List<string>(); //图片路径
        List<string> logList = new List<string>(); //打印信息
        string[] filePath = TextureFormatTool.GetFilePath(Selection.objects);
        var assetsPath = AssetDatabase.FindAssets("t:GameObject", filePath);
        for (int i = 0; i < assetsPath.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(assetsPath[i]);
            ModelImporter modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;
            if (!modelImporter)
                continue;
            EditorUtility.DisplayProgressBar("format Model", path, (float)(i + 1) / assetsPath.Length);
            //检查read/write选项
            if (modelImporter.isReadable)
            {
                logList.Add(",该文件read/write 为 true");
                pathList.Add(path);
            }

            //检查mesh压缩
            if (modelImporter.meshCompression != ModelImporterMeshCompression.High)
            {
                logList.Add(",可以优化，当前压缩类型为" + modelImporter.meshCompression);
                pathList.Add(path);
            }

            //检查带动画文件状态
            if (modelImporter.importAnimation)
            {
                if (!modelImporter.optimizeGameObjects)
                {
                    logList.Add("该文件optimizeGameObjects选项 为 false");
                    pathList.Add(path);
                }

                if (modelImporter.animationCompression == ModelImporterAnimationCompression.KeyframeReduction)
                {
                    logList.Add("该文件animationCompression类型为" + modelImporter.animationCompression);
                    pathList.Add(path);
                }
            }
            //检查法线/切线选项
            //if(modelImporter.importNormals == ModelImporterNormals.Import)
            //{
            //    logList.Add("该文件法线/切线选项为开启状态");
            //    pathList.Add(path);
            //}
        }

        TextureCheckEditorWindow.imageList = pathList;
        TextureCheckEditorWindow.errorList = logList;
        TextureCheckEditorWindow.ShowWindow();
        EditorUtility.ClearProgressBar();
        AssetDatabase.StopAssetEditing();
        AssetDatabase.Refresh();
    }
}