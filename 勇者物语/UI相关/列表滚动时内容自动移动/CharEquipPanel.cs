using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public enum TapEquipStateNew
{
    All = -1,       //旧   新
    Hat = 0,        //项链 头盔
    Necklace,       //头盔 项链
    Clothes,        //腰带 衣服
    Bracer,         //武器 护腕
    Weapon,         //护腕 武器
    Ring,           //衣服 戒指
    Fashion,        //戒指 时装（用原裤子替代）
    Wing,           //裤子 神翼（腰带替代）
}

/// <summary>
/// 人物界面右侧装备列表面板 
/// </summary>
public class CharEquipPanel 
{
    public GameObject InfoPanel = null;
    public TapEquipState tapState = TapEquipState.All;
    // 实例化列表
    private List<GameObject> lstEquipItem = new List<GameObject>();
    private NetItemInfo EquipItem = null;
    public SelectInfo info = new SelectInfo();
    private Transform tranScrollView = null;
    private float cellHeight = 108f;
    // 网格父节点以及单元网格宽度
    private Transform tranUIGrid = null;
    // 动态滑动条
    private ActiveDraggablePanelNew activeDraggablePanel = null;
    //装备列表
    private List<NetItemInfo> lstEquip = null;
    //当前选中的边侧栏装备类
    private GameObject curTap = null;
    //当前选中的装备
    private GameObject curItem = null;
    //装备槽位对应slot的映射
    //数组序号为slot值，映射使用时+1，10代表无对应
    int[] equipTypeIndex = new int[] { -1, 1, 0, 10, 4, 10, 2, 3, 5, 10, 10, 10 };

    public void Init(Transform infoPanel)
    {
        InfoPanel = infoPanel.gameObject as GameObject;
        InfoPanel.SetActive(true);

        tranScrollView = InfoPanel.transform.Find("ListPanel");
        tranUIGrid = tranScrollView.transform.Find("Grid");

        activeDraggablePanel = tranScrollView.GetComponent("ActiveDraggablePanelNew") as ActiveDraggablePanelNew;
        activeDraggablePanel.cellHeight = cellHeight;
        activeDraggablePanel.onFresh = ActiveFresh;

        lstEquip = GameRecord.Instance().GetCurRecord().GetNetPacketInfo().GetEquipList(TapEquipState.All, EquipSortType.FromBigToSmall);
        //CreateItems();
        AddEventListener();
        //选择标签页
        curTap = InfoPanel.transform.Find("Toggle_-1").gameObject;
        SelectTap(TapEquipState.All);
    }

    public void Reflash()
    {
        lstEquip = GameRecord.Instance().GetCurRecord().GetNetPacketInfo().GetEquipList(tapState, EquipSortType.FromBigToSmall);
        CreateItems();
        AddEventListener();
        SelectTap(tapState);
    }

    private void AddEventListener()
    {
        Transform tran = null;
        if (InfoPanel != null)
        {
            for (int i = -1; i <= 6; i++)
            {
                tran = InfoPanel.transform.Find("Toggle_" + i.ToString());
                O_UIEventListener.Get(tran.gameObject).onClick = OnTapButtontClicked;
            }
        }

    }

    private void OnTapButtontClicked(GameObject go)
    {

        string name = go.name.Substring(go.name.LastIndexOf('_') + 1, go.name.Length - go.name.LastIndexOf('_') - 1);
        int value = Array.IndexOf(equipTypeIndex,System.Convert.ToInt32(name)) - 1;
        lstEquip.Clear();
        lstEquip = GameRecord.Instance().GetCurRecord().GetNetPacketInfo().GetEquipList((TapEquipState)value, EquipSortType.FromBigToSmall);
        SelectTap(value);
        //替换标签栏图案
        curTap.transform.Find("Background").GetComponent<O_UISprite>().spriteName = "common_LabelB";
        curTap.transform.Find("Background").localScale = new Vector3(70f, 64f, 0f);
        curTap = go;
        curTap.transform.Find("Background").GetComponent<O_UISprite>().spriteName = "common_LabelA";
        curTap.transform.Find("Background").localScale = new Vector3(92f, 67f, 0f);
    }

