using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
public class CreateAssetBundle : Editor {
    [MenuItem("TNN/Asset Bundle Scene/Bundle Window")]
    static void CreateSceneBundle()
    {
        foreach (Object o in Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets))
        {
            string path = AssetDatabase.GetAssetPath(o);

            //BuildPipeline.BuildPlayer(new string[] { path }, Application.streamingAssetsPath + "/" + ".unity3d", BuildTarget.Android, BuildOptions.BuildAdditionalStreamedScenes);
            if (path.Contains(".unity"))
            {
                CreateBundle(new string[] { path }, Application.streamingAssetsPath + "/Windows32/" , o.name + ".unity3d", BuildTarget.StandaloneWindows64);
            }
            else
            {
                CreateBundle(o, Application.streamingAssetsPath + "/Windows32/" , o.name + ".unity3d", BuildTarget.StandaloneWindows64);
            }
        }

    }
    [MenuItem("TNN/Asset Bundle Scene/Bundle Android")]
    static void CreateSceneBundleAndroid()
    {
        foreach (Object o in Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets))
        {
            string path = AssetDatabase.GetAssetPath(o);

            //BuildPipeline.BuildPlayer(new string[] { path }, Application.streamingAssetsPath + "/" + ".unity3d", BuildTarget.Android, BuildOptions.BuildAdditionalStreamedScenes);
            if (path.Contains(".unity"))
            {
                CreateBundle(new string[] { path }, Application.streamingAssetsPath + "/Android/" , o.name + ".unity3d", BuildTarget.Android);
            }
            else
            {
                //CreateBundle(o, Application.streamingAssetsPath + "/Android/" , o.name + ".unity3d", BuildTarget.Android);
            }
        }
    }
    static void CreateBundle(string[] levels, string locationPathName,string objName,BuildTarget target)
    {
        BuildPipeline.PushAssetDependencies();

        uint crc = 0;
        BuildPipeline.BuildStreamedSceneAssetBundle(levels, locationPathName + objName, target,out crc);

        
        string head = Application.streamingAssetsPath + "/";
        locationPathName = locationPathName.Substring(head.Length, locationPathName.Length - head.Length);
        
        Dictionary<string, uint> crcs = new Dictionary<string, uint>();
        crcs.Add(objName, crc);

        BundleCRCInfoMgr.AddOrUpdateBundleCRCInfo(crcs);
        //BuildPipeline.BuildStreamedSceneAssetBundle(levels, locationPathName, target, BuildOptions.BuildAdditionalStreamedScenes);
        BuildPipeline.PopAssetDependencies();
        AssetDatabase.Refresh();
    }
    static void CreateBundle(Object obj, string locationPathName, string objName, BuildTarget target)
    {
        BuildPipeline.PushAssetDependencies();
        uint crc = 0;
        BuildPipeline.BuildAssetBundle(obj, new Object[] { obj }, locationPathName + objName, out crc, BuildAssetBundleOptions.CollectDependencies 
                                | BuildAssetBundleOptions.CompleteAssets, target);
        Debug.LogError(crc);
        //BuildPipeline.BuildStreamedSceneAssetBundle(levels, locationPathName, target, BuildOptions.BuildAdditionalStreamedScenes);
        BuildPipeline.PopAssetDependencies();
        AssetDatabase.Refresh();
    }
}
