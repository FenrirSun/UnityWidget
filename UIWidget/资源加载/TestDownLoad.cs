using Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///描述：缺少描述
///@作者 请填写作者
///@创建日期 2020-01-16 16-43-49
///@版本号 1.0
///</summary>
public class TestDownLoad : MonoBehaviour
{
    public AssetBundle amy;

    public GameObject amyGo;

    public AssetBundle controller;

    //两种加载方式，传统的 WWW 和 AssetBundle，此外还可以通过系统的 HttpWebRequest 或者 unity 的 UnityWebRequest （见下一个方法）
    IEnumerator Start()
    {
        var www = new WWW(@"file://G:\ArtProjects\AnimatorOverideController\work\amy");

        yield return www;

        amy = www.assetBundle;

        amyGo = GameObject.Instantiate<GameObject>(amy.LoadAsset(amy.GetAllAssetNames()[0]) as GameObject);

        //var www2 = new WWW(@"file://C:\Users\admin\AppData\LocalLow\edu100\AdaptiveLearing\Bundles\objects\animationcontroller\controller");
        var loader = AssetBundle.LoadFromFileAsync(@"G:\ArtProjects\AnimatorOverideController\work\controller");

        yield return loader;

        controller = loader.assetBundle;

        var overrideController = new AnimatorOverrideController();

        var controllerName = controller.GetAllAssetNames()[0];
        var controllerAsset = controller.LoadAsset(controllerName) as RuntimeAnimatorController;
        overrideController.runtimeAnimatorController = controllerAsset;

        var allEntitys = EntityContainer.GetAllEntity();

        foreach (var entity in allEntitys.Values)
        {
            if (entity is RoleEntity _role)
            {
                var animator = entity.GetComponent<AnimationComponent>().Anim;//.GetComponent<Animator>();

                animator.runtimeAnimatorController = null;

                animator.runtimeAnimatorController = overrideController;
            }
        }
    }

    /// <summary>
    /// 协程：下载文件
    /// </summary>
    IEnumerator DownloadFile()
    {
        UnityWebRequest uwr = UnityWebRequest.Get("http://www.linxinfa.test.mp4.mp4"); //创建UnityWebRequest对象，将Url传入
        uwr.SendWebRequest();                                                                                  //开始请求
        if (uwr.isNetworkError || uwr.isHttpError)                                                             //如果出错
        {
            Debug.Log(uwr.error); //输出 错误信息
        }
        else
        {
            while (!uwr.isDone) //只要下载没有完成，一直执行此循环
            {
                ProgressBar.value = uwr.downloadProgress; //展示下载进度
                SliderValue.text  = Math.Floor(uwr.downloadProgress * 100) + "%";
                yield return 0;
            }

            if (uwr.isDone) //如果下载完成了
            {
                print("完成");
                ProgressBar.value = 1; //改变Slider的值
                SliderValue.text  = 100 + "%";
            }

            byte[] results = uwr.downloadHandler.data;
            // 注意真机上要用Application.persistentDataPath
            CreateFile(Application.streamingAssetsPath + "/MP4/test.mp4", results, uwr.downloadHandler.data.Length);
            AssetDatabase.Refresh(); //刷新一下
        }
    }

    void CreateFile(string path, byte[] bytes, int length)
    {
        Stream sw;
        FileInfo file = new FileInfo(path);
        if (!file.Exists)
        {
            sw = file.Create();
        }
        else
        {
            return;
        }

        sw.Write(bytes, 0, length);
        sw.Close();
        sw.Dispose();
    }

    private void OnDestroy()
    {
        amy?.Unload(true);
        controller?.Unload(true);
    }
}
