//ÓÎÏ·¿ªÊŒÈë¿Ú£¬by alex 2011.12.28
using UnityEngine;
using System.Collections;

public class GameStarter : MonoBehaviour
{
    public GameObject goUIRoot = null;
    public bool needTrigger = true;
    public string loginMap = "qymap_1";
    public bool SimulatePlayer = false;
    public bool resetTask = false;
    public bool debugAccount = false;
    public bool cameraCollision = false;
	public bool ShowDebug=true;
    public bool TestMode = false;
    public bool jobAndLevelMapping = false;
	public int  gameQuality=1;
	public bool ShowGuide=false;
    public int GuideGroupID = -1;
    public int GuideStepID = -1;
    public bool ShowMission = false;
    public int MissionID = 1;
    public int MissionState = 0;
    public bool IsComplete = true;
    public string Version = "1.0";
    public static bool AsyncScene = true;
    public bool TestAssetbundle = false;
    public bool ResFromLocal = true;
    public int Sifu = 0;

    // 游戏启动可选项设置
    public bool ResourceNet = true;

    // FTP地址
    public string FtpAddress = "192.168.1.120";

    public static GameObject instance = null;
    public static GameStarter Instance()
    {
        if (instance == null)
        {
            instance = GameObject.Find("GameStarterprefabs");
        }
        return instance.GetComponent("GameStarter") as GameStarter;
    }

    void Awake()
    {
        Debug.Log("game Awake time=" + Time.realtimeSinceStartup);
        DontDestroyOnLoad(gameObject);

        GameQuality.Instance().m_GameQuality = (enumGameQuality)gameQuality;
        GameQuality.Instance().SetGameQuality();
        
        var gapp = GameApp.Instance();
        gapp.ResetTask = resetTask;
        gapp.DebugAccount = debugAccount;
        gapp.ShowDebug = ShowDebug;
        gapp.ShowGuide = ShowGuide;
        gapp.GuideGroupID = GuideGroupID;
        gapp.GuideStepID = GuideStepID;
        gapp.IsComplete = IsComplete;
        gapp.LoginFirst = true;
        gapp.Version = Version;
        gapp.NeedTrigger = needTrigger;
        gapp.LoginMap = loginMap;
        gapp.TestMode = TestMode;
        gapp.ShowMission = ShowMission;
        gapp.MissionID = MissionID;
        gapp.MissionState = MissionState;
        gapp.CameraCollision = cameraCollision;
        gapp.JobAndLevelMapping = jobAndLevelMapping;
        gapp.ResFromLocal = ResFromLocal;
        gapp.SiFu = Sifu;
        GameEvent.Instance();



        Debug.Log("game Awake time1=" + Time.realtimeSinceStartup);
    }

    void Start()
    {
        Debug.Log("game Awake time2=" + Time.realtimeSinceStartup);
          // DB预初始化
        DBManager.Instance().PreInit();
        // 预初始化
        GameApp.Instance().PreInit();
        O_Localization.instance.PreInit();
        Debug.Log("game Awake time3=" + Time.realtimeSinceStartup);
        StartCoroutine(ShowLogo());

        
    }

    void Update()
    {
 
    }

    public IEnumerator ShowLogo()
    {
      //  yield return new WaitForSeconds(3);
        goUIRoot.SetActive(true);
        SetUIRootHeight(goUIRoot);
        GetFtpAddress();
        UIManager.SetLoadingSlider(0.1f);
        yield return 1;
    }
    public IEnumerator GotoLogin()
    {
        UIManager.SetLoadingString(O_Localization.instance.GetText("GameStartLocal", 5));
         yield return new WaitForSeconds(1f);
     //   GameApp.GetUIManager().OpenWnd("LoadingWnd");
        if (ResFromLocal)
        { 
           // OnResLoaded();
            var uiManager = GameApp.GetUIManager();
            var loadingwnd = uiManager.GetWnd("LoadingWnd") as LoadingWnd;
            O_Localization.instance.init();
            loadingwnd.SetSlider(0.2f);
            yield return 1;

            DBManager.Instance().InitStep(1,OnLoadXMLDataFinished);
            yield return 1;
            //loadingwnd.SetSlider(0.4f);
            //yield return 1;

        }
        else if (TestAssetbundle == false)
        {
            Debug.Log("game start InitVersionInfo=" + Time.time);
            GameApp.Instance().InitVersionInfo();
        }

    }
    public void OnLoadXMLDataFinished()
    {
        StartCoroutine(GotoLoginNext());
    }

