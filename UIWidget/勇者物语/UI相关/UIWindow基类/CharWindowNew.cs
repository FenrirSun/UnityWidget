using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 人物界面控制脚本
/// </summary>
//[UIWndAttributes("UINew/CharWnd/CharWindowNew", flags = UIWndFlags.BackGroundMask)]
public class CharWindowNew : UIBaseWnd
{
    public CharInfo CharInfoBoard;             //人物信息显示面板
    public CharEquipPanel EquipList;           //装备列表
    public EquipInfoPanel equipInfo;      //装备详情面板
    public GameObject blackMask = null;   //显示详情时的黑色遮罩

    public UIPlayer uiPlayer = null;           //UI克隆人物

    private O_UILabel LevelNum = null;
    private O_UILabel namelab = null;
    private O_UILabel combatePower = null;
    private O_UISprite VIPLevel = null;
    private O_UILabel HPNum = null;
    private O_UILabel ATKNum = null;
    private O_UILabel FDEFNum = null;
    private O_UILabel EDEFNum = null;
    private O_UISlider EXPPercent = null;

    private O_UILabel m_StrengthNum;
    private O_UILabel m_GoldNum;
    private O_UILabel m_DiamondNum;

    private bool isCharDetailShow = false;
    private GameObject selectedSlot = null;     //选中的装备槽
    //是否有更强力的装备的提示"New"
    bool[] equipNewTips = new bool[] { false, false, false, false, false, false };
    //装备槽位对应slot的映射
    //映射使用时slottype+1，UI对应序号-1，10代表无对应
    int[] equipTypeIndex = new int[] { -1, 1, 0, 10, 4, 10, 2, 3, 5, 10, 10 ,10};
    //All = 0,       //旧   新
    //Hat = 1,        //项链 头盔
    //Necklace,       //头盔 项链
    //Clothes,        //腰带 衣服
    //Bracer,         //武器 护腕
    //Weapon,         //护腕 武器
    //Ring,           //衣服 戒指
    //Fashion,        //戒指 时装（用原裤子替代）
    //Wing,           //裤子 神翼（腰带替代）
    ResonateInfo _resonateInfo { 
        get 
        {
            return GameRecord.Instance().GetCurRecord().GetNetPacketInfo().GetResonateInfo();
        } 
    }

    protected override void OnOpen()
    {
        ServerPlayerMgr.UpdateMainPlayerItemPackage();
        base.OpenWnd();
        CharInfoBoard = new CharInfo();
        CharInfoBoard.Init(WndRootGameobject.transform.Find("CharDetailPanel/CharDetailObject"));
        CharInfoBoard.InfoPanel.SetActive(false);

        EquipList = new CharEquipPanel();
        EquipList.Init(WndRootGameobject.transform.Find("EquipListPanel"));

        equipInfo = new EquipInfoPanel();
        equipInfo.Resonate = _resonateInfo;
        equipInfo.Init(WndRootGameobject.transform.Find("EquipInfo").gameObject);

        m_StrengthNum = WndRootGameobject.transform.Find("Title/StrengthNum").GetComponent<O_UILabel>();
        m_GoldNum = WndRootGameobject.transform.Find("Title/GoldNum").GetComponent<O_UILabel>();
        m_DiamondNum = WndRootGameobject.transform.Find("Title/DiamondNum").GetComponent<O_UILabel>();

        //设置自适应的参考照相机
        Camera cam = WndRootGameobject.transform.parent.parent.Find("View Camera").GetComponent<Camera>() as Camera;
        WndRootGameobject.transform.Find("CloseBtn").GetComponent<O_UIAnchor>().uiCamera = cam;
        WndRootGameobject.transform.Find("Title").GetComponent<O_UIAnchor>().uiCamera = cam;

        FreshEquipItems();
        UpdateProperty();
        ClonePlayer();
        AddListener();
        SetStateOne();

    }

    public void Reflash()
    {
        ServerPlayerMgr.UpdateMainPlayerItemPackage();
        FreshEquipItems();
        UpdateProperty();
        EquipList.Reflash();
        equipInfo.Resonate = _resonateInfo;
        equipInfo.Show(false);
        blackMask.SetActive(false);
    }

