using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// UI管理器
/// 需保证窗口类名和prefab名称一致
/// prefab需要统一放在Resources/UIPrefab下面，可建文件夹
/// 使用工具Export Prefab Path可自动保存路径
/// </summary>
public class UIManager : MonoBehaviour 
{
    //千位是不同的窗口层级
    //百位是通用遮罩层级（窗口固定为500，边界碰撞框为200，黑色遮罩为0）
    //个位和十位给窗口内部用
    public enum EUILayer
    {
        BackLayer = 10000,   //背景层
        HUDLayer = 20000,    //HUD层
        TalkLayer = 30000,   //对话层
        WndLayer = 40000,    //窗口层
        TipsLayer = 50000,   //提示层
    }

    private static UIManager m_Instance;
    public static UIManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = (UIManager)GameObject.FindObjectOfType(typeof(UIManager));
                if (m_Instance == null)
                {
                    UIRoot root = (UIRoot)GameObject.FindObjectOfType(typeof(UIRoot));

                    if (root == null)
                    {
                        UIPanel m_Root = NGUITools.CreateUI(false);
                        m_Instance = m_Root.gameObject.AddComponent<UIManager>();
                        InitUIRoot(m_Root.GetComponent<UIRoot>());
                    }
                    else
                    {
                        m_Instance = root.gameObject.AddComponent<UIManager>();
                        InitUIRoot(root);
                    }
                }

