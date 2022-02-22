using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class Console : MonoBehaviour {

    static Console instance = null;
    static public Console Instance { get {return instance; } }

    public bool ConsoleOn = false;
    public int maxLogLines = 50;

    string strCMD="";

    bool bShowConsole = false;

    List<string> logs = new List<string>();

    float sbValue = 0;
    float sbSize = 1;
    float sbTopValue = 0;
    float sbBottomValue = 10;
    string logText = "";

    StringBuilder sb = new StringBuilder();

    Vector2 scrollPosition = Vector2.zero;


    Dictionary<string, string> cmdList = new Dictionary<string, string>();

    void OnGUI()
    {
        if (ConsoleOn)
        {
            if (bShowConsole)
            {
                GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height / 2));

                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal(); //第一排
                if (GUILayout.Button("Console", GUILayout.Width(80)))
                {
                    bShowConsole = !bShowConsole;
                }
                GUI.SetNextControlName("cmd");
                strCMD = GUILayout.TextField(strCMD);
                if (GUILayout.Button("CMD", GUILayout.Width(80)))
                {
                    OnCMD();
                }

                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal(); //第二排

                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                GUILayout.TextArea(sb.ToString());
                GUILayout.EndScrollView();


                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                GUILayout.EndArea();


                if (GUI.GetNameOfFocusedControl() == "cmd") //获取这个user控件是否处于焦点状态
                {
                    if( Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.Used )
                    {
                        OnCMD();
                    }
                    if(Event.current.keyCode == KeyCode.DownArrow && Event.current.type == EventType.Used)
                    {
                        foreach(string cmd in cmdList.Keys)
                        {
                            if(cmd.StartsWith(strCMD))
                            {
                                strCMD = cmd;
                                break;
                            }
                        }
                    }
                }

                if (Event.current.keyCode == KeyCode.F1 && Event.current.type == EventType.Used)
                {
                    bShowConsole = !bShowConsole;
                }

            }
            else
            {
                GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height / 2));
                GUILayout.BeginHorizontal(); //第一排

                if (GUILayout.Button("Console", GUILayout.Width(80)))
                {
                    bShowConsole = !bShowConsole;
                }
                GUILayout.EndHorizontal();

                GUILayout.EndArea();
            }
        }
    }

	// Use this for initialization
	void Start () {
        instance = this;
        DontDestroyOnLoad(this);
        Application.RegisterLogCallback(HandleLog);

        cmdList.Add("help","帮助");
        cmdList.Add("deleteSave","删除本地存档");
        cmdList.Add("bloom [flag]", "开关泛光特效，[flag]=1 开 [flag]=0 关");
    }

    void OnEnable()
    {
        instance = this;
        DontDestroyOnLoad(this);
        Application.RegisterLogCallback(HandleLog);
    }
	
    void OnDisable()
    {
        Application.RegisterLogCallback(null);
    }

	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Application.RegisterLogCallback(HandleLog);
            bShowConsole = !bShowConsole;
        }
	}

    public void Log(string log)
    {
        if (logs.Count >= maxLogLines)
        {
            sb.Remove(0, logs[0].Length+2);
            logs.RemoveAt(0);
        }
        sb.Append(log);
        sb.Append("\r\n");
        logs.Add(log);
    }

    void OnCMD()
    {
        if (!string.IsNullOrEmpty(strCMD))
            Log(">" + strCMD);
        scrollPosition.y += 20;

        ProcessCMD(strCMD);

        strCMD = "";
    }

    //parse and process command
    public void ProcessCMD(string cmd)
    {
        string[] subStrings = cmd.Split(' ');

        if (subStrings.Length < 1)
            return;
        switch(subStrings[0])
        {
            case "help":
                Log("------Help------");
                foreach(KeyValuePair<string,string> pair in cmdList)
                {
                    Log(pair.Key +" : "+pair.Value);
                }
                break;
            case "deleteSave":
                string strPath = Application.persistentDataPath + "/MLand2/RegisterOL.xml";
                try
                {
                    File.Delete(strPath);
                    Log("Deleted " + strPath);
                }
                catch
                {
                    Log("Can't find save file");
                }
                break;
            case "bloom":
                if (subStrings.Length > 1)
                    CmdBloom(subStrings[1]);
                break;
            default:
                Log("Unknown command!");
                break;
        }

    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error)
        {
            Log("[Error]"+logString);
            Log(stackTrace);
        }
        else if (type == LogType.Exception)
        {
            Log("[Exception]"+logString);
            Log(stackTrace);
        }
        else if(type == LogType.Warning)
        {
            Log("[Warning]" + logString);
        }
    }


    void CmdBloom(string para = "")
    {
        int bloomOn = 0;
        int.TryParse(para, out bloomOn);
        GameObject mainCamera = GameObject.Find("Main Camera");
        if (mainCamera == null)
        {
            Log("Not found Main Camera");
            return;
        }
        FastBloom bloomComp = mainCamera.GetComponent<FastBloom>();
        if (bloomComp == null)
        {
            Log("Not found bloom component on Main Camera");
            return;
        }
        if (bloomOn > 0)
        {
            bloomComp.enabled = true;
            Log("Bloom on");
        }
        else
        {
            bloomComp.enabled = false;
            Log("Bloom off");
        }
    }
}