    public void UpdateProperty()
    {
        //设置名字
        namelab = WndRootGameobject.transform.Find("CharacterPanel/NameLabel").gameObject.GetComponent<O_UILabel>();
        namelab.text = CommonFun.GetName(GameApp.GetWorldManager().MainPlayer.GetProperty().ActorName);
        //设置角色等级
        LevelNum = WndRootGameobject.transform.Find("CharacterPanel/CapacityPanel/LevelLabel").gameObject.GetComponent<O_UILabel>();
        LevelNum.text = GameApp.GetWorldManager().MainPlayer.GetProperty().Level.ToString();
        //设置角色战斗力
        combatePower = WndRootGameobject.transform.Find("CharacterPanel/CombatPower/Label").gameObject.GetComponent<O_UILabel>();
        combatePower.text = GameApp.GetWorldManager().MainPlayer.GetProperty().FightValue.ToString();
        //设置VIP等级
        SetVipSprite();
        //设置面板HP
        HPNum = WndRootGameobject.transform.Find("CharacterPanel/CapacityPanel/HP").gameObject.GetComponent<O_UILabel>();
        HPNum.text = GameApp.GetWorldManager().MainPlayer.GetProperty().AttackProp.dwMaxHP.ToString();
        //设置面板攻击力
        if (GameApp.GetWorldManager().MainPlayer.GetProperty().Job == 2)
        {
            O_UISprite atkSprite = WndRootGameobject.transform.Find("CharacterPanel/CapacityPanel/Sprite_2").gameObject.GetComponent<O_UISprite>();
            atkSprite.spriteName = "common_icon_Physical attack";
            ATKNum = WndRootGameobject.transform.Find("CharacterPanel/CapacityPanel/ATK").gameObject.GetComponent<O_UILabel>();
            ATKNum.text = GameApp.GetWorldManager().MainPlayer.GetProperty().AttackProp.dwPhysicalDamage.ToString();
        }
        else
        {
            O_UISprite atkSprite = WndRootGameobject.transform.Find("CharacterPanel/CapacityPanel/Sprite_2").gameObject.GetComponent<O_UISprite>();
            atkSprite.spriteName = "common_icon_Magic attack";
            ATKNum = WndRootGameobject.transform.Find("CharacterPanel/CapacityPanel/ATK").gameObject.GetComponent<O_UILabel>();
            ATKNum.text = GameApp.GetWorldManager().MainPlayer.GetProperty().AttackProp.dwMagicDamage.ToString();
        }
        //设置面板物理防御
        FDEFNum = WndRootGameobject.transform.Find("CharacterPanel/CapacityPanel/F_DEF").gameObject.GetComponent<O_UILabel>();
        FDEFNum.text = GameApp.GetWorldManager().MainPlayer.GetProperty().AttackProp.dwPhysicalDefence.ToString();
        //设置面板魔法防御
        EDEFNum = WndRootGameobject.transform.Find("CharacterPanel/CapacityPanel/M_DEF").gameObject.GetComponent<O_UILabel>();
        EDEFNum.text = GameApp.GetWorldManager().MainPlayer.GetProperty().AttackProp.dwMagicDefence.ToString();
        //设置面板exp条
        EXPPercent = WndRootGameobject.transform.Find("CharacterPanel/CapacityPanel/EXPBar").gameObject.GetComponent<O_UISlider>();
        EXPPercent.sliderValue = (float)GameApp.GetWorldManager().MainPlayer.GetProperty().CurrentEXP / (float)GameApp.GetWorldManager().MainPlayer.GetProperty().EXP;

        RefreshCommon();
    }

    public void FreshEquipItems()
    {
        for (int i = 0; i <= 6; i++ )
        {
            Transform tran = WndRootGameobject.transform.Find("CharacterPanel/EquipSlotPanel/Button_" + i);
            tran.Find("colour").gameObject.SetActive(false);
            tran.Find("Sprite").gameObject.SetActive(false);
            tran.Find("Selected").gameObject.SetActive(false);
            tran.Find("Slot").gameObject.SetActive(false);
            tran.Find("level").gameObject.SetActive(false);
            tran.Find("LevelSprite").gameObject.SetActive(false);
            WndRootGameobject.transform.Find("TipsPanel/Arrow_" + (i + 1).ToString()).gameObject.SetActive(false);
        }
        foreach (NetItemInfo item in GameRecord.Instance().GetCurRecord().GetNetPacketInfo().dicEquipInUse.Values)
            {
                SetEquipItem(item);
            }
    }

