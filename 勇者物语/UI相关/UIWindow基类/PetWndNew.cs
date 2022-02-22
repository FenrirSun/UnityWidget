using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PetWndNew : UIBaseWnd
{
    private Dictionary<int, PetItem> dicPetMessage = new Dictionary<int, PetItem>();    //chongwu表里的数据，宠物ID从1开始
    private ServerPet petUpdateInfo = null;
    private DaoJuItem[] ResetItemArray ;    //重置道具列表
    private int curResetItemID = 0;
    private GameObject selectedResetItem = null;
    private DaoJuItem[] LevelUpItemArray;    //喂养道具列表
    private int curLevelUpItemID = 0;
    private GameObject selectedLevelUpItem = null;
    public int curFightoutPetId = 0;
    public int curShowPetID = 1;
    public bool canPetEvolve = false;    //判断当前宠物能否进化（仅元灵）
    public Pet_Info curPetInfo = null;
    private bool showSkillInfo = false;  //判断是显示技能详情还是技能升级

    private Material PetMaterial = null;
    private Transform InfoPanel = null;
    private Transform SkillPanel = null;
    private Transform SkillInfoPanel = null;
    private Transform petItemParent = null;
    private Transform SynthesisPanel = null;
    private Transform EvolvePanel = null;
    private Transform ResetPanel = null;
    private Transform LevelUpPanel = null;
    private GameObject BlackMask = null;
    private Transform petmodelFather = null;
    
    private O_UISlider cur_Exp;
    

    protected override void OnOpen()
    {
        base.OnOpen();
        AddEventListener();
        if (PetMaterial == null)
        {
            PetMaterial = Resources.Load("LogicPrefabs/Pet/UIPetMaterial", typeof(Material)) as Material;
        }
        SaveUIWidget(WndRootGameobject.transform);
        dicPetMessage = DBManager.GetDBPet().GetDicPet();
        petItemParent = WndRootGameobject.transform.Find("PetListPanel/ListPanel/Grid");
        InfoPanel = WndRootGameobject.transform.Find("InfoPanel");
        SkillPanel = WndRootGameobject.transform.Find("SkillPanel");
        SkillInfoPanel = WndRootGameobject.transform.Find("SkillInfoPanel");
        SynthesisPanel = WndRootGameobject.transform.Find("SynthesisPanel");
        EvolvePanel = WndRootGameobject.transform.Find("EvolvePanel");
        ResetPanel = WndRootGameobject.transform.Find("ResetPanel");
        BlackMask = WndRootGameobject.transform.Find("BlackMask").gameObject;
        LevelUpPanel = WndRootGameobject.transform.Find("LevelUpPanel");

        //面板Exp进度条
        cur_Exp = WndRootGameobject.transform.Find("InfoPanel/Title/EXPBar").GetComponent<O_UISlider>();

        petmodelFather = WndRootGameobject.transform.Find("InfoPanel/petModel");

        ResetItemArray = DBManager.GetDBDaoJu().GetItemsByTypeId(18);   // TypeId = 18的为宠物的重置道具
        LevelUpItemArray = DBManager.GetDBDaoJu().GetItemsByTypeId(19); // TypeId = 19的为宠粮

        //设置自适应的参考照相机
        Camera cam = WndRootGameobject.transform.parent.parent.Find("View Camera").GetComponent<Camera>() as Camera;
        WndRootGameobject.transform.Find("CloseBtn").GetComponent<O_UIAnchor>().uiCamera = cam;
        WndRootGameobject.transform.Find("Title").GetComponent<O_UIAnchor>().uiCamera = cam;

        if (curFightoutPetId != 0)
        {
            curShowPetID = curFightoutPetId;
            ShowPetItem();
            ReflashPetInfo(curFightoutPetId);
            ReflashSkillInfo(curFightoutPetId);
            ShowPetModel(curFightoutPetId);
        }
        else
        {
            ShowPetItem();
            ShowInfoOrSyns(curShowPetID);
        }
        RefreshCommon();
        ServerPetMgr.petUpdateDelegate = PetInfoUpdate;
    }

    private void AddEventListener()
    {
        Transform CloseWnd = WndRootGameobject.transform.Find("CloseBtn/CloseBtn");
        if (CloseWnd != null)
            O_UIEventListener.Get(CloseWnd.gameObject).onClick = OnCloseWnd;

        Transform UpgradeSkill = WndRootGameobject.transform.Find("SkillPanel/SkillBtnPanel/SkillUpgradeBtn");
        if (UpgradeSkill != null)
            O_UIEventListener.Get(UpgradeSkill.gameObject).onClick = OnUpgradeSkillClicked;

        Transform OnPressSkill = WndRootGameobject.transform.Find("SkillPanel/SkillBtnPanel/SkillUpgradeBtn");
        if (OnPressSkill != null)
            O_UIEventListener.Get(OnPressSkill.gameObject).onPress = OnPressSkillClicked;

        Transform ResetWndOpen = WndRootGameobject.transform.Find("InfoPanel/ResetBtn");
        if (ResetWndOpen != null)
            O_UIEventListener.Get(ResetWndOpen.gameObject).onClick = OnResetWndOpen;

        Transform OnBattle = WndRootGameobject.transform.Find("InfoPanel/BattleBtn");
        if (OnBattle != null)
            O_UIEventListener.Get(OnBattle.gameObject).onClick = OnOnBattleClicked;

        Transform OnFeedPet = WndRootGameobject.transform.Find("InfoPanel/LevelUpBtn");
        if (OnFeedPet != null)
            O_UIEventListener.Get(OnFeedPet.gameObject).onClick = OnLevelUpWndOpen;

        Transform OpenEvolvePanel = WndRootGameobject.transform.Find("InfoPanel/EvolveBtn");
        if (OpenEvolvePanel != null)
            O_UIEventListener.Get(OpenEvolvePanel.gameObject).onClick = OnEvolvePanelOpen;

        Transform PetSummon = WndRootGameobject.transform.Find("SynthesisPanel/SynthesisBtn");
        if (PetSummon != null)
            O_UIEventListener.Get(PetSummon.gameObject).onClick = OnPetSummonClicked;

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

    protected override void OnClose()
    {
        base.OnClose();
    }
    
    void OnCloseWnd(GameObject go)
    {
        curShowPetID = 1;
        curPetInfo = null;
        showSkillInfo = false;
        ServerPetMgr.petUpdateDelegate = null;
        selectedResetItem = null;
        selectedLevelUpItem = null;
        curLevelUpItemID = 0;
        curResetItemID = 0;
        CloseWnd();
    }

    private void OnCloseEvoWnd(GameObject go)
    {
        EvolvePanel.gameObject.SetActive(false);
        BlackMask.gameObject.SetActive(false);
    }

    private void OnCloseResetWnd(GameObject go)
    {
        ResetPanel.gameObject.SetActive(false);
        BlackMask.gameObject.SetActive(false);
        selectedResetItem = null;
    }

    private void OnCloseLevelUpWnd(GameObject go)
    {
        LevelUpPanel.gameObject.SetActive(false);
        BlackMask.gameObject.SetActive(false);
        selectedResetItem = null;
    }

    private void OnCloseSkillInfoPanel(GameObject go)
    {
        SkillInfoPanel.gameObject.SetActive(false);
    }

    private void ShowPetItem()
    {
        dicEditLabel["EditLabel_OwnNumber"].text = string.Format("已拥有宠物：[ffffff]{0}[-]/{1}", ServerPetMgr.dicPetMgr.Count, dicPetMessage.Count);
        for (int i = 0; i < dicPetMessage.Count; i++)
        {
            if (dicPetMessage.ContainsKey(i + 1) == true)
            {
                PetItem petItem = DBManager.GetDBPet().Get(i + 1);
                
                Transform petSeat = petItemParent.Find("PetBar_" + i.ToString());
                SetPetItem(i + 1, petSeat.transform);
                if (ObtainPetOrNot(i + 1))
                {
                    Transform Own = petSeat.transform.FindChild("Own");
                    petSeat.transform.FindChild("ToOwn").gameObject.SetActive(false);
                    Own.gameObject.SetActive(true);
                    SetPetItemOwn(i + 1, Own);
                }
                else
                {
                    petSeat.FindChild("ToOwn").gameObject.SetActive(true);
                    petSeat.FindChild("Own").gameObject.SetActive(false);
                    O_UISprite petSpriteTemp = petSeat.Find("Sprite").GetComponent<O_UISprite>();
                    petSpriteTemp.spriteName += "b";
                    petSeat.FindChild("ToOwn/Stone").GetComponent<O_UISprite>().spriteName = DBManager.GetDBDaoJu().Get(petItem.stoneIconID).SpriteName;
                    petSeat.FindChild("ToOwn/StoneNum").GetComponent<O_UILabel>().text = string.Format("[ffffff]{0}[-][000000]/{1}[-]",
                        GameRecord.Instance().GetCurRecord().GetNetPacketInfo().GetDaoJuCount(petItem.stoneIconID), petItem.hechengNum);
                }

                O_UIEventListener.Get(petSeat.gameObject).onClick = OnPetSeatClick;
                
            }
        }

    }

    private void SetPetItem(int id, Transform petSeat)
    {
        PetItem dicPet = dicPetMessage[id];
        O_UISprite sprite = petSeat.Find("Sprite").GetComponent<O_UISprite>();
        O_UISprite color = petSeat.Find("Color").GetComponent<O_UISprite>();
        Transform selected = petSeat.Find("Selected");
        if (sprite != null)
        {
            sprite.atlas = IconResourceMgr.Instance().GetAtlasByPath(dicPet.AtlasPath);
            sprite.spriteName = dicPet.SpriteName;
        }
        if (selected != null)
        {
            if (curShowPetID == id)
                selected.gameObject.SetActive(true);
            else
                selected.gameObject.SetActive(false);
        }
        if (ObtainPetOrNot(id))
        {
            ServerPet serverPet = ServerPetMgr.GetPetByDbId(id);
            color.gameObject.SetActive(true);
            if (color != null)
            {
                DBPetAptitude.Quality quality = DBPetAptitude.Instance.Items[serverPet.Info.aptitude].quality;
                switch (quality)
                {
                    case DBPetAptitude.Quality.GREEN:
                        color.spriteName = "common_qualityB";
                        break;
                    case DBPetAptitude.Quality.BLUE:
                        color.spriteName = "common_qualityC";
                        break;
                    case DBPetAptitude.Quality.PURPLE:
                        color.spriteName = "common_qualityD";
                        break;
                    case DBPetAptitude.Quality.GOLD:
                        color.spriteName = "common_qualityF";
                        break;
                    default:
                        break;
                }

            }
        }
        else
        {
            color.gameObject.SetActive(false);
        }
    }

    private void SetPetItemOwn(int id, Transform Own, bool isNext = false)
    {
        ServerPet serverPet = ServerPetMgr.GetPetByDbId(id);
        Transform levelTran = Own.Find("Level");
        Transform sizeTran = Own.Find("Size");
        Transform upgrade = Own.Find("Upgrade");
        Transform plusTran = Own.Find("Plus");
        Transform state = Own.FindChild("State");
        if (levelTran != null)
        {
            O_UILabel level = levelTran.GetComponent<O_UILabel>();
            level.text = string.Format("[ffc63e]LV.[-][ffffff]{0}[-]", serverPet.Info.level);
        }
        if (sizeTran != null && plusTran != null)
        {
            O_UISprite size = sizeTran.GetComponent<O_UISprite>();
            O_UILabel plus = plusTran.GetComponent<O_UILabel>();

            size.spriteName = "Pet_SpiritBar02b";
            int evolutionLv = serverPet.Info.evolutionLv;
            if (isNext)
                evolutionLv += 1;
            if (evolutionLv == 1)
            {
                size.spriteName = "Pet_SpiritBar02c";
                size.transform.localScale = new Vector3(66f, 24f, 0f);
                plus.gameObject.SetActive(false);
            }
            else if (evolutionLv == 2)
            {
                size.spriteName = "Pet_SpiritBar02b";
                size.transform.localScale = new Vector3(56f, 46f, 0f);
                plus.gameObject.SetActive(false);
            }
            else if (evolutionLv == 3)
            {
                size.spriteName = "Pet_SpiritBar02a";
                size.transform.localScale = new Vector3(56f, 46f, 0f);
                plus.gameObject.SetActive(false);
            }
            else
            {
                size.spriteName = "Pet_SpiritBar02a";
                size.transform.localScale = new Vector3(56f, 46f, 0f);
                plus.text = string.Format("{0}", evolutionLv - 3);
                plus.gameObject.SetActive(true);

            }
        }
        if(state != null)
        {
            if (id == curFightoutPetId)
            {
                state.gameObject.SetActive(true);
            }
            else
            {
                state.gameObject.SetActive(false);
            }
        }
        if (upgrade != null)
        {
            upgrade.gameObject.SetActive(false);
        }
    }

    private void ReflashPetInfo(int id)
    {
        ServerPet serverPet = ServerPetMgr.GetPetByDbId(id);
        PetLevelItem petLevel = DBManager.GetDBPetLevel().Get(serverPet.Info.level);
        dicEditLabel["EditLabel_PetName"].text = string.Format("[ffc000]LV.[-]{0} {1}", 
            serverPet.Info.level.ToString(), O_Localization.instance.GetText("PetLocal", serverPet.DbData.NameID));
        if (serverPet.Info.level == 100)
        {
            cur_Exp.sliderValue = 1;
            dicEditLabel["EditLabel_ExpPercent"].text = "100%";
        }
        else
        {
            cur_Exp.sliderValue = serverPet.Info.curExp / (float)petLevel.needExp;
            dicEditLabel["EditLabel_ExpPercent"].text = string.Format("{0}%", (int)Mathf.Round(cur_Exp.sliderValue*100));
        }
        dicEditLabel["EditLabel_ZizhiNum"].text = serverPet.Info.aptitude.ToString();
        dicEditLabel["EditLabel_HPNum"].text = ((int)(serverPet.GrowProp.hp + serverPet.AddProp.hp)).ToString();
        dicEditLabel["EditLabel_ApNum"].text = ((int)(serverPet.GrowProp.att + serverPet.AddProp.att)).ToString();
        dicEditLabel["EditLabel_DpNum"].text = ((int)(serverPet.GrowProp.def + serverPet.AddProp.def)).ToString();
        dicEditLabel["EditLabel_AeNum"].text = ((int)(serverPet.GrowProp.matt + serverPet.AddProp.matt)).ToString();
        dicEditLabel["EditLabel_DeNum"].text = ((int)(serverPet.GrowProp.res + serverPet.AddProp.res)).ToString();
        dicEditLabel["EditLabel_CsNum"].text = ((int)(serverPet.GrowProp.crt + serverPet.AddProp.crt)).ToString();
        dicEditLabel["EditLabel_RenxingNum"].text = ((int)(serverPet.GrowProp.tgh + serverPet.AddProp.tgh)).ToString();
        dicEditLabel["EditLabel_DaNum"].text = ((int)(serverPet.GrowProp.dod + serverPet.AddProp.dod)).ToString();
        dicEditLabel["EditLabel_HitNum"].text = ((int)(serverPet.GrowProp.hit + serverPet.AddProp.hit)).ToString();

        DBPetAptitude.Quality quality = DBPetAptitude.Instance.Items[serverPet.Info.aptitude].quality;
        int costItemNum = DBPetEvoCost.Instance.GetCostItemNum(id, quality, serverPet.Info.evolutionLv + 1);
        int costCurItemNum = GameRecord.Instance().GetCurRecord().GetNetPacketInfo().GetDaoJuCount(serverPet.DbData.stoneIconID);
        canPetEvolve = costItemNum <= costCurItemNum;
        dicEditLabel["EditLabel_StoneNum"].text = string.Format("{0}/{1}",costCurItemNum, costItemNum);
        dicEditSprite["EditSprite_InfoStoneSprite"].spriteName = DBManager.GetDBDaoJu().Get(serverPet.DbData.stoneIconID).SpriteName;

        int evolutionLv = serverPet.Info.evolutionLv;
        if (evolutionLv == 1)
        {
            dicEditSprite["EditSprite_EvoBtnSize"].spriteName = "Pet_SpiritBar03c";
            dicEditSprite["EditSprite_EvoBtnSize"].gameObject.transform.localScale = new Vector3(71f,70f,0f);
            dicEditSprite["EditSprite_EvoBtnSize"].gameObject.transform.localPosition = new Vector3(1.5f, -1.4f, 0f);
            dicEditLabel["EditLabel_EvoBtnPlus"].gameObject.SetActive(false);
            InfoPanel.Find("BG1").GetComponent<O_UISprite>().spriteName = "Pet_SpiritBar01a";
        }
        else if (evolutionLv == 2)
        {
            dicEditSprite["EditSprite_EvoBtnSize"].spriteName = "Pet_SpiritBar03b";
            dicEditSprite["EditSprite_EvoBtnSize"].gameObject.transform.localScale = new Vector3(82f, 70f, 0f);
            dicEditSprite["EditSprite_EvoBtnSize"].gameObject.transform.localPosition = new Vector3(4f, -4f, 0f);
            dicEditLabel["EditLabel_EvoBtnPlus"].gameObject.SetActive(false);
            InfoPanel.Find("BG1").GetComponent<O_UISprite>().spriteName = "Pet_SpiritBar01b";
        }
        else if (evolutionLv == 3)
        {
            dicEditSprite["EditSprite_EvoBtnSize"].spriteName = "Pet_SpiritBar03a";
            dicEditSprite["EditSprite_EvoBtnSize"].gameObject.transform.localScale = new Vector3(82f, 70f, 0f);
            dicEditSprite["EditSprite_EvoBtnSize"].gameObject.transform.localPosition = new Vector3(4f, -4f, 0f);
            dicEditLabel["EditLabel_EvoBtnPlus"].gameObject.SetActive(false);
            InfoPanel.Find("BG1").GetComponent<O_UISprite>().spriteName = "Pet_SpiritBar01c";
        }
        else
        {
            dicEditSprite["EditSprite_EvoBtnSize"].spriteName = "Pet_SpiritBar03a";
            dicEditSprite["EditSprite_EvoBtnSize"].gameObject.transform.localScale = new Vector3(69.7f, 59.5f, 0f);
            dicEditSprite["EditSprite_EvoBtnSize"].gameObject.transform.localPosition = new Vector3(3f, 7f, 0f);
            dicEditLabel["EditLabel_EvoBtnPlus"].text = string.Format("+{0}", evolutionLv - 3);
            dicEditLabel["EditLabel_EvoBtnPlus"].gameObject.SetActive(true);
            InfoPanel.Find("BG1").GetComponent<O_UISprite>().spriteName = "Pet_SpiritBar01c";
        }

        if (canPetEvolve)
            InfoPanel.Find("EvolveBtn/EvoTipDecrite").gameObject.SetActive(true);
        else
            InfoPanel.Find("EvolveBtn/EvoTipDecrite").gameObject.SetActive(false);

        if(curFightoutPetId == id)
            takeBtnShowPackUp();
        else
            takeBtnShowFightOut();
            
        //增加的技能
        //property.FindChild("RoleJiaCheng/jihuo").GetComponent<O_UILabel>().text = O_Localization.instance.GetText("HscUILocal", 2045) +
        //    string.Format(O_Localization.instance.GetText("HscUILocal", 2045 + petItem.ID), petItem.addNum);s
    }

    private void ReflashSkillInfo(int id)
    {
        ServerPet serverPet = ServerPetMgr.GetPetByDbId(id);

        if (serverPet.Info.skillLv < DBPetSkillBringUp.Instance.GetMaxLevelById(serverPet.DbData.skill1))
        {
            DBPetSkillBringUp.Item levelUpItem = DBPetSkillBringUp.Instance.GetItem(serverPet.DbData.skill1, serverPet.Info.skillLv + 1);
            DaoJuItem levelupItem = DBManager.GetDBDaoJu().Get(levelUpItem.itemId);
            int curItemNum = GameRecord.Instance().GetCurRecord().GetNetPacketInfo().GetDaoJuCount(levelUpItem.itemId);
            dicEditLabel["EditLabel_SkillCurLevel"].text = string.Format("{0}/{1}", serverPet.Info.skillLv.ToString(), DBPetSkillBringUp.Instance.GetMaxLevelById(serverPet.DbData.skill1));
            dicEditLabel["EditLabel_SkillDesc"].text = string.Format("普通技\n当前等级\n{0}级", serverPet.Info.skillLv.ToString());
            dicEditSprite["EditSprite_SkillUpConsumeSprite"].atlas = IconResourceMgr.Instance().GetAtlasByPath(levelupItem.AtlasPath);
            dicEditSprite["EditSprite_SkillUpConsumeSprite"].spriteName = levelupItem.SpriteName;
            if (curItemNum >= levelUpItem.itemCnt)
                dicEditLabel["EditLabel_SkillConsumeNum"].text = string.Format("[79e100]{0}[-]/{1}", curItemNum, levelUpItem.itemCnt);
            else
                dicEditLabel["EditLabel_SkillConsumeNum"].text = string.Format("[ff0000]{0}[-]/{1}", curItemNum, levelUpItem.itemCnt);

            dicEditLabel["EditLabel_SkillConsumeGold"].text = levelUpItem.gold.ToString();
            if(CheckPetSkillUpgradeable() == 0)
                dicEditSprite["EditSprite_CurSkillUpTip"].gameObject.SetActive(true);
            else
                dicEditSprite["EditSprite_CurSkillUpTip"].gameObject.SetActive(false);
        }
        else
        {
            DBPetSkillBringUp.Item levelUpItem = DBPetSkillBringUp.Instance.GetItem(serverPet.DbData.skill1, serverPet.Info.skillLv);
            DaoJuItem levelupItem = DBManager.GetDBDaoJu().Get(levelUpItem.itemId);
            int curItemNum = GameRecord.Instance().GetCurRecord().GetNetPacketInfo().GetDaoJuCount(levelUpItem.itemId);
            dicEditLabel["EditLabel_SkillCurLevel"].text = string.Format("{0}/{1}", serverPet.Info.skillLv.ToString(), DBPetSkillBringUp.Instance.GetMaxLevelById(serverPet.DbData.skill1));
            dicEditSprite["EditSprite_SkillUpConsumeSprite"].atlas = IconResourceMgr.Instance().GetAtlasByPath(levelupItem.AtlasPath);
            dicEditSprite["EditSprite_SkillUpConsumeSprite"].spriteName = levelupItem.SpriteName;
            dicEditLabel["EditLabel_SkillConsumeNum"].text = string.Format("[79e100]{0}[-]/{1}", curItemNum, "0");
            dicEditLabel["EditLabel_SkillConsumeGold"].text = "0";
            dicEditSprite["EditSprite_CurSkillUpTip"].gameObject.SetActive(false);
        }
    }

    private void ReflashSynthesisPanel(int id)
    {
        PetItem dicPet = dicPetMessage[id];
        dicEditLabel["EditLabel_SynName"].text = O_Localization.instance.GetText("PetLocal", dicPet.NameID);
        dicEditSprite["EditSprite_SynPetSprite"].spriteName = DBManager.GetDBDaoJu().Get(dicPet.stoneIconID).SpriteName;
        dicEditLabel["EditLabel_SynConsumeNumber"].text = string.Format("[ffffff]{0}[-][919191]/{1}[-]",
                       GameRecord.Instance().GetCurRecord().GetNetPacketInfo().GetDaoJuCount(dicPet.stoneIconID), dicPet.hechengNum);
    }
    private void ReflashEvolvePanel(int id)
    {
        ServerPet serverPet = ServerPetMgr.GetPetByDbId(id);
        DBPetAptitude.Quality quality = DBPetAptitude.Instance.Items[serverPet.Info.aptitude].quality;
        int costGold = DBPetEvoCost.Instance.GetCostGold(id, quality, serverPet.Info.evolutionLv + 1);
        int costItemNum = DBPetEvoCost.Instance.GetCostItemNum(id, quality, serverPet.Info.evolutionLv + 1);
        int costCurNum = GameRecord.Instance().GetCurRecord().GetNetPacketInfo().GetDaoJuCount(serverPet.DbData.stoneIconID);

        dicEditLabel["EditLabel_EvoSpriteName"].text = O_Localization.instance.GetText("PetLocal", serverPet.DbData.NameID);
        if (costCurNum < costItemNum)
            dicEditLabel["EditLabel_EvoCurNum"].text = string.Format("[ff0000]{0}[-]",costCurNum.ToString());
        else
            dicEditLabel["EditLabel_EvoCurNum"].text = string.Format("[79e100]{0}[-]", costCurNum.ToString());

        dicEditSprite["EditSprite_EvoYuanlingSprite"].spriteName = DBManager.GetDBDaoJu().Get(serverPet.DbData.stoneIconID).SpriteName;

        if (serverPet.Info.evolutionLv < DBPetEvoCost.Instance.GetMaxLevel(id, quality))
        {
            ShowNextEvoLvProp(serverPet.Info.evolutionLv + 1);
            SetPetItemOwn(id, EvolvePanel.Find("ConsumePanel/Next"), true);
            dicEditLabel["EditLabel_EvoNeedNum"].text = costItemNum.ToString();
            if (GameApp.GetWorldManager().MainPlayer.GetProperty().GoldNumber < costGold)
            {
                dicEditLabel["EditLabel_EvoBtnCostCoin"].gameObject.SetActive(false);
                dicEditLabel["EditLabel_EvoBtnCostCoinRed"].gameObject.SetActive(true);
                dicEditLabel["EditLabel_EvoBtnCostCoinRed"].text = costGold.ToString();
                EvolvePanel.Find("EvolveBtn/Button").GetComponent<BoxCollider>().enabled = false;
            }
            else
            {
                dicEditLabel["EditLabel_EvoBtnCostCoin"].gameObject.SetActive(true);
                dicEditLabel["EditLabel_EvoBtnCostCoinRed"].gameObject.SetActive(false);
                dicEditLabel["EditLabel_EvoBtnCostCoin"].text = costGold.ToString();
                EvolvePanel.Find("EvolveBtn/Button").GetComponent<BoxCollider>().enabled = true;
            }
        }
        else
        {
            dicEditLabel["EditLabel_EvoNeedNum"].text = "0";
            dicEditLabel["EditLabel_EvoPropChange"].text = "该宠物已达升级上限";
            SetPetItemOwn(id, EvolvePanel.Find("ConsumePanel/Next"));
            dicEditLabel["EditLabel_EvoBtnCostCoin"].text = "0";
            EvolvePanel.Find("EvolveBtn/Button").GetComponent<BoxCollider>().enabled = false;
        }
        SetPetItemOwn(id, EvolvePanel.Find("ConsumePanel/Cur"));
        SetPetItem(id, EvolvePanel.Find("ConsumePanel/Cur"));
        SetPetItem(id, EvolvePanel.Find("ConsumePanel/Next"));
    }
    private void ReflashResetPanel(int id)
    {
        ServerPet serverPet = ServerPetMgr.GetPetByDbId(id);
        dicEditLabel["EditLabel_ResetSpriteName"].text = O_Localization.instance.GetText("PetLocal", serverPet.DbData.NameID);
        dicEditLabel["EditLabel_ResetCurNum"].text = serverPet.Info.aptitude.ToString();
        dicEditLabel["EditLabel_ResetNeedNum"].text = serverPet.DbData.AptitudeMax.ToString();
        SetPetItem(id, ResetPanel.Find("ConsumePanel"));

        Transform itemGrid = ResetPanel.Find("ListPanel/Grid");
        Transform[] childs = itemGrid.GetComponentsInChildren<Transform>(true);
        List<Transform> childItems = new List<Transform>();
        foreach (Transform item in childs)
        {
            if (item.parent == itemGrid)
                childItems.Add(item);
        }

        foreach (Transform item in childItems)
        {
            if (item.name.Substring(0, 5) == "item_")
                GameObject.DestroyImmediate(item.gameObject);
        }

        GameObject itemSeat = itemGrid.Find("ItemTemp").gameObject;
        foreach( DaoJuItem item in ResetItemArray)
        {
            GameObject itemTemp = GameObject.Instantiate(itemSeat) as GameObject;
            itemTemp.SetActive(true);
            itemTemp.transform.parent = itemGrid;
            itemTemp.transform.localScale = new Vector3(1f, 1f, 1f);
            itemTemp.transform.localPosition = new Vector3(0f, 0f, -2f);
            itemTemp.name = "item_" + item.ID.ToString();
            itemTemp.transform.Find("Sprite").GetComponent<O_UISprite>().atlas = IconResourceMgr.Instance().GetAtlasByPath(item.AtlasPath);
            itemTemp.transform.Find("Sprite").GetComponent<O_UISprite>().spriteName = item.SpriteName;
            itemTemp.transform.Find("SpriteNum").GetComponent<O_UILabel>().text = GameRecord.Instance().GetCurRecord().GetNetPacketInfo().GetDaoJuCount(item.ID).ToString();
            if (curResetItemID == item.ID)
            {
                selectedResetItem = itemTemp.transform.Find("Selected").gameObject;
                selectedResetItem.SetActive(true);
            }

            Transform itemClick = itemTemp.transform.Find("Button");
            if (itemClick != null)
                O_UIEventListener.Get(itemClick.gameObject).onClick = OnResetItemClicked;
        }
        itemGrid.GetComponent<O_UIGrid>().repositionNow = true;
    }

    private void ReflashLevelUpPanel(int id)
    {
        ServerPet serverPet = ServerPetMgr.GetPetByDbId(id);
        dicEditLabel["EditLabel_LevelUpSpriteName"].text = O_Localization.instance.GetText("PetLocal", serverPet.DbData.NameID);
        dicEditLabel["EditLabel_LevelUpCurLevel"].text = serverPet.Info.level.ToString();
        dicEditLabel["EditLabel_LevelUpNextExp"].text = (DBManager.GetDBPetLevel().Get(serverPet.Info.level).needExp - serverPet.Info.curExp).ToString();
        SetPetItem(id, LevelUpPanel.Find("ConsumePanel"));

        Transform itemGrid = LevelUpPanel.Find("ListPanel/Grid");
        Transform[] childs = itemGrid.GetComponentsInChildren<Transform>(true);
        List<Transform> childItems = new List<Transform>();
        foreach (Transform item in childs)
        {
            if (item.parent == itemGrid)
                childItems.Add(item);
        }

        foreach (Transform item in childItems)
        {
            if (item.name.Substring(0, 5) == "item_")
                GameObject.DestroyImmediate(item.gameObject);
        }

        GameObject itemSeat = itemGrid.Find("ItemTemp").gameObject;
        foreach (DaoJuItem item in LevelUpItemArray)
        {
            GameObject itemTemp = GameObject.Instantiate(itemSeat) as GameObject;
            itemTemp.SetActive(true);
            itemTemp.transform.parent = itemGrid;
            itemTemp.transform.localScale = new Vector3(1f, 1f, 1f);
            itemTemp.transform.localPosition = new Vector3(0f, 0f, -2f);
            itemTemp.name = "item_" + item.ID.ToString();
            itemTemp.transform.Find("Sprite").GetComponent<O_UISprite>().atlas = IconResourceMgr.Instance().GetAtlasByPath(item.AtlasPath);
            itemTemp.transform.Find("Sprite").GetComponent<O_UISprite>().spriteName = item.SpriteName;
            itemTemp.transform.Find("SpriteNum").GetComponent<O_UILabel>().text = GameRecord.Instance().GetCurRecord().GetNetPacketInfo().GetDaoJuCount(item.ID).ToString();
            if (curLevelUpItemID == item.ID)
            {
                selectedLevelUpItem = itemTemp.transform.Find("Selected").gameObject;
                selectedLevelUpItem.SetActive(true);
                dicEditLabel["EditLabel_LevelUpItemDes"].text = O_Localization.instance.GetText("DaoJuLocal", DBManager.GetDBDaoJu().Get(curLevelUpItemID).MiaoShu);
            }

            Transform itemClick = itemTemp.transform.Find("Button");
            if (itemClick != null)
                O_UIEventListener.Get(itemClick.gameObject).onClick = OnLevelUpItemClicked;
        }
        itemGrid.GetComponent<O_UIGrid>().repositionNow = true;
    }


    private void ShowPetModel(int id)
    {
        PetItem petitem = ServerPetMgr.GetPetByDbId(id).DbData;
        foreach (Transform child in petmodelFather)
        {
            GameObject.Destroy(child.gameObject);
        }
        CoroutineProvider.Instance().StartCoroutine(ShowPetModle(petitem));
    }

    private IEnumerator ShowPetModle(PetItem petitem)
    {
        DBAvatarItem dbItem;
        GameApp.GetConsoleDebug().Log("GetAvatarItemByID id=" + petitem.moxingID);
        if (DBManager.GetAvatarDB().GetAvatarItemByID(petitem.moxingID, out dbItem))
        {
            string name = GlobalSetting.CharacterPath + dbItem.m_strBaseModelFilename;
            GameApp.GetConsoleDebug().Log("GetSyncResource name=" + name);
            Object obj = GameApp.GetResourceManager().GetResource(name);
            if (obj == null)
            {
                GameApp.GetResourceManager().LoadAsync(name);
                GameApp.GetConsoleDebug().Log("GetSyncResource time1=" + Time.time);

                while (GameApp.GetResourceManager().IsResLoaded(name) == false)
                {
                    yield return null;
                }

                GameApp.GetConsoleDebug().Log("GetSyncResource time2=" + Time.time);
                obj = GameApp.GetResourceManager().GetResource(name);
            }
            if (obj != null)
            {
                GameApp.GetConsoleDebug().Log("GetSyncResource 111111111111111");
                GameObject model = GameObject.Instantiate(obj) as GameObject;
                PetMaterial.mainTexture = model.GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture;
                model.GetComponentInChildren<SkinnedMeshRenderer>().material = PetMaterial;
                model.transform.parent = petmodelFather;
                model.transform.localPosition = new Vector3(-20, -115, -300);
                model.transform.localRotation = Quaternion.Euler(0, -150, 0);
                model.transform.localScale *= 0.6f;
                //model.transform.localScale = new Vector3(petitem.bili, petitem.bili, petitem.bili);
                for (int i = 0; i < model.transform.childCount; i++)
                {
                    Transform tran = model.transform.GetChild(i);
                    tran.gameObject.layer = (int)ML2Layer.LayerUI;
                }
                model.AddComponent<O_SpinWithMouse>();
                CapsuleCollider col = model.AddComponent<CapsuleCollider>();
                col.height = 2f;
                col.center = new Vector3(0f, 1f, 0f);
                model.layer = (int)ML2Layer.LayerUI;//UI层

                GameApp.GetConsoleDebug().Log("petok---------------------");
            }
            GameApp.GetConsoleDebug().Log("petok------!!!!!!!!!!-----------");
        }
        yield return 1;
    }

    //宠物列表中点击相应宠物
    void OnPetSeatClick(GameObject go)
    {
        int id = int.Parse(go.name.Substring(go.name.Length - 1, 1)) + 1;
        petItemParent.Find(string.Format("PetBar_{0}/Selected", curShowPetID - 1)).gameObject.SetActive(false);
        curShowPetID = id;
        petItemParent.Find(string.Format("PetBar_{0}/Selected", curShowPetID - 1)).gameObject.SetActive(true);
        ShowInfoOrSyns(id);
    }

    //打开属性重置界面
    void OnResetWndOpen(GameObject go)
    {
        Transform ResetProperty = WndRootGameobject.transform.Find("ResetPanel/ResetBtn/Button");
        if (ResetProperty != null)
            O_UIEventListener.Get(ResetProperty.gameObject).onClick = OnResetPropertyClicked;
        Transform CloseResetWnd = WndRootGameobject.transform.Find("ResetPanel/CloseBtn/Button");
        if (CloseResetWnd != null)
            O_UIEventListener.Get(CloseResetWnd.gameObject).onClick = OnCloseResetWnd;

        ReflashResetPanel(curShowPetID);
        ResetPanel.gameObject.SetActive(true);
        BlackMask.gameObject.SetActive(true);
    }

    void OnLevelUpWndOpen(GameObject go)
    {
        Transform PetLevelUpBtn = WndRootGameobject.transform.Find("LevelUpPanel/PetLevelUpBtn/Button");
        if (PetLevelUpBtn != null)
            O_UIEventListener.Get(PetLevelUpBtn.gameObject).onClick = OnPetLevelUpClicked;
        Transform CloseLevelUpWnd = WndRootGameobject.transform.Find("LevelUpPanel/CloseBtn/Button");
        if (CloseLevelUpWnd != null)
            O_UIEventListener.Get(CloseLevelUpWnd.gameObject).onClick = OnCloseLevelUpWnd;

        ReflashLevelUpPanel(curShowPetID);
        LevelUpPanel.gameObject.SetActive(true);
        BlackMask.gameObject.SetActive(true);
    }

    //重置道具点击按钮
    void OnResetItemClicked(GameObject go)
    {
        curResetItemID = System.Convert.ToInt32(go.transform.parent.name.Substring(5, go.transform.parent.name.Length - 5));
        if (selectedResetItem != null)
            selectedResetItem.SetActive(false);
        selectedResetItem =  go.transform.parent.Find("Selected").gameObject;
        selectedResetItem.SetActive(true);
    }

    //重置属性按钮
    void OnResetPropertyClicked(GameObject go)
    {
        if (GameRecord.Instance().GetCurRecord().GetNetPacketInfo().GetDaoJuCount(curResetItemID) == 0)
        {
            MsgBoxOk.text = "道具数量不足,无法重置。";
            MsgBoxOk.Pop();
        }
        else
        {
            GameApp.GetNetHandler().PetResetProp(curShowPetID, curResetItemID);
        }
    }

    void OnLevelUpItemClicked(GameObject go)
    {
        curLevelUpItemID = System.Convert.ToInt32(go.transform.parent.name.Substring(5, go.transform.parent.name.Length - 5));
        dicEditLabel["EditLabel_LevelUpItemDes"].text = O_Localization.instance.GetText("DaoJuLocal", DBManager.GetDBDaoJu().Get(curLevelUpItemID).MiaoShu);
        if (selectedLevelUpItem != null)
            selectedLevelUpItem.SetActive(false);
        selectedLevelUpItem = go.transform.parent.Find("Selected").gameObject;
        selectedLevelUpItem.SetActive(true);
    }

    //宠物升级按钮
    void OnPetLevelUpClicked(GameObject go)
    {
        if(curLevelUpItemID == 0)
        {
            MsgBoxOk.text = "请选择道具";
            MsgBoxOk.Pop();
            return;
        }
        if (GameRecord.Instance().GetCurRecord().GetNetPacketInfo().GetDaoJuCount(curLevelUpItemID) == 0)
        {
            MsgBoxOk.text = "道具数量不足,无法喂养";
            MsgBoxOk.Pop();
            return;
        }
        else
        {
            GameApp.GetNetHandler().PetLevelUp(curShowPetID, curLevelUpItemID, 1);
        }
    }

    //升级技能按钮
    void OnUpgradeSkillClicked(GameObject go)
    {
        int errCode = CheckPetSkillUpgradeable();
        switch(errCode)
        {
            case 0:
                GameApp.GetNetHandler().PetSkillLevelUp(curShowPetID);
                break;
            case 1:
                ServerPet serverPet = ServerPetMgr.GetPetByDbId(curShowPetID);
                DBPetSkillBringUp.Item levelUpItem = DBPetSkillBringUp.Instance.GetItem(serverPet.DbData.skill1, serverPet.Info.skillLv + 1);
                MsgBoxOk.text = string.Format("宠物等级不足，需求等级：{0}",levelUpItem.needLv);
                MsgBoxOk.Pop();
                break;
            case 2:
                MsgBoxOk.text = "升级道具数量不足";
                MsgBoxOk.Pop();
                break;
            case 3:
                MsgBoxOk.text = "金币数量不足";
                MsgBoxOk.Pop();
                break;
            case 4:
                MsgBoxOk.text = "技能等级已达上限";
                MsgBoxOk.Pop();
                break;
        }

    }

    //长按显示技能详情
    void OnPressSkillClicked(GameObject go , bool isPressed)
    {
        if(isPressed)
        {
            showSkillInfo = true;
            CoroutineProvider.Instance().StartCoroutine(CalculatePressTime());
        }
        else
        {
            showSkillInfo = false;
        }

    }
    void OpenSkillInfoPanel()
    {
        SkillInfoPanel.gameObject.SetActive(true);
        Transform CloseSkillInfo = SkillInfoPanel.transform.Find("CloseSkillInfoBtn");
        if (CloseSkillInfo != null)
            O_UIEventListener.Get(CloseSkillInfo.gameObject).onClick = OnCloseSkillInfoPanel;

        ServerPet serverPet = ServerPetMgr.GetPetByDbId(curShowPetID);
        dicEditLabel["EditLabel_SkillInfoPanelName"].text = serverPet.DbData.Name;
        dicEditLabel["EditLabel_SkillInfoPanelDesc"].text = "技能描述";
    }
    private IEnumerator CalculatePressTime()
    {
        yield return new WaitForSeconds(1.5f);
        if(showSkillInfo)
            OpenSkillInfoPanel();
        yield return true;
    }
    //出战按钮
    void OnOnBattleClicked(GameObject go)
    {
        // 收起宠物
        var net = GameApp.GetNetHandler();
        var pet = GameApp.GetWorldManager().MainPlayer.m_Pet;
        if(curFightoutPetId == curShowPetID)
        {
            if (pet != null)
            {
                bool success = net.PetPackUp();
                if (success)
                {
                    pet.Destroy();
                    curFightoutPetId = 0;
                    takeBtnShowFightOut();
                    ShowPetItem();
                }
                return;
            }

        }
        else
        {
            if(curFightoutPetId != 0)
            {
                if (net.PetPackUp())
                {
                    pet.Destroy();
                    curFightoutPetId = 0;
                }
            }
            net.PetFightOut(curShowPetID);
            curFightoutPetId = curShowPetID;
        }

    }

    //打开进化界面
    void OnEvolvePanelOpen(GameObject go)
    {
        Transform CloseEvoWnd = WndRootGameobject.transform.Find("EvolvePanel/CloseBtn/Button");
        if (CloseEvoWnd != null)
            O_UIEventListener.Get(CloseEvoWnd.gameObject).onClick = OnCloseEvoWnd;
        Transform PetEvolve = WndRootGameobject.transform.Find("EvolvePanel/EvolveBtn/Button");
        if (PetEvolve != null)
            O_UIEventListener.Get(PetEvolve.gameObject).onClick = OnPetEvolveClicked;
        ReflashEvolvePanel(curShowPetID);
        EvolvePanel.gameObject.SetActive(true);
        BlackMask.gameObject.SetActive(true);
    }

    //宠物进化确认
    void OnPetEvolveClicked(GameObject go)
    {
        if (canPetEvolve)
        {
            GameApp.GetNetHandler().PetEvolve(curShowPetID);
        }
        else
        {
            MsgBoxOk.text = "元灵不足";
            MsgBoxOk.Pop();
        }
    }


    //召唤按钮
    void OnPetSummonClicked(GameObject go)
    {
        if (GameRecord.Instance().GetCurRecord().GetNetPacketInfo().GetDaoJuCount(dicPetMessage[curShowPetID].stoneIconID) >= dicPetMessage[curShowPetID].hechengNum)
        {
            GameApp.GetNetHandler().ActivatePet(curShowPetID);
        }

    }

    void takeBtnShowFightOut()
    {
        dicEditLabel["EditLabel_FightOutLabel"].gameObject.SetActive(true);
        dicEditLabel["EditLabel_StandbyLabel"].gameObject.SetActive(false);
        dicEditLabel["EditLabel_FightOutLabel"].transform.parent.Find("Background").GetComponent<O_UISprite>().spriteName = "common_button01";
    }

    void takeBtnShowPackUp()
    {
        dicEditLabel["EditLabel_FightOutLabel"].gameObject.SetActive(false);
        dicEditLabel["EditLabel_StandbyLabel"].gameObject.SetActive(true);
        dicEditLabel["EditLabel_StandbyLabel"].transform.parent.Find("Background").GetComponent<O_UISprite>().spriteName = "common_button02";
    }

    private void ShowNextEvoLvProp(int evolveLv)
    {
        var target = DBPetEvoLv.Instance.Items[evolveLv].target;
        var effect = DBPetEvoLv.Instance.Items[evolveLv].effect;
        float rate = DBPetEvoEffect.Instance.Items[effect].rate;
        string character = "主角";

        if(target == DBPetEvoLv.Target.PET)
            character = "宠物";

        switch (effect)
        {
            case DBPetEvoEffect.Effect.INC_HP:
                dicEditLabel["EditLabel_EvoPropChange"].text = string.Format("{0}生命值增加[79e100]{1}%[-]", character, (int)(rate * 100));
                break;
            case DBPetEvoEffect.Effect.INC_ATT:
                dicEditLabel["EditLabel_EvoPropChange"].text = string.Format("{0}物理攻击增加[79e100]{1}%[-]", character, (int)(rate * 100));
                break;
            case DBPetEvoEffect.Effect.INC_DEF:
                dicEditLabel["EditLabel_EvoPropChange"].text = string.Format("{0}物理防御增加[79e100]{1}%[-]", character, (int)(rate * 100));
                break;
            case DBPetEvoEffect.Effect.INC_EXP:
                dicEditLabel["EditLabel_EvoPropChange"].text = string.Format("{0}获得经验增加[79e100]{1}%[-]", character, (int)(rate * 100));
                break;
            case DBPetEvoEffect.Effect.INC_RES:
                dicEditLabel["EditLabel_EvoPropChange"].text = string.Format("{0}魔法防御增加[79e100]{1}%[-]", character, (int)(rate * 100));
                break;
            case DBPetEvoEffect.Effect.INC_SKILL_POWER:
                dicEditLabel["EditLabel_EvoPropChange"].text = string.Format("{0}技能伤害增加[79e100]{1}%[-]", character, (int)(rate * 100));
                break;
            case DBPetEvoEffect.Effect.REDUCE_SKILL_CD:
                dicEditLabel["EditLabel_EvoPropChange"].text = string.Format("{0}技能冷却时间减少[79e100]{1}%[-]", character, (int)(rate * 100));
                break;
        }
    }

    private void PetInfoUpdate()
    {
        EvolvePanel.gameObject.SetActive(false);
        BlackMask.gameObject.SetActive(false);
        ShowInfoOrSyns(curShowPetID);
        ReflashResetPanel(curShowPetID);
        ReflashLevelUpPanel(curShowPetID);
        ShowPetItem();
        RefreshCommon();
    }
    public void ActivatePetAck(PKT_CLI_GS_WN_ACTIVE_PET_ACK p)
    {
        if (p.ack == 0)
        {
            ShowInfoOrSyns(curShowPetID);
            ShowPetItem();
            HUD hud = GameApp.GetUIManager().GetWnd("HUD") as HUD;
            hud.petBtnFlag = false;
            hud.RefreshBtnFlag();
        }
    }

    public void PetFightOutAck(PKT_CLI_GS_WN_SET_FIGHT_PET_ACK p)
    {
        //或者按钮变灰
        if (p.Ack == 1)
        {
            ShowPetItem();
            Pet.CreateForPlayer(GameApp.GetWorldManager().MainPlayer);
            takeBtnShowPackUp();
        }
    }
    public void PetResetPropAck(PKT_CLI_GS_WN_PET_RESET_APTITUDE_ACK p)
    {
        if (p.ErrCode == 0)
        {
            dicEditSprite["EditSprite_ResetDecorate"].gameObject.SetActive(false);
            dicEditSprite["EditSprite_ResetDecorate"].gameObject.SetActive(true);
        }
        else if (p.ErrCode == (int)GameApp.errCode.GOLD_NOT_ENOUGH)
        {
            MsgBoxOk.text = O_Localization.instance.GetText("HyjUILocal", 196);
            MsgBoxOk.Pop();
        }
        else if (p.ErrCode == (int)GameApp.errCode.ITEM_NOT_ENOUGH)
        {
            MsgBoxOk.text = "道具数量不足！";
            MsgBoxOk.Pop();
        }
        else
        {
            MsgBoxOk.text = "重置失败,请重试！";
            MsgBoxOk.Pop();
        }

    }

    public void PetLevelUpAck(PKT_CLI_GS_WN_PET_UP_LEVEL_ACK p)
    {
        Debug.Log("PKT_CLI_GS_WN_PET_UP_LEVEL_ACK" + p.Ack);
        switch (p.Ack)
        {
            case (int)GameApp.errCode.SUCCESS:
                break;
            case (int)GameApp.errCode.ALREADY_REACH_LIMIT:
                MsgBoxOk.text ="宠物等级不能超过人物等级";
                MsgBoxOk.Pop();
                break;
            case (int)GameApp.errCode.ITEM_NOT_ENOUGH:
                MsgBoxOk.text = "道具数量不足";
                MsgBoxOk.Pop();
                break;
            case (int)GameApp.errCode.GOLD_NOT_ENOUGH:
                MsgBoxOk.text = "金币数量不足";
                MsgBoxOk.Pop();
                break;
        }

    }

    public void PetEvolveAck(PKT_CLI_GS_WN_PET_EVOLUTION_ACK p)
    {
        if (p.ErrCode == (int)GameApp.errCode.SUCCESS)
        {
            //EvolvePanel.gameObject.SetActive(false);
            //BlackMask.gameObject.SetActive(false);
            //ShowInfoOrSyns(curShowPetID);
            //ShowPetItem();
            ServerPlayerMgr.MainPlayer.Pet.Evolution(1);
        }
        if (p.ErrCode == (int)GameApp.errCode.GOLD_NOT_ENOUGH)
        {
            MsgBoxOk.text = O_Localization.instance.GetText("HyjUILocal", 196);
            MsgBoxOk.Pop();
        }
        if (p.ErrCode == (int)GameApp.errCode.ITEM_NOT_ENOUGH)
        {
            MsgBoxOk.text = "元灵不足！";
            MsgBoxOk.Pop();
        }
        if (p.ErrCode == (int)GameApp.errCode.ALREADY_REACH_LIMIT)
        {
            MsgBoxOk.text = "已达进化上限";
            MsgBoxOk.Pop();
        }
    }
    public void PetSkillLevelUpAck(PKT_CLI_GS_WN_PET_SKILL_LEVELUP_ACK p)
    {
        if (p.ErrCode == (int)GameApp.errCode.SUCCESS)
        {
            ServerPet serverPet = ServerPetMgr.GetPetByDbId(curShowPetID);
            serverPet.SkillLvUp(1);
            ReflashSkillInfo(curShowPetID);
            return;
        }
        if (p.ErrCode == (int)GameApp.errCode.GOLD_NOT_ENOUGH)
        {
            MsgBoxOk.text = O_Localization.instance.GetText("HyjUILocal", 196);
            MsgBoxOk.Pop();
        }
        if (p.ErrCode == (int)GameApp.errCode.ITEM_NOT_ENOUGH)
        {
            MsgBoxOk.text = "升级道具不足！";
            MsgBoxOk.Pop();
        }
        if (p.ErrCode == (int)GameApp.errCode.ALREADY_REACH_LIMIT)
        {
            MsgBoxOk.text = "技能等级已达上限";
            MsgBoxOk.Pop();
        }
        if (p.ErrCode == (int)GameApp.errCode.PET_LEVEL_NOT_ENOUGH)
        {
            MsgBoxOk.text = "技能等级不能超过宠物等级";
            MsgBoxOk.Pop();
        }
    }
    private void RefreshCommon()
    {
        dicEditLabel["EditLabel_StrengthNum"].text = string.Format("[ffffff]{0}[-][f7a86e]/{1}[-]", GameRecord.Instance().GetCurRecord().GetBuyInfo().m_nCurStrength.ToString(),
                         GameRecord.Instance().GetCurRecord().GetBuyInfo().m_nMaxStrength.ToString());
        dicEditLabel["EditLabel_GoldNum"].text = GameApp.GetWorldManager().MainPlayer.GetProperty().GoldNumber.ToString();
        dicEditLabel["EditLabel_DiamondNum"].text = GameApp.GetWorldManager().MainPlayer.GetProperty().Diamond.ToString();
    }

    public void FightPetSetAck(PKT_CLI_GS_WN_FIGHT_PET_SET_NTF p)
    {
        curFightoutPetId = p.PetId;
    }

    //判断是显示宠物详情还是显示合成界面
    private void ShowInfoOrSyns(int id)
    {
        if (ObtainPetOrNot(id) == true)
        {
            InfoPanel.gameObject.SetActive(true);
            SkillPanel.gameObject.SetActive(true);
            SynthesisPanel.gameObject.SetActive(false);
            ReflashPetInfo(id);
            ReflashSkillInfo(id);
            ShowPetModel(id);
        }
        else
        {
            InfoPanel.gameObject.SetActive(false);
            SkillPanel.gameObject.SetActive(false);
            SynthesisPanel.gameObject.SetActive(true);
            ReflashSynthesisPanel(id);
        }
    }

    //判断宠物技能可否升级
    private int CheckPetSkillUpgradeable()
    {
        ServerPet serverPet = ServerPetMgr.GetPetByDbId(curShowPetID);
        DBPetSkillBringUp.Item levelUpItem = DBPetSkillBringUp.Instance.GetItem(serverPet.DbData.skill1, serverPet.Info.skillLv + 1);
        int curItemNum = GameRecord.Instance().GetCurRecord().GetNetPacketInfo().GetDaoJuCount(levelUpItem.itemId);
        if (levelUpItem.needLv > serverPet.Info.level)
            return 1;//宠物等级不足
        if (curItemNum < levelUpItem.itemCnt)
            return 2;//道具数量不足
        if (GameApp.GetWorldManager().MainPlayer.GetProperty().GoldNumber < levelUpItem.gold)
            return 3;//金币数量不足
        if (serverPet.Info.skillLv >= DBPetSkillBringUp.Instance.GetMaxLevelById(serverPet.DbData.skill1))
            return 4;//技能等级达到上限
        return 0;//可以升级

    }
    //判断宠物是否已获取
    private bool ObtainPetOrNot(int id)
    {
        bool b_Obtain = false;
        foreach (ServerPet petInDic in ServerPetMgr.dicPetMgr.Values)
        {
            if (petInDic.DbData.ID == id)
            {
                b_Obtain = true;
            }
        }
        return b_Obtain;
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
}