    private void SelectTap(int tapValue)
    {
        if (tapValue == (int)tapState)
            return;
        SelectTap((TapEquipState)tapValue);
        tapState = (TapEquipState)tapValue;
    }

    private void SelectTap(TapEquipState tap)
    {
        // scroll view 移到正确的位置
        O_SpringPanel.Begin(tranScrollView.gameObject, new Vector3(201f, 132f, 0f), 13f);
        activeDraggablePanel.StopOneSecond();
        activeDraggablePanel.CurStartIndex = 0;

        // 删除之前的节点
        foreach (GameObject go in lstEquipItem)
        {
            GameObject.DestroyImmediate(go);
        }
        lstEquipItem.Clear();

        // 重新创建新节点
        CreateItems();

        // 打开新的面板
        activeDraggablePanel.lstData = lstEquip;
        int maxCount = activeDraggablePanel.GetMaxCount();
        //int maxCount = lstEquip.Count;
        int posIndex = 0;
        foreach (NetItemInfo item in lstEquip)
        {
            if (posIndex >= maxCount)
                break;

            GameObject go = lstEquipItem[posIndex];
            go.SetActive(true);
            go.transform.Find("Frontground").gameObject.SetActive(false);
            go.GetComponent<BoxCollider>().enabled = true;
            int index = activeDraggablePanel.CurStartIndex + posIndex;
            SetContent(go, item, index);
            posIndex++;
        }
        if (posIndex <= 4)
        {
            for (int i = posIndex; i <= 4; i++)
            {
                GameObject go = lstEquipItem[posIndex];
                go.SetActive(true);
                go.transform.localPosition = new Vector3(0f, 0f, 0f);
                go.transform.Find("Frontground").gameObject.SetActive(true);
                go.GetComponent<BoxCollider>().enabled = false;
                posIndex++;
            }
        }
        for (int i = posIndex; i < maxCount; ++i)
        {
            GameObject go = lstEquipItem[i];
            go.SetActive(false);
        }
        //InfoPanel.transform.Find("ListPanel").GetComponent<O_UIDraggablePanel>()
        //InfoPanel.transform.Find("ListPanel").localPosition = new Vector3(201f, 132f, 0f);
        InfoPanel.transform.Find("ListPanel/Grid").GetComponent<O_UIGrid>().Reposition();
    }