    private void SetEquipItem(NetItemInfo item)
    {
        //如果该装备槽没有装备，将相关信息置空
        if (item.ID <= 0 || item.ID >= NetItemInfo.EMPTY_EQUIP_SITE)
            return;
        DBItemBaseProp db = DBItem.Get(item.ID);
        Transform tran = WndRootGameobject.transform.Find("CharacterPanel/EquipSlotPanel/Button_" + equipTypeIndex[db.SlotType + 1].ToString());
        // 根据slotType来对应显示位置，而不是通过item.index来对应
        if (equipTypeIndex[db.SlotType + 1] != 10)
        {
            tran.Find("colour").gameObject.SetActive(true);
            tran.Find("Sprite").gameObject.SetActive(true);
            tran.Find("Slot").gameObject.SetActive(true);
            tran.Find("level").gameObject.SetActive(true);
            tran.Find("LevelSprite").gameObject.SetActive(true);
            O_UIEventListener.Get(tran.gameObject).onClick = OnEquipClicked;
            // 设置品质
            switch (db.Rank)
            {
                case 1:
                    CommonFun.SetSprite(tran.gameObject, "colour", "common_qualityB");
                    break;
                case 2:
                    CommonFun.SetSprite(tran.gameObject, "colour", "common_qualityC");
                    break;
                case 3:
                    CommonFun.SetSprite(tran.gameObject, "colour", "common_qualityD");
                    break;
                case 4:
                    CommonFun.SetSprite(tran.gameObject, "colour", "common_qualityF");
                    break;
                default:
                    CommonFun.SetSprite(tran.gameObject, "colour", "common_qualityA");
                    break;
            }
            // 设置Icon
            O_UISprite icon = (tran.Find("Sprite").GetComponent("O_UISprite") as O_UISprite);
            icon.atlas = IconResourceMgr.Instance().GetAtlasByPath(db.AtlasPath);
            icon.spriteName = db.SpriteName;
            icon.enabled = true;
            //设置等级信息
            SetItemLevel(tran, item);
            //设置宝石信息
            SetCrystalState(tran.Find("Slot"), item, db);
            //设置升级提示箭头
            if (item.Lvl < GameApp.GetWorldManager().MainPlayer.GetProperty().Level && equipTypeIndex[db.SlotType + 1] <= 6 )
            {
                //零时代码，由于装备槽修改，现在没有slottype为7的槽
                if (TipEquipUp())
                    WndRootGameobject.transform.Find("TipsPanel/Arrow_" + (equipTypeIndex[db.SlotType + 1] + 1).ToString()).gameObject.SetActive(true);
                else
                    WndRootGameobject.transform.Find("TipsPanel/Arrow_" + (equipTypeIndex[db.SlotType + 1] + 1).ToString()).gameObject.SetActive(false);
            }
            if(equipNewTips[equipTypeIndex[db.SlotType + 1]])
                WndRootGameobject.transform.Find("TipsPanel/New_" + (equipTypeIndex[db.SlotType + 1] + 1).ToString()).gameObject.SetActive(true);
            else
                WndRootGameobject.transform.Find("TipsPanel/New_" + (equipTypeIndex[db.SlotType + 1] + 1).ToString()).gameObject.SetActive(false);
        }
    }

    private void SetVipSprite()
    {
        VIPLevel = WndRootGameobject.transform.Find("CharacterPanel/VIPSprite").gameObject.GetComponent<O_UISprite>();
        int level = GameApp.GetWorldManager().MainPlayer.GetProperty().VipLevel;
        if(level == 0)
        {
            VIPLevel.gameObject.SetActive(false);
            return;
        }
        if(level <= 9)
        {
            VIPLevel.spriteName = "VIP00" + level.ToString();
            VIPLevel.transform.localScale = new Vector3(38.4f, 36f, 1f);
            VIPLevel.gameObject.SetActive(true);
            return;
        }
        if(level >= 10)
        {
            VIPLevel.spriteName = "VIP0" + level.ToString();
            VIPLevel.transform.localScale = new Vector3(52.8f, 36f, 1f);
            VIPLevel.gameObject.SetActive(true);
            return;
        }
    }

