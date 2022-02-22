using System;
using System.Xml;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CXmlSkillInfluence
{
    public CXmlSkillValueOfLvl<int> DamagePower;
    public CXmlSkillDamage XmlDamage { get; private set; }
    public CXmlSkillHitAction XmlHitAction { get; private set; }
    public CXmlSkillGravity XmlGravity { get; private set; }
    public List<CXmlSkillBuff> XmlBuffs { get; private set; }
    public CXmlSkillSpirit spiritSqueeze { get; private set; }

    public CXmlSkillInfluence() 
    {
    }
    public void Init(XmlElement ele)
    {
        DamagePower = new CXmlSkillValueOfLvl<int>(ele, "damagePower", 0);

        foreach (XmlNode node in ele.ChildNodes)
        {
            switch (node.Name)
            {
                case "damage":
                    {
                        XmlDamage = new CXmlSkillDamage();
                        XmlDamage.Init(node as XmlElement);
                    }
                    break;
                case "hitAction":
                    {
                        XmlHitAction = new CXmlSkillHitAction();
                        XmlHitAction.Init(node as XmlElement);
                    }
                    break;
                case "gravity":
                    {
                        XmlGravity = new CXmlSkillGravity();
                        XmlGravity.Init(node as XmlElement);
                    }
                    break;
                case "buffs":
                    {
                        XmlBuffs = new List<CXmlSkillBuff>();
                        foreach (XmlNode nodeBuff in node.ChildNodes)
                        {
                            if (nodeBuff.Name == "buff")
                            {
                                CXmlSkillBuff xmlBuff = new CXmlSkillBuff();
                                xmlBuff.Init(nodeBuff as XmlElement);
                                XmlBuffs.Add(xmlBuff);
                            }
                        }
                    }
                    break;
                case "SpiritSqueeze":
                    {
                        spiritSqueeze = new CXmlSkillSpirit();
                        spiritSqueeze.Init(node as XmlElement);
                    }
                    break;
                default:
                    break;

            }
        }
    }

#if UNITY_EDITOR
    private bool m_Draw = false;
    public bool m_Effective = false;

    public void Draw()
    {
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        m_Effective = EditorGUILayout.Toggle(m_Effective, GUILayout.Width(10));
        m_Draw = EditorGUILayout.Foldout(m_Draw, "Influence");
        GUILayout.EndHorizontal();

        if (m_Draw)
        {
            if (DamagePower == null)
            {
                DamagePower = new CXmlSkillValueOfLvl<int>("damagePower", 0);
            }

            DamagePower.SetName("¸ÕÐÔ");

            DamagePower.Draw();

            if (XmlDamage == null)
            {
                XmlDamage = new CXmlSkillDamage();
            }
            XmlDamage.Draw();

            if (XmlHitAction == null)
            {
                XmlHitAction = new CXmlSkillHitAction();
            }

            XmlHitAction.Draw();

            if (XmlGravity == null)
            {
                XmlGravity = new CXmlSkillGravity();
            }
            XmlGravity.Draw();

            if (XmlBuffs == null)
            {
                XmlBuffs = new List<CXmlSkillBuff>();
            }

            for (int i = 0; i < XmlBuffs.Count; i++ )
            {
                GUILayout.BeginHorizontal();
                XmlBuffs[i].Draw();
                if(GUILayout.Button("Delete"))
                {
                    XmlBuffs.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
            }

            if(GUILayout.Button("Ìí¼ÓBuff"))
            {
                XmlBuffs.Add(new CXmlSkillBuff());
            }
        }
        GUILayout.EndVertical();
    }
    public void Export(XmlDocument doc, XmlNode parent, string name = "influence")
    {
        XmlElement influence = doc.CreateElement(name);

        if (DamagePower != null)
        {
            DamagePower.Export(doc, influence, "damagePower");
        }

        if (XmlDamage != null)
        {
            XmlDamage.Export(doc,influence);
        }

        if (XmlHitAction != null) 
        {
            XmlHitAction.Export(doc,influence);
        }

        if (XmlGravity != null)
        {
            XmlGravity.Export(doc,influence);
        }

        XmlElement buffs = doc.CreateElement("buffs");

        if (XmlBuffs != null && XmlBuffs.Count > 0)
        {
            foreach (var it in XmlBuffs)
            {
                it.Export(doc,buffs);
            }
        }

        influence.AppendChild(buffs);
        parent.AppendChild(influence);
    }
#endif

    //public void Influence(ulong targetID,CSkillEvent ev)
    //{
    //   // »÷ÍË»÷·É

    //    GuiTextDebug.OutputToGUI("target id = " + targetID + " influenced!");
    //    ML2GameObject target = GameApp.GetWorldManager().GetObject(targetID);
    //    if (target != null && Camera.main != null)
    //    {
    //        Vector3 pos = target.m_ObjInstance.transform.position;
    //        pos.y += 1.0f;
    //        Vector3 viewPos = Camera.main.WorldToViewportPoint(target.m_ObjInstance.transform.position);
    //        GameApp.GetUIManager().DisplayDamage(10, viewPos.x, viewPos.y);
    //    }
    //}
}