    private void SetContent(GameObject go, NetItemInfo info, int index)
    {
        DBItemBaseProp item = DBItem.Get(info.ID);
        if (item == null)
            return;
        //置灰选中框
        go.transform.Find("Selected").gameObject.SetActive(false);

        // 设置索引信息
        go.name = "EquipItem_" + index.ToString();
        go.transform.localPosition = new Vector3(go.transform.localPosition.x, go.transform.localPosition.y,-1f);

        // 设置名字
        CommonFun.SetLabelText(go, "NameLabel", Definition.Instance().EquipColor[item.Rank] +
             O_Localization.instance.GetText("ItemsLocal", item.NameID));

        // 设置等级
        CharWindowNew.SetItemLevel(go.transform.Find("Avatar"), info);

        //设置战斗力
        if (info.ID > 0 && info.ID < 9999)
        {
            int changeValue = FightValueChange(info);
            if (changeValue >= 0)
            {
                CommonFun.SetLabelText(go, "CombatPowerLabel", string.Format("[79e100]战力◎{0}[-]", changeValue));
            }
            else
            {
                changeValue *= -1;
                CommonFun.SetLabelText(go, "CombatPowerLabel", string.Format("[ff0000]战力※{0}[-]", changeValue));
            }
        }


        // 设置品质
        (go.transform.Find("Avatar/Colour").GetComponent("O_UISprite") as O_UISprite).enabled = true;
        switch (item.Rank)
        {
            case 1:
                CommonFun.SetSprite(go, "Avatar/Colour", "common_qualityB");
                break;
            case 2:
                CommonFun.SetSprite(go, "Avatar/Colour", "common_qualityC");
                break;
            case 3:
                CommonFun.SetSprite(go, "Avatar/Colour", "common_qualityD");
                break;
            case 4:
                CommonFun.SetSprite(go, "Avatar/Colour", "common_qualityF");
                break;
            default:
                CommonFun.SetSprite(go, "Avatar/Colour", "common_qualityA");
                break;
        }

        // 设置Icon
        O_UISprite icon = (go.transform.Find("Avatar/Icon").GetComponent("O_UISprite") as O_UISprite);
        icon.atlas = IconResourceMgr.Instance().GetAtlasByPath(item.AtlasPath);
        icon.spriteName = item.SpriteName;
        icon.enabled = true;

        #region 设置4个属性

        int maxPropNum = 4;
        int curPropIndex = 1;
        //int Star = info.BreachNum;
        //int Index = item.SlotType;

        //单装备属性列表
        List<KeyValuePair<string, int>> propList = new List<KeyValuePair<string, int>>();
        KeyValuePair<string, int> prop = new KeyValuePair<string, int>();
        
        if (item.HP != 0 && curPropIndex <= maxPropNum)
        {
            int value = GetCurValue(info, "HP");
            prop = new KeyValuePair<string, int>("HP", value);
            propList.Add(prop);
            curPropIndex++;
        }

        if (item.Ap != 0 && curPropIndex <= maxPropNum)
        {
            int value = GetCurValue(info, "Ap");
            prop = new KeyValuePair<string, int>("AP", value);
            propList.Add(prop);
            curPropIndex++;
        }

        if (item.Ae != 0 && curPropIndex <= maxPropNum)
        {
            int value = GetCurValue(info, "Ae");
            prop = new KeyValuePair<string, int>("AE", value);
            propList.Add(prop);
            curPropIndex++;
        }

        if (item.Dp != 0 && curPropIndex <= maxPropNum)
        {
            int value = GetCurValue(info, "Dp");
            prop = new KeyValuePair<string, int>("DP", value);
            propList.Add(prop);
            curPropIndex++;
        }

        if (item.De != 0 && curPropIndex <= maxPropNum)
        {
            int value = GetCurValue(info, "De");
            prop = new KeyValuePair<string, int>("DE", value);
            propList.Add(prop);
            curPropIndex++;
        }

        if (item.Hit != 0 && curPropIndex <= maxPropNum)
        {
            int value = GetCurValue(info, "Hit");
            prop = new KeyValuePair<string, int>("Hit", value);
            propList.Add(prop);
            curPropIndex++;
        }

        if (item.CS != 0 && curPropIndex <= maxPropNum)
        {
            int value = GetCurValue(info, "CS");
            prop = new KeyValuePair<string, int>("CS", value);
            propList.Add(prop);
            curPropIndex++;
        }

        if (item.DA != 0 && curPropIndex <= maxPropNum)
        {
            int value = GetCurValue(info, "DA");
            prop = new KeyValuePair<string, int>("DA", value);
            propList.Add(prop);
            curPropIndex++;
        }

        if (item.CSD != 0 && curPropIndex <= maxPropNum)
        {
            int value = GetCurValue(info, "CSD");
            prop = new KeyValuePair<string, int>("CSD", value);
            propList.Add(prop);
            curPropIndex++;
        }

        for (int i = 1; i <= propList.Count ;  ++i)
        {
            go.SetActive(true);
            go.transform.Find("Capacity/Value_" + i.ToString()).GetComponent<O_UILabel>().text = propList[i - 1].Value.ToString();
            switch (propList[i-1].Key)
            {
                case "HP":
                    go.transform.Find("Capacity/Sprite_" + i.ToString()).GetComponent<O_UISprite>().spriteName = "common_icon_Life";
                    break;
                case "AP":
                    go.transform.Find("Capacity/Sprite_" + i.ToString()).GetComponent<O_UISprite>().spriteName = "common_icon_Physical attack";;
                    break;
                case "AE":
                    go.transform.Find("Capacity/Sprite_" + i.ToString()).GetComponent<O_UISprite>().spriteName = "common_icon_Magic attack";
                    break;
                case "AS":
                    ;
                    break;
                case "DP":
                    go.transform.Find("Capacity/Sprite_" + i.ToString()).GetComponent<O_UISprite>().spriteName = "common_icon_Physical def";
                    break;
                case "DE":
                    go.transform.Find("Capacity/Sprite_" + i.ToString()).GetComponent<O_UISprite>().spriteName = "common_icon_Magic def";
                    break;
                case "DS":
                    ;
                    break;
                case "Hit":
                    go.transform.Find("Capacity/Sprite_" + i.ToString()).GetComponent<O_UISprite>().spriteName = "common_icon_Hit";
                    break;
                case "CS":
                    go.transform.Find("Capacity/Sprite_" + i.ToString()).GetComponent<O_UISprite>().spriteName = "common_icon_Crit";
                    break;
                case "DA":
                    go.transform.Find("Capacity/Sprite_" + i.ToString()).GetComponent<O_UISprite>().spriteName = "common_icon_Dodge";
                    break;
                case "CSD":
                    go.transform.Find("Capacity/Sprite_" + i.ToString()).GetComponent<O_UISprite>().spriteName = "common_icon_Toughness";
                    break;
            }
        }
        if (propList.Count < 4)
        {
            for (int i = propList.Count + 1; i <= 4; ++i)
            {
                go.transform.Find("Capacity/Sprite_" + i).gameObject.SetActive(false);
                go.transform.Find("Capacity/Value_" + i).gameObject.SetActive(false);
            }
        }

        #endregion
        //设置星级
        SetStar(go,info.BreachNum);

        // 设置3个宝石
        CharWindowNew.SetCrystalState(go.transform.Find("Slot"), info, item);
        //InfoPanel.transform.Find("ListPanel/Grid").GetComponent<O_UIGrid>().Reposition();
    }

