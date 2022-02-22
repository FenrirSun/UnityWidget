using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public enum DAMAGETYPE : ushort
{
    COMMOM_DAMAGE = 0,   //普通伤害
    CSR_DAMAGE,          //暴击
    DEFENSE_DAMAGE,      //格挡
    HEAL,                //治疗
    VA_DAMAGE,           //闪避，damageValue为0则Miss
    ABSORBED_DAMAGE,     //伤害吸收，适用于BUFF防御盾
}

public class CXmlSkillGroup {
    public string op { get; private set; }
   // public float damageMultiple{ get; private set; }
    public List<CXmlSkillDamgeItem> damageItemList = new List<CXmlSkillDamgeItem>();
    public CXmlSkillValueOfLvl<float> damageMultiple;
    public CXmlSkillValueOfLvl<float> critical;
    public string strDamageType;
    public int giftID;

    public CXmlSkillGroup() { }
    public void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        op = kXmlRead.Str("op");
        strDamageType = kXmlRead.Str("damagetype");
        giftID = kXmlRead.Int("giftID", 0);
        damageMultiple = new CXmlSkillValueOfLvl<float>(ele, "damageMultiple", 1.0f);
        critical = new CXmlSkillValueOfLvl<float>(ele, "critical", 0);
        foreach (XmlNode node in ele.ChildNodes)
        {
            if(node.Name == "damageItem")
            {
                CXmlSkillDamgeItem damageItem = new CXmlSkillDamgeItem();
                damageItem.giftID = giftID;
                damageItem.Init(node as XmlElement);
                damageItemList.Add(damageItem);
            }
        }
    }

    public void ProcessDamage(ServerCreature cast, ServerCreature target, bool beHit)
    {
        DAMAGETYPE damageType = DAMAGETYPE.COMMOM_DAMAGE;
	    if (cast == null || target == null)
	    {
		    return;
	    }
        string strType = "";
	    float fDamageValue = 0;

        if (!beHit)
        {
            fDamageValue = 0;
            damageType = DAMAGETYPE.VA_DAMAGE;
        }
        else
        {
            if (op == "and")
            {
                foreach (CXmlSkillDamgeItem item in damageItemList)
                {
                    fDamageValue += item.GetDamageValue(cast, target, out strType);
                }
            }
            else if (op == "or")
            {
                foreach (CXmlSkillDamgeItem item in damageItemList)
                {
                    fDamageValue = System.Math.Max(fDamageValue, item.GetDamageValue(cast, target, out strType));
                }
            }

            fDamageValue *= damageMultiple.GetValue(cast);

          	SkillLevelPara slp = null;
            if (giftID != 0)
                slp = DBManager.GetDBSkillLevelPara().GetSkillInfo(giftID, cast);


            int randValue = 0;
            var gameApp = GameApp.Instance();
            if (((cast is ServerPlayer || cast is ServerPet) || (cast is ServerMonster && target is ServerMonster)) && ((ServerMonster)target).monsterConfig.DamegeRate >= 0.01)
            {
                randValue = gameApp.PlayerDmgRand.Next(100);
            }
            else
            {
                randValue = gameApp.CommonRand.Next(100);
            }

            int crRate = (int)((cast.attackProp.fCSR * 100) + critical.GetValue(cast));
            if (slp!=null)
            {
                for (int i = 0; i < slp.paraType.Count; i++)
                {
                    if (slp.paraType[i] == (int)DBSkillLevelPara.ESKillLevelParaType.eCrAdd)
                        crRate += slp.para[i];
                }
            }


            if (randValue < crRate)
            {
                fDamageValue *= 2;
                damageType = DAMAGETYPE.CSR_DAMAGE;
            }

            if (strDamageType != null && strDamageType == "heal")
            {
                fDamageValue *= cast.attackProp.fExportHealM;
                fDamageValue *= target.attackProp.fImportHealM;
                damageType = DAMAGETYPE.HEAL;
            }
            else
            {
                if (strType == "physical")
                {
                    fDamageValue *= cast.attackProp.fExportPDM;
                    fDamageValue *= cast.attackProp.fExportDM;

                    fDamageValue *= target.attackProp.fImportPDM;
                    fDamageValue *= target.attackProp.fImportDM;
                }
                else if (strType == "element")
                {
                    fDamageValue *= cast.attackProp.fExportEDM;
                    fDamageValue *= cast.attackProp.fExportDM;

                    fDamageValue *= target.attackProp.fImportEDM;
                    fDamageValue *= target.attackProp.fImportDM;
                }
                else if (strType == "spirit")
                {
                    fDamageValue *= cast.attackProp.fExportSDM;
                    fDamageValue *= cast.attackProp.fExportDM;

                    fDamageValue *= target.attackProp.fImportSDM;
                    fDamageValue *= target.attackProp.fImportDM;
                }
                else
                {
                    fDamageValue *= cast.attackProp.fExportDM;
                    fDamageValue *= target.attackProp.fImportDM;
                }
            }
        }

        uint iDamagePower = (uint)fDamageValue;
        if (target.objType == (uint)SceneObject.ObjectType.OBJECT_MONSTER && ((ServerMonster)target).monsterConfig.DamegeRate < 0.01)
        {
            damageType = DAMAGETYPE.DEFENSE_DAMAGE;
            iDamagePower = 0;
        }

        if (cast.objType == (uint)SceneObject.ObjectType.OBJECT_MONSTER && GameApp.GetSceneManager().IsBossScene())
        {
            target.OnDamage((uint)iDamagePower, damageType,1);
        }
        else
        {
            target.OnDamage((uint)iDamagePower, damageType);
        }
    }

