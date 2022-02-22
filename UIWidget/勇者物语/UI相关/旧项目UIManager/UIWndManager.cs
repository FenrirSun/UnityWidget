using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

public class UIWndManager : MonoBehaviour {

    public enum eUILayer
    {
        MainLayer = 100000,//主摄像机
        NpcLayer = 200000,//NPC对话
        WndLayer = 300000,//窗口层
        Tips = 900000,
    }
    private static UIWndManager m_Instance;
    public static UIWndManager instance
    {
        get
        {
            if(m_Instance == null)
            {
                m_Instance = (UIWndManager)GameObject.FindObjectOfType(typeof(UIWndManager));
                if (m_Instance == null)
                {
                    UIRoot root = (UIRoot)GameObject.FindObjectOfType(typeof(UIRoot));

                    if (root == null)
                    {
                        UIPanel m_Root = NGUITools.CreateUI(false);
                        m_Instance = m_Root.gameObject.AddComponent<UIWndManager>();
                        InitUIRoot(m_Root.GetComponent<UIRoot>());
                    }
                    else
                    {
                        m_Instance = root.gameObject.AddComponent<UIWndManager>();
                        InitUIRoot(root);
                    }
                }

                m_Instance.Init();
            }

            return m_Instance;
        }
    }
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
    private Dictionary<eUILayer, Transform> m_ChildrenRoot = new Dictionary<eUILayer, Transform>();
    private UIPanel m_Root;
    private Dictionary<eUILayer, Dictionary<int, UIWndBase>> m_Wnds = new Dictionary<eUILayer, Dictionary<int, UIWndBase>>();
    /// <summary>
    /// 初始化，把UI装备好
    /// </summary>
    public void Init()
    {
        //m_Root = NGUITools.CreateUI(false);
        if (m_Root == null) { m_Root = GetComponent<UIPanel>(); }
        Type layers = typeof(eUILayer);
        Array uiLayerArray = Enum.GetValues(layers);

        for (int i = 0; i < uiLayerArray.Length; i++ )
        {
            AddLayer((eUILayer)Enum.Parse(typeof(eUILayer), uiLayerArray.GetValue(i).ToString()));
        }

        GameObject.DontDestroyOnLoad(m_Root.gameObject);
    }
    public static void InitUIRoot(UIRoot root)
    {
        root.scalingStyle = UIRoot.Scaling.ConstrainedOnMobiles;
        root.fitHeight = true;
        root.fitWidth = true;
        root.manualHeight = 720;
        root.manualWidth = 1136;
    }
    private void AddLayer(eUILayer layer)
    {
        Transform layerTran = new GameObject(layer.ToString()).GetComponent<Transform>();
        m_ChildrenRoot.Add(layer, layerTran);

        layerTran.parent = m_Root.gameObject.GetComponent<Transform>();
        layerTran.localPosition = Vector3.zero;
        layerTran.localRotation = Quaternion.identity;
        layerTran.localScale = Vector3.one;

        m_Wnds.Add(layer, new Dictionary<int, UIWndBase>());
    }
    private void AddWnd(eUILayer layer, UIWndBase wnd)
    {
        if (!m_Wnds.ContainsKey(layer))
        {
            m_Wnds.Add(layer, new Dictionary<int, UIWndBase>());
        }
        m_Wnds[layer].Add(wnd.id, wnd);
        UpdateState();
    }
    private void UpdateState()
    {
        IsMovable = true;
        IsMask = false;
        foreach (KeyValuePair<eUILayer, Dictionary<int, UIWndBase>> layer in m_Wnds)
        {
            foreach (KeyValuePair<int, UIWndBase> wnd in layer.Value)
            {
                if (wnd.Value.isMovable == false) { IsMovable = false; }

                var uiAttributes = (UIWndAttributes)(wnd.Value.GetType()).GetCustomAttributes(typeof(UIWndAttributes), true).FirstOrDefault();

                if (uiAttributes != null)
                {
                    if ((uiAttributes.flags & UIWndFlags.BackGroundMask) != 0)
                    {
                        IsMask = true;
                    }
                }
            }
        }

        EnableMask(IsMask);
    }
    private int GetNextPanelID(eUILayer layer)
    {
        int id = (int)layer;

        if (m_Wnds.ContainsKey(layer))
        {
            foreach (var it in m_Wnds[layer])
            {
                if (id < it.Value.id)
                {
                    id = it.Value.id;
                }
            }
        }
        else
        {
            m_Wnds.Add(layer, new Dictionary<int, UIWndBase>());
        }

        return (id + 100);
    }
    public T Open<T>(eUILayer layer) where T : UIWndBase, new()
    {
        return Open<T>( layer, null);
    }
    public T Open<T>(eUILayer layer, object data) where T : UIWndBase, new()
    {
        T t = new T();

        var uiAttributes = (UIWndAttributes)typeof(T).GetCustomAttributes(typeof(UIWndAttributes), true).FirstOrDefault();

        if(uiAttributes != null)
        {
            GameObject prefab = GameObject.Instantiate(Resources.Load(uiAttributes.path)) as GameObject;

            t.Create(layer, prefab, GetNextPanelID(layer), data);

            Transform prefab_Tran = prefab.GetComponent<Transform>();

            Vector3 localP = prefab_Tran.position;
            Quaternion localR = prefab_Tran.rotation;
            Vector3 localS = prefab_Tran.localScale;

            if (m_ChildrenRoot.ContainsKey(layer))
            {
                prefab_Tran.parent = m_ChildrenRoot[layer];
            }
            else
            {
                prefab_Tran.parent = m_Root.GetComponent<Transform>();
            }

            prefab_Tran.localPosition = localP;
            prefab_Tran.localRotation = localR;
            prefab_Tran.localScale = localS;

            if((uiAttributes.flags & UIWndFlags.BackGroundMask) != 0)
            {
                AddPanelMask(t);
            }
        }

        AddWnd(layer, t);

        return t;
    }
    public void Close(UIWndBase wnd)
    {
        wnd.Close();
        m_Wnds[wnd.layer].Remove(wnd.id);
        UpdateState();
    }
    public void Close(Type wndType)
    {
        List<UIWndBase> wnds = new List<UIWndBase>();
        foreach (KeyValuePair<eUILayer, Dictionary<int, UIWndBase>> layer in m_Wnds)
        {
            foreach (KeyValuePair<int, UIWndBase> wnd in layer.Value)
            {
                if(wndType == wnd.Value.GetType())
                {
                    wnds.Add(wnd.Value);
                }
            }
        }
        for (int i = 0; i < wnds.Count; i++ )
        {
            Close(wnds[i]);
        } 
    }