    private int GetCurValue(NetItemInfo info, string prop)
    {
        int value = 0;
        float valuePlus = 0;
        int curValue = 0;
        DBItemBaseProp item = DBItem.Get(info.ID);

        PropertyInfo valueInfo = item.GetType().GetProperty(prop);        
        if(valueInfo != null)
            value = (int)valueInfo.GetValue(item, null);
        PropertyInfo valuePlusInfo = item.GetType().GetProperty(string.Format("{0}{1}", prop, "Plus"));
        if(valuePlusInfo != null)
            valuePlus = (float)valuePlusInfo.GetValue(item, null);

        curValue = (int)((value + (int)(valuePlus * (float)info.Lvl)) * ((float)DBManager.GetDBStar().Get(info.BreachNum).Part[item.SlotType] / (float)100 + 1));

        return curValue;
    }

    private void SetStar(GameObject go,int MaxLV)
    {
        for (int i = 1; i <= 10; i++)
        {
            if (i < MaxLV + 1)
            {
                go.transform.Find("Star/Star_" + i).gameObject.SetActive(true);
            }
            else
            {
                go.transform.Find("Star/Star_" + i).gameObject.SetActive(false);
            }
        }

    }

    private void CreateItems()
    {
        //int maxCount = lstEquip.Count;
        int maxCount = activeDraggablePanel.GetMaxCount();
        for (int i = 0; i < maxCount; ++i)
        {
            //GameObject go = (GameObject)GameObject.Instantiate(Resources.Load("UINew/CharWnd/Button_1"));
            Transform temp = InfoPanel.transform.Find("ListPanel/Grid/EquipItemTemp");
            GameObject go = GameObject.Instantiate(temp.gameObject) as GameObject;
            go.SetActive(true);
            go.transform.parent = InfoPanel.transform.Find("ListPanel/Grid");
            go.transform.localScale = new Vector3(1f, 1f, 1f);
            //go.transform.localPosition = new Vector3(i * cellWidth, 0, -20f);
            go.name += "_";
            go.name += i.ToString();

            O_UIEventListener.Get(go).onClick = ShowEquipInfo;
            lstEquipItem.Add(go);
        }
    }

