using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI���ڻ���
/// </summary>
public class UIBaseWnd
{
    public static Dictionary<string, UIBaseWnd> DicWnd = new Dictionary<string, UIBaseWnd>();

    public static UIBaseWnd GetWndByName(string strName)
    {
        UIBaseWnd wnd = null;
        if (DicWnd.TryGetValue(strName, out wnd))
        {
            return wnd;
        }
        return null;
    }

    public enum eWndStatus
    {
        WS_CLOSE,
        WS_OPEN,
    }

    // UIManager��ݷ��ʽӿ�
    protected UIManager UIMgr
    {
        get { return GameApp.GetUIManager(); }
    }

    public GameObject WndRootGameobject; //����prefab��gameobject
    protected eWndStatus m_eWndStatus;   //����״̬
    protected GameObject PrefabAsset;    //Wnd��Prefab.Asset

#region �����ļ�UIWnd.xml�е�����
    public string m_strWndName;          //��������
    protected string m_strPrefabPath;    //����Prefab��Ӧ·��
    protected bool m_bMutex;             //�Ƿ�����������л����ϵ���򿪱�����ʱ�ر����������л����ϵ�Ĵ��ڣ�
    protected bool m_bModel;             //�Ƿ�ģ̬
    protected bool m_bBlurEffect;        //�򿪴���ʱ�Ƿ�������ģ��
    protected bool m_bCanCharMove;       //���ڴ�ʱ����ɷ��ƶ�,1�ǿ��ƶ�
#endregion

    private static string strMutexWnd;   //�洢��ǰ���ⴰ��
    protected Dictionary<string, O_UILabel> dicEditLabel = new Dictionary<string, O_UILabel>();   //���浱ǰ���ڵ����пɱ༭Label�ؼ�
    protected Dictionary<string, O_UISprite> dicEditSprite = new Dictionary<string, O_UISprite>();   //���浱ǰ���ڵ����пɱ༭Sprite�ؼ�

    /// <summary>
    /// ��ʼ������Ĭ�ϲ���
    /// </summary>
    /// <param name="strWndName"></param>
    public virtual void Init(UIWndData data)
    {
        m_strWndName     = data.WndName;
        m_strPrefabPath  = data.PrefabPath;
        m_bMutex         = data.Mutex;
        m_bModel         = data.Model;
        m_bBlurEffect    = data.BlurEffect;
        m_bCanCharMove   = data.CanCharMove;

    }

    /// <summary>
    /// �򿪴���
    /// </summary>
    public virtual void OpenWnd()
    {
        Debug.Log("OpenWnd=" + m_strWndName);
        if (m_strPrefabPath == null)
        {
            Debug.LogError("Open window " + m_strWndName + "fail!" + "Please check PrefabPath");
            return;
        }

        //�����ظ��򿪴���
        if (DicWnd.ContainsKey(m_strWndName) && DicWnd[m_strWndName].IsOpen())
        {
            Debug.LogWarning("Can't Open window more than once! m_strWndName=" + m_strWndName);
            return;
        }
        Debug.Log("OpenWnd111=" + m_strWndName);
        //�رյ�ǰ����Ĵ���
        if (strMutexWnd != null && strMutexWnd != m_strWndName)
        {
            GameApp.GetUIManager().CloseWnd(strMutexWnd);
            strMutexWnd = null;
        }

        //�򿪱�������
        if (m_bBlurEffect)
        {
            GameApp.GetUIManager().EnableBlurEffect(true);
            GameApp.GetUIManager().EnableBackgroundMask(true);
        }

        if (GameApp.GetUIManager().uiCamera != null)
        {

            GameObject camera = UICameraMgr.mInstance.GetUICameraByLevel(UICameraMgr.eUICameraLevel.Level_2);
            if (string.Equals(m_strWndName, "HUD") && !DicWnd["HUD"].IsOpen())
            {
                camera = GameApp.GetUIManager().uiCamera.gameObject;
            }
            O_UIAnchor anchor = camera.GetComponentInChildren<O_UIAnchor>(); 
            camera = anchor.gameObject;

            PrefabAsset = Resources.Load(m_strPrefabPath) as GameObject;
            OnPrefabLoaded();
            GameObject child = Object.Instantiate(PrefabAsset) as GameObject;
            if (child == null)
            {
                Debug.LogError("Instantiate window " + m_strWndName + "fail!" + "Please check P refabPath");
                return;
            }
            Vector3 originalPosition = child.transform.localPosition; 
            Vector3 originalScale = child.transform.localScale;
            Quaternion originalQuat = child.transform.localRotation;
            child.layer = (int)ML2Layer.LayerUI;

            Transform t = child.transform;
            t.parent = camera.transform;
            t.localPosition = originalPosition;
            t.localRotation = originalQuat;
            t.localScale = originalScale;

            WndRootGameobject = child; 

            if (m_bMutex)
            {
                strMutexWnd = m_strWndName;
            }
            SetWndStatus(eWndStatus.WS_OPEN);
        }

        if (GameApp.GetPlayerGuideManager() != null)
        {
            GameApp.GetPlayerGuideManager().EmitShowGuide();
        }

    }

