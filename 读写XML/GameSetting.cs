using UnityEngine;
using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

public class GameSetting : MonoBehaviour
{
    public static bool HasGetPriceFormNet = false;
    public const int LevelNum = 100;
    public float Plus0 = 0f;
    public float Plus1 = 0f;
    public float Plus2 = 0f;
    public float Plus3 = 0f;
    public int[] LevelState = new int[LevelNum];
    public bool bEvaluate;
    //以上是原有的变量，估计没啥用了

    public float gapTime = 10f;         //检测的间隔时间，每个这个时间检测一次
    public int floorFrames = 24;        //检测的最低帧数，连续两次小于这个值就关掉特效
    private XmlDocument XmlDoc;
    private XmlElement XmlRoot;
    public static int fastBloomState;   //-1 为未设置（默认开启），0为关闭，1为开启
    private string strPath = Application.persistentDataPath + "/MLand2/GameSetting.xml";


    public GameSetting()
    {
        bEvaluate = false;
        fastBloomState = -1;
    }

    void Awake()
    {
        DontDestroyOnLoad(transform);
    }

    public void Init()
    {
        if (File.Exists(strPath))
        {
            LoadGameSettingFiles();
        }
        else
        {
            SaveGameSettingFile();
        }
        startTime = Time.time;
        changeBloomTimes = 0;
    }

    private float startTime;
    private int changeBloomTimes;
    void Update()
    {
        if(fastBloomState == -1)
        {
            UpdateTick();
            if(Time.time - startTime >= gapTime)
            {
                startTime = Time.time;
                if (mLastFps < floorFrames)
                {
                    changeBloomTimes += 1;
                    if (changeBloomTimes >= 2)
                    {
                        SetFastBloomState(0);
                        SaveGameSettingFile();
                    }
                }
                else
                    changeBloomTimes = 0;
            }
        }

    }

    private long mFrameCount = 0;
    private long mLastFrameTime = 0;
    static long mLastFps = 0;
    private void UpdateTick()
    {
        if (true)
        {
            mFrameCount++;
            long nCurTime = TickToMilliSec(System.DateTime.Now.Ticks);
            if (mLastFrameTime == 0)
            {
                mLastFrameTime = TickToMilliSec(System.DateTime.Now.Ticks);
            }

            if ((nCurTime - mLastFrameTime) >= 1000)
            {
                long fps = (long)(mFrameCount * 1.0f / ((nCurTime - mLastFrameTime) / 1000.0f));

                mLastFps = fps;

                mFrameCount = 0;

                mLastFrameTime = nCurTime;
            }
        }
    }

    public static long TickToMilliSec(long tick)
    {
        return tick / (10 * 1000);
    }

    public void LoadGameSettingFiles()
    {
        XmlDoc = new XmlDocument();
        XmlDoc.Load(strPath);
        XmlRoot = XmlDoc.DocumentElement;
        if (XmlRoot != null)
        {
            XmlNode serverNode = XmlRoot.SelectSingleNode("Servers");

            foreach (XmlElement node in XmlRoot.ChildNodes)
            {
                if (node.Name == "FastBloom" && node.HasAttribute("State"))
                {
                    fastBloomState = Convert.ToInt32(node.GetAttribute("State"));
                }
            }
        }
    }

    public void SaveGameSettingFile()
    {
        XmlDoc = new XmlDocument();
        XmlRoot = XmlDoc.CreateElement("GameSettings");
        XmlDoc.AppendChild(XmlRoot);

        XmlElement recordNode = XmlDoc.CreateElement("FastBloom");
        XmlRoot.AppendChild(recordNode);
        recordNode.SetAttribute("State", fastBloomState.ToString());

        string strMLand2Path = Application.persistentDataPath + "/MLand2";
        if (!Directory.Exists(strMLand2Path))
            Directory.CreateDirectory(strMLand2Path);
        //if (File.Exists(strPath))
        //    File.Delete(strPath);
        XmlDoc.Save(strPath);
    }

    public void SetFastBloomState(int state)
    {
        if (state == 0)
            fastBloomState = 0;
        else
            fastBloomState = 1;

        SetFastBloom();
    }

    public static void SetFastBloom()
    {
        FastBloom bloomComp = Camera.main.GetComponent<FastBloom>();
        if (fastBloomState == 0)
            bloomComp.enabled = false;
        else
            bloomComp.enabled = true;
    }

    public void Load(XmlElement parent)
    {
        XmlNode rootNode = parent.SelectSingleNode("GameSetting");
        if (rootNode != null)
        {
            CXmlRead creader = new CXmlRead(rootNode as XmlElement);
            bEvaluate = bool.Parse(creader.Str("evaluate", "false"));

            XmlNode plusNode = rootNode.SelectSingleNode("PlusNode");
            XmlNode levelNode = rootNode.SelectSingleNode("LevelNode");
            if (plusNode != null)
            {
                Plus0 = XmlConvert.ToSingle((plusNode as XmlElement).GetAttribute("Plus0"));
                Plus1 = XmlConvert.ToSingle((plusNode as XmlElement).GetAttribute("Plus1"));
                Plus2 = XmlConvert.ToSingle((plusNode as XmlElement).GetAttribute("Plus2"));
                Plus3 = XmlConvert.ToSingle((plusNode as XmlElement).GetAttribute("Plus3"));
            }
            if (levelNode != null)
            {
                for (int i = 0; i < LevelNum; ++i)
                {
                    CXmlRead reader = new CXmlRead(levelNode as XmlElement);
                    LevelState[i] = reader.Int("Level" + i.ToString(), 0);
                }
            }
        }
    }

    public void Save(XmlDocument doc, XmlElement parent)
    {
        XmlElement recordNode = doc.CreateElement("GameSetting");
        parent.AppendChild(recordNode);

        (recordNode as XmlElement).SetAttribute("evaluate", bEvaluate.ToString());
        XmlElement plusNode = doc.CreateElement("PlusNode");
        XmlElement levelNode = doc.CreateElement("LevelNode");
        recordNode.AppendChild(plusNode);
        recordNode.AppendChild(levelNode);
        plusNode.SetAttribute("Plus0", Plus0.ToString());
        plusNode.SetAttribute("Plus1", Plus1.ToString());
        plusNode.SetAttribute("Plus2", Plus2.ToString());
        plusNode.SetAttribute("Plus3", Plus3.ToString());
        for (int i = 0; i < LevelNum; ++i)
        {
            levelNode.SetAttribute("Level" + i.ToString(), LevelState[i].ToString());
        }

    }

    public void OnNetUpdate(PKG_ID_SHOP_GET_PRICE_INFO_NTF p)
    {
        if (p == null)
            return;

        Plus0 = p.plus0;
        Plus1 = p.plus1;
        Plus2 = p.plus2;
        Plus3 = p.plus3;

        for (int i = 0; i < p.mapLvlNum; ++i)
        {
            LevelState[i] = (int)p.lvlStateList[i];
        }

    }

}
