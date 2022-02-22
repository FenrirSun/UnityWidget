using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OperationTipsUI : MonoBehaviour
{
    private GameObject TipsWithMask;    //有遮罩的tips（游戏暂停时等地方可用）
    private O_UILabel TipsWithMaskLabel;
    public bool needTips = true;       // 控制tips是否显示
    

    static private OperationTipsUI WndRoot;//本窗口root
    static public OperationTipsUI Instance()
    {
        if (WndRoot == null)
        {
            GameApp.GetUIManager().AddUI(Resources.Load("UI/OperationTips/OperationTips") as GameObject,UICameraMgr.eUICameraLevel.Level_11);

            //GameObject viewPrefab = Resources.Load("UI/Camera/TopViewUI") as GameObject;
            //GameApp.GetUIManager().SetUIRootHeight(viewPrefab);
            //GameObject TopView = UnityEngine.Object.Instantiate(viewPrefab) as GameObject;
            //DontDestroyOnLoad(TopView);
            //TopView.transform.Translate(1000f, 0f, -100f);
            //TopView.transform.Find("Anchor").GetComponent<UIAnchor>().uiCamera = GameApp.GetUIManager().uiCamera;
            //GameObject viewCamera = TopView.transform.Find("View Camera").gameObject;
            //UIViewport viewport = viewCamera.GetComponent<UIViewport>();
            //viewport.sourceCamera = GameApp.GetUIManager().uiCamera; ;

            WndRoot = UICameraMgr.mInstance.GetUICameraByLevel(UICameraMgr.eUICameraLevel.Level_11).transform.GetComponentInChildren<OperationTipsUI>();
        }
        return WndRoot;
    }

    public O_UILabel[] Labels;
    public Transform[] Backs;

    private const int MaxLineCount = 4;
    private const float ShowTime = 1f;
    private int CurLineCount = 0;
    private LinkedList<string> buffs = new LinkedList<string>();

    private class COperationItem
    {
        public O_UILabel Label;
        public Transform Back;
        public float EndTime;
    }
    private COperationItem[] Items = new COperationItem[MaxLineCount];

    public void Pop(string str)
    {
        if (needTips == false)
            return;

        if (CurLineCount >= MaxLineCount)
        {
            buffs.AddLast(str);
        }
        else
        {
            //忽略外部传进来的字体颜色，强制使用EarthYellow
            //int index = str.IndexOf(']');
            //if (index > -1)
            //{
            //    str = str.Remove(0, index + 1);
            //}
            //Items[CurLineCount].Label.text = FontMgr.FontColor_Green + str;
            Items[CurLineCount].Label.text = "[00ff12]" + str;
            Items[CurLineCount].EndTime = Time.realtimeSinceStartup + ShowTime;
            CommonFun.ShowAll(Items[CurLineCount].Label.gameObject.transform, true);
            CommonFun.ShowAll(Items[CurLineCount].Back, true);
            CurLineCount++;
        }
    }
    
    /// <summary>
    /// 弹出有遮罩的tips（游戏暂停时等地方可用）,必须手动关闭
    /// </summary>
    /// <param name="str"></param>
    public void PopTipsWithMask(string str)
    {
        O_NGUITools.SetActive(TipsWithMask, true);
        TipsWithMaskLabel.text = FontMgr.FontColor_EarthYellow + str;
    }

    public void PopTipsWithMask()
    {
       PopTipsWithMask(O_Localization.instance.GetText("TipsLocal", 20));
    }

    public void CloseTipsWithMask()
    {
        O_NGUITools.SetActive(TipsWithMask, false);
    }

    public bool isTipsWithMaskOpened()
    {
        if (TipsWithMask == null || TipsWithMask.activeInHierarchy==false)
        {
            return false;
        }

        return true;
    }

    void Awake()
    {
        //WndRoot = this;
        for (int i = 0; i < MaxLineCount; ++i)
        {
            Items[i] = new COperationItem();
            Items[i].Label = Labels[i];
            Items[i].Back = Backs[i];
            Items[i].EndTime = 0f;
            CommonFun.ShowAll(Labels[i].gameObject.transform, false);
            CommonFun.ShowAll(Backs[i], false);
        }

        if (TipsWithMask == null)
        {
            TipsWithMask = GameApp.GetUIManager().AddUI(Resources.Load("UI/OperationTips/TipWithMask") as GameObject, UICameraMgr.eUICameraLevel.Level_11);
            TipsWithMaskLabel = TipsWithMask.transform.Find("Label").gameObject.GetComponent<O_UILabel>();
            O_NGUITools.SetActive(TipsWithMask, false);
        }
    }

    void Start()
    {

    }

    void Update()
    {
        if (CurLineCount > 0)
        {
            if (Time.realtimeSinceStartup > Items[0].EndTime)
            {
                for (int i = 0; i < CurLineCount; ++i )
                {
                    if (i != CurLineCount - 1)
                    {
                        Items[i].Label.text = Items[i + 1].Label.text;
                        Items[i].EndTime = Items[i + 1].EndTime;
                    }
                    else
                    {
                        if (i != MaxLineCount - 1)
                        {
                            CommonFun.ShowAll(Items[i].Label.transform, false);
                            CommonFun.ShowAll(Items[i].Back, false);
                            CurLineCount--;
                        }
                        else
                        {
                            if (buffs.Count > 0)
                            {
                                Items[i].Label.text = buffs.First.Value;
                                Items[i].EndTime = Time.realtimeSinceStartup + ShowTime;
                                buffs.RemoveFirst();
                            }
                            else
                            {
                                CommonFun.ShowAll(Items[i].Label.transform, false);
                                CommonFun.ShowAll(Items[i].Back, false);
                                CurLineCount--;
                            }
                        }
                    }
                }

            }
        }
    }
}