    public IEnumerator GotoLoginNext()
    {

        var uiManager = GameApp.GetUIManager();
        var loadingwnd = uiManager.GetWnd("LoadingWnd") as LoadingWnd;

        loadingwnd.SetSlider(0.4f);
        yield return 1;

        WorldLevelManager.Instance().Init();
        loadingwnd.SetSlider(0.6f);
        yield return 1;

        GameApp.Instance().Init();
        loadingwnd.SetSlider(0.9f);
        yield return 1;

        loadingwnd.SetSlider(1f);
        UIManager.CloseLoadingWnd();
        uiManager.InitUIScreen();
        uiManager.InitDBUIWnd();
        Application.LoadLevelAdditive("Login");

    }


    void OnGUI()
    {
        if (TestAssetbundle)
        {
            if (GUI.Button(new Rect(10, 10, 200, 100), "Clean Cache"))
            {
                Caching.CleanCache();
            }

            if (GUI.Button(new Rect(10, 120, 200, 100), "Continue"))
            {
                GameApp.Instance().InitVersionInfo();
            }

            GUI.TextField(new Rect(10, 240, 50, 30), "FTP:");
            FtpAddress = GUI.TextField(new Rect(60, 240, 200, 50), FtpAddress);

        }
    }

    // 资源加载完毕
    public void OnResLoaded()
    {
        LoadingWnd loadingwnd = GameApp.GetUIManager().GetWnd("LoadingWnd") as LoadingWnd;
        O_Localization.instance.init();
        loadingwnd.SetSlider(0.1f);
        DBManager.Instance().Init();
        Debug.Log("sdfsdfsdfsdfsdfsdf");
        loadingwnd.SetSlider(0.4f);
       
        WorldLevelManager.Instance().Init();
        loadingwnd.SetSlider(0.5f);

        GameApp.Instance().Init();
        loadingwnd.SetSlider(0.8f);
      
        GamePlusInternal.ActivateGamePlusLogin();
 
        if (Application.platform == RuntimePlatform.Android && gameQuality == 2)
        {
            string vendor = SystemInfo.graphicsDeviceVendor.ToLower();
            if (vendor.IndexOf("nvidia") == -1)
            {
                MsgBoxOk.text = O_Localization.instance.GetText("TipsLocal", 43);
                MsgBoxOk.okText = O_Localization.instance.GetText("TipsLocal", 44);
                MsgBoxOk.onOkClicked = Application.Quit;
                MsgBoxOk.Pop();
            }
        }
        loadingwnd.SetSlider(1f);
        UIManager.CloseLoadingWnd();
       Application.LoadLevelAdditive("Login");
    }

    public void SetUIRootHeight(GameObject go)
    {
        O_UIRoot root = go.GetComponentInChildren<O_UIRoot>();
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (Screen.height == 640)
            {
                root.manualHeight = 640;
            }
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            if (Screen.height < 768 - 100)
            {
                root.manualHeight = 640;
            }
            else
            {
                root.manualHeight = 768;
            }
        }
        else if  (RuntimePlatform.WindowsWebPlayer == Application.platform)
        {
            root.manualHeight = 960;
        }
        else
        {
            root.manualHeight = 768;
        }
    }

    private void GetFtpAddress()
    {
        Debug.Log("GetFtpAddress------------------------------");
        GameRecord.Instance().LoadLocalFile();
        GameApp.GetNetHandler().ConnectToLSServer();
        GameApp.GetUIManager().OpenWnd("LoadingWnd");
        UIManager.SetLoadingString(O_Localization.instance.GetText("GameStartLocal", 1));
    }

    public void StartLoading()
    {
        ResManager.Instance.LoadVersionInfo(() => { 
            //onload
            StartCoroutine(GotoLogin());
        }, 
        () => { 
            //onfailed
            MsgBoxOk.text = "版本信息获取失败，请重试！";
            MsgBoxOk.okText = "OK";
            MsgBoxOk.onOkClicked = () => {
                StartLoading();
            };
            MsgBoxOk.Pop();

        });
    }

}