    private void ShowEquipInfo(GameObject go)
    {
        string index = go.name.Substring(go.name.LastIndexOf('_') + 1, go.name.Length - go.name.LastIndexOf('_') - 1);
        int value = System.Convert.ToInt32(index);
        CharWindowNew charwnd = GameApp.GetUIManager().GetWnd("CharWindowNew") as CharWindowNew;
        charwnd.ShowEquipInfoFromList(lstEquip[value]);
        //设置选中框
        if(curItem != null)
            curItem.transform.Find("Selected").gameObject.SetActive(false);
        curItem = go;
        curItem.transform.Find("Selected").gameObject.SetActive(true);

    }

    public static int FightValueChange(NetItemInfo net_equip)
    {
        DBItemBaseProp dbItem = DBItem.Get(net_equip.ID);
        NetItemInfo item = null;
        GameRecord.Instance().GetCurRecord().GetNetPacketInfo().dicEquipInUse.TryGetValue(dbItem.SlotType, out item);
        if (net_equip.ID <= 0)
            return 0;
        if (item == null || item.ID <= 0 )
        {
            int newValue = EquipStrengthWnd.CalculateFightcapcity(net_equip);
            return newValue;
        }
        else
        {
            int oldValue = EquipStrengthWnd.CalculateFightcapcity(item);
            int newValue = EquipStrengthWnd.CalculateFightcapcity(net_equip);

            return newValue - oldValue;
        }
    }

    public void UnsetSelectItem()
    {
        if (curItem != null)
        {
            curItem.transform.Find("Selected").gameObject.SetActive(false);
            curItem = null;
        }
    }
    public void ActiveFresh(int direction)
    {
        // 向后翻页
        if (direction == 1)
        {
            // 把前面十个拿到后面，并刷新数据
            int moveNum = activeDraggablePanel.KeepCount - activeDraggablePanel.MinCount;
            int i = 0;
            for (; i < moveNum; ++i)
            {
                if ((activeDraggablePanel.CurStartIndex + activeDraggablePanel.GetMaxCount() - 1 + i + 1) >= activeDraggablePanel.lstData.Count)
                    break;
                GameObject go = lstEquipItem[0];
                lstEquipItem.RemoveAt(0);
                lstEquipItem.Add(go);

                //go.transform.localPosition = new Vector3((activeDraggablePanel.CurStartIndex + activeDraggablePanel.GetMaxCount() + i) * cellWidth, 0, 0f);
                go.transform.localPosition = new Vector3(0, -(activeDraggablePanel.CurStartIndex + activeDraggablePanel.GetMaxCount() + i) * cellHeight, -1f);
            }
            activeDraggablePanel.CurStartIndex += i;

            // 刷新移到后面的节点的数据
            for (int j = 0; j < i; ++j)
            {
                int index = activeDraggablePanel.CurStartIndex + activeDraggablePanel.GetMaxCount() - j;
                SetContent(lstEquipItem[activeDraggablePanel.GetMaxCount() - 1 - j],
                    activeDraggablePanel.lstData[activeDraggablePanel.CurStartIndex + activeDraggablePanel.GetMaxCount() - 1 - j],
                    index - 1);
            }
        }
        else if (direction == -1)    // 向前翻页
        {
            // 把后面十个拿到前面，并刷新数据
            int moveNum = activeDraggablePanel.KeepCount - activeDraggablePanel.MinCount;
            int i = 0;
            for (; i < moveNum; ++i)
            {
                if ((activeDraggablePanel.CurStartIndex - i - 1) < 0)
                    break;

                int lastIndex = activeDraggablePanel.GetMaxCount() - 1;
                GameObject go = lstEquipItem[lastIndex];
                lstEquipItem.RemoveAt(lastIndex);
                lstEquipItem.Insert(0, go);
                go.transform.localPosition = new Vector3(0f, -(activeDraggablePanel.CurStartIndex - i - 1) * cellHeight, 0f);
            }
            activeDraggablePanel.CurStartIndex -= i;

            // 刷新移到前面的节点的数据
            for (int j = 0; j < i; ++j)
            {
                int index = activeDraggablePanel.CurStartIndex + j + 1;
                SetContent(lstEquipItem[j], activeDraggablePanel.lstData[activeDraggablePanel.CurStartIndex + j], index - 1);
            }
        }
    }

}