    private void AddListener()
    {
        Transform CloseWnd = WndRootGameobject.transform.Find("CloseBtn/CloseBtn");
        if (CloseWnd != null)
            O_UIEventListener.Get(CloseWnd.gameObject).onClick = OnCloseWnd;

        Transform ShowDetail = WndRootGameobject.transform.Find("CharacterPanel/CapacityPanel/ShowDetailBtn");
        if (ShowDetail != null)
            O_UIEventListener.Get(ShowDetail.gameObject).onClick = OnCharDetailClicked;

        Transform add_gold_btn = WndRootGameobject.transform.Find("Title/AddGold");
        if (add_gold_btn != null)
            O_UIEventListener.Get(add_gold_btn.gameObject).onClick = AddGold;

        Transform add_diamond_btn = WndRootGameobject.transform.Find("Title/AddDiamond");
        if (add_diamond_btn != null)
            O_UIEventListener.Get(add_diamond_btn.gameObject).onClick = AddDiamond;

        Transform add_strength_btn = WndRootGameobject.transform.Find("Title/AddStrength");
        if (add_strength_btn != null)
            O_UIEventListener.Get(add_strength_btn.gameObject).onClick = AddStrength;
    }

    void OnCloseWnd(GameObject go)
    {
        CloseWnd();
    }

    protected override void OnClose()
    {
        base.OnClose();
        ItemSlot.Reset();
        equipNewTips = new bool[] { false, false, false, false, false, false };
    }

    protected override void OnBeforeClose()
    {
        if (uiPlayer != null)
        {
            uiPlayer.Release();
            uiPlayer.CanBeDeleted = true;
            uiPlayer = null;
        }

        GameObject.Destroy(blackMask);
        GameObject.Destroy(selectedSlot);
        CharInfoBoard = null;

        EquipList = null;
        equipInfo = null;

        blackMask = null;
        selectedSlot = null;
        LevelNum = null;
        namelab = null;
        combatePower = null;
        VIPLevel = null;
        HPNum = null;
        ATKNum = null;
        FDEFNum = null;
        EDEFNum = null;
        EXPPercent = null;

    }

    //装备槽按钮，点击后显示装备详情
    private void OnEquipClicked(GameObject go)
    {
        string str = go.name.Substring(go.name.LastIndexOf('_') + 1, go.name.Length - go.name.LastIndexOf('_') - 1);
        int index = System.Array.IndexOf(equipTypeIndex, System.Convert.ToInt32(str)) - 1;
        NetItemInfo item = null;
        GameRecord.Instance().GetCurRecord().GetNetPacketInfo().dicEquipInUse.TryGetValue(index, out item);
        if (item == null || item.ID <= 0 || item.ID >= 9999)
        {
            return;
        }

        if(selectedSlot != null)
        {
            selectedSlot.SetActive(false);
            selectedSlot = go.transform.Find("Selected").gameObject;
            selectedSlot.SetActive(true);
        }
        else
        {
            selectedSlot = go.transform.Find("Selected").gameObject;
            selectedSlot.SetActive(true);
        }

        ShowEquipInfo(index);
    }

    public void ShowEquipInfo(int index, bool openFromSlot = true)
    {
        SetStateOne();
        blackMask = WndRootGameobject.transform.Find("BlackMask").gameObject;
        equipInfo.Show(true);
        equipInfo.Fresh(index);
        blackMask.SetActive(true);
        //blackMask.transform.localPosition = new Vector3(0f, 0f, 6f);

    }

    public void ShowEquipInfoFromList(NetItemInfo item)
    {
        SetStateTwo();
        equipInfo.ShowLeft(true);
        equipInfo.Fresh(item);
        blackMask = WndRootGameobject.transform.Find("BlackMask").gameObject;
        blackMask.SetActive(true);
        //blackMask.transform.localPosition = new Vector3(-475f, 0f, -6f);
    }

    private void OnCharDetailClicked(GameObject go)
    {
        if (isCharDetailShow == false)
            CoroutineProvider.Instance().StartCoroutine(ShowCharDetail());
        else
            CoroutineProvider.Instance().StartCoroutine(UnshowCharDetail());
    }


    IEnumerator ShowCharDetail()
    {
        CharInfoBoard.InfoPanel.SetActive(true);
        O_TweenPosition tPos = CharInfoBoard.InfoPanel.GetComponent<O_TweenPosition>() as O_TweenPosition;
        tPos.from = new Vector3(0f, -521f, 0f);
        tPos.to = new Vector3(0f, 0f, 0f);
        tPos.Play(true);
        yield return new WaitForSeconds(0.3f);

        WndRootGameobject.transform.Find("CharacterPanel/CapacityPanel/ShowDetailBtn/Background").gameObject.GetComponent<O_UISprite>().spriteName = "person_buttonB";
        isCharDetailShow = true;
        yield return 1;
    }

