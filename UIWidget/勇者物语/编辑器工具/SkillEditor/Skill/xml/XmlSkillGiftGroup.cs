using System.Collections.Generic;
using System;
using System.Xml;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CXmlSkillGiftGroup
{
    public List<CXmlSkillGift> Gifts = new List<CXmlSkillGift>();
    //private List<int> NeedLevels = new List<int>();
    public string Chain { get; private set; }
    private int Job;

    public CXmlSkillGiftGroup(int job)
    {
        Job = job;
    }

    public void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        //kXmlRead.Vec("needLevel", NeedLevels);
        Chain = kXmlRead.Str("chain");
        foreach (XmlNode node in ele.ChildNodes)
        {
            switch (node.Name)
            {
                case "gift":
                    {
                        CXmlSkillGift xmlSkillGift = new CXmlSkillGift(this);
                        xmlSkillGift.Init(node as XmlElement);
                        Gifts.Add(xmlSkillGift);
                        GameApp.GetSkillManager().AddXmlSkillGift(xmlSkillGift);
                        //Debug.LogError(xmlSkillGift.ID);
                    }
                    break;
            }
        }
    }

    //public int GetMainGiftMaxLvl()
    //{
    //    int lvl = (int)GameApp.GetWorldManager().MainPlayer.GetProperty().Level;
    //    for (int i = 0; i < NeedLevels.Count; ++i)
    //    {
    //        if (NeedLevels[i] > lvl)
    //        {
    //            return i - 1;
    //        }
    //    }
    //    return NeedLevels.Count - 1;
    //}

    public int GetMainGiftNeedLevel()
    {
        return Gifts[0].NeedLevels[1];
    }

    //public int GetMainGiftNeedLevel(int lvl)
    //{
    //    return Gifts[0].NeedLevels[lvl];
    //}

#if UNITY_EDITOR
    private int m_NewGiftID = -1;
    public void Draw()
    {
        GUILayout.BeginVertical("box");

        for (int i = 0; i < Gifts.Count;i++ )
        {
            GUILayout.BeginHorizontal();
            Gifts[i].Draw();
            if(GUILayout.Button("Delete"))
            {
                Gifts.RemoveAt(i);
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();

        if (GameApp.GetSkillManager().xmlSkillGifts.ContainsKey(m_NewGiftID))
        {
            GUI.color = Color.red;
        }
        m_NewGiftID = EditorGUILayout.IntField(m_NewGiftID);

        GUI.color = Color.white;
        if(GUILayout.Button("Ìí¼ÓGift"))
        {
            if (!GameApp.GetSkillManager().xmlSkillGifts.ContainsKey(m_NewGiftID))
            {
                CXmlSkillGift newGift = new CXmlSkillGift(this);
                newGift.SetID(m_NewGiftID);
                Gifts.Add(newGift);
                GameApp.GetSkillManager().AddXmlSkillGift(newGift);
            }

        }

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    public void Export(XmlDocument doc, XmlNode parent, string name = "skillGroup")
    {
        XmlElement skillGroup = doc.CreateElement(name);

        skillGroup.SetAttribute("chain", Chain);

        foreach (var it in Gifts) 
        {
            it.Export(doc,skillGroup);
        }

        parent.AppendChild(skillGroup);
    }
    public void SetChainName(string chainName)
    {
        Chain = chainName;
    }
#endif
}