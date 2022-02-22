using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class ResManager : MonoBehaviour {

    //单件
    static private ResManager s_Instance = null;
    static public ResManager Instance { get{return s_Instance;} }

    //是否是release版，如果是，则会在相应平台寻找路径
    static public bool IsRelease = false;
    static public bool IsLoadRemote = false;


    //在component上的控制flag
    public bool DebugMode = false;
    public bool IsReleaseMode = false;
    public bool IsLoadRemoteMode = false;


    //不同平台下StreamingAssets的路径,本地资源根路径
    public static string LocalRootURL 
    { 
        get
        { 
            return
#if UNITY_ANDROID
		"jar:file://" + Application.dataPath + "!/assets/";
#elif UNITY_IPHONE
		Application.dataPath + "/Raw/";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
        "file://" + Application.dataPath + "/StreamingAssets/";
#else
        string.Empty;
#endif
        }
    }


    //远程更新资源地址根路径
    public static string RemoteRootURL { get { return "file://" + Application.dataPath + "/../../GameRes/"; } } //这是本地测试用

    //平台区分路径
    public static string PlantformURL
    {
        get
        {
            return
#if UNITY_ANDROID
		"Android/";
#elif UNITY_IPHONE
         "IOS/";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
 "Windows32/";
#else
        string.Empty;
#endif
        }
    }

    //模块子路径
    static public readonly string DataSheetURL= "DB/";   //数据表
    static public readonly string TextureURL = "Texture/";      //独立贴图
    static public readonly string UIURL = "UI/";                //UI
    static public readonly string AtlasURL = "Atlas/";          //图集
    static public readonly string SceneURL = "Scene/";          //场景
    static public readonly string ModelURL = "Model/";          //物件
    static public readonly string BGMURL = "Sound/BGM/";         //背景音乐
    static public readonly string SEURL = "Sound/SE/";           //音效

    public static string VersionFileName { get { return "VersionInfo"; } }

   
    //本地资源路径，不是release版直接返回streamingAsset
    static public string LocalePath { get { return IsRelease?(LocalRootURL+PlantformURL):( "file://" + Application.dataPath + "/StreamingAssets/"+PlantformURL );} }
    //远程热更新资源路径
    static public string RemotePath { get { return RemoteRootURL+PlantformURL;} }

    //制作资源的游戏内根路径
    static public string ResMakePath { get { return Application.dataPath + "/StreamingAssets/" + PlantformURL; } }
    //制作资源的复制资源路径
    static public string ResMakeRemoteRootURL { get { return Application.dataPath + "/../../GameRes/"; } }


    //回调代理
    public delegate void DelVoid();
    public delegate void DelOnLoadTextAsset(TextAsset ta);
    public delegate void DelOnBundleLoad(Object bundle);


    //资源版本信息
    Dictionary<string, int> dicVersionInfo = new Dictionary<string, int>();
    Dictionary<string, int> dicLocalVersionInfo = new Dictionary<string, int>();

    void Awake()
    {
        s_Instance = this;
        IsRelease = IsReleaseMode;
        IsLoadRemote = IsLoadRemoteMode;
    }

	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public string Test()
    {
        return "OK";
    }

    public bool LoadTextAsset(string bundleName ,DelOnLoadTextAsset onLoad,DelVoid onFailed = null)
    {
        string resURL = LocalePath + bundleName;

        int version = 0;
        if (IsLoadRemote && dicVersionInfo.ContainsKey(bundleName) && dicLocalVersionInfo.ContainsKey(bundleName))
        {
            if (dicVersionInfo[bundleName]>dicLocalVersionInfo[bundleName]) //远端版本较新
            {
                resURL = RemotePath + bundleName;       //切换远端地址
                version = dicVersionInfo[bundleName];   //切换较新版本号
            }
        }

        StartCoroutine(LoadBundle(resURL,version,(bundle)=>{
            if (bundle)
            {
                TextAsset ta = bundle as TextAsset;
                if (onLoad != null)
                    onLoad(ta);
            }
            else
            {
                if (onFailed != null)
                    onFailed();
            }
        }));

        return true;
    }

    public void LoadVersionInfo(DelVoid onLoad, DelVoid onFailed)
    {
        if (!IsLoadRemote)
        {
            onLoad();
            return;
        }

        string localVersionInfo = LocalePath + VersionFileName;
        string versionInfo = RemotePath + VersionFileName;
        StartCoroutine(LoadBundle(localVersionInfo, 0, (localBundle) =>
        {
            if (localBundle)
            {
                TextAsset localTA = localBundle as TextAsset;
                dicLocalVersionInfo = ParseVersionInfo(localTA);
                StartCoroutine(LoadBundle(versionInfo, 0, (remoteBundle) =>
                {
                    if (remoteBundle)
                    {
                        TextAsset remoteTA = remoteBundle as TextAsset;
                        dicVersionInfo = ParseVersionInfo(remoteTA);
                        onLoad();
                    }
                    else
                    {
                        onFailed();
                    }
                }));

            }
            else
            {
                onFailed();
            }
        }));
    }


    IEnumerator LoadBundle(string url,int version, DelOnBundleLoad onResLoad = null)
    {
        url = url + ".assetbundle";
        WWW www = null;
        if (version > 0)
            www = WWW.LoadFromCacheOrDownload(url, version);
        else
            www = new WWW(url);

        yield return www;

        if (www.error != null)
        {
            Debug.LogError("Load Bundle Failed! " + url + " Error Is " + www.error);
            if (onResLoad != null)
                onResLoad(null);
            yield break;
        }

        if (onResLoad != null)
            onResLoad(www.assetBundle.mainAsset);

        www.assetBundle.Unload(false);
    }

    //解析版本信息
    public static Dictionary<string, int> ParseVersionInfo(TextAsset taVerInfo)
    {
        Dictionary<string, int> dicInfo = new Dictionary<string, int>();
        XmlDocument xmlDoc = new XmlDocument();
        if (taVerInfo == null)
        {
            Debug.LogError("Load Version Info Failed!");
            return null;
        }
        xmlDoc.Load(new StringReader(taVerInfo.text));
        XmlElement xmlRoot = xmlDoc.DocumentElement;
        XmlNodeList nodeList = xmlRoot.ChildNodes;
        foreach (XmlNode node in nodeList)
        {
            if ((node is XmlElement) == false)
                continue;

            CXmlRead reader = new CXmlRead(node as XmlElement);
            string bundleName = reader.Str("bundleName", "");
            int version = reader.Int("version", 0);
            if (!dicInfo.ContainsKey(bundleName))
                dicInfo.Add(bundleName, version);
        }
        return dicInfo;
    }
}

