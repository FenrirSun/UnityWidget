using System;
using System.Xml;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif
public class CXmlSkillTarget
{
    public string Value{get;private set;}

    public CXmlSkillTarget(){}

    public void Init(XmlElement ele)
    {
       CXmlRead kXmlRead = new CXmlRead(ele);
       Value = kXmlRead.Str("value");
    }

    public ulong GetTargetBeforeCreate(CSkillEvent preEv,CSkill skill)
    {
        ulong nRet = 0;
        if (Value == "auto")
        {
            nRet = skill.DefaultTargetID;
        }
        else if(Value == "preEvent")
        {
            nRet = preEv.TargetID;
        }
        else if (Value == "self")
        {
            nRet = skill.Owner.m_NetObjectID;
        }
        return nRet;
    }


#if UNITY_EDITOR
    private bool m_Draw = false;
    public bool m_Effective = false;
    public bool m_IsDrawEffectToggle = false;

    public enum TargetType
    {
        t_preEvent,
        t_auto,
        t_none,
        t_self,
        t_event,
    }
    public void Draw()
    {
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        if (m_IsDrawEffectToggle)
        {
            m_Effective = EditorGUILayout.Toggle(m_Effective, GUILayout.Width(10));
        } 
        m_Draw = EditorGUILayout.Foldout(m_Draw, "Target");
        GUILayout.EndHorizontal();
        if(m_Draw)
        {
            GUILayout.BeginHorizontal();
            if (string.IsNullOrEmpty(Value))
            {
                Value = CutStr(TargetType.t_none.ToString());
            }
            EditorGUILayout.LabelField("目标类型:", GUILayout.Width(70));
            Value = CutStr(EditorGUILayout.EnumPopup((TargetType)System.Enum.Parse(typeof(TargetType), "t_" + Value)).ToString());
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

    }

    private string CutStr(string str)
    {
        if(str.StartsWith("t_"))
        {
            str = str.Substring(2, str.Length - 2);
        }
        return str;
    }
    
    public void Export(XmlDocument doc, XmlNode parent,string name = "target")
    {
        XmlElement target = doc.CreateElement(name);

        target.SetAttribute("value",Value);

        parent.AppendChild(target);
    }
#endif
}