    private void EnableMask(bool open)
    {
        GameApp.GetUIManager().EnableBackgroundMask(open);
    }
    public UIWndBase GetWnd(Type wndType)
    {
        foreach (KeyValuePair<eUILayer, Dictionary<int, UIWndBase>> layer in m_Wnds)
        {
            foreach (KeyValuePair<int, UIWndBase> wnd in layer.Value)
            {
                if (wndType == wnd.Value.GetType())
                {
                    return wnd.Value;
                }
            }
        }
        return null;
    }
    public UIWndBase[] GetWnds(Type wndType)
    {
        List<UIWndBase> wnds = new List<UIWndBase>();

        foreach (KeyValuePair<eUILayer, Dictionary<int, UIWndBase>> layer in m_Wnds)
        {
            foreach (KeyValuePair<int, UIWndBase> wnd in layer.Value)
            {
                if (wndType == wnd.Value.GetType())
                {
                    wnds.Add(wnd.Value);
                }
            }
        }

        return wnds.ToArray();
    }
    public void AddPanelMask(UIWndBase wnd)
    {
        GameObject mask = GameObject.Instantiate(Resources.Load("Extranal/DefaultPrefab/LayerMask")) as GameObject;
        UIMaskDependent dependent = mask.GetComponent<UIMaskDependent>();
        if(dependent == null)
        {
            dependent = mask.AddComponent<UIMaskDependent>();
        }
        dependent.parent = wnd.wnd;

        mask.transform.parent = wnd.wnd.GetComponent<Transform>().parent;

        mask.GetComponent<UIPanel>().depth = wnd.wnd.GetComponent<UIPanel>().depth - 1;

        UIEventListener.Get(mask.transform.FindChild("Image").gameObject).onClick = wnd.ClickMask;

        //如果ＵＩ修改完毕删除
        GameApp.GetUIManager().EnableBackgroundMask(true);
    }
    public void AddPanelMask(UIPanel panel)
    {
        GameObject mask = GameObject.Instantiate(Resources.Load("Extranal/DefaultPrefab/LayerMask")) as GameObject;
        UIMaskDependent dependent = mask.GetComponent<UIMaskDependent>();
        if (dependent == null)
        {
            dependent = mask.AddComponent<UIMaskDependent>();
        }
        dependent.parent = panel.gameObject;

        mask.transform.parent = panel.gameObject.GetComponent<Transform>().parent;

        mask.GetComponent<UIPanel>().depth = panel.gameObject.GetComponent<UIPanel>().depth - 1;

    }
    public bool IsOpen(Type wndType)
    {
        foreach(KeyValuePair<eUILayer, Dictionary<int, UIWndBase>> layer in m_Wnds)
        {
            foreach(KeyValuePair<int,UIWndBase> wnd in layer.Value)
            {
                if(wnd.GetType() == wndType)
                {
                    return true;
                }
            }
        }

        return false;
    }
    public bool IsOpen(UIWndBase wnd)
    {
        if (wnd == null) { return false; }

        if (m_Wnds.ContainsKey(wnd.layer) && m_Wnds[wnd.layer].ContainsKey(wnd.id)) { return true; }

        return false;
    }
    public void Update()
    {
        foreach (KeyValuePair<eUILayer, Dictionary<int, UIWndBase>> layer in m_Wnds)
        {
            foreach (KeyValuePair<int, UIWndBase> wnd in layer.Value)
            {
                wnd.Value.Update(Time.deltaTime);
            }
        }
    }
}
