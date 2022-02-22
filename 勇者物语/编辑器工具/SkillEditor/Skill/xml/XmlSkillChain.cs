using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif
public class CXmlSkillChain
{
    public string Name { get; private set; }
    //public double CoolDown { get; private set; }
    public CXmlSkillValueOfLvl<float> CoolDown;
    public int Index { get; private set; }
    public float Distance { get; private set; }
    private List<CXmlSkill> xmlSkills;
    public int XmlSkillCount
    {
        get
        {
            return xmlSkills.Count;
        }
    }

    public CXmlSkillChain()
    {
        xmlSkills = new List<CXmlSkill>();
    }

    public void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        Name = kXmlRead.Str("name");
        Index = kXmlRead.Int("index");
        Distance = kXmlRead.Float("distance");

        //CoolDown = kXmlRead.Float("coolDown");
        CoolDown = new CXmlSkillValueOfLvl<float>(ele, "coolDown", 0.0f);

        CXmlRead xmlRead = new CXmlRead(null);
        foreach(XmlNode nodeSkill in ele.ChildNodes)
        {
            if (nodeSkill.Name == "skill")
            {
                xmlRead.SetXmlElement(nodeSkill as XmlElement);
                string szName = xmlRead.Str("filename");
              //  XmlElement rootSkillsEle = CXmlRead.GetRootElement("DB/Skill/" + szName);

                XmlDocument xmlDoc = new XmlDocument();
                TextAsset textAsset = GameApp.GetResourceManager().LoadDB("DB/Skill/" + szName);
                if (textAsset == null)
                {
                    Debug.LogError("Load szName DB Failed!" + szName);
                    return;
                }
                //  Debug.Log("DB/Localization/LocalizationFileName----------------");
                xmlDoc.Load(new StringReader(textAsset.text));
                XmlElement rootSkillsEle = xmlDoc.DocumentElement;

                if (rootSkillsEle == null)
                {
                    Debug.LogError(szName + "error!");
                }
                else
                {
                    CXmlSkill xmlSkill = new CXmlSkill(xmlSkills.Count, this);
                    xmlSkill.Init(rootSkillsEle.FirstChild as XmlElement);
                    xmlSkills.Add(xmlSkill);
#if UNITY_EDITOR
                    xmlSkill.m_FileName = szName;
#endif
                    GameApp.GetSkillManager().AddXmlSkill(xmlSkill);
                }
            }
        }
    }

    public CXmlSkill GetXmlSkill(int iIndex)
    {
        return xmlSkills[iIndex];
    }

#if UNITY_EDITOR
    public bool m_Draw = false;
    public bool m_PreDraw = false;
    public void Draw()
    {
        xmlSkills.Sort(Sort);
        GUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("¼¼ÄÜÁ´:", GUILayout.Width(40));
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("ID:", GUILayout.Width(30));
        Index = EditorGUILayout.IntField(Index);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Ãû×Ö:", GUILayout.Width(30));
        Name = EditorGUILayout.TextField(Name);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("¾àÀë:", GUILayout.Width(30));
        Distance = EditorGUILayout.FloatField(Distance);
        EditorGUILayout.Space();
        GUILayout.EndHorizontal();

        if (CoolDown == null)
        {
            CoolDown = new CXmlSkillValueOfLvl<float>("CD:",0);
        }
        CoolDown.SetName("CD:");
        CoolDown.Draw();
        GUILayout.EndVertical();
    }

    public void AddSkill(CXmlSkill skill)
    {
        if (xmlSkills == null)
        {
            xmlSkills = new List<CXmlSkill>();
        }

        xmlSkills.Add(skill);
        GameApp.GetSkillManager().AddXmlSkill(skill);
    }
    public void RemoveSkill(int id)
    {
        for (int i = 0; i < xmlSkills.Count; i++ )
        {
            if(xmlSkills[i].ID.Equals(id))
            {
                xmlSkills.RemoveAt(i);
                break;
            }
        }
    }
    public void CreateEditor(int id,string name) 
    {
        Index = id;
        Name = name;
    }

    public void Export(XmlDocument doc, XmlNode parent)
    {
        XmlElement chain = doc.CreateElement("chain");

        chain.SetAttribute("index",Index.ToString());
        chain.SetAttribute("name", Name);
        chain.SetAttribute("distance", Distance.ToString());
        if (CoolDown != null)
        {
            CoolDown.Export(doc, chain, "coolDown");
        }
        Dictionary<string, CXmlSkill> writed = new Dictionary<string, CXmlSkill>();
        for (int i = 0; i < xmlSkills.Count; i++ )
        {
            if (!writed.ContainsKey(xmlSkills[i].ID.ToString()))
            {
                XmlElement skillPath = doc.CreateElement("skill");
                if (!string.IsNullOrEmpty(xmlSkills[i].m_FileName))
                {
                    skillPath.SetAttribute("filename",  xmlSkills[i].m_FileName);

                }
                else { skillPath.SetAttribute("filename", xmlSkills[i].Name); }
                xmlSkills[i].Export();
                chain.AppendChild(skillPath);

                writed.Add(xmlSkills[i].ID.ToString(), xmlSkills[i]);
            }

        }

        parent.AppendChild(chain);
    }

    private int Sort(CXmlSkill a,CXmlSkill b)
    {
        return a.ID.CompareTo(b.ID);
    }
#endif
}