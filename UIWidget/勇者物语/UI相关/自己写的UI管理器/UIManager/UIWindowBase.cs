using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UI窗口基类
/// editLabel等字典根据tag储存了需要编辑的文字，图片和对象
/// 调用时使用editLabel[name]即可直接编辑
/// 如不需要此功能可重写Open()方法
/// 注：这个版本的sortingorder有问题，最好别用
/// </summary>
public class UIWindowBase
{
    public GameObject wndRoot { get; private set; }
    public UIPanel panel { get; private set; }
    public UIWndMargin margin { get; set; }

    //背景黑色遮罩
    public bool IsMask = false;
    //打开界面时人物可否移动
    public bool IsMovable = false;
    //界面周围空白区域可否点击(默认视同关闭按钮)
    public bool CanMarginClick = true;

    //储存当前窗口的所有可编辑Label控件
    public Dictionary<string, UILabel> editLabel = new Dictionary<string, UILabel>();
    //储存当前窗口的所有可编辑Sprite控件
    public Dictionary<string, UISprite> editSprite = new Dictionary<string, UISprite>();
    //储存当前窗口的所有可编辑GameObject控件
    public Dictionary<string, GameObject> editObject = new Dictionary<string, GameObject>();

    public void Create(GameObject go, int depth)
    {
        wndRoot = go;
        panel = wndRoot.GetComponent<UIPanel>();
        if (panel == null)
        {
            Debug.LogError("There must be a panel in the prefab's root.");
        }
        else
        {
            panel.depth = depth;
        }
        Open();
    }

    public virtual void AddEventListener()
    {
    }

    public virtual void Open()
    {
        SaveUIWidget(wndRoot.transform);
        SetPanelDepth();
        AddEventListener();
    }

    public virtual void Close()
    {
        UIManager.Instance.Close(wndRoot.name);
    }

    public virtual void OnClose()
    {
        if (margin != null)
            margin.OnClose();
        editLabel = null;
        editSprite = null;
        editObject = null;
        GameObject.Destroy(wndRoot);
    }

    //点击界面周围的空白区域
    public virtual void ClickMargin(GameObject go) 
    {
        UIManager.Instance.Close(wndRoot.name);
    }

	public virtual void Update () 
    {
    }

    //游戏中窗口的深度不定(与打开顺序有关）
    //窗口内部的panel只要编辑的时候按顺序即可，但不要超过99
    protected void SetPanelDepth()
    {
        UIPanel[] panels = wndRoot.GetComponentsInChildren<UIPanel>();
        foreach (UIPanel p in panels)
        {
            p.depth += panel.depth;
        }
    }

    /// 通过tag储存窗口中需要编辑的控件
    protected void SaveUIWidget(Transform transform)
    {
        foreach (Transform child in transform)
        {
            if (child.tag == "EditLabel")
            {
                UILabel label = child.GetComponent<UILabel>();
                if (label == null)
                    Debug.LogError("There should be a UILabel in " + child.name);
                editLabel.Add(child.name, label);
            }
            else if (child.tag == "EditSprite")
            {
                UISprite sprite = child.GetComponent<UISprite>();
                if (sprite == null)
                    Debug.LogError("There should be a UISprite in " + child.name);
                editSprite.Add(child.name, sprite);
            }
            else if (child.tag == "EditObject")
                editObject.Add(child.name, child.gameObject);

            SaveUIWidget(child);
        }
    }
}
