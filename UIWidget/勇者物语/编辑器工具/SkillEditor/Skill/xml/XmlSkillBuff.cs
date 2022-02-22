using System;
using System.Xml;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CXmlSkillBuff
{
    public CXmlSkillValueOfLvl<int> ID;
    public CXmlSkillValueOfLvl<float> Prob;
    public CXmlSkillBuff() 
    {
    }
    public void Init(XmlElement ele)
    {
        ID = new CXmlSkillValueOfLvl<int>(ele, "id", 0);
        Prob = new CXmlSkillValueOfLvl<float>(ele, "prob", 100f);
    }

    public void TriggerBuff(ServerCreature target, ServerCreature cast, int damagePower)
    {
        try
        {
            if (target != null && cast != null && target.scene != null && target.creatureState != ServerCreature.CREATURESTATE.DEAD)
            {
                if (GameApp.Instance().PlayerDmgRand.Next(100) < Prob.GetValue(cast))
                {
                    //叠加规则
                    AddBuffToCreature(ID.GetValue(cast), target, cast, damagePower);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("add buff error " + ex.Message);
        }
    }

    public static void AddBuffToCreature(int buffID, ServerCreature target, ServerCreature cast, int damagePower = 0)
    {
        ServerBuff buff = new ServerBuff();
        buff.buffBeginTime = Time.time;
        buff.buffID = buffID;

        BuffData data = GameApp.GetBuffManager().GetBuffData(buff.buffID);
        buff.casterID = cast.ulObjectID;
        buff.targetID = target.ulObjectID;
        buff.buffEndTime = buff.buffBeginTime + data.LifeSeconds.GetValue(cast);
        buff.buffInstanseID = SceneObject.ulGameObjectID++;
        buff.damagePower = damagePower + cast.attackProp.wDamagePower;
        buff.groupID = data.GroupID;
        buff.beDamage = false;
        buff.beAbsorbshield = false;
        buff.transferMapBreak = data.transferMapBreak;

        if(data.Influence != null)
        {
            //可能存在缺省
            if (data.Influence.Movable != null)
            {
                if (data.Influence.Movable.DependOnSteelPower == false || damagePower + cast.attackProp.wDamagePower > target.attackProp.wSteelPower)
                {
                    buff.temp.movable = data.Influence.Movable.Value;
                }
            }

            if (data.Influence.Skillable != null)
            {
                if (data.Influence.Skillable.DependOnSteelPower == false || damagePower + cast.attackProp.wDamagePower > target.attackProp.wSteelPower)
                {
                    buff.temp.skillable = data.Influence.Skillable.Value;
                }
            }

            if (data.Influence.MoveSpeedUp != null)
            {
                if (data.Influence.MoveSpeedUp.DependOnSteelPower == false || damagePower + cast.attackProp.wDamagePower > target.attackProp.wSteelPower)
                {
                    if (data.Influence.MoveSpeedUp.CalType == "num")
                    {
                        buff.temp.fSpeedUp = data.Influence.MoveSpeedUp.Num.GetValue(cast);
                    }
                    else if (data.Influence.MoveSpeedUp.CalType == "per")
                    {
                        buff.temp.fSpeedUp = BaseMovement.MoveSpeed * data.Influence.MoveSpeedUp.Num.GetValue(cast);
                    }
                }
            }
        }

        if (data.Damages != null)
        {
            buff.beDamage = true;
            buff.damageMultiple = data.Damages.DamageMultiple.GetValue(cast);
            buff.critical = data.Damages.Critical.GetValue(cast);
            buff.damageHeal = data.Damages.Type;
            buff.stringType = data.Damages.Damages.Type;

            float a = data.Damages.Damages.ParamA.GetValue(cast);
            float b = data.Damages.Damages.ParamB.GetValue(cast);
            float num = data.Damages.Damages.Num.GetValue(cast);

            buff.damageValue1 = 0;

            if (data.Damages.Damages.CalType == "num")
            {
                buff.damageValue1 = num;
            }
            else if (data.Damages.Damages.CalType == "perMax")
            {
                buff.damageValue1 = num * target.GetMaxHP();
            }
            else if (data.Damages.Damages.CalType == "perCur")
            {
                buff.damageValue1 = num * target.GetHP();
            }
            else if (data.Damages.Damages.CalType == "formula")
            {
                float fDefence = 0;
                float fAttack = 0;
                if (buff.stringType == "physical")
                {
                    fAttack = cast.attackProp.dwPhysicalDamage;
                    fDefence = target.attackProp.dwPhysicalDefence;
                }
                else if (buff.stringType == "element")
                {
                    fAttack = cast.attackProp.dwMagicDamage;
                    fDefence = target.attackProp.dwMagicDefence;
                }
                else if (buff.stringType == "spirit")
                {
                    fAttack = cast.attackProp.dwSpiritDamage;
                    fDefence = target.attackProp.dwSpiritDefence;
                }

                if (fAttack > fDefence)
                {
                    buff.damageValue1 = (int)System.Math.Round(a * ((int)fAttack - (int)fDefence), 0, MidpointRounding.AwayFromZero) + b;
                }
                else
                {
                    buff.damageValue1 = (float)System.Math.Max((int)System.Math.Round(b * 0.5f, 0, MidpointRounding.AwayFromZero), (int)(float)(System.Math.Pow((double)0.98, (double)((int)fDefence - (int)fAttack)) * (double)b));
                }
            }

            //暴击伤害影响
            if (buff.beCS)
            {
                buff.damageValue1 *= 2;
            }

            //攻击方状态影响
            if (buff.damageHeal == "heal")
            {
                buff.damageValue1 *= cast.attackProp.fExportHealM;
            }
            else
            {
                if (buff.stringType == "physical")
                {
                    buff.damageValue1 *= cast.attackProp.fExportPDM;
                    buff.damageValue1 *= cast.attackProp.fExportDM;
                }
                else if (buff.stringType == "element")
                {
                    buff.damageValue1 *= cast.attackProp.fExportEDM;
                    buff.damageValue1 *= cast.attackProp.fExportDM;
                }
                else if (buff.stringType == "spirit")
                {
                    buff.damageValue1 *= cast.attackProp.fExportSDM;
                    buff.damageValue1 *= cast.attackProp.fExportDM;
                }
                else
                {
                    buff.damageValue1 *= cast.attackProp.fExportDM;
                }
            }

            buff.damageSecondTime = data.Damages.Damages.FrequenceSeconds.GetValue(cast);
            buff.lastDamageTime = Time.time;
        }

        if(data.AbsorbShield != null)
        {
            buff.beAbsorbshield = true;
            buff.absorbshieldValue = data.AbsorbShield.HP.GetValue(cast);
        }

        if(data.Influence != null)
        {
            if (data.Influence.BeAttacked != null)
            {
                buff.temp.beAttacked = data.Influence.BeAttacked.Value;
            }

            if (data.Influence.Visible != null)
            {
                buff.temp.visible = data.Influence.Visible.Value;
            }
        }
        
        if (data.PropertyUp != null)
        {
            foreach(BuffPropertyItemData item in data.PropertyUp.PropertyUps)
            {
                item.AddBuffPropToCreature(cast, target, ref buff);
            }
        }

        List<ServerBuff> buffRemoveList = new List<ServerBuff>();
        bool giveUp = false;
        foreach (ServerBuff serverBuff in target.buffList)
        {
            if (serverBuff.buffID == buff.buffID)
            {
                if (data.SameBuffRule == 0)
                {
                    giveUp = true;
                }
                else if (data.SameBuffRule == 1)
                {
                    buffRemoveList.Add(serverBuff);
                }

            }
            else if (serverBuff.groupID == buff.groupID)
            {
                if (data.SameGroupRule == 0)
                {
                    giveUp = true;
                }
                else if (data.SameGroupRule == 1)
                {
                    buffRemoveList.Add(serverBuff);
                }
            }
        }
        foreach (ServerBuff removeBuff in buffRemoveList)
        {
            target.RemoveBuffFromCreature(removeBuff, 1);
        }

        if (!giveUp)
        {
            target.AddBuffToCreature(buff);
        }
    }

#if UNITY_EDITOR
    private string[] m_Where = new string[3] { "land", "air", "fall" };
    public void Draw()
    {
        GUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Buff:", GUILayout.Width(30));
        if(ID == null)
        {
            ID = new CXmlSkillValueOfLvl<int>("Buff ID:",0);
        }
        ID.SetName("Buff ID");

        if (Prob == null)
        {
            Prob = new CXmlSkillValueOfLvl<float>("Buff 概率:", 0);
        }
        Prob.SetName("Buff 概率");

        ID.Draw();
        Prob.Draw();
        GUILayout.EndVertical();
    }
    public void Export(XmlDocument doc, XmlNode parent, string name = "buff")
    {
        XmlElement buff = doc.CreateElement(name);

        if(ID != null)
        {
            ID.Export(doc,buff,"id");
        }
        if (Prob != null)
        {
            Prob.Export(doc, buff, "prob");
        }
        parent.AppendChild(buff);
    }
#endif
}