    IEnumerator UnshowCharDetail()
    {
        O_TweenPosition tPos = CharInfoBoard.InfoPanel.GetComponent<O_TweenPosition>() as O_TweenPosition;
        tPos.from = new Vector3(0f, -521f, 0f);
        tPos.to = new Vector3(0f, 0f, 0f);
        tPos.Play(false);
        yield return new WaitForSeconds(0.3f);
        CharInfoBoard.InfoPanel.SetActive(false);

        WndRootGameobject.transform.Find("CharacterPanel/CapacityPanel/ShowDetailBtn/Background").gameObject.GetComponent<O_UISprite>().spriteName = "person_buttonA";
        isCharDetailShow = false;
        yield return 1;
    }

    private void RefreshCommon()
    {
        m_StrengthNum.text = string.Format("[ffffff]{0}[-][f7a86e]/{1}[-]", GameRecord.Instance().GetCurRecord().GetBuyInfo().m_nCurStrength.ToString(),
                         GameRecord.Instance().GetCurRecord().GetBuyInfo().m_nMaxStrength.ToString());
        m_GoldNum.text = GameApp.GetWorldManager().MainPlayer.GetProperty().GoldNumber.ToString();
        m_DiamondNum.text = GameApp.GetWorldManager().MainPlayer.GetProperty().Diamond.ToString();
    }

    private void AddGold(GameObject go)
    {
        GameApp.GetNetHandler().OpenAddGold();
    }
    private void AddDiamond(GameObject go)
    {
        OperationTipsUI.Instance().PopTipsWithMask(O_Localization.instance.GetText("TipsLocal", 20));
        GameApp.GetNetHandler().SendDiamondShop();
    }
    private void AddStrength(GameObject go)
    {
        GameApp.GetNetHandler().OpenAddTili();
    }
    public static void SetItemLevel(Transform root, NetItemInfo item)
    {
        if(item.Lvl == 0)
        {
            root.Find("LevelSprite").gameObject.SetActive(false);
            root.Find("level").gameObject.SetActive(false);
        }
        else
        {
            root.Find("LevelSprite").gameObject.SetActive(true);
            root.Find("level").gameObject.SetActive(true);
            CommonFun.SetLabelText(root.gameObject, "level", item.Lvl.ToString());
        }
    }
    public static void SetCrystalState(Transform root, NetItemInfo itemInfo, DBItemBaseProp itemBase)
    {
        int gemType = -1;
        int crystalValue = 0;
        for (int i = 1; i <= 4; i++)
        {
            switch(i)
            {
                case 1:
                    crystalValue = itemInfo.Crystal1;
                    gemType = itemBase.gemType1;
                    break;
                case 2:
                    crystalValue = itemInfo.Crystal2;
                    gemType = itemBase.gemType2;
                    break;
                case 3:
                    crystalValue = itemInfo.Crystal3;
                    gemType = itemBase.gemType3;
                    break;
                default:
                    crystalValue = itemInfo.Crystal4;
                    gemType = itemBase.gemType4;
                    break;
            }
            Transform tranCrystal = root.Find(string.Format("Slot_{0}", i));
            if(gemType <= 0)
            {
                tranCrystal.gameObject.SetActive(false);
                continue;
            }
            tranCrystal.gameObject.SetActive(true);
            // 0 未开锁
            // 1 开锁 为空
            // 2 开锁 大于1的宝石ID
            if (crystalValue == 0)
            {
                tranCrystal.Find("jewel").gameObject.SetActive(false);
                tranCrystal.Find("Gem").gameObject.SetActive(false);
                tranCrystal.Find("GemLock").gameObject.SetActive(true);
            }
            else if (crystalValue == 1)
            {
                tranCrystal.Find("jewel").gameObject.SetActive(false);
                tranCrystal.Find("Gem").gameObject.SetActive(true);
                tranCrystal.Find("GemLock").gameObject.SetActive(false);
            }
            else
            {
                tranCrystal.Find("Gem").gameObject.SetActive(true);
                tranCrystal.Find("GemLock").gameObject.SetActive(false);
                // 设置宝石图标
                JewelItem item = DBManager.GetDBJewel().Get(crystalValue);
                O_UISprite icon = (tranCrystal.Find("jewel").GetComponent("O_UISprite") as O_UISprite);
                icon.atlas = IconResourceMgr.Instance().GetAtlasByPath(item.AtlasPath);
                icon.spriteName = item.SpriteName;
            }
        }
    }