#if UNITY_EDITOR
    private bool m_Draw = false;
    private string[] m_OP = new string[2] { "and", "max" };
    private string[] m_DamageType = new string[2] { "damage", "heal" };
    public void Draw()
    {
        GUILayout.BeginHorizontal("box");
        int index_OP = -1;
        int index_cal = -1;

        for (int i = 0; i < m_OP.Length; i++)
        {
            if (m_OP[i].Equals(op))
            {
                index_OP = i;
                break;
            }
        }

        for (int i = 0; i < m_DamageType.Length; i++)
        {
            if (m_DamageType[i].Equals(strDamageType))
            {
                index_cal = i;
                break;
            }
        }

        if (index_OP == -1)
        {
            op = m_OP[0];
            index_OP = 0;
        }

        if (index_cal == -1)
        {
            strDamageType = m_DamageType[0];
            index_cal = 0;
        }

        EditorGUILayout.LabelField("伤害计算方式:", GUILayout.Width(80));
        op = m_OP[EditorGUILayout.Popup(index_OP, m_OP)];
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("伤害方式:", GUILayout.Width(60));
        strDamageType = m_DamageType[EditorGUILayout.Popup(index_cal, m_DamageType)];
        EditorGUILayout.Space();

        GUILayout.EndHorizontal();

        if (damageMultiple == null)
        {
            damageMultiple = new CXmlSkillValueOfLvl<float>("加成倍率", 1.0f);
        }
        damageMultiple.SetName("加成倍率");
        if (critical == null)
        {
            critical = new CXmlSkillValueOfLvl<float>("暴击率加成", 0.0f);
        }
        critical.SetName("暴击率加成");

        damageMultiple.Draw();
        critical.Draw();

        GUILayout.BeginVertical("box");
        for (int i = 0; i < damageItemList.Count; i++ )
        {
            damageItemList[i].Draw();
        }
        GUILayout.EndVertical();
    }
    public void Export(XmlDocument doc, XmlNode parent, string name = "group")
    {
        XmlElement group = doc.CreateElement(name);

        group.SetAttribute("op",op);
        group.SetAttribute("type", strDamageType);
        if (damageMultiple != null) 
        {
            damageMultiple.Export(doc, group, "damageMultiple");
        }
        if (critical != null)
        {
            critical.Export(doc, group, "critical");
        }
        if(damageItemList.Count > 0)
        {
            CXmlSkillDamgeItem item = null;
            foreach(var it in damageItemList)
            {
                GUILayout.BeginHorizontal();
                it.Export(doc,group);

                if(GUILayout.Button("Delete"))
                {
                    item = it;
                }
                GUILayout.EndHorizontal();
            }
            if(item != null)
            {
                damageItemList.Remove(item);
            }
        }


        if(GUILayout.Button("添加DamageItem"))
        {
            damageItemList.Add(new CXmlSkillDamgeItem());
        }
        parent.AppendChild(group);
    }
#endif
}