    /// <summary>
    /// �رմ���
    /// </summary>
    public virtual void CloseWnd()
    {
       // Debug.Log("---------------------------------------------------------------------base close="+m_strWndName);
        OnBeforeClose();

        if (m_bBlurEffect)
        {
            GameApp.GetUIManager().EnableBlurEffect(false);
            GameApp.GetUIManager().EnableBackgroundMask(false);
        }

        if (WndRootGameobject != null)
        {
            Debug.Log("1111111111-----------------------base close=" + m_strWndName);
            GameObject.Destroy(WndRootGameobject);
            SetWndStatus(eWndStatus.WS_CLOSE);
            WndRootGameobject = null;
        }

        if (PrefabAsset != null)
        {
            PrefabAsset = null;
        }

        Resources.UnloadUnusedAssets();

     //   if (GameQuality.Instance().m_GameQuality == enumGameQuality.GQ_Fast)
      //  {
         //   Resources.UnloadUnusedAssets();
          //  System.GC.Collect();
     //   }
        if (strMutexWnd != null && strMutexWnd == m_strWndName)
        {
            strMutexWnd = null;
        }
        if (GameApp.GetPlayerGuideManager() != null)
        { 
            GameApp.GetPlayerGuideManager().EmitShowGuide();
        }
        dicEditLabel.Clear();
        dicEditSprite.Clear();
    }

    /// <summary>
    /// �ݹ鴢�洰�������õĿؼ�
    /// </summary>
    protected void SaveUIWidget(Transform transform)
    {

        foreach (Transform child in transform) 
        {
            if(child.name.Length > 9)
            {
                if (child.name.Substring(0, 9) == "EditLabel")
                    dicEditLabel.Add(child.name, child.GetComponent<O_UILabel>());
                if (child.name.Substring(0, 10) == "EditSprite")
                    dicEditSprite.Add(child.name, child.GetComponent<O_UISprite>());
            }
            
            SaveUIWidget(child);
        }
    }

    /// <summary>
    /// ��/�رմ���
    /// </summary>
    public virtual void SwitchWnd()
    {
       if (IsOpen())
       {
           CloseWnd();
       }
       else if (IsClose())
       {
           OpenWnd();
       }
    }

    /// <summary>
    /// ���ô��ڵ�ǰ״̬
    /// </summary>
    /// <param name="e"></param>
    public virtual void SetWndStatus(eWndStatus e)
    {
        UIManager uiManager = GameApp.GetUIManager();
        
        m_eWndStatus = e;
        switch (e)
        {
            case eWndStatus.WS_CLOSE:
                OnClose();
                uiManager.OnWndClosed(this);
                break;
            case eWndStatus.WS_OPEN:
                OnOpen();
                uiManager.OnWndOpened(this);
                break;
        }
    }

    /// <summary>
    /// ��ȡ����״̬
    /// </summary>
    /// <returns></returns>
    public virtual eWndStatus GetWndStatus()
    {
        return m_eWndStatus;
    }

    /// <summary>
    /// �����Ƿ����
    /// </summary>
    public virtual bool IsOpen()
    {
        return m_eWndStatus == eWndStatus.WS_OPEN;
    }

    /// <summary>
    /// �����Ƿ�ر���
    /// </summary>
    public virtual bool IsClose()
    {
        return m_eWndStatus == eWndStatus.WS_CLOSE;
    }