                m_Instance.InitUIManager();
            }

            return m_Instance;
        }
    }

    public class WindowInfo
    {
        public EUILayer layer;
        public string name;
        public UIWindowBase wndBase;
    }

    private UIPanel m_Panel;
    private Dictionary<EUILayer, Transform> m_UILayers = new Dictionary<EUILayer, Transform>();
    private List<WindowInfo> m_Windows = new List<WindowInfo>();

    public PrefabSaveModle prefabPath;
    public bool IsMovable
    {
        get;
        private set;
    }
    public bool IsMask
    {
        get;
        private set;
    }
    public static void InitUIRoot(UIRoot root)
    {
        root.scalingStyle = UIRoot.Scaling.ConstrainedOnMobiles;
        root.fitHeight = true;
        root.fitWidth = true;
        root.manualHeight = 720;
        root.manualWidth = 1136;
    }

    public void InitUIManager()
    {
        try
        {
            prefabPath = Resources.Load<PrefabSaveModle>("UIPrefab/PrefabPathAsset") as PrefabSaveModle;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }

        if (m_Panel == null) { m_Panel = GetComponent<UIPanel>(); }
        Type layers = typeof(EUILayer);
        Array uiLayerArray = Enum.GetValues(layers);

        for (int i = 0; i < uiLayerArray.Length; i++)
        {
            AddLayer((EUILayer)Enum.Parse(typeof(EUILayer), uiLayerArray.GetValue(i).ToString()));
        }

        GameObject.DontDestroyOnLoad(m_Panel.gameObject);
    }

    public T Open<T>(EUILayer layer) where T : UIWindowBase, new()
    {
        T t = new T();

        if (prefabPath != null)
        {
            string prePath = GetPrefabPath<T>();
            GameObject prefab = GameObject.Instantiate(Resources.Load(prePath)) as GameObject;
            if (prefab == null)
                Debug.LogError("Can't find window prefab, window name:" + typeof(T).FullName);

            //开多个窗口时，新开的窗口总是在前面
            int depth = (int)layer + GetWndNumInLayer(layer) * 1000 + 500;
            prefab.name = prefab.name.Substring(0, prefab.name.Length - 7);
            t.Create(prefab, depth);

            Transform prefab_Tran = prefab.GetComponent<Transform>();

            Vector3 localP = prefab_Tran.position;
            Quaternion localR = prefab_Tran.rotation;
            Vector3 localS = prefab_Tran.localScale;

            if (m_UILayers.ContainsKey(layer))
            {
                prefab_Tran.parent = m_UILayers[layer];
            }
            else
            {
                prefab_Tran.parent = transform;
            }

            prefab_Tran.localPosition = localP;
            prefab_Tran.localRotation = localR;
            prefab_Tran.localScale = localS;

            EnableWndMargin(t);
        }

        AddWnd(layer, t);

        return t;
    }

    public void Close<T>() where T : UIWindowBase, new()
    {
        string name = typeof(T).FullName;
        Close(name);
    }

    public void Close(string name)
    {
        WindowInfo wndInfo = GetWindow(name);
        if (wndInfo != null)
        {
            wndInfo.wndBase.OnClose();
            m_Windows.Remove(wndInfo);
            UpdateState();
        }
    }

    public void CloseAllWnd()
    {
        List<WindowInfo> wndToClose = new List<WindowInfo>();
        foreach (WindowInfo wndInfo in m_Windows)
        {
            wndToClose.Add(wndInfo);
        }

        foreach (WindowInfo wndinfo in wndToClose)
        {
            Close(wndinfo.name);
        }
    }

    public int GetWndNum()
    {
        int num = 0;
        Type layers = typeof(EUILayer);
        Array uiLayerArray = Enum.GetValues(layers);

        for (int i = 0; i < uiLayerArray.Length; i++)
        {
            num += GetWndNumInLayer((EUILayer)uiLayerArray.GetValue(i));
        }
        return num;
    }
    public int GetWndNumInLayer(EUILayer layer)
    {
        int num = 0;
        foreach (WindowInfo wnd in m_Windows)
        {
            if (wnd.layer == layer)
            {
                num++;
            }
        }
        return num;
    }

    public WindowInfo GetWindow<T>() where T : UIWindowBase
    {
        string name = typeof(T).FullName;
        return GetWindow(name);
    }
    public WindowInfo GetWindow(string name)
    {
        foreach (WindowInfo wndInfo in m_Windows)
        {
            if (name == wndInfo.name)
            {
                return wndInfo;
            }
        }
        return null;
    }
    
    public bool IsOpen<T>() where T : UIWindowBase
    {
        WindowInfo wnd = GetWindow<T>();
        if (wnd == null)
        {
            return false;
        }
        return true;
    }

    public string GetPrefabPath<T>()
    {
        string name = typeof(T).FullName;
        return GetPrefabPath(name);
    }
    public string GetPrefabPath(string name)
    {
        foreach(PrefabSaveModle.PrefabPath path in prefabPath.windowPath)
        {
            if (name == path.name)
                return path.path;
        }
        Debug.LogError("NO PATH FOUND! Check Your Prefab Name!");
        return "";
    }

    private void EnableWndMargin (UIWindowBase wnd)
    {
        if (wnd.CanMarginClick)
        {
            GameObject prefab = GameObject.Instantiate(Resources.Load(GetPrefabPath<UIWndMargin>())) as GameObject;
            if (prefab == null)
            {
                Debug.LogError("Window Margin Prefab Disappear!");
                return;
            }
            prefab.transform.parent = m_UILayers[EUILayer.WndLayer].transform;
            prefab.transform.localPosition = Vector3.zero;
            prefab.transform.localRotation = Quaternion.identity;
            prefab.transform.localScale = Vector3.one;
            UIWndMargin mask = new UIWndMargin();
            mask.Create(prefab, wnd.panel.depth - 300);
            wnd.margin = mask;
            mask.onMaskClicked = wnd.ClickMargin;
        }
    }
	
    private void EnableBackgroundMask(bool IsMask)
    {
        if (IsMask)
        {
            if (GetWindow<UIBackGroundMask>() == null)
            {
                var maskWnd = Open<UIBackGroundMask>(EUILayer.WndLayer);
                maskWnd.panel.depth = (int)EUILayer.WndLayer;
            }
        }
        else
        {
            Close<UIBackGroundMask>();
        }
    }

    private void AddLayer(EUILayer layer)
    {
        Transform layerTran = new GameObject(layer.ToString()).transform;
        layerTran.parent = transform;
        layerTran.localPosition = Vector3.zero;
        layerTran.localRotation = Quaternion.identity;
        layerTran.localScale = Vector3.one;
        layerTran.name = layer.ToString();

        UIPanel layerPanel = layerTran.gameObject.AddComponent<UIPanel>();
        layerPanel.depth = (int)layer;

        m_UILayers.Add(layer, layerTran);
    }

    private void AddWnd(EUILayer layer, UIWindowBase wnd)
    {
        foreach (WindowInfo wndInfo in m_Windows)
        {
            if (wndInfo.name == wnd.wndRoot.name)
                return;
        }

        WindowInfo window = new WindowInfo()
        {
            layer = layer,
            name = wnd.wndRoot.name,
            wndBase = wnd
        };
        m_Windows.Add(window);

        UpdateState();
    }

    private void UpdateState()
    {
        IsMovable = false;
        IsMask = false;
        foreach (WindowInfo window in m_Windows)
        {
            if (window.wndBase.IsMask == true)
                IsMask = true;
            if (window.wndBase.IsMovable == true)
                IsMovable = true;
        }

        EnableBackgroundMask(IsMask);
    }

	private void Update () 
    {
	    foreach (WindowInfo wnd in m_Windows)
	    {
            wnd.wndBase.Update();
	    }
	}
}
