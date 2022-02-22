using UnityEngine;
using System.Collections;
using System.Xml;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CXmlSkillDamgeItem {
    public string type { get; private set; }
    public string calType { get; private set; }
    public int giftID { get; set; }

    //public float a { get; private set; }
    //public float b { get; private set; }
    //public float num { get; private set; }
    public CXmlSkillValueOfLvl<float> a;
    public CXmlSkillValueOfLvl<float> b;
    public CXmlSkillValueOfLvl<float> num;

    public CXmlSkillDamgeItem() { }
    public void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        type = kXmlRead.Str("type");
        calType = kXmlRead.Str("calType");
        a = new CXmlSkillValueOfLvl<float>(ele, "a", 0f);
        b = new CXmlSkillValueOfLvl<float>(ele, "b", 0f);
        num = new CXmlSkillValueOfLvl<float>(ele, "num", 0f);
    }

    public float GetDamageValue(ServerCreature cast, ServerCreature target, out string damageType)
    {
        damageType = type;
        float dam = 0;

        if (giftID != 0)
        {
            SkillLevelPara slp = DBManager.GetDBSkillLevelPara().GetSkillInfo(giftID, cast);
            if (slp != null)
            {
                float fAttack = 0;
                float fDefence = 0;

                //defence cal
                if (type == "physical")
                {
                    fDefence = (float)target.attackProp.dwPhysicalDefence;
                }
                else if (type == "element")
                {
                    fDefence = (float)target.attackProp.dwMagicDefence;
                }

                //attack cal

                for (int i = 0; i < slp.paraType.Count; i++)
                {
                    float subAttack = 0;
                    if (type == "physical")
                    {
                        if (slp.paraType[i] == (int)DBSkillLevelPara.ESKillLevelParaType.ePhysicFix)
                            subAttack = (float)slp.para[i];
                        else if (slp.paraType[i] == (int)DBSkillLevelPara.ESKillLevelParaType.ePhysicRate)
                            subAttack = (float)cast.attackProp.dwPhysicalDamage * (float)slp.para[i] / 100.0f;
                        else if (slp.paraType[i] == (int)DBSkillLevelPara.ESKillLevelParaType.ePhysicHPP)
                            subAttack = (float)(target.GetHP()) * (float)(slp.para[i]) / 100.0f;
                    }
                    else if (type == "element")
                    {
                        if (slp.paraType[i] == (int)DBSkillLevelPara.ESKillLevelParaType.eElementFix)
                            subAttack = (float)slp.para[i];
                        else if (slp.paraType[i] == (int)DBSkillLevelPara.ESKillLevelParaType.eElementRate)
                            subAttack = (float)cast.attackProp.dwMagicDamage * (float)slp.para[i] / 100.0f;
                        else if (slp.paraType[i] == (int)DBSkillLevelPara.ESKillLevelParaType.eElementHPP)
                            subAttack = (float)(target.GetHP()) * (float)(slp.para[i]) / 100.0f;
                    }
                    fAttack += subAttack;
                }

                //final damage

                if (fAttack > fDefence)
                {
                    dam = fAttack - fDefence;
                    //dam = (int)round(ShenQiBuffCount(dam, pRoleSession, pCast, pTarget));
                }
                else
                {
                    float ff = 1.2f + (cast.GetLevel() / (cast.GetLevel() + 100));
                    var randValue = 0;
                    var gameApp = GameApp.Instance();
                    if (cast is ServerPlayer || (cast is ServerMonster && target is ServerMonster))
                    {
                        randValue = gameApp.PlayerDmgRand.Next(1, 2 * cast.GetLevel());
                    }
                    else
                    {
                        randValue = gameApp.CommonRand.Next(1, 2 * cast.GetLevel());
                    }
                    dam = (int)System.Math.Round(((int)System.Math.Round(SkillBuff(cast, b) * ff, 0, System.MidpointRounding.AwayFromZero) + cast.GetLevel()) * (2 * fAttack / (fAttack + fDefence)), 0, System.MidpointRounding.AwayFromZero) + randValue;
                    dam = (int)System.Math.Round(ShenQiBuffCount(dam, target));
                }
            }
        }
        else
        {
            if (calType == "num")
            {
                dam = 2.5f * num.GetValue(cast);
                Debug.LogWarning("GetDamageValue num dam=" + dam);
            }
            else if (calType == "perMax")
            {
                dam = num.GetValue(cast) * target.GetMaxHP();
            }
            else if (calType == "perCur")
            {
                dam = num.GetValue(cast) * target.GetHP();
            }
            else if (calType == "formula")
            {
                float fAttack = 0;
                float fDefence = 0;
                if (type == "physical")
                {
                    fAttack = cast.attackProp.dwPhysicalDamage;
                    fDefence = target.attackProp.dwPhysicalDefence;
                }
                else if (type == "element")
                {
                    fAttack = cast.attackProp.dwMagicDamage;
                    fDefence = target.attackProp.dwMagicDefence;
                }
                else if (type == "spirit")
                {
                    fAttack = cast.attackProp.dwSpiritDamage;
                    fDefence = target.attackProp.dwSpiritDefence;
                }

                float ff = 1.2f+(cast.GetLevel() / (cast.GetLevel() + 100));
                if (fAttack > fDefence)
                {
                    dam = (int)System.Math.Round(a.GetValue(cast) * System.Math.Max(0, ((int)fAttack - (int)fDefence)), 0, System.MidpointRounding.AwayFromZero) + (int)System.Math.Round(SkillBuff(cast, b) * ff, 0, System.MidpointRounding.AwayFromZero) + cast.GetLevel();
                    dam = (int)System.Math.Round(ShenQiBuffCount(dam, target));
                }
                else
                {
                    var randValue = 0;
                    var gameApp = GameApp.Instance();
                    if ((cast is ServerPlayer || cast is ServerPet) || (cast is ServerMonster && target is ServerMonster))
                    {
                        randValue = gameApp.PlayerDmgRand.Next(1, 2 * cast.GetLevel());
                    }
                    else
                    {
                        randValue = gameApp.CommonRand.Next(1, 2 * cast.GetLevel());
                    }
                    dam = (int)System.Math.Round(((int)System.Math.Round(SkillBuff(cast, b) * ff, 0, System.MidpointRounding.AwayFromZero) + cast.GetLevel()) * (2 * fAttack / (fAttack + fDefence)), 0, System.MidpointRounding.AwayFromZero) + randValue;
                    dam = (int)System.Math.Round(ShenQiBuffCount(dam, target));
                }
            }
        }
    
        return dam;
    }


    private float SkillBuff(ServerCreature cast,CXmlSkillValueOfLvl<float> x)
    {
        
        float dam = x.GetValue(cast);
        Debug.Log("Value=" + x.GetValue(cast));
        if (ServerPlayerMgr.GetMainPlay()!=null&&cast.ulObjectID == ServerPlayerMgr.GetMainPlay().ulObjectID && ShenQiBuff.Intance().skillbuff.ContainsKey(x.GiftID))//如果存在技能buff则走此逻辑
        {

            dam = x.GetskillBuffValue(cast).Value * (1 + 0.05f * x.GetskillBuffValue(cast).Key);
            Debug.Log("Value=" + x.GetskillBuffValue(cast).Value + "Key=" + x.GetskillBuffValue(cast).Key);

        }
        return dam;
    }


    private float ShenQiBuffCount(float Damage, ServerCreature target)
    {
        float dam = Damage;
        ShenQiBuff buff = ShenQiBuff.Intance();
        if(buff!=null)
        {
            if (target.ulObjectID == ServerPlayerMgr.GetMainPlay().ulObjectID)//如果目标是主角时候
            {
                if (buff.shenqiBuff.ContainsKey(8) && buff.shenqiBuff.ContainsKey(9))
                {
                    dam = dam * (1 - buff.shenqiBuff[8] * 0.01f) - buff.shenqiBuff[9];//最终受到的伤害=技能最终伤害*（1-伤害减免）-伤害减免值
                    if (dam <= 0)
                        dam = 0;
                }
            }
            else
            {
                if (buff.shenqiBuff.ContainsKey(11) && buff.shenqiBuff.ContainsKey(10))
                {
                    dam = dam * (1 + buff.shenqiBuff[11] * 0.01f) + buff.shenqiBuff[10];//最终输出的伤害=技能最终伤害*（1+伤害强度%）+神圣伤害
                }
            }
        }
        return dam;
    }