    /// <summary>
    /// ����
    /// </summary>
    public virtual void Update()
    {
    }

    /// <summary>
    /// ���ڴ�ʱ����ɷ��ƶ�
    /// </summary>
    /// <returns>true��ʾ���ƶ�</returns>
    public virtual bool CanCharMove()
    {
        return m_bCanCharMove;
    }

    /// <summary>
    /// SetDontDestroyOnLoad
    /// </summary>
    public virtual void SetDontDestroyOnLoad()
    {
        GameObject.DontDestroyOnLoad(WndRootGameobject);
    }

    /// <summary>
    /// ���ڴ�ʱ����
    /// </summary>
    protected virtual void OnOpen()
    {
        if (GameApp.GetSceneManager() != null && GameApp.GetSceneManager().m_currMap != null && (GameApp.GetSceneManager().m_currMap.m_SceneType == ESceneType.Scene_Level || GameApp.GetSceneManager().m_currMap.m_SceneType == ESceneType.Scene_Custom))
        {
            if (m_strWndName == "CharWindow" ||
                m_strWndName == "GemEmbedWnd" ||
                m_strWndName == "GemDigWnd" ||
                m_strWndName == "GemTradeWnd" ||
                m_strWndName == "GemAwardWnd" ||
                m_strWndName == "CoinTradeWnd" ||
                m_strWndName == "GameOptionWnd" ||
                m_strWndName == "ShoppingWnd" ||
                m_strWndName == "DailyPay" ||
                m_strWndName == "FirstPay" ||
                m_strWndName == "AwardWnd" ||
                m_strWndName == "EquipPackageWnd" ||
                m_strWndName == "EquipEatWnd" ||
                m_strWndName == "EquipChangeWnd" ||
                m_strWndName == "SkillChipPackageWnd" ||
                m_strWndName == "EquipShopWnd" ||
                m_strWndName == "RankingEquipWnd" ||
                m_strWndName == "RankingFightValueWnd" ||
                m_strWndName == "RankingPopularityWnd" ||
                m_strWndName == "GemTradeWnd" )
            {  
                Debug.Log("OnOpen time.scale = 0 " + m_strWndName);
                Time.timeScale = 0f;
            }
        }
    }

    /// <summary>
    /// ���ڹر�ʱ����
    /// </summary>
    protected virtual void OnClose()
    {
        if (GameApp.GetSceneManager() != null && GameApp.GetSceneManager().m_currMap != null && (GameApp.GetSceneManager().m_currMap.m_SceneType == ESceneType.Scene_Level || GameApp.GetSceneManager().m_currMap.m_SceneType == ESceneType.Scene_Custom))
        {
            if (m_strWndName == "CharWindow" ||
                    m_strWndName == "GemEmbedWnd" ||
                    m_strWndName == "GemDigWnd" ||
                    m_strWndName == "GemTradeWnd" ||
                    m_strWndName == "GemAwardWnd" ||
                    m_strWndName == "CoinTradeWnd" ||
                    m_strWndName == "GameOptionWnd" ||
                    m_strWndName == "ShoppingWnd" ||
                    m_strWndName == "DailyPay" ||
                    m_strWndName == "FirstPay" ||
                    m_strWndName == "AwardWnd" ||
                    m_strWndName == "EquipPackageWnd" ||
                    m_strWndName == "EquipEatWnd" ||
                    m_strWndName == "EquipChangeWnd" ||
                    m_strWndName == "SkillChipPackageWnd" ||
                    m_strWndName == "EquipShopWnd" ||
                    m_strWndName == "RankingEquipWnd" ||
                    m_strWndName == "RankingFightValueWnd" ||
                    m_strWndName == "RankingPopularityWnd" ||
                    m_strWndName == "GemTradeWnd" )
            {
                Debug.Log("OnClose time.scale = 1 " + m_strWndName);
                Time.timeScale = 1f;
            }
        }
        RewardItemMsg.CloseItemMsg();
    }

    /// <summary>
    /// ���ڹر�ǰ����
    /// </summary>
    protected virtual void OnBeforeClose()
    {

    }

    /// <summary>
    /// ��Դ������ϻص�
    /// </summary>
    protected virtual void OnPrefabLoaded()
    {

    }

}