    //主角克隆，放到UI面板上
    void ClonePlayer()
    {
        if (uiPlayer == null)
        {
            uiPlayer = new UIPlayer();
            uiPlayer.CloneFromInstance(GameApp.GetWorldManager().MainPlayer);
            uiPlayer.m_ObjInstance.name = "UIPlayer";
            uiPlayer.m_ObjInstance.AddComponent<O_SpinWithMouse>();
            CapsuleCollider col = uiPlayer.m_ObjInstance.AddComponent<CapsuleCollider>();
            col.height = 2f;
            col.center = new Vector3(0f, 1f, 0f);

            GameApp.GetWorldManager().UIPlayers.Add(uiPlayer);
            Transform t = uiPlayer.m_ObjInstance.transform;
            t.parent = WndRootGameobject.transform.Find("CharacterPanel");
            t.localPosition = new Vector3(-229f, -162f, 360f);
            //t.localPosition = new Vector3(-229f, -132f, -200f);
            t.localRotation = Quaternion.Euler(0f, 160f, 0f);
            t.localScale = new Vector3(175f, 175f, 175f);
            CommonFun.SetLayer(uiPlayer.m_ObjInstance.transform, WndRootGameobject.layer);
            uiPlayer.SetLayer(ML2Layer.LayerUI);
        }
    }

    private void SetStateOne()
    {
        //三个在前面的面板
        Transform characterPanel = WndRootGameobject.transform.Find("CharacterPanel");
        Transform CharDetailPanel = WndRootGameobject.transform.Find("CharDetailPanel");
        Transform tipsPanel = WndRootGameobject.transform.Find("TipsPanel");

        CharDetailPanel.localPosition = new Vector3(CharDetailPanel.localPosition.x,CharDetailPanel.localPosition.y, -600f);
        characterPanel.localPosition = new Vector3(characterPanel.localPosition.x,characterPanel.localPosition.y, -700f);
        tipsPanel.localPosition = new Vector3(tipsPanel.localPosition.x,tipsPanel.localPosition.y, -710f);
        //一个在后面的面板
        EquipList.InfoPanel.transform.localPosition = new Vector3(EquipList.InfoPanel.transform.localPosition.x,
            EquipList.InfoPanel.transform.localPosition.y, -50f);

    }

    private void SetStateTwo()
    {
        Transform characterPanel = WndRootGameobject.transform.Find("CharacterPanel");
        Transform CharDetailPanel = WndRootGameobject.transform.Find("CharDetailPanel");
        Transform tipsPanel = WndRootGameobject.transform.Find("TipsPanel");
        //三个在后面的面板
        CharDetailPanel.localPosition = new Vector3(CharDetailPanel.localPosition.x,CharDetailPanel.transform.localPosition.y, -50f);
        characterPanel.localPosition = new Vector3(characterPanel.localPosition.x,
            characterPanel.localPosition.y, -80f);
        tipsPanel.localPosition = new Vector3(tipsPanel.localPosition.x,
            tipsPanel.localPosition.y, -97f);
        //一个在前面的面板
        EquipList.InfoPanel.transform.localPosition = new Vector3(EquipList.InfoPanel.transform.localPosition.x,
            EquipList.InfoPanel.transform.localPosition.y, -131f);

    }

    public void UnsetSelectedSlot()
    {
        if (selectedSlot != null)
        {
            selectedSlot.SetActive(false);
            selectedSlot = null;
        }
    }

    public void CreateEquipTips(NetItemInfo info, bool visiable = true)
    {
        DBItemBaseProp db = DBItem.Get(info.ID);
        if (equipTypeIndex[db.SlotType + 1] < 0 || equipTypeIndex[db.SlotType + 1] > 6)
            return;
        equipNewTips[equipTypeIndex[db.SlotType + 1]] = visiable;
    }

    public bool TipEquipUp()
    {
        if (GameRecord.Instance().GetCurRecord().GetNetPacketInfo().GetDaoJuCount(11000) == 0
            && GameRecord.Instance().GetCurRecord().GetNetPacketInfo().GetDaoJuCount(11001) == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }


}