#if UNITY_EDITOR
    private string[] m_DamageItem = new string[3] { "physical", "element", "spirit" };
    private string[] m_CalType = new string[] { "num ", " perMax ", " perCur ", " formula ", " petFormula" };
    public void Draw()
    {
        GUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("伤害:", GUILayout.Width(120));

        int index_damage = -1;
        int index_cal = -1;

        for (int i = 0; i < m_DamageItem.Length; i++ ) 
        {
            if(m_DamageItem[i].Equals(type))
            {
                index_damage = i;
                break;
            }
        }

        for (int i = 0; i < m_CalType.Length; i++)
        {
            if(m_CalType[i].Equals(calType))
            {
                index_cal = i;
                break;
            }
        }

        if(index_damage == -1)
        {
            type = m_DamageItem[0];
            index_damage = 0;
        }

        if(index_cal == -1)
        {
            calType = m_CalType[0];
            index_cal = 0;
        }

        if(a == null)
        {
            a = new CXmlSkillValueOfLvl<float>("参数A",0.0f);
        }
        a.SetName("参数A");
        if(b == null)
        {
            b = new CXmlSkillValueOfLvl<float>("参数B",0.0f);
        }
        b.SetName("参数B");
        
        if(num == null)
        {
            num = new CXmlSkillValueOfLvl<float>("计算基数",0.0f);
        }
        num.SetName("计算基数");

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("伤害类型:", GUILayout.Width(60));
        type = m_DamageItem[EditorGUILayout.Popup(index_damage, m_DamageItem)];
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("计算方法:", GUILayout.Width(60));
        calType = m_CalType[EditorGUILayout.Popup(index_cal, m_CalType)];
        EditorGUILayout.Space();
        GUILayout.EndHorizontal();

        a.Draw();
        b.Draw();
        num.Draw();

        GUILayout.EndVertical();
    }
    public void Export(XmlDocument doc, XmlNode parent, string name = "damageItem")
    {
        XmlElement damageItem = doc.CreateElement(name);

        damageItem.SetAttribute("type", type);
        damageItem.SetAttribute("calType", calType);

        if(a != null)
        {
            a.Export(doc, damageItem, "a");
        }
        if (b != null)
        {
            b.Export(doc, damageItem, "b");
        }
        if (num != null)
        {
            num.Export(doc, damageItem, "num");
        }
        parent.AppendChild(damageItem);
    }
#endif